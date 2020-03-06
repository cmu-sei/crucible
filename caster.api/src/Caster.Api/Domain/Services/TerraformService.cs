/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Diagnostics;
using System.Text;
using Caster.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;

namespace Caster.Api.Domain.Services
{
    public interface ITerraformService
    {
        TerraformResult InitializeWorkspace(string workingDirectory, string workspaceName, bool defaultWorkspace, DataReceivedEventHandler outputHandler);
        TerraformResult Init(string workingDirectory, DataReceivedEventHandler outputHandler);
        TerraformResult SelectWorkspace(string workingDirectory, string workspaceName, DataReceivedEventHandler outputHandler);
        TerraformResult Plan(string workingDirectory, bool destroy, string[] targets, DataReceivedEventHandler outputHandler);
        TerraformResult Apply(string workingDirectory, DataReceivedEventHandler outputHandler);
        TerraformResult Show(string workingDirectory);
        TerraformResult Taint(string workingDirectory, string address, string statePath);
        TerraformResult Untaint(string workingDirectory, string address, string statePath);
        TerraformResult Refresh(string workingDirectory, string statePath);
    }

    public class TerraformResult
    {
        public string Output { get; set; }
        public int ExitCode { get; set; }

        public bool IsError
        {
            get
            {
                return ExitCode != 0;
            }
        }
    }

    public class TerraformService : ITerraformService
    {
        private readonly TerraformOptions _options;
        private readonly ILogger<TerraformService> _logger;
        private StringBuilder _outputBuilder = new StringBuilder();

        public TerraformService(TerraformOptions options, ILogger<TerraformService> logger)
        {
            _options = options;
            _logger = logger;
        }

        private TerraformResult Run(string workingDirectory, string arguments, DataReceivedEventHandler outputHandler)
        {
            int exitCode;
            _outputBuilder.Clear();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _options.BinaryPath;
            startInfo.EnvironmentVariables.Add("TF_IN_AUTOMATION", "true");
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.StartInfo.Arguments = arguments.Replace("\"", "\"\"\""); // process args need quotes to be escaped as triple quotes

                process.OutputDataReceived += outputHandler;
                process.OutputDataReceived += OutputHandler;

                process.ErrorDataReceived += outputHandler;
                process.ErrorDataReceived += OutputHandler;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                exitCode = process.ExitCode;
            }

            return new TerraformResult
            {
                Output = _outputBuilder.ToString(),
                ExitCode = exitCode
            };
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _outputBuilder.Append(e.Data);
                _logger.LogTrace(e.Data);
            }
        }

        /// <summary>
        /// Combines Init and Select Workspace
        /// </summary>
        public TerraformResult InitializeWorkspace(string workingDirectory, string workspaceName, bool defaultWorkspace, DataReceivedEventHandler outputHandler)
        {
            var result = this.Init(workingDirectory, outputHandler);

            if (!result.IsError)
            {
                if (!defaultWorkspace)
                {
                    var workspaceResult = this.SelectWorkspace(workingDirectory, workspaceName, outputHandler);
                    result.Output += workspaceResult.Output;
                    result.ExitCode = workspaceResult.ExitCode;
                }
            }

            return result;
        }

        public TerraformResult Init(string workingDirectory, DataReceivedEventHandler outputHandler)
        {
            string args = "init -input=false";

            if (!string.IsNullOrEmpty(_options.PluginDirectory))
            {
                args +=  $" -plugin-dir={_options.PluginDirectory}";
            }

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult SelectWorkspace(string workingDirectory, string workspaceName, DataReceivedEventHandler outputHandler)
        {
            return this.Run(workingDirectory, $"workspace select {workspaceName}", outputHandler);
        }

        public TerraformResult Plan(string workingDirectory, bool destroy, string[] targets, DataReceivedEventHandler outputHandler)
        {
            string args = "plan -input=false -out=plan";

            if (destroy)
            {
                args += " -destroy";
            }

            foreach(string target in targets)
            {
                args += $" --target={SanitizeArgument(target)}";
            }

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult Apply(string workingDirectory, DataReceivedEventHandler outputHandler)
        {
            return this.Run(workingDirectory, "apply plan", outputHandler);
        }

        public TerraformResult Show(string workingDirectory)
        {
            return this.Run(workingDirectory, "show -json plan", null);
        }

        public TerraformResult Taint(string workingDirectory, string address, string statePath)
        {
            return this.Run(workingDirectory, $"taint{this.GetStatePathArg(statePath)} {SanitizeArgument(address)}", null);
        }

        public TerraformResult Untaint(string workingDirectory, string address, string statePath)
        {
            return this.Run(workingDirectory, $"untaint{this.GetStatePathArg(statePath)} {SanitizeArgument(address)}", null);
        }

        public TerraformResult Refresh(string workingDirectory, string statePath)
        {
            return this.Run(workingDirectory, $"refresh{this.GetStatePathArg(statePath)}", null);
        }

        private string GetStatePathArg(string statePath)
        {
            return string.IsNullOrEmpty(statePath) ? string.Empty : $" -state={statePath}";
        }

        private string SanitizeArgument(string arg)
        {
            return arg.Split(' ', 2)[0];
        }
    }
}


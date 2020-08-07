/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Generic;
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
        private string _workspaceName = null;

        public TerraformService(TerraformOptions options, ILogger<TerraformService> logger)
        {
            _options = options;
            _logger = logger;
        }

        private TerraformResult Run(string workingDirectory, IEnumerable<string> argumentList, DataReceivedEventHandler outputHandler, bool redirectStandardError = true)
        {
            int exitCode;
            _outputBuilder.Clear();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _options.BinaryPath;
            startInfo.WorkingDirectory = workingDirectory;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = redirectStandardError;
            startInfo.EnvironmentVariables.Add("TF_IN_AUTOMATION", "true");

            if (!string.IsNullOrEmpty(_workspaceName))
            {
                startInfo.EnvironmentVariables.Add("TF_WORKSPACE", _workspaceName);
            }

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                if (argumentList != null)
                {
                    foreach (string arg in argumentList)
                    {
                        process.StartInfo.ArgumentList.Add(arg);
                    }
                }

                process.OutputDataReceived += outputHandler;
                process.OutputDataReceived += OutputHandler;

                if (redirectStandardError)
                {
                    process.ErrorDataReceived += outputHandler;
                    process.ErrorDataReceived += OutputHandler;
                }

                process.Start();
                process.BeginOutputReadLine();

                if (redirectStandardError)
                {
                    process.BeginErrorReadLine();
                }

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
            // Set TF_WORKSPACE env var for init to workaround bug with an empty configuration
            // Will need to avoid this for a remote state init
            _workspaceName = workspaceName;
            var result = this.Init(workingDirectory, outputHandler);
            _workspaceName = null;

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
            List<string> args = new List<string>();
            args.Add("init");
            args.Add("-input=false");

            if (!string.IsNullOrEmpty(_options.PluginDirectory))
            {
                args.Add($"-plugin-dir={_options.PluginDirectory}");
            }

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult SelectWorkspace(string workingDirectory, string workspaceName, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("workspace");
            args.Add("select");
            args.Add(workspaceName);

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult Plan(string workingDirectory, bool destroy, string[] targets, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("plan");
            args.Add("-input=false");
            args.Add("-out=plan");

            if (destroy)
            {
                args.Add("-destroy");
            }

            foreach (string target in targets)
            {
                args.Add($"--target={target}");
            }

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult Apply(string workingDirectory, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("apply");
            args.Add("plan");

            return this.Run(workingDirectory, args, outputHandler);
        }

        public TerraformResult Show(string workingDirectory)
        {
            List<string> args = new List<string>();
            args.Add("show");
            args.Add("-json");
            args.Add("plan");

            return this.Run(workingDirectory, args, null, redirectStandardError: false);
        }

        public TerraformResult Taint(string workingDirectory, string address, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("taint");
            AddStatePathArg(statePath, ref args);
            args.Add(address);

            return this.Run(workingDirectory, args, null);
        }

        public TerraformResult Untaint(string workingDirectory, string address, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("untaint");
            AddStatePathArg(statePath, ref args);
            args.Add(address);

            return this.Run(workingDirectory, args, null);
        }

        public TerraformResult Refresh(string workingDirectory, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("refresh");
            AddStatePathArg(statePath, ref args);
            return this.Run(workingDirectory, args, null);
        }

        private void AddStatePathArg(string statePath, ref List<string> args)
        {
            if (!string.IsNullOrEmpty(statePath))
            {
                args.Add($"-state={statePath}");
            }
        }
    }
}

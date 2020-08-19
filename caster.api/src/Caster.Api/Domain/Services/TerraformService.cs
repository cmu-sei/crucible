/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;

namespace Caster.Api.Domain.Services
{
    public interface ITerraformService
    {
        TerraformResult InitializeWorkspace(Workspace workspace, DataReceivedEventHandler outputHandler);
        TerraformResult Init(Workspace workspace, DataReceivedEventHandler outputHandler);
        TerraformResult SelectWorkspace(Workspace workspace, DataReceivedEventHandler outputHandler);
        TerraformResult Plan(Workspace workspace, bool destroy, string[] targets, DataReceivedEventHandler outputHandler);
        TerraformResult Apply(Workspace workspace, DataReceivedEventHandler outputHandler);
        TerraformResult Show(Workspace workspace);
        TerraformResult Taint(Workspace workspace, string address, string statePath);
        TerraformResult Untaint(Workspace workspace, string address, string statePath);
        TerraformResult Refresh(Workspace workspace, string statePath);
        bool IsValidVersion(string version);
        IEnumerable<string> GetVersions();
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
        private const string _binaryName = "terraform";
        private readonly TerraformOptions _options;
        private readonly ILogger<TerraformService> _logger;
        private StringBuilder _outputBuilder = new StringBuilder();
        private string _workspaceName = null;

        public TerraformService(TerraformOptions options, ILogger<TerraformService> logger)
        {
            _options = options;
            _logger = logger;
        }

        private string GetBinaryPath(Workspace workspace)
        {
            return System.IO.Path.Combine(
                _options.BinaryPath,
                string.IsNullOrEmpty(workspace.TerraformVersion) ?
                    _options.DefaultVersion :
                    workspace.TerraformVersion,
                _binaryName
            );
        }

        private TerraformResult Run(Workspace workspace, IEnumerable<string> argumentList, DataReceivedEventHandler outputHandler, bool redirectStandardError = true)
        {
            int exitCode;
            _outputBuilder.Clear();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = this.GetBinaryPath(workspace);
            startInfo.WorkingDirectory = workspace.GetPath(_options.RootWorkingDirectory);
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
        public TerraformResult InitializeWorkspace(Workspace workspace, DataReceivedEventHandler outputHandler)
        {
            // Set TF_WORKSPACE env var for init to workaround bug with an empty configuration
            // Will need to avoid this for a remote state init
            _workspaceName = workspace.Name;
            var result = this.Init(workspace, outputHandler);
            _workspaceName = null;

            if (!result.IsError)
            {
                if (!workspace.IsDefault)
                {
                    var workspaceResult = this.SelectWorkspace(workspace, outputHandler);
                    result.Output += workspaceResult.Output;
                    result.ExitCode = workspaceResult.ExitCode;
                }
            }

            return result;
        }

        public TerraformResult Init(Workspace workspace, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("init");
            args.Add("-input=false");

            if (!string.IsNullOrEmpty(_options.PluginDirectory))
            {
                args.Add($"-plugin-dir={_options.PluginDirectory}");
            }

            return this.Run(workspace, args, outputHandler);
        }

        public TerraformResult SelectWorkspace(Workspace workspace, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("workspace");
            args.Add("select");
            args.Add(workspace.Name);

            return this.Run(workspace, args, outputHandler);
        }

        public TerraformResult Plan(Workspace workspace, bool destroy, string[] targets, DataReceivedEventHandler outputHandler)
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

            return this.Run(workspace, args, outputHandler);
        }

        public TerraformResult Apply(Workspace workspace, DataReceivedEventHandler outputHandler)
        {
            List<string> args = new List<string>();
            args.Add("apply");
            args.Add("plan");

            return this.Run(workspace, args, outputHandler);
        }

        public TerraformResult Show(Workspace workspace)
        {
            List<string> args = new List<string>();
            args.Add("show");
            args.Add("-json");
            args.Add("plan");

            return this.Run(workspace, args, null, redirectStandardError: false);
        }

        public TerraformResult Taint(Workspace workspace, string address, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("taint");
            AddStatePathArg(statePath, ref args);
            args.Add(address);

            return this.Run(workspace, args, null);
        }

        public TerraformResult Untaint(Workspace workspace, string address, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("untaint");
            AddStatePathArg(statePath, ref args);
            args.Add(address);

            return this.Run(workspace, args, null);
        }

        public TerraformResult Refresh(Workspace workspace, string statePath)
        {
            List<string> args = new List<string>();
            args.Add("refresh");
            AddStatePathArg(statePath, ref args);
            return this.Run(workspace, args, null);
        }

        public bool IsValidVersion(string version)
        {
            var path = System.IO.Path.Combine(
                _options.BinaryPath,
                version);

            return System.IO.Directory.Exists(path);
        }

        public IEnumerable<string> GetVersions()
        {
            return System.IO.Directory.EnumerateDirectories(_options.BinaryPath)
                .Select(x => System.IO.Path.GetFileName(x))
                .OrderByDescending(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort());
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

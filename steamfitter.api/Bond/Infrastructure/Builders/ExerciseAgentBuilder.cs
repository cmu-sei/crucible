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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Management;
using Bond.Infrastructure.Code;
using Bond.Infrastructure.Models;
using NLog;
using Steamfitter.Api.Data.Models;

namespace Bond.Infrastructure.Builders
{
    /// <summary>
    /// Builds a MachineSurvey object to be returned to the server
    /// </summary>
    internal static class MachineSurveyBuilder
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Build the MachineSurvey information
        /// </summary>
        /// <returns>MachineSurvey</returns>
        internal static BondAgent Build()
        {
            var bondAgent = new BondAgent
            {
                MonitoredTools = GetMonitoredTools(),
                LocalUsers = LoadLocalUsers(),
                OperatingSystem = new OS(Environment.OSVersion),
                AgentName = ApplicationDetails.Name,
                AgentVersion = ApplicationDetails.Version,
                AgentInstalledPath = ApplicationDetails.InstalledPath,
                MachineName = Environment.MachineName,
                FQDN = Dns.GetHostName(),
                GuestIp = GetLocalIpAddress()
            };

            //machineSurvey.Id =
            bondAgent.VmWareUuid = GetGuestInfoVar(bondAgent);

            bondAgent.SshPorts = BondManager.CurrentPorts;
            return bondAgent;
        }

        /// <summary>
        /// Gets the local users from Win32_UserAccount
        /// </summary>
        /// <returns>List of LocalUsers</returns>
        private static List<LocalUser> LoadLocalUsers()
        {
            var users = new List<LocalUser>();
            try
            {
                var query = new SelectQuery("Win32_UserAccount");
                var searcher = new ManagementObjectSearcher(query);
                foreach (var user in searcher.Get())
                {
                    var u = new LocalUser {Username = user["Name"].ToString(), Domain = user["Domain"].ToString()};
                    u.IsCurrent = (u.Username == Environment.UserName);
                    users.Add(u);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            return users;
        }

        /// <summary>
        /// Gather the information for each MonitoredTool in the app's configuration 
        /// </summary>
        private static IEnumerable<MonitoredTool> GetMonitoredTools()
        {
            var files = new List<MonitoredTool>();

            if (BondManager.Configuration == null || BondManager.Configuration.MonitoredTools == null) 
                return files;
            
            foreach (var toolConfig in BondManager.Configuration.MonitoredTools)
            {
                var file = new MonitoredTool();
                try
                {
                    var rootPath = Path.GetPathRoot(toolConfig.Location);
                    if (rootPath == null)
                        continue;

                    var fs = new FileSearch();
                    fs.WalkDirectoryTree(new DirectoryInfo(rootPath), toolConfig.FileName);

                    foreach (var fileSearch in fs.AllFiles)
                    {
                        file.Location = fileSearch.FullName;
                        var fvi = FileVersionInfo.GetVersionInfo(file.Location);
                        file.Name = fvi.FileDescription;
                        file.Version = fvi.FileVersion;
                        file.IsRunning = ProgramIsRunning(file.Location);
                        files.Add(file);
                        break;
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }

            return files;
        }

        /// <summary>
        /// Is there a process running with the same name as the file?
        /// </summary>
        private static bool ProgramIsRunning(string path)
        {
            var filePath = Path.GetDirectoryName(path);
            if (filePath == null) return false;
            var fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            var pList = Process.GetProcessesByName(fileName);
            return pList.Any(p => p.MainModule.FileName.StartsWith(filePath, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Gets the IP address from the network interface
        /// </summary>
        /// <returns>IP address</returns>
        private static string GetLocalIpAddress()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// if configured, try to set machine.name based on some vmtools.exe guestinfovars value
        /// </summary>
        /// <param name="bondAgent">The guestinfovars value</param>
        private static Guid GetGuestInfoVar(BondAgent bondAgent)
        {
            try
            {
                if (BondManager.Configuration.VmTools.IsEnabled)
                {
                    var p = new Process
                    {
                        StartInfo =
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = $"{BondManager.Configuration.VmTools.VmWareToolsLocation}",
                            Arguments = $"--cmd \"info-get {BondManager.Configuration.VmTools.IdFormatKey}\""
                        }
                    };
                    p.Start();
                    var output = p.StandardOutput.ReadToEnd().Trim();
                    p.WaitForExit();
                    if (!string.IsNullOrEmpty(output))
                    {
                        var o = BondManager.Configuration.VmTools.IdFormatValue;
                        o = o.Replace("[formatkeyvalue]", output);
                        o = o.Replace("[machinename]", bondAgent.MachineName);

                        return Guid.Parse(o);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            return Guid.Empty;
        }
    }
}

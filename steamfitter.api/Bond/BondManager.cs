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
using System.Collections.Generic;
using System.Linq;
using Bond.Infrastructure.Builders;
using Bond.Infrastructure.Code;
using Bond.Infrastructure.Models;
using NLog;
using Steamfitter.Api.Data.Models;

namespace Bond
{
    /// <summary>
    /// cyberforce version of exercise client
    /// </summary>
    public static class BondManager
    {
        internal static ClientConfiguration Configuration { get; private set; }
        internal static List<SshPort> CurrentPorts { get; private set; }
        private static ApplicationMode _mode { get; set; }
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static void Run(string[] args)
        {
            CurrentPorts = new List<SshPort>();

            try
            {
                Console.WriteLine(RunEx(args));
            }
            catch (Exception e)
            {
                _log.Fatal($"Fatal exception in {ApplicationDetails.Name} {ApplicationDetails.Version}: {e}");
                Console.ReadLine();
            }
        }

        internal static string RunEx(string[] args)
        {
            if (args != null && args.ToList().Contains("--version"))
            {
                return ApplicationDetails.VersionString;
            }

            if (args != null && args.ToList().Contains("--debug"))
            {
                _mode = ApplicationMode.Debug;
            }

#if DEBUG
            _mode = ApplicationMode.Debug;
#endif

            _log.Info($"{ApplicationDetails.Name} {ApplicationDetails.Version} running in {_mode} mode. Installed path: {ApplicationDetails.InstalledPath} - Local time: {DateTime.Now} UTC: {DateTime.UtcNow}");

            // load configuration
            Configuration = ClientConfigurationBuilder.Build();

            // start reporter
            ReportService.Run(Configuration.Reporter);

            // now start ssh            
            SshService.Run(Configuration.Ssh);
            
            _log.Info($"{ApplicationDetails.Name} {ApplicationDetails.Version} exiting - Local time: {DateTime.Now} UTC: {DateTime.UtcNow}");
            
            return string.Empty;
        }
    }
}

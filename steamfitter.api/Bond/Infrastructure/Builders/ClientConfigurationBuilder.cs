/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.IO;
using Bond.Infrastructure.Code;
using Bond.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Bond.Infrastructure.Builders
{
    /// <summary>
    /// Load the configuration from appsettings.json into a proper class
    /// </summary>
    internal class ClientConfigurationBuilder
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal static ClientConfiguration Build()
        {
            var currentDir = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var config = configuration.GetSection("ClientConfiguration").Get<ClientConfiguration>();

            if (config.Reporter == null)
            {
                var msg = $"{ApplicationDetails.Name} issue loading configuration from {currentDir}...";
                _log.Fatal(msg);
                throw new IOException(msg);
            }

            _log.Info($"{ApplicationDetails.Name} configuration loaded...");
            return config;
        }
    }
}

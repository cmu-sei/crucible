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
using Steamfitter.Api.Data.Models;

namespace Bond.Infrastructure.Models
{
    /// <summary>
    /// The configuration for a bond client
    /// </summary>
    public class ClientConfiguration
    {
        /// <summary>
        /// Configuration for running SSH 
        /// </summary>
        public class SshOptions
        {
            /// <summary>
            /// Turn SSH on and off
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// Interval for checking if SSH is connected
            /// </summary>
            public int CheckProcessIntervalInSeconds { get; set; }

            /// <summary>
            /// SSH Server host
            /// </summary>
            public string Host { get; set; }

            /// <summary>
            /// SSH Server port
            /// </summary>
            public int Port { get; set; }

            /// <summary>
            /// SSH Server username
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// SSH Server password
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// SSH connection keep-alive setting
            /// </summary>
            public int KeepAliveIntervalInSeconds { get; set; }

            /// <summary>
            /// SSH port forwarding setup info  
            /// </summary>
            public IEnumerable<SshPort> SshPorts { get; set; }
        }

        /// <summary>
        /// Configuration for client reporting
        /// </summary>
        public class ReporterOptions
        {
            /// <summary>
            /// Is this client reporting home?
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// URI to post client information
            /// </summary>
            public string PostUrl { get; set; }

            /// <summary>
            /// Interval for how often to post client information
            /// </summary>
            public int IntervalInSeconds { get; set; }
        }

        /// <summary>
        /// For configuring a client to retrieve values from vmwaretools guestinfovars values
        /// </summary>
        public class VmToolsOptions
        {
            /// <summary>
            /// Is this client using guestinfovars values?
            /// </summary>
            public bool IsEnabled { get; set; }

            /// <summary>
            /// The guestinfovars value to read 
            /// </summary>
            public string IdFormatKey { get; set; }

            /// <summary>
            /// The format string to replace with guestinfovars values 
            /// </summary>
            public string IdFormatValue { get; set; }

            /// <summary>
            /// The default location for vmwaretoolsd.exe on the client machine
            /// </summary>
            public string VmWareToolsLocation { get; set; }
        }

        public class MonitoredTool
        {
            public string Name { get; set; }
            public string FileName { get; set; }
            public string Location { get; set; }
        }
        
        public IEnumerable<MonitoredTool> MonitoredTools { get; set; }
        
        public VmToolsOptions VmTools { get; set; }
        public SshOptions Ssh { get; set; }
        public ReporterOptions Reporter { get; set; }
    }
}

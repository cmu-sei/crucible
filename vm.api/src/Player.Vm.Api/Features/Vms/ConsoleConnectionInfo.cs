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
using System.ComponentModel.DataAnnotations;
using Player.Vm.Api.Domain.Models;

namespace Player.Vm.Api.Features.Vms
{
    public class ConsoleConnectionInfo
    {
        /// <summary>
        /// The hostname or address to use to connect
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// The port to use to connect
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// The protocol to use to connect, such as rdp, ssh, etc
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// The optional username to use to connect
        /// If omitted, the user will be prompted on connection
        /// Note: This must be set to connect to a Windows machine using Network Level Authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The optional password to use to connect
        /// If omitted, the user will be prompted on connection
        /// Note: This must be set to connect to a Windows machine using Network Level Authentication
        /// </summary>
        public string Password { get; set; }
    }
}

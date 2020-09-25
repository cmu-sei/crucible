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
    public class Vm
    {
        /// <summary>
        /// Virtual Machine unique id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Url to the Vm's console
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The Vm's name
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Id of the Vm's owner if it is a personal Vm
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// A list of networks that a regular user can access
        /// </summary>
        public string[] AllowedNetworks { get; set; }

        /// <summary>
        /// The Vm's last known power state
        /// </summary>
        public PowerState PowerState { get; set; }

        /// <summary>
        /// A list of IP addresses of the Vm
        /// </summary>
        public string[] IpAddresses { get; set; }

        /// <summary>
        /// The Ids of the Team's the Vm is a part of
        /// </summary>
        public IEnumerable<Guid> TeamIds { get; set; }

        /// <summary>
        /// True if this Vm currently has pending tasks (power on, power off, etc)
        /// </summary>
        public bool HasPendingTasks { get; set; }

        /// <summary>
        /// The connection info for connecting to a Vm console through Guacamole.
        /// This is used for non-VMware Vms such as in Azure or AWS.
        /// </summary>
        public ConsoleConnectionInfo ConsoleConnectionInfo { get; set; }
    }
}

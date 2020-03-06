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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caster.Api.Domain.Models
{
    public class Host
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Datastore { get; set; }
        public int MaximumMachines { get; set; }
        public bool Enabled { get; set; }
        public bool Development { get; set; }

        public Guid? ExerciseId { get; set; }
        public virtual Exercise Exercise { get; set; }  

        public virtual ICollection<HostMachine> Machines { get; set; } = new HashSet<HostMachine>();

        public File GetHostFile()
        {
            return new File
            {
                Name = "generated_host_values.auto.tfvars",
                Content = $"vsphere_host_name = \"{Name}\"\nvsphere_datastore = \"{Datastore}\""
            };
        }
    }

    public class HostMachine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; }

        public Guid HostId { get; set; } 
        public virtual Host Host { get; set; }
    }
}


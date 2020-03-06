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
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Domain.Models
{
    public class PlanOutput
    {
        [JsonPropertyName("resource_changes")]
        public ResourceChange[] ResourceChanges { get; set; }

        public ResourceChange[] GetAddedMachines()
        {
            if (ResourceChanges == null) return new ResourceChange[] {};
            return ResourceChanges.Where(r => r.Type == "vsphere_virtual_machine" && r.Change.Actions.Contains(ChangeType.Create)).ToArray();
        }
    }

    public class ResourceChange
    {
        public string Address { get; set; }
        [JsonPropertyName("module_address")]
        public string ModuleAddress { get; set; }
        public string Name { get; set; }
        public string Mode { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string Deposed { get; set; }
        public Change Change { get; set; }
    }

    public class Change
    {
        public ChangeType[] Actions { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChangeType
    {
        [EnumMember(Value = "no-op")]
        Noop,
        Create,
        Read,
        Update,
        Delete
    }
}


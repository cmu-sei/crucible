/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Generic;
using System.Text.Json;

namespace Caster.Api.Features.Resources
{
    public class Resource
    {
        /// <summary>
        /// The Id of this Resource
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Name of this Resource
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Terraform identifier for the type of this Resource.
        /// e.g. vsphere_virtual_machine, vsphere_virtual_network, etc
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The Terraform Resource Address that references this Resource's Base Resource.
        /// If this Resource was created with count > 1, using the Base Address as a target
        /// will target all of the resources created from it.
        /// BaseAddress will be null if the Resource was created with count = 1.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// The Terraform Resource Address that references this specific Resource.
        /// Used for the targets list when creating a Run
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// True if this Resource is tainted, meaning it will be destroyed and re-created on next Run
        /// </summary>
        public bool Tainted { get; set; }

        /// <summary>
        /// Type-specific additional attributes for searching on. Always returned.
        /// </summary>
        public Dictionary<string, object> SearchableAttributes { get; set; }

        /// <summary>
        /// Raw json of all additional type-specific attributes. Only returned when requesting a single Resource; otherwise null.
        /// </summary>
        public JsonElement? Attributes { get; set; }
    }
}


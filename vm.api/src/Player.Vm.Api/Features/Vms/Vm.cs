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
        [Required]
        [Display(Name = "id", Description = "Virtual Machine GUID")]
        public Guid Id { get; set; }

        [Display(Name = "url", Description = "Virtual Machine URL")]
        public string Url { get; set; }

        [Display(Name = "name", Description = "Virtual Machine Display Name")]
        public string Name { get; set; }

        [Display(Name = "viewId", Description = "The Id of the View that this Vm's Teams belong to")]
        public Guid ViewId { get; set; }

        [Display(Name = "userId", Description = "UserId of this Vm's owner if it is a personal Vm")]
        public Guid? UserId { get; set; }

        [Display(Name = "isOwner", Description = "True if this is a personal Vm and the calling user is it's owner")]
        public bool IsOwner { get; set; }

        [Display(Name = "canAccessNicConfiguration", Description = "True if the user is a ViewAdmin for this Vm's view")]
        public bool CanAccessNicConfiguration { get; set; }

        [Display(Name = "allowedNetworks", Description = "A list of networks that a regular user can access")]
        public List<string> AllowedNetworks { get; set; }

        [Display(Name = "teamId", Description = "The Id of the primary team this Vm's User belong to")]
        public Guid TeamId { get; set; }

        public PowerState PowerState { get; set; }

    }
}

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
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace S3.Player.Api.Data.Data.Models
{
    public class TeamEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? RoleId { get; set; }
        public RoleEntity Role { get; set; }

        public Guid ViewId { get; set; }
        public virtual ViewEntity View { get; set; }

        public virtual ICollection<ApplicationInstanceEntity> Applications { get; set; } = new List<ApplicationInstanceEntity>();
        public virtual ICollection<TeamMembershipEntity> Memberships { get; set; } = new List<TeamMembershipEntity>();
        public virtual ICollection<TeamPermissionEntity> Permissions { get; set; } = new List<TeamPermissionEntity>();

        public TeamEntity() {}

        public TeamEntity Clone()
        {
            var entity = this.MemberwiseClone() as TeamEntity;
            entity.Applications = new List<ApplicationInstanceEntity>();
            entity.Memberships = new List<TeamMembershipEntity>();
            entity.Permissions = new List<TeamPermissionEntity>();
            entity.Id = Guid.Empty;
            entity.ViewId = Guid.Empty;
            entity.View = null;

            return entity;
        }
    }
}

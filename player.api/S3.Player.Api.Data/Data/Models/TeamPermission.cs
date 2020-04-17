/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace S3.Player.Api.Data.Data.Models
{
    public class TeamPermissionEntity
    {
        public TeamPermissionEntity() { }

        public TeamPermissionEntity(Guid teamId, Guid permissionId)
        {
            TeamId = teamId;
            PermissionId = permissionId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public virtual TeamEntity Team { get; set; }

        public Guid PermissionId { get; set; }
        public virtual PermissionEntity Permission { get; set; }
    }

    public class TeamPermissionConfiguration : IEntityTypeConfiguration<TeamPermissionEntity>
    {
        public void Configure(EntityTypeBuilder<TeamPermissionEntity> builder)
        {
            builder.HasIndex(x => new { x.TeamId, x.PermissionId }).IsUnique();

            builder
                .HasOne(rp => rp.Team)
                .WithMany(r => r.Permissions);
        }
    }
}


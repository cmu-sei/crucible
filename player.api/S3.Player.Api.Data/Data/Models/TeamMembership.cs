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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace S3.Player.Api.Data.Data.Models
{
    public class TeamMembershipEntity
    {
        public TeamMembershipEntity() { }

        public TeamMembershipEntity(Guid teamId, Guid userId)
        {
            TeamId = teamId;
            UserId = userId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public virtual TeamEntity Team { get; set; }

        public Guid UserId { get; set; }
        public virtual UserEntity User { get; set; }

        public Guid ViewMembershipId { get; set; }
        public virtual ViewMembershipEntity ViewMembership { get; set; }

        public Guid? RoleId { get; set; }
        public RoleEntity Role { get; set; }
    }

    public class TeamMembershipConfiguration : IEntityTypeConfiguration<TeamMembershipEntity>
    {
        public void Configure(EntityTypeBuilder<TeamMembershipEntity> builder)
        {
            builder.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();

            builder
                .HasOne(tu => tu.Team)
                .WithMany(t => t.Memberships)
                .HasForeignKey(tu => tu.TeamId);

            builder
                .HasOne(tm => tm.ViewMembership)
                .WithMany(t => t.TeamMemberships)
                .HasForeignKey(tm => tm.ViewMembershipId);

            builder
                .HasOne(tu => tu.User)
                .WithMany(u => u.TeamMemberships)
                .HasForeignKey(tu => tu.UserId)
                .HasPrincipalKey(u => u.Id);
        }
    }
}

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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace S3.Player.Api.Data.Data.Models
{
    public class ViewMembershipEntity
    {
        public ViewMembershipEntity() { }

        public ViewMembershipEntity(Guid viewId, Guid userId)
        {
            ViewId = viewId;
            UserId = userId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid ViewId { get; set; }
        public virtual ViewEntity View { get; set; }

        public Guid UserId { get; set; }
        public virtual UserEntity User { get; set; }

        public Guid? PrimaryTeamMembershipId { get; set; }
        public virtual TeamMembershipEntity PrimaryTeamMembership { get; set; }

        public virtual ICollection<TeamMembershipEntity> TeamMemberships { get; set; } = new List<TeamMembershipEntity>();
    }

    public class ViewMembershipConfiguration : IEntityTypeConfiguration<ViewMembershipEntity>
    {
        public void Configure(EntityTypeBuilder<ViewMembershipEntity> builder)
        {
            builder.HasIndex(e => new { e.ViewId, e.UserId }).IsUnique();

            builder
                .HasOne(em => em.View)
                .WithMany(e => e.Memberships)
                .HasForeignKey(em => em.ViewId);

            builder
                .HasOne(em => em.User)
                .WithMany(u => u.ViewMemberships)
                .HasForeignKey(em => em.UserId)
                .HasPrincipalKey(u => u.Id);

            builder
                .HasOne(x => x.PrimaryTeamMembership);


                //.WithOne(y => y.ViewMembership)
                //.HasForeignKey<ViewMembershipEntity>(x => x.PrimaryTeamMembershipId);
        }
    }
}

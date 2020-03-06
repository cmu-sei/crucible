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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Steamfitter.Api.Data.Models
{
    public class UserPermissionEntity
    {
        public UserPermissionEntity() { }

        public UserPermissionEntity(Guid userId, Guid permissionId)
        {
            UserId = userId;
            PermissionId = permissionId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public UserEntity User { get; set; }

        public Guid PermissionId { get; set; }
        public PermissionEntity Permission { get; set; }
    }

    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermissionEntity>
    {
        public void Configure(EntityTypeBuilder<UserPermissionEntity> builder)
        {
            builder.HasIndex(x => new { x.UserId, x.PermissionId }).IsUnique();

            builder
                .HasOne(u => u.User)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(x => x.UserId);
            builder
                .HasOne(u => u.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(x => x.PermissionId);
        }
    }
}


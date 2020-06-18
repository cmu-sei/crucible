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
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Data.Data.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace S3.Player.Api.Data.Data
{
    public class PlayerContext : DbContext
    {
        private DbContextOptions<PlayerContext> _options;

        public PlayerContext(DbContextOptions<PlayerContext> options) : base(options) {
            _options = options;
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<ViewEntity> Views { get; set; }
        public DbSet<TeamEntity> Teams { get; set; }
        public DbSet<ApplicationTemplateEntity> ApplicationTemplates { get; set; }
        public DbSet<ApplicationEntity> Applications { get; set; }
        public DbSet<ApplicationInstanceEntity> ApplicationInstances { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
        public DbSet<ViewMembershipEntity> ViewMemberships { get; set; }
        public DbSet<TeamMembershipEntity> TeamMemberships { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<PermissionEntity> Permissions { get; set; }
        public DbSet<RolePermissionEntity> RolePermissions { get; set; }
        public DbSet<TeamPermissionEntity> TeamPermissions { get; set; }
        public DbSet<UserPermissionEntity> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurations();

            // Apply PostgreSQL specific options
            if (Database.IsNpgsql())
            {
                modelBuilder.AddPostgresUUIDGeneration();
                modelBuilder.UsePostgresCasing();
            }
        }
    }
}

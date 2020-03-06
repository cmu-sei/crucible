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
using System.Linq;
using System.Threading.Tasks;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Data
{
    public partial class CasterContext : DbContext
    {
        private DbContextOptions<CasterContext> _options;

        public CasterContext(DbContextOptions<CasterContext> options) : base(options) {
            _options = options;
        }

        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Directory> Directories { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Apply> Applies { get; set; }
        public DbSet<RemovedResource> RemovedResources { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<ModuleVersion> ModuleVersions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Host> Hosts { get; set; }
        public DbSet<HostMachine> HostMachines { get; set; }

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


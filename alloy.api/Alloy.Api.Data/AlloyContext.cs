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
using Alloy.Api.Data.Models;
using Alloy.Api.Data.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alloy.Api.Data
{
    public class AlloyContext : DbContext
    {
        private DbContextOptions<AlloyContext> _options;

        public AlloyContext(DbContextOptions<AlloyContext> options) : base(options) {
            _options = options;
        }
        
        public DbSet<EventTemplateEntity> EventTemplates { get; set; }
        public DbSet<EventEntity> Events { get; set; }

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

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default(CancellationToken))
        {
            var addedEntries = ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
            var modifiedEntries = ChangeTracker.Entries().Where(x => x.State == EntityState.Modified);
            foreach (var entry in addedEntries)
            {
                ((BaseEntity)entry.Entity).DateCreated = DateTime.UtcNow;
                ((BaseEntity)entry.Entity).DateModified = null;
                ((BaseEntity)entry.Entity).ModifiedBy = null;
            }
            foreach (var entry in modifiedEntries)
            {
                ((BaseEntity)entry.Entity).DateModified = DateTime.UtcNow;
                ((BaseEntity)entry.Entity).CreatedBy = (Guid)entry.OriginalValues["CreatedBy"];
                ((BaseEntity)entry.Entity).DateCreated = (DateTime)entry.OriginalValues["DateCreated"];
            }
            return await base.SaveChangesAsync(ct);
        }
    }
}


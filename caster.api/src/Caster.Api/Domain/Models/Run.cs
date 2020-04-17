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
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caster.Api.Domain.Models
{
    public class Run
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public virtual Plan Plan { get; set; }
        public virtual Apply Apply { get; set; }

        public Guid WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDestroy { get; set; }
        public RunStatus Status { get; set; } = RunStatus.Queued;

        public string[] Targets { get; set; }
    }

    public enum RunStatus
    {
        Queued = 0,
        Failed = 1,
        Rejected = 2,
        Planning = 3,
        Planned = 4,
        Applying = 5,
        Applied = 6
    }

    public class RunConfiguration : IEntityTypeConfiguration<Run>
    {
        public void Configure(EntityTypeBuilder<Run> builder)
        {
            builder
                .Property<string[]>(r => r.Targets)
                .HasConversion(
                    list => String.Join('\n', list),
                    str => str.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                );

            builder
                .HasOne(r => r.Plan)
                .WithOne(p => p.Run)
                .HasForeignKey<Plan>(p => p.RunId);

            builder
                .HasOne(r => r.Apply)
                .WithOne(a => a.Run)
                .HasForeignKey<Apply>(a => a.RunId);

            builder.HasIndex(r => r.CreatedAt);
        }
    }
}


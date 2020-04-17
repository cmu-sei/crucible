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

namespace S3.VM.Api.Data.Models
{
    public class VmTeamEntity
    {
        public VmTeamEntity() { }

        public VmTeamEntity(Guid teamId, Guid vmId)
        {
            TeamId = teamId;
            VmId = vmId;
        }

        public Guid TeamId { get; set; }
        public virtual TeamEntity Team { get; set; }

        public Guid VmId { get; set; }
        public virtual VmEntity Vm { get; set; }
    }

    public class VmTeamConfiguration : IEntityTypeConfiguration<VmTeamEntity>
    {
        public void Configure(EntityTypeBuilder<VmTeamEntity> builder)
        {
            builder.HasKey(e => new { e.TeamId, e.VmId });

            builder
                .HasOne(tu => tu.Team)
                .WithMany(t => t.VmTeams)
                .HasForeignKey(tu => tu.TeamId);

            builder
                .HasOne(tu => tu.Vm)
                .WithMany(u => u.VmTeams)
                .HasForeignKey(tu => tu.VmId);
        }
    }
}


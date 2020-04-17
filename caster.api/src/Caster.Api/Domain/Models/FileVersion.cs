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

namespace Caster.Api.Domain.Models
{
    public class FileVersion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid FileId { get; set; }
        public virtual File File { get; set; }

        public string Name { get; set; }

        public Guid? ModifiedById { get; set; }
        public virtual User ModifiedBy { get; set; }

        public string Content { get; set;}
        public DateTime? DateSaved { get; set; }

        public string Tag { get; set; }
        public Guid? TaggedById { get; set; }
        public virtual User TaggedBy { get; set; }

        public DateTime? DateTagged { get; set; }

        public FileVersion() {}

        public FileVersion(File file)
        {
            this.FileId = file.Id;
            this.Name = file.Name;
            this.Content = file.Content;
            this.ModifiedById = file.ModifiedById;
            this.DateSaved = file.DateSaved;
        }
    }

    public class FileVersionConfiguration : IEntityTypeConfiguration<FileVersion>
    {
        public void Configure(EntityTypeBuilder<FileVersion> builder)
        {
            builder
                .HasOne(f => f.File)
                .WithMany(fh => fh.FileVersions)
                .IsRequired();
        }
    }
}


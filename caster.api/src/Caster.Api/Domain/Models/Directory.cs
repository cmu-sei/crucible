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
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caster.Api.Domain.Models
{
    public class Directory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public Guid? ParentId { get; set; }
        public virtual Directory Parent { get; set; }

        public string Path { get; set; }

        public string TerraformVersion { get; set; }

        public virtual ICollection<File> Files { get; set; } = new HashSet<File>();
        public virtual ICollection<Workspace> Workspaces { get; set; } = new HashSet<Workspace>();
        public virtual ICollection<Directory> Children { get; set; } = new HashSet<Directory>(); // Only immediate children

        public Directory() { }

        public Directory(string name, Directory parent = null, Guid? id = null)
        {
            if (id.HasValue)
            {
                this.Id = id.Value;
                this.Name = name;
            }
            else
            {
                this.Id = Guid.NewGuid();
                this.SetImportName(name);
            }

            if (parent != null)
            {
                this.SetPath(parent.Path);
                this.Parent = parent;
                this.ParentId = parent.Id;
                this.ProjectId = parent.ProjectId;
            }
            else
            {
                this.SetPath();
            }
        }

        public string GetPathNames()
        {
            if (this.Parent != null)
            {
                return $"{this.Parent.GetPathNames()}{this.Name}/";
            }
            else
            {
                return $"{this.Name}/";
            }
        }

        public void SetPath(string parentPath = null)
        {
            if (parentPath == null)
            {
                Path = $"{this.Id}/";
            }
            else
            {
                Path = $"{parentPath}{this.Id}/";
            }
        }

        public Guid[] PathIds()
        {
            return Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(x => new Guid(x)).ToArray();
        }

        public string GetExportName(bool includeId)
        {
            if (!includeId)
            {
                return this.Name;
            }
            else
            {
                return $"{this.Name}__{this.Id}";
            }
        }

        public void SetImportName(string name)
        {
            var endIndex = name.LastIndexOf("__");
            var delimiterLength = "__".Length;

            if (endIndex != -1 &&
                name.Length > endIndex + delimiterLength &&
                Guid.TryParse(name.Substring(endIndex + delimiterLength), out Guid guid))
            {
                this.Id = guid;
                this.Name = name.Substring(0, endIndex);
            }
            else
            {
                this.Name = name;
            }
        }
    }

    public class DirectoryConfiguration : IEntityTypeConfiguration<Directory>
    {
        public void Configure(EntityTypeBuilder<Directory> builder)
        {
            builder.HasIndex(d => d.Path);
            builder.HasIndex(d => d.ProjectId);
            builder.HasIndex(d => d.ParentId);

            builder
                .HasOne(d => d.Parent)
                .WithMany(d => d.Children)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}

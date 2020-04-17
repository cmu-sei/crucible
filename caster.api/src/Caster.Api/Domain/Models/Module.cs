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
using System.Text.Json;
using Caster.Api.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caster.Api.Domain.Models
{
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ModuleVersion> Versions { get; set; } = new List<ModuleVersion>();
        public DateTime? DateModified { get; set; }
    }

    public class ModuleVersion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public virtual Module Module { get; set; }
        public string Name { get; set; }
        public string UrlLink { get; set; }
        public List<ModuleVariable> Variables { get; set; } = new List<ModuleVariable>();
        public List<string> Outputs { get; set; } = new List<string>();

    }

    public class ModuleVariable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string VariableType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsOptional
        {
            get
            {
                return this.DefaultValue != null;
            }
        }
        public bool RequiresQuotes
        {
            get
            {
                if (this.VariableType == null)
                {
                    return false;
                }

                return this.VariableType.ToLower() == "string";
            }
        }
    }

    public class VersionConfiguration : IEntityTypeConfiguration<ModuleVersion>
    {
        public void Configure(EntityTypeBuilder<ModuleVersion> builder)
        {
            builder
                .Property(p => p.Variables)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, DefaultJsonSettings.Settings),
                    v => JsonSerializer.Deserialize<List<ModuleVariable>>(v, DefaultJsonSettings.Settings));

            builder
                .Property(p => p.Outputs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, DefaultJsonSettings.Settings),
                    v => JsonSerializer.Deserialize<List<string>>(v, DefaultJsonSettings.Settings));

        }

    }

}

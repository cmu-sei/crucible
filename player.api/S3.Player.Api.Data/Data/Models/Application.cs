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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace S3.Player.Api.Data.Data.Models
{
    public class ApplicationTemplateEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Icon { get; set; }

        public bool Embeddable { get; set; }
        public bool LoadInBackground { get; set; }

        public ApplicationTemplateEntity()
        {

        }
    }

    public class ApplicationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Icon { get; set; }

        public bool? Embeddable { get; set; }
        public bool? LoadInBackground { get; set; }

        public Guid ViewId { get; set; }
        public virtual ViewEntity View { get; set; }

        [ForeignKey(nameof(Template))]
        public Guid? ApplicationTemplateId { get; set; }
        public virtual ApplicationTemplateEntity Template { get; set; }

        public ApplicationEntity()
        {

        }

        public ApplicationEntity Clone()
        {
            var entity = this.MemberwiseClone() as ApplicationEntity;
            entity.Id = Guid.Empty;
            entity.ViewId = Guid.Empty;
            entity.View = null;

            return entity;
        }

        public string GetName()
        {
            string name = null;

            if (this.Name != null)
            {
                name = this.Name;
            }
            else if (this.Template != null)
            {
                name = this.Template.Name;
            }

            return name;
        }
    }

    public class ApplicationInstanceEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public virtual TeamEntity Team { get; set; }

        public Guid ApplicationId { get; set; }
        public virtual ApplicationEntity Application { get; set; }

        public float DisplayOrder { get; set; }

        public ApplicationInstanceEntity() { }

        public ApplicationInstanceEntity Clone()
        {
            var entity = this.MemberwiseClone() as ApplicationInstanceEntity;
            entity.Id = Guid.Empty;
            entity.TeamId = Guid.Empty;
            entity.Team = null;
            entity.Application = null;
            entity.ApplicationId = Guid.Empty;

            return entity;
        }
    }
}

/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.Player.Api.Data.Data.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace S3.Player.Api.ViewModels
{
    public class ApplicationTemplate
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        [Url]
        public string Url { get; set; }

        public string Icon { get; set; }

        public bool Embeddable { get; set; }
        public bool LoadInBackground { get; set; }
    }

    public class ApplicationTemplateForm
    {
        public string Name { get; set; }

        [Url]
        public string Url { get; set; }

        public string Icon { get; set; }

        public bool Embeddable { get; set; }
        public bool LoadInBackground { get; set; }
    }

    public class Application
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        [Url]
        public string Url { get; set; }

        public string Icon { get; set; }

        public bool? Embeddable { get; set; }
        public bool? LoadInBackground { get; set; }

        [Required]
        public Guid ViewId { get; set; }

        public Guid? ApplicationTemplateId { get; set; }
    }

    public class ApplicationInstance
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public float DisplayOrder { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The location of the application. {teamId}, {teamName}, {viewId} and {viewName} will be replaced dynamically if included
        /// </summary>
        public string Url { get; set; }

        public string Icon { get; set; }

        public bool Embeddable { get; set; }
        public bool LoadInBackground { get; set; }

        public Guid ViewId { get; set; }
    }

    public class ApplicationInstanceForm
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public Guid ApplicationId { get; set; }

        public float DisplayOrder { get; set; }
    }
}

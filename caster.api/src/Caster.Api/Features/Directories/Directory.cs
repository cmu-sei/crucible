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
using Caster.Api.Features.Files;
using Caster.Api.Features.Workspaces;

namespace Caster.Api.Features.Directories
{
    public class Directory
    {
        /// <summary>
        /// Id of the directory.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the project this directory is under.
        /// </summary>

        public Guid ProjectId { get; set; }
        /// <summary>
        /// Optional Id of the directory this directory is under
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// List of files in the directory. Null if not requested
        /// </summary>
        public List<File> Files { get; set; }

        /// <summary>
        /// List of workspaces in the directory. Null if not requested
        /// </summary>
        public List<Workspace> Workspaces { get; set; }
    }
}

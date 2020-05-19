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

namespace Caster.Api.Features.Modules
{
    // modules must be uniquely identified in terraform cloud/enterprise
    // <ORGANIZATION>/<MODULE NAME>/<PROVIDER>
    // this is stored in Path

    public class ModuleSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
    }

    public class Module
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public List<ModuleVersion> Versions { get; set; } = new List<ModuleVersion>();
        public DateTime? DateModified { get; set; }
    }

    public class ModuleVersion
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Name { get; set; }
        public string UrlLink { get; set; }
        public DateTime DateCreated { get; set; }
        public List<ModuleVariable> Variables { get; set; } = new List<ModuleVariable>();
        public List<string> Outputs { get; set; } = new List<string>();

    }

    public class ModuleVariable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string VariableType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsOptional { get; set; }
    }
}

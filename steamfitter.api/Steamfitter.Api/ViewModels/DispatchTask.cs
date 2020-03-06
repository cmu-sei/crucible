/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Steamfitter.Api.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Steamfitter.Api.ViewModels
{
    public class DispatchTask : Base
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? ScenarioId { get; set; }
        public Guid? SessionId { get; set; }
        public Guid? UserId { get; set; }
        public TaskAction Action { get; set; }
        public string VmMask { get; set; }
        public List<Guid> VmList { get; set; }
        public string ApiUrl { get; set; }
        public string InputString { get; set; }
        public string ExpectedOutput { get; set; }
        public int ExpirationSeconds { get; set; }
        public int DelaySeconds { get; set; }
        public int IntervalSeconds { get; set; }
        public int Iterations { get; set; }
        public Guid? TriggerTaskId { get; set; }
        public TaskTrigger TriggerCondition { get; set; }
    }
}

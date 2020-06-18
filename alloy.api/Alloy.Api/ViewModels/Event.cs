/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Alloy.Api.Data;
using Alloy.Api.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alloy.Api.ViewModels
{
    public class Event : Base
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public Guid? EventTemplateId { get; set; }
        public Guid? ViewId { get; set; }
        public Guid? WorkspaceId { get; set; }
        public Guid? RunId { get; set; }
        public Guid? ScenarioId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EventStatus Status { get; set; }
        public InternalEventStatus InternalStatus { get; set; }
        public int FailureCount { get; set; }
        public EventStatus LastLaunchStatus { get; set; }
        public InternalEventStatus LastLaunchInternalStatus { get; set; }
        public EventStatus LastEndStatus { get; set; }
        public InternalEventStatus LastEndInternalStatus { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime? LaunchDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}

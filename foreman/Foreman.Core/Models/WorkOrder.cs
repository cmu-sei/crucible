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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Foreman.Core.Models
{
    [Table("workorders")]
    public class WorkOrder
    {
        [Key]
        public Guid Id { get; set; }
        
        public string GroupName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType Status { get; set; }

        public IList<WorkOrderTrigger> Triggers { get; set; }

        public IList<WorkOrderResult> Results { get; private set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public JobType Job { get; set; }

        public Guid WebhookId { get; set; }

        public IList<WorkOrderParameter> Params { get; set; }
        
        public int? JobKey { get; set; }
        
        public DateTime CreatedUtc { get; set; }

        public WorkOrder()
        {
            this.CreatedUtc = DateTime.UtcNow;
            this.Triggers = new List<WorkOrderTrigger>();
            this.Results = new List<WorkOrderResult>();
            this.Params = new List<WorkOrderParameter>();
        }

        [Table("workorderparameters")]
        public class WorkOrderParameter
        {
            [Key] 
            public int Id { get; set; }

            public string Name { get; set; }
            public string Value { get; set; }

            public WorkOrderParameter() { }

            public WorkOrderParameter(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        [Table("workordertriggers")]
        public class WorkOrderTrigger
        {
            [Key]
            public int Id { get; set; }
            
            public string GroupName { get; set; }
            public int Interval { get; set; }
        }

        [Table("workorderresults")]
        public class WorkOrderResult
        {
            [Key]
            public int Id { get; set; }
            
            public DateTime CreatedUtc { get; set; }
            public string Payload { get; set; }

            public WorkOrderResult()
            {
            }

            public WorkOrderResult(string payload)
            {
                this.Payload = payload;
                this.CreatedUtc = DateTime.UtcNow;
            }
        }
    }
}

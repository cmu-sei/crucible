/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

namespace Stackstorm.Api.Client.Models
{
    public class Execution
    {
        public string status { get; set; }
        public string start_timestamp { get; set; }
        public TriggerType trigger_type { get; set; }
        public Runner runner { get; set; }
        public Rule rule { get; set; }
        public Trigger trigger { get; set; }
        public ExecutionContext context { get; set; }
        public Action action { get; set; }
        public string id { get; set; }
        public string end_timestamp { get; set; }
        
        public string elapsed_seconds{ get; set; }
        
        public object result { get; set; }
        
        public Execution()
        {
            this.result = new object();
        }

        public bool IsComplete()
        {
            return this.status.ToUpper() != "REQUESTED" && this.status.ToUpper() != "SCHEDULED" && this.status.ToUpper() != "RUNNING";
        }
        
    }


}

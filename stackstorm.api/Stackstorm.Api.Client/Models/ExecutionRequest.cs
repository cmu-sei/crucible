/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Generic;

namespace Stackstorm.Api.Client.Models
{
    public class ExecutionRequest
    {
        public Dictionary<string, string> parameters { get; private set; }
 
        public string action { get; private set; }

        public ExecutionRequest()
        {
            this.parameters = new Dictionary<string, string>();
        }

        public ExecutionRequest(string action, Dictionary<string, string> parameterDictionary)
        {
            this.parameters = parameterDictionary ?? new Dictionary<string, string>();
            this.action = action;
        }

        public void AddParameter(string key, string value)
        {
            if (this.parameters.ContainsKey(key))
            {
                this.parameters[key] = $"{this.parameters[key]}, {value}";
            }
            else
            {
                this.parameters.Add(key, value);
            }
        }

        public void AddParameter(string key, IEnumerable<string> values)
        {
            if (values == null) return;
            
            foreach (var val in values)
            {
                AddParameter(key, val);
            }
        }
    }
}

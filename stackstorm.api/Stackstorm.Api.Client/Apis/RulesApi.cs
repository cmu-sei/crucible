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
using System.Threading.Tasks;
using Stackstorm.Api.Client.Models;

namespace Stackstorm.Api.Client.Apis
{
    /// <summary> The rules api. </summary>
    /// <seealso cref="T:Stackstorm.Api.Client.Apis.IRulesApi"/>
    public class RulesApi :
        IRulesApi
    {
        /// <summary> The host process. </summary>
        private ISt2Client _host;

        /// <summary>
        ///  Initializes a new instance of the Stackstorm.Api.Client.Apis.RulesApi class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="host"> The host. </param>
        public RulesApi(ISt2Client host)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            _host = host;
        }

        /// <summary> Gets rules . </summary>
        /// <returns> The rules . </returns>
        public async Task<IList<Rule>> GetRulesAsync()
        {
            return await _host.GetApiRequestAsync<IList<Rule>>("/v1/rules/");
        }

        /// <summary> Gets rules for pack . </summary>
        /// <param name="packName"> Name of the pack. </param>
        /// <returns> The rules for pack . </returns>
        public async Task<IList<Rule>> GetRulesForPackAsync(string packName)
        {
            return await _host.GetApiRequestAsync<IList<Rule>>("/v1/rules?pack=" + packName);
        }

        /// <summary> Gets rules by name . </summary>
        /// <param name="name"> The rule name. </param>
        /// <returns> The rules by name . </returns>
        public async Task<IList<Rule>> GetRulesByNameAsync(string name)
        {
            return await _host.GetApiRequestAsync<IList<Rule>>("/v1/rules?name=" + name);
        }

        /// <summary> Deletes the rule described by ruleId. </summary>
        /// <param name="ruleId"> Identifier for the rule. </param>
        /// <returns> A Task. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IRulesApi.DeleteRule(string)"/>
        public async Task DeleteRuleAsync(string ruleId)
        {
            await _host.DeleteApiRequestAsync("/v1/rules/" + ruleId);
        }
    }
}

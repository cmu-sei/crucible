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
using System.Threading.Tasks;
using Stackstorm.Api.Client.Models;

namespace Stackstorm.Api.Client.Apis
{
    /// <summary> Interface for rules API. </summary>
    public interface IRulesApi
    {
        /// <summary> Gets rules. </summary>
        /// <returns> The rules. </returns>
        Task<IList<Rule>> GetRulesAsync();

        /// <summary> Gets rules for pack. </summary>
        /// <param name="packName"> Name of the pack. </param>
        /// <returns> The rules for pack. </returns>
        Task<IList<Rule>> GetRulesForPackAsync(string packName);

        /// <summary> Gets rules by name. </summary>
        /// <param name="name"> The name. </param>
        /// <returns> The rules by name. </returns>
        Task<IList<Rule>> GetRulesByNameAsync(string name);

        /// <summary> Deletes the rule described by ruleId. </summary>
        /// <param name="ruleId"> Identifier for the rule. </param>
        /// <returns> A Task. </returns>
        Task DeleteRuleAsync(string ruleId);
    }
}

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
    /// <summary> Interface for executions  API. </summary>
    public interface IExecutionsApi
    {
        /// <summary> Gets execution. </summary>
        /// <param name="id"> The identifier. </param>
        /// <returns> The execution. </returns>
        Task<Execution> GetExecutionAsync(string id);

        /// <summary> Gets a list of executions. </summary>
        /// <param name="limit"> The number of items to return (default 5). </param>
        /// <returns> A list of <see cref="Execution"/>. </returns>
        Task<IList<Execution>> GetExecutionsAsync(int limit = 5);

        /// <summary> Gets executions for action. </summary>
        /// <param name="actionName"> Name of the action. </param>
        /// <param name="limit">   The number of items to return (default 5). </param>
        /// <returns> A list of <see cref="Execution"/>. </returns>
        Task<IList<Execution>> GetExecutionsForActionAsync(string actionName, int limit = 5);

        /// <summary> Executes the action. </summary>
        /// <param name="actionName"> Name of the action. </param>
        /// <param name="parameters"> The parameters for the given action. </param>
        /// <returns> The resulting execution; </returns>
        Task<Execution> ExecuteActionAsync(string actionName, Dictionary<string, string> parameters);

        /// <summary> Executes the action. </summary>
        /// <param name="actionName"> Name of the action. </param>
        /// <param name="parameters"> The parameters for the given action. </param>
        /// <returns> The resulting execution; </returns>
        Task<Execution> ExecuteActionAsync(string actionName, Dictionary<string, object> parameters);
    }
}

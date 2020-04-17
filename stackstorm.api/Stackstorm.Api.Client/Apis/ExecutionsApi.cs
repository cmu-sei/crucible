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
    /// <summary>The executions API. </summary>
    /// <seealso cref="T:Stackstorm.Api.Client.Apis.IExecutionsApi"/>
    public class ExecutionsApi : IExecutionsApi
    {
        private ISt2Client _host;

        /// <summary>
        /// Initializes a new instance of the Stackstorm.Api.Client.Apis.ExecutionsApi class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="host">The host. </param>
        public ExecutionsApi(ISt2Client host)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            _host = host;
        }

        /// <summary>Gets execution. </summary>
        /// <param name="id">The identifier. </param>
        /// <returns>The execution. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IExecutionsApi.GetExecutionAsync(string)"/>
        public async Task<Execution> GetExecutionAsync(string id)
        {
            return await _host.GetApiRequestAsync<Execution>("/v1/executions/" + id);
        }

        /// <summary>Gets a list of executions. </summary>
        /// <param name="limit">The number of items to return (default 5). </param>
        /// <returns>A list of <see cref="Execution"/>. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IExecutionsApi.GetExecutionsAsync(int)"/>
        public async Task<IList<Execution>> GetExecutionsAsync(int limit = 5)
        {
            return await _host.GetApiRequestAsync<IList<Execution>>("/v1/executions?limit=" + limit);
        }

        /// <summary>Gets executions for action. </summary>
        /// <param name="actionName">Name of the action. </param>
        /// <param name="limit"> The number of items to return (default 5). </param>
        /// <returns>A list of <see cref="Execution"/>. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IExecutionsApi.GetExecutionsForActionAsync(string,int)"/>
        public async Task<IList<Execution>> GetExecutionsForActionAsync(string actionName, int limit = 5)
        {
            return await _host.GetApiRequestAsync<IList<Execution>>("/v1/executions?action=" + actionName + "&limit=" + limit);
        }

        /// <summary>Executes the action. </summary>
        /// <param name="actionName">Name of the action. </param>
        /// <param name="parameters">The parameters for the given action. </param>
        /// <returns>The resulting execution; </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IExecutionsApi.ExecuteActionAsync(string,Dictionary{string,string})"/>
        public async Task<Execution> ExecuteActionAsync(string actionName, Dictionary<string, string> parameters)
        {
            ExecuteActionRequest request = new ExecuteActionRequest
            {
                action = actionName,
                parameters = parameters
            };
            return await _host.PostApiRequestAsync<Execution, ExecuteActionRequest>("/v1/executions/", request);
        }

        /// <summary>Executes the action. </summary>
        /// <param name="actionName">Name of the action. </param>
        /// <param name="parameters">The parameters for the given action. </param>
        /// <returns>The resulting execution; </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IExecutionsApi.ExecuteActionAsync(string,Dictionary{string,object})"/>
        public async Task<Execution> ExecuteActionAsync(string actionName, Dictionary<string, object> parameters)
        {
            ExecuteComplexActionRequest request = new ExecuteComplexActionRequest
            {
                action = actionName,
                parameters = parameters
            };
            return await _host.PostApiRequestAsync<Execution, ExecuteComplexActionRequest>("/v1/executions/", request);
        }
    }
}

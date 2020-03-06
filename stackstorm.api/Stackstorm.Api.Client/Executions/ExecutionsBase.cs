/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Stackstorm.Api.Client;
using Stackstorm.Api.Client.Models;

namespace stackstorm.api.client.Executions
{
    public class ExecutionsBase
    {
        private ISt2Client _host;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        
        protected ExecutionsBase(ISt2Client host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }
        
        internal async Task<Execution> AddExecution(string action, Dictionary<string, string> parameters)
        {
            if (!_host.HasToken())
            {
                await _host.RefreshTokenAsync();
            }
            try
            {
                var tempParameters = new Dictionary<string, string>();
                //delete any empty parameters
                if (parameters != null)
                {
                    foreach (var p in parameters)
                        if (!string.IsNullOrEmpty(p.Value))
                            tempParameters.Add(p.Key, p.Value);
                }

                var executionRequest = new ExecutionRequest(action, tempParameters);
                
                //var requestString = Newtonsoft.Json.JsonConvert.SerializeObject(executionRequest);
                //Console.WriteLine(requestString);

                var r = await _host.PostApiRequestAsync<Execution, ExecutionRequest>("v1/executions", executionRequest);
                return await Resolve(r);
            }
            catch (Exception e)
            {
                _log.Error(e);
                return null;
            }
        }

        private async Task<Execution> Resolve(Execution executionResult)
        {
            if (executionResult.id == null)
                throw new Exception();

            while (executionResult.IsComplete() == false)
            {
                executionResult = await _host.Executions.GetExecutionAsync(executionResult.id);
                _log.Trace($"Execution status is {executionResult.status}");
                Thread.Sleep(2000);
            }

            return executionResult;
        }
    }
}

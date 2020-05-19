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
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Foreman.Core.Infrastructure.Options;
using Foreman.Core.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Quartz;
using IdentityModel.Client;

namespace Foreman.Core.Jobs
{
    public class WebHookJob : BaseJob, IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var woid = context.JobDetail.Key.Name;
                Console.WriteLine($"{DateTime.UtcNow} Processing WebHookJob {woid}");

                var dataMap = context.JobDetail.JobDataMap;

                var url = dataMap.GetString("url");
                var method = dataMap.GetString("method");
                var mustAuthenticate = dataMap.GetBoolean("mustAuthenticate");
                var payload = dataMap.GetString("payload");
                var tempParameters = dataMap.GetString("parameters");
                var parameters = new List<WorkOrder.WorkOrderParameter>();
                if (!string.IsNullOrEmpty(tempParameters) && tempParameters != "[]")
                {
                    parameters = JsonConvert.DeserializeObject<List<WorkOrder.WorkOrderParameter>>(dataMap.GetString("parameters"));
                }

                // replacers  
                payload = payload.Replace("[datetime.now]", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                foreach (var parameter in parameters)
                {
                    url = url.Replace(parameter.Name, parameter.Value);
                    payload = payload.Replace(parameter.Name, parameter.Value);
                }
                
                try
                {
                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                    var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        // get authentication token from the IdentityServer, if required
                        if (mustAuthenticate)
                        {
                            var token = await RequestTokenAsync();
                            httpClient.DefaultRequestHeaders.Add("authorization", new List<string>{token});
                        }

                        // Do the actual request and await the response
                        HttpResponseMessage httpResponse;
                        switch (method)
                        {
                            case "GET":
                            {
                                httpResponse = await httpClient.GetAsync(url);
                                break;
                            }
                            case "POST":
                            {
                                httpResponse = await httpClient.PostAsync(url, httpContent);
                                break;
                            }
                            default:
                            {
                                throw new NotImplementedException($"HTTP method {method} is not currently implemented.");
                            }
                        }

                        Console.WriteLine($"Webhook response {url} {method} {httpResponse.StatusCode}");

                        this.Status = $"Code {(int)httpResponse.StatusCode} {httpResponse.StatusCode}";
                        // If the response contains content we want to read it!
                        if (httpResponse.Content != null)
                        {
                            var responseContent = await httpResponse.Content.ReadAsStringAsync();
                            this.Result = responseContent; 
                            Console.WriteLine($"Webhook notification sent with {responseContent}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Webhook failed response {url} {method} - {e}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception! {e}");
            }
            
            await base.Execute(context);
        }

        private async Task<string> RequestTokenAsync()
        {
            // get Configuration, since we can't use DI with Quartz
            var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"{path}/appSettings.json", true)
                .AddJsonFile($"{path}/appsettings.Development.json", true)
                .AddJsonFile($"{path}/appsettings.Production.json", true)
                .Build();
            var authConfig = new ResourceOwnerAuthorizationOptions();
            configuration.GetSection("ResourceOwnerAuthorization").Bind(authConfig);
            
            // get the bearer token from IdentityServer
            var disco = await DiscoveryClient.GetAsync(authConfig.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var client = new TokenClient(disco.TokenEndpoint, authConfig.ClientId, authConfig.ClientSecret);
            var token = await client.RequestResourceOwnerPasswordAsync(
                        authConfig.UserName, 
                        authConfig.Password, 
                        authConfig.Scope);
            return $"Bearer {token.AccessToken}";
        }

    }

}

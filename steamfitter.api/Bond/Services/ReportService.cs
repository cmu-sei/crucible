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
using System.Threading;
using Bond.Infrastructure.Builders;
using Bond.Infrastructure.Models;
using NLog;
using RestSharp;
using Steamfitter.Api.Data.Models;

namespace Bond
{
    /// <summary>
    /// Manages sending client information home to central api
    /// </summary>
    internal static class ReportService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        internal static void Run(ClientConfiguration.ReporterOptions config)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                RunEx(config);
            }).Start();
        }

        private static void RunEx(ClientConfiguration.ReporterOptions config)
        {
            // give ssh some time to catch up
            Thread.Sleep((config.IntervalInSeconds * 1000));

            if (!config.IsEnabled)
                return;

            while (true)
            {
                try
                {
                    // build payload
                    if (BondManager.CurrentPorts.Count > 0)
                    {
                        var payload = MachineSurveyBuilder.Build();
                        DoPost(config, payload);
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }

                Thread.Sleep((config.IntervalInSeconds * 1000));
            }
        }

        private static void DoPost(ClientConfiguration.ReporterOptions config, ExerciseAgent exerciseAgent)
        {
            //call home
            var client = new RestClient(config.PostUrl);
            var request = new RestRequest(Method.POST) {RequestFormat = DataFormat.Json};

            request.AddJsonBody(exerciseAgent);

            var response = client.Execute(request);
            _log.Info($"Post response {response.StatusCode}: {response.Content}");

            // is there a need for a return here?
            // return JsonConvert.DeserializeObject<Catalog>(dataString);
        }
    }
}

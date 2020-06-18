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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Steamfitter.Api;
using Steamfitter.Api.Models;
using Alloy.Api.Data.Models;

namespace Alloy.Api.Infrastructure.Extensions
{
    public static class SteamfitterApiExtensions
    {
        public static SteamfitterApiClient GetSteamfitterApiClient(IHttpClientFactory httpClientFactory, string apiUrl, TokenResponse tokenResponse)
        {
            var client = ApiClientsExtensions.GetHttpClient(httpClientFactory, apiUrl, tokenResponse);
            var apiClient = new SteamfitterApiClient(client, true);
            apiClient.BaseUri = client.BaseAddress;
            return apiClient;
        }

        public static async Task<Scenario> CreateSteamfitterScenarioAsync(SteamfitterApiClient steamfitterApiClient, EventEntity eventEntity, Guid scenarioTemplateId, CancellationToken ct)
        {
            try
            {
                var scenario = await steamfitterApiClient.CreateScenarioFromScenarioTemplateAsync(scenarioTemplateId, ct);
                scenario.Name = $"{scenario.Name.Replace("From ScenarioTemplate ", "")} - {eventEntity.Username}";
                scenario.ViewId = eventEntity.ViewId;
                scenario = await steamfitterApiClient.UpdateScenarioAsync((Guid)scenario.Id, scenario, ct);
                return scenario;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<bool> StartSteamfitterScenarioAsync(SteamfitterApiClient steamfitterApiClient, Guid scenarioId, CancellationToken ct)
        {
            try
            {
                await steamfitterApiClient.StartScenarioAsync(scenarioId, ct);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> EndSteamfitterScenarioAsync(string steamfitterApiUrl, Guid? scenarioId, SteamfitterApiClient steamfitterApiClient, CancellationToken ct)
        {
            // no scenario to end
            if (scenarioId == null)
            {
                return true;
            }
            try
            {
                await steamfitterApiClient.EndScenarioAsync((Guid)scenarioId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}



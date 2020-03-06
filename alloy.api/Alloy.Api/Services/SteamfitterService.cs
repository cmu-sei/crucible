/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Steamfitter.Api;
using Steamfitter.Api.Models;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alloy.Api.Services
{
    public interface ISteamfitterService
    {
        Task<IEnumerable<Scenario>> GetScenariosAsync(CancellationToken ct);
        // Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken ct);
        // Task<IEnumerable<Session>> GetSessionsAsync(CancellationToken ct);
        // Task<Session> GetSessionAsync(Guid sessionId, CancellationToken ct);
        // Task<Session> CreateSessionFromScenarioAsync(Guid scenarioId, CancellationToken ct);
        // Task<Session> StartSessionAsync(Guid scenarioId, CancellationToken ct);
        // Task<Session> PauseSessionAsync(Guid scenarioId, CancellationToken ct);
        // Task<Session> ContinueSessionAsync(Guid scenarioId, CancellationToken ct);
        // Task<Session> EndSessionAsync(Guid scenarioId, CancellationToken ct);
        // Task<bool> DeleteSessionAsync(Guid scenarioId, CancellationToken ct);
    }

    public class SteamfitterService : ISteamfitterService
    {
        private readonly ISteamfitterApiClient _steamfitterApiClient;
        private readonly Guid _userId;

        public SteamfitterService(IHttpContextAccessor httpContextAccessor, ClientOptions clientSettings, ISteamfitterApiClient steamfitterApiClient)
        {
            _userId = httpContextAccessor.HttpContext.User.GetId();
            _steamfitterApiClient = steamfitterApiClient;
        }       

        public async Task<IEnumerable<Scenario>> GetScenariosAsync(CancellationToken ct)
        {
            var scenarios = await _steamfitterApiClient.GetScenariosAsync(ct);

            return scenarios;
        }

        // public async Task<Scenario> GetScenarioAsync(Guid scenarioId, CancellationToken ct)
        // {
        //     var scenario = await _steamfitterApiClient.GetScenarioAsync(scenarioId, ct);

        //     return scenario;
        // }

        // public async Task<IEnumerable<Session>> GetSessionsAsync(CancellationToken ct)
        // {
        //     var sessions = await _steamfitterApiClient.GetSessionsAsync(ct);

        //     return sessions;
        // }

        // public async Task<Session> GetSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     var session = await _steamfitterApiClient.GetSessionAsync(sessionId, ct);

        //     return session;
        // }

        // public async Task<Session> CreateSessionFromScenarioAsync(Guid scenarioId, CancellationToken ct)
        // {
        //     var newSession = await _steamfitterApiClient.CreateSessionFromScenarioAsync(scenarioId, ct);
        //     return newSession;
        // }

        // public async Task<Session> StartSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     return await _steamfitterApiClient.StartSessionAsync(sessionId, ct);
        // }

        // public async Task<Session> PauseSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     return await _steamfitterApiClient.PauseSessionAsync(sessionId, ct);
        // }

        // public async Task<Session> ContinueSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     return await _steamfitterApiClient.ContinueSessionAsync(sessionId, ct);
        // }

        // public async Task<Session> EndSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     return await _steamfitterApiClient.EndSessionAsync(sessionId, ct);
        // }

        // public async Task<bool> DeleteSessionAsync(Guid sessionId, CancellationToken ct)
        // {
        //     // TODO: figure out why the swagger client code retunrs void and not bool
        //     await _steamfitterApiClient.DeleteSessionAsync(sessionId, ct);
        //     return true;
        // }



    }
}


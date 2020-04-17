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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.Services;
using Alloy.Api.ViewModels;
using Alloy.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Steamfitter.Api.Models;

namespace Alloy.Api.Controllers
{
    public class SteamfitterController : BaseController
    {
        private readonly ISteamfitterService _steamfitterService;
        private readonly IAuthorizationService _authorizationService;

        public SteamfitterController(ISteamfitterService steamfitterService, IAuthorizationService authorizationService)
        {
            _steamfitterService = steamfitterService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Scenarios
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Scenarios.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("scenarios")]
        [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarios")]
        public async Task<IActionResult> GetScenarios(CancellationToken ct)
        {
            var list = await _steamfitterService.GetScenariosAsync(ct);
            return Ok(list);
        }

        // /// <summary>
        // /// Get a Scenario
        // /// </summary>
        // /// <remarks>
        // /// Returns the selected Scenario
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpGet("scenarios/{scenarioId}")]
        // [ProducesResponseType(typeof(Scenario), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getScenario")]
        // public async Task<IActionResult> GetScenario(string scenarioId, CancellationToken ct)
        // {
        //     var list = await _steamfitterService.GetScenarioAsync(Guid.Parse(scenarioId), ct);
        //     return Ok(list);
        // }

        // /// <summary>
        // /// Gets all sessions
        // /// </summary>
        // /// <remarks>
        // /// Returns a list of all of Sessions.
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpGet("sessions")]
        // [ProducesResponseType(typeof(IEnumerable<Session>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getSessions")]
        // public async Task<IActionResult> GetSessions(CancellationToken ct)
        // {
        //     var list = await _steamfitterService.GetSessionsAsync(ct);
        //     return Ok(list);
        // }

        // /// <summary>
        // /// Get a Session
        // /// </summary>
        // /// <remarks>
        // /// Returns the selected Session
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpGet("sessions/{sessionId}")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getSession")]
        // public async Task<IActionResult> GetSession(string sessionId, CancellationToken ct)
        // {
        //     var list = await _steamfitterService.GetSessionAsync(Guid.Parse(sessionId), ct);
        //     return Ok(list);
        // }

        // /// <summary>
        // /// Create a Session from a Scenario
        // /// </summary>
        // /// <remarks>
        // /// Returns the created Session
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpPost("scenarios/{scenarioId}/sessions")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "createSessionFromScenario")]
        // public async Task<IActionResult> CreateSessionFromScenario(string scenarioId, CancellationToken ct)
        // {
        //     var newSession = await _steamfitterService.CreateSessionFromScenarioAsync(Guid.Parse(scenarioId), ct);
        //     return Ok(newSession);
        // }

        // /// <summary>
        // /// Start a Session
        // /// </summary>
        // /// <remarks>
        // /// Returns the started Session
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpPost("sessions/{sessionId}/start")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "startSession")]
        // public async Task<IActionResult> StartSession(string sessionId, CancellationToken ct)
        // {
        //     var startedSession = await _steamfitterService.StartSessionAsync(Guid.Parse(sessionId), ct);
        //     return Ok(startedSession);
        // }

        // /// <summary>
        // /// Pause a Session
        // /// </summary>
        // /// <remarks>
        // /// Returns the paused Session
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpPost("sessions/{sessionId}/pause")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "pauseSession")]
        // public async Task<IActionResult> PauseSession(string sessionId, CancellationToken ct)
        // {
        //     var pausedSession = await _steamfitterService.PauseSessionAsync(Guid.Parse(sessionId), ct);
        //     return Ok(pausedSession);
        // }

        // /// <summary>
        // /// Continue a Session
        // /// </summary>
        // /// <remarks>
        // /// Returns the continued Session
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpPost("sessions/{sessionId}/continue")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "continueSession")]
        // public async Task<IActionResult> ContinueSession(string sessionId, CancellationToken ct)
        // {
        //     var continuedSession = await _steamfitterService.ContinueSessionAsync(Guid.Parse(sessionId), ct);
        //     return Ok(continuedSession);
        // }

        // /// <summary>
        // /// Delete a Session
        // /// </summary>
        // /// <remarks>
        // /// Returns boolean for success
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpPost("sessions/{sessionId}/delete")]
        // [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "deleteSession")]
        // public async Task<IActionResult> DeleteSession(string sessionId, CancellationToken ct)
        // {
        //     var retVal = await _steamfitterService.DeleteSessionAsync(Guid.Parse(sessionId), ct);
        //     return Ok(retVal);
        // }

    }

}


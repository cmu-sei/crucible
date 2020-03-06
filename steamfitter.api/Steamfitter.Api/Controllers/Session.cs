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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;
using Steamfitter.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    public class SessionController : BaseController
    {
        private readonly ISessionService _SessionService;
        private readonly IAuthorizationService _authorizationService;

        public SessionController(ISessionService SessionService, IAuthorizationService authorizationService)
        {
            _SessionService = SessionService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Session in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Sessions in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("Sessions")]
        [ProducesResponseType(typeof(IEnumerable<Session>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSessions")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _SessionService.GetAsync(ct);
            return Ok(list);
        }

        // /// <summary>
        // /// Gets all Sessions for a User
        // /// </summary>
        // /// <remarks>
        // /// Returns all Sessions where the specified User is a member of at least one of it's Teams
        // /// <para />
        // /// Accessible to a SuperUser or the specified User itself
        // /// </remarks>
        // /// <returns></returns>
        // [HttpGet("users/{id}/Sessions")]
        // [ProducesResponseType(typeof(IEnumerable<Session>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getUserSessions")]
        // public async Task<IActionResult> GetByUserId(int id, CancellationToken ct)
        // {
        //     var list = await _SessionService.GetByUserIdAsync(id, ct);
        //     return Ok(list);
        // }

        // /// <summary>
        // /// Gets all Sessions for the current User
        // /// </summary>
        // /// <remarks>
        // /// Returns all Sessions where the current User is a member of at least one of it's Teams
        // /// <para />
        // /// Accessible only to the current User.
        // /// <para/>
        // /// This is a convenience endpoint and simply returns a 302 redirect to the fully qualified users/{id}/Sessions endpoint 
        // /// </remarks>
        // [HttpGet("me/Sessions")]
        // [ProducesResponseType(typeof(IEnumerable<Session>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getMySessions")]
        // public async Task<IActionResult> GetMy(CancellationToken ct)
        // {
        //     return RedirectToAction(nameof(this.GetByUserId), new { id = User.GetId() });
        // }

        /// <summary>
        /// Gets a specific Session by id
        /// </summary>
        /// <remarks>
        /// Returns the Session with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Session
        /// </remarks>
        /// <param name="id">The id of the Session</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("Sessions/{id}")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSession")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var Session = await _SessionService.GetAsync(id, ct);

            if (Session == null)
                throw new EntityNotFoundException<Session>();

            return Ok(Session);
        }

        /// <summary>
        /// Creates a new Session
        /// </summary>
        /// <remarks>
        /// Creates a new Session with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="session">The data to create the Session with</param>
        /// <param name="ct"></param>
        [HttpPost("Sessions")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createSession")]
        public async Task<IActionResult> Create([FromBody] Session session, CancellationToken ct)
        {
            var createdSession = await _SessionService.CreateAsync(session, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdSession.Id }, createdSession);
        }

        /// <summary>
        /// Creates a new Session from a Scenario
        /// </summary>
        /// <remarks>
        /// Creates a new Session from the specified Scenario
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="id">The Scenario ID to create the Session with</param>
        /// <param name="ct"></param>
        [HttpPost("Scenarios/{id}/Sessions")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createSessionFromScenario")]
        public async Task<IActionResult> CreateFromScenario(Guid id, CancellationToken ct)
        {
            var createdSession = await _SessionService.CreateFromScenarioAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdSession.Id }, createdSession);
        }

        /// <summary>
        /// Creates a new Session from a Session
        /// </summary>
        /// <remarks>
        /// Creates a new Session from the specified Session
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="id">The Session ID to copy into a new Session</param>
        /// <param name="ct"></param>
        [HttpPost("Sessions/{id}/Copy")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "copySession")]
        public async Task<IActionResult> CopySession(Guid id, CancellationToken ct)
        {
            var createdSession = await _SessionService.CreateFromSessionAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdSession.Id }, createdSession);
        }

        /// <summary>
        /// Updates an Session
        /// </summary>
        /// <remarks>
        /// Updates an Session with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="session">The updated Session values</param>
        /// <param name="ct"></param>
        [HttpPut("Sessions/{id}")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateSession")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Session session, CancellationToken ct)
        {
            var updatedSession = await _SessionService.UpdateAsync(id, session, ct);
            return Ok(updatedSession);
        }

        /// <summary>
        /// Start a Session
        /// </summary>
        /// <remarks>
        /// Updates a Session to active and executes initial DispatchTasks
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>  
        /// <param name="id">The Id of the Session to update</param>
        /// <param name="ct"></param>
        [HttpPut("Sessions/{id}/start")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "startSession")]
        public async Task<IActionResult> Start([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedSession = await _SessionService.StartAsync(id, ct);
            return Ok(updatedSession);
        }

        /// <summary>
        /// Pause a Session
        /// </summary>
        /// <remarks>
        /// Updates a Session to paused
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>  
        /// <param name="id">The Id of the Session to update</param>
        /// <param name="ct"></param>
        [HttpPut("Sessions/{id}/pause")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "pauseSession")]
        public async Task<IActionResult> Pause([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedSession = await _SessionService.PauseAsync(id, ct);
            return Ok(updatedSession);
        }

        /// <summary>
        /// Continue a Session
        /// </summary>
        /// <remarks>
        /// Updates a Session to active
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>  
        /// <param name="id">The Id of the Session to update</param>
        /// <param name="ct"></param>
        [HttpPut("Sessions/{id}/continue")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "continueSession")]
        public async Task<IActionResult> Continue([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedSession = await _SessionService.ContinueAsync(id, ct);
            return Ok(updatedSession);
        }

        /// <summary>
        /// End a Session
        /// </summary>
        /// <remarks>
        /// Updates a Session to ended
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>  
        /// <param name="id">The Id of the Session to update</param>
        /// <param name="ct"></param>
        [HttpPut("Sessions/{id}/end")]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "endSession")]
        public async Task<IActionResult> End([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedSession = await _SessionService.EndAsync(id, ct);
            return Ok(updatedSession);
        }

        /// <summary>
        /// Deletes an Session
        /// </summary>
        /// <remarks>
        /// Deletes an Session with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Session
        /// </remarks>    
        /// <param name="id">The id of the Session to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("Sessions/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteSession")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _SessionService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


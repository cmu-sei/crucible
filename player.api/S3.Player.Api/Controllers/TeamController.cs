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
using S3.Player.Api.Extensions;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.Services;
using S3.Player.Api.ViewModels;
using Microsoft.AspNetCore.SignalR;
using S3.Player.Api.Hubs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace S3.Player.Api.Controllers
{
    public class TeamController : BaseController
    {
        private readonly ITeamService _teamService;
        private readonly IHubContext<TeamHub> _teamHub;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        public TeamController(ITeamService teamService, IHubContext<TeamHub> hub, INotificationService notificationService, IAuthorizationService authorizationService)
        {
            _teamService = teamService;
            _teamHub = hub;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Teams in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Teams in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("teams")]
        [ProducesResponseType(typeof(IEnumerable<Team>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTeams")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _teamService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Teams for an Exercise
        /// </summary>
        /// <remarks>
        /// Returns all Teams within a specific Exercise
        /// <para />
        /// Accessible to a SuperUser or a User on an Admin Team within that Exercise
        /// </remarks>
        /// <param name="id">The id of the Exercise</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("exercises/{id}/teams")]
        [ProducesResponseType(typeof(IEnumerable<Team>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseTeams")]
        public async Task<IActionResult> GetByExercise(Guid id, CancellationToken ct)
        {
            var list = await _teamService.GetByExerciseIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Teams for a User by Exercise
        /// </summary>
        /// <remarks>
        /// Returns all Teams within a specific Exercise that a User is a member of
        /// <para />
        /// Accessible to a SuperUser, a User on an Admin Team within that Exercise, or the specified User itself
        /// </remarks>
        /// <param name="userId">The id of the User</param>
        /// <param name="ct"></param>
        /// <param name="exerciseId">The id of the Exercise</param>
        /// <returns></returns>
        [HttpGet("users/{userId}/exercises/{exerciseId}/teams")]
        [ProducesResponseType(typeof(IEnumerable<Team>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserExerciseTeams")]
        public async Task<IActionResult> GetByExerciseForUser(Guid exerciseId, Guid userId, CancellationToken ct)
        {
            var list = await _teamService.GetByExerciseIdForUserAsync(exerciseId, userId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Teams for the current User by Exercise
        /// </summary>
        /// <remarks>
        /// Returns all Teams within a specific Exercise that the current User is a member of
        /// <para />
        /// Accessible only to the current User.
        /// <para/>
        /// This is a convenience endpoint and simply returns a 302 redirect to the fully qualified users/{userId}/exercises/{exerciseId}/teams endpoint 
        /// </remarks>        
        /// <param name="id">The id of the Exercise</param>
        [HttpGet("me/exercises/{id}/teams")]
        [SwaggerOperation(operationId: "getMyExerciseTeams")]
        public async Task<IActionResult> GetByExerciseForMe(Guid id)
        {            
            return RedirectToAction(nameof(this.GetByExerciseForUser), new { userId = User.GetId(), exerciseId = id });            
        }

        /// <summary>
        /// Gets a specific Team by id
        /// </summary>
        /// <remarks>
        /// Returns the Team with the id specified
        /// <para />
        /// Accessible to a SuperUser, a User on an Admin Team within the Team's Exercise, or a User that is a member of the specified Team
        /// </remarks>
        /// <param name="id">The id of the Team</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("teams/{id}")]
        [ProducesResponseType(typeof(Team), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTeam")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var team = await _teamService.GetAsync(id, ct);

            if (team == null)
                throw new EntityNotFoundException<Team>();

            return Ok(team);
        }

        /// <summary>
        /// Creates a new Team within an Exercise
        /// </summary>
        /// <remarks>
        /// Creates a new Team within an Exercise with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>     
        /// <param name="id">The id of the Exercise</param>
        /// <param name="form">The data to create the Team with</param>
        /// <param name="ct"></param>
        [HttpPost("exercises/{id}/teams")]
        [ProducesResponseType(typeof(Team), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createTeam")]
        public async Task<IActionResult> Create([FromRoute] Guid id, [FromBody]TeamForm form, CancellationToken ct)
        {
            var createdTeam = await _teamService.CreateAsync(id, form, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdTeam.Id }, createdTeam);
        }

        /// <summary>
        /// Updates a Team
        /// </summary>
        /// <remarks>
        /// Updates a Team with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>     
        /// <param name="id">The id of the Team</param>
        /// <param name="form">The updated Team values</param>
        /// <param name="ct"></param>
        [HttpPut("teams/{id}")]
        [ProducesResponseType(typeof(Team), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateTeam")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TeamForm form, CancellationToken ct)
        {
            var updatedTeam = await _teamService.UpdateAsync(id, form, ct);
            return Ok(updatedTeam);
        }

        /// <summary>
        /// Deletes a Team
        /// </summary>
        /// <remarks>
        /// Deletes a Team with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Team's Exercise
        /// </remarks>    
        /// <param name="id">The id of the Team</param>
        /// <param name="ct"></param>
        [HttpDelete("teams/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteTeam")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _teamService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Sends a new Team Notification
        /// </summary>
        /// <remarks>
        /// Creates a new Team within an Exercise with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>     
        /// <param name="id">The id of the Team</param>
        /// <param name="incomingData">The data to create the Team with</param>
        /// <param name="ct"></param>
        [HttpPost("teams/{id}/notifications")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [SwaggerOperation(operationId: "broadcastToTeam")]
        public async Task<IActionResult> Broadcast([FromRoute] Guid id, [FromBody] Notification incomingData, CancellationToken ct)
        {
            if (!incomingData.IsValid())
            {
                throw new ArgumentException(String.Format("Message was NOT sent to team {0}", id.ToString()));
            }
            var notification = await _notificationService.PostToTeam(id, incomingData, ct);
            if (notification.ToId != id)
            {
                throw new ForbiddenException("Message was not sent to team " + id.ToString());
            }
            await _teamHub.Clients.Group(id.ToString()).SendAsync("Reply", notification);
            return Ok("Message was sent to team " + id.ToString());
        }

        /// <summary>
        /// Sets a User's Primary Team
        /// </summary>
        /// <remarks>
        /// Sets the specified Team as a Primary Team for the specified User
        /// <para />
        /// Accessible only to the specified User
        /// </remarks>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="userId">The id of the User</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("users/{userId}/teams/{teamId}/primary")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "setUserPrimaryTeam")]
        public async Task<IActionResult> SetPrimary(Guid teamId, Guid userId, CancellationToken ct)
        {
            var team = await _teamService.SetPrimaryAsync(teamId, userId, ct);
            return Ok(team);
        }
    }
}


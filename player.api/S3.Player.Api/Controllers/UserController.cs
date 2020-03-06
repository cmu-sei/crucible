/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using S3.Player.Api.Hubs;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.Services;
using S3.Player.Api.ViewModels;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace S3.Player.Api.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IHubContext<UserHub> _userHub;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        public UserController(IUserService service, IHubContext<UserHub> userHub, INotificationService notificationService, IAuthorizationService authorizationService)
        {
            _userService = service;
            _notificationService = notificationService;
            _userHub = userHub;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Users in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Users in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUsers")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _userService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific User by id
        /// </summary>
        /// <remarks>
        /// Returns the User with the id specified
        /// <para />
        /// Accessible to a SuperUser, a User on an Admin Team within any of the specified Users' Exercises, or a User that shares any Teams with the specified User
        /// </remarks>
        /// <param name="id">The id of the User</param>
        /// <param name="ct"></param>
        [HttpGet("users/{id}")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUser")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var user = await _userService.GetAsync(id, ct);

            if (user == null)
                throw new EntityNotFoundException<User>();

            return Ok(user);
        }

        /// <summary>
        /// Gets all Users for an Exercise
        /// </summary>
        /// <remarks>
        /// Returns all Users within a specific Exercise
        /// <para />
        /// Accessible to a SuperUser or a User on an Admin Team within that Exercise
        /// </remarks>
        /// <param name="id">The id of the Exercise</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("exercises/{id}/users")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseUsers")]
        public async Task<IActionResult> GetByExercise(Guid id, CancellationToken ct)
        {
            var list = await _userService.GetByExerciseAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Users for a Team
        /// </summary>
        /// <remarks>
        /// Returns all Users within a specific Team
        /// <para />
        /// Accessible to a SuperUser, a User on an Admin Team within the Team's Exercise, or other members of the Team
        /// </remarks>
        /// <param name="id">The id of the Team</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("teams/{id}/users")]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTeamUsers")]
        public async Task<IActionResult> GetByTeam(Guid id, CancellationToken ct)
        {
            var list = await _userService.GetByTeamAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Adds a User to a Team
        /// </summary>
        /// <remarks>
        /// Adds the specified User to the specified Team
        /// <para />
        /// Accessible to a SuperUser, or a User on an Admin Team within the Team's Exercise
        /// </remarks>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="userId">The id of the User</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("teams/{teamId}/users/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "addUserToTeam")]
        public async Task<IActionResult> AddUserToTeam(Guid teamId, Guid userId, CancellationToken ct)
        {
            await _userService.AddToTeamAsync(teamId, userId, ct);
            return Ok();
        }

        /// <summary>
        /// Removes a User from a Team
        /// </summary>
        /// <remarks>
        /// Removes the specified User from the specified Team
        /// <para />
        /// Accessible to a SuperUser, or a User on an Admin Team within the Team's Exercise
        /// </remarks>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="userId">The id of the User</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete("teams/{teamId}/users/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "removeUserFromTeam")]
        public async Task<IActionResult> RemoveUserFromTeam(Guid teamId, Guid userId, CancellationToken ct)
        {
            await _userService.RemoveFromTeamAsync(teamId, userId, ct);
            return Ok();
        }

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <remarks>
        /// Creates a new User with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>        
        /// <param name="user">The data to create the User with</param>
        /// <param name="ct"></param>
        [HttpPost("users")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createUser")]
        public async Task<IActionResult> Create([FromBody] User user, CancellationToken ct)
        {
            var createdUser = await _userService.CreateAsync(user, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// Updates a User
        /// </summary>
        /// <remarks>
        /// Updates a User with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>     
        /// <param name="id">The Id of the User to update</param>
        /// <param name="user">The updated User values</param>
        /// <param name="ct"></param>
        [HttpPut("users/{id}")]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateUser")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] User user, CancellationToken ct)
        {
            var updatedUser = await _userService.UpdateAsync(id, user, ct);
            return Ok(updatedUser);
        }

        /// <summary>
        /// Deletes a User
        /// </summary>
        /// <remarks>
        /// Deletes the User with the specified id
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>      
        /// <param name="id">The id of the User to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("users/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteUser")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Sends a new User Notification
        /// </summary>
        /// <remarks>
        /// Accessible only to a SuperUser or a User on an Admin User within the specified Exercise
        /// </remarks>
        /// <param name="exerciseId">The id of the Exercise</param>
        /// <param name="userId">The id of the User</param>
        /// <param name="incomingData">The data to create the Notification</param>
        /// <param name="ct"></param>
        [HttpPost("exercises/{exerciseId}/users/{userId}/notifications")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [SwaggerOperation(operationId: "broadcastToUser")]
        public async Task<IActionResult> Broadcast([FromRoute] Guid exerciseId, Guid userId, [FromBody] Notification incomingData, CancellationToken ct)
        {
            if (!incomingData.IsValid())
            {
                throw new ArgumentException(String.Format("Message was NOT sent to user {0} in exercise {1}", userId.ToString(), exerciseId.ToString()));
            }
            var notification = await _notificationService.PostToUser(exerciseId, userId, incomingData, ct);
            if (notification.ToId != userId)
            {
                throw new ForbiddenException(String.Format("Message was NOT sent to user {0} in exercise {1}", userId.ToString(), exerciseId.ToString()));
            }

            await _userHub.Clients.Group(String.Format("{0}_{1}", exerciseId.ToString(), userId.ToString())).SendAsync("Reply", notification);
            return Ok(String.Format("Message was sent to user {0} in exercise {1}", userId.ToString(), exerciseId.ToString()));
        }
    }
}


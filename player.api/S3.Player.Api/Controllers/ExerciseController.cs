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
using S3.Player.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace S3.Player.Api.Controllers
{
    public class ExerciseController : BaseController
    {
        private readonly IExerciseService _exerciseService;
        private readonly IHubContext<ExerciseHub> _exerciseHub;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        public ExerciseController(IExerciseService exerciseService, IHubContext<ExerciseHub> exerciseHub, INotificationService notificationService, IAuthorizationService authorizationService)
        {
            _exerciseService = exerciseService;
            _exerciseHub = exerciseHub;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Exercise in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Exercises in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("exercises")]
        [ProducesResponseType(typeof(IEnumerable<Exercise>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExercises")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _exerciseService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Exercises for a User
        /// </summary>
        /// <remarks>
        /// Returns all Exercises where the specified User is a member of at least one of it's Teams
        /// <para />
        /// Accessible to a SuperUser or the specified User itself
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/exercises")]
        [ProducesResponseType(typeof(IEnumerable<Exercise>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserExercises")]
        public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _exerciseService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Exercises for the current User
        /// </summary>
        /// <remarks>
        /// Returns all Exercises where the current User is a member of at least one of it's Teams
        /// <para />
        /// Accessible only to the current User.
        /// <para/>
        /// This is a convenience endpoint and simply returns a 302 redirect to the fully qualified users/{id}/exercises endpoint 
        /// </remarks>
        [HttpGet("me/exercises")]
        [ProducesResponseType(typeof(IEnumerable<Exercise>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyExercises")]
        public async Task<IActionResult> GetMy(CancellationToken ct)
        {
            return RedirectToAction(nameof(this.GetByUserId), new { id = User.GetId() });
        }

        /// <summary>
        /// Gets a specific Exercise by id
        /// </summary>
        /// <remarks>
        /// Returns the Exercise with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Exercise
        /// </remarks>
        /// <param name="id">The id of the Exercise</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("exercises/{id}")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExercise")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var exercise = await _exerciseService.GetAsync(id, ct);

            if (exercise == null)
                throw new EntityNotFoundException<Exercise>();

            return Ok(exercise);
        }

        /// <summary>
        /// Creates a new Exercise
        /// </summary>
        /// <remarks>
        /// Creates a new Exercise with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="exercise">The data to create the Exercise with</param>
        /// <param name="ct"></param>
        [HttpPost("exercises")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createExercise")]
        public async Task<IActionResult> Create([FromBody] Exercise exercise, CancellationToken ct)
        {
            var createdExercise = await _exerciseService.CreateAsync(exercise, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdExercise.Id }, createdExercise);
        }

        /// <summary>
        /// Updates an Exercise
        /// </summary>
        /// <remarks>
        /// Updates an Exercise with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="exercise">The updated Exercise values</param>
        /// <param name="ct"></param>
        [HttpPut("exercises/{id}")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateExercise")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Exercise exercise, CancellationToken ct)
        {
            var updatedExercise = await _exerciseService.UpdateAsync(id, exercise, ct);
            return Ok(updatedExercise);
        }

        /// <summary>
        /// Deletes an Exercise
        /// </summary>
        /// <remarks>
        /// Deletes an Exercise with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>    
        /// <param name="id">The id of the Exercise to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("exercises/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteExercise")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _exerciseService.DeleteAsync(id, ct);
            return NoContent();
        }
        
        /// <summary>
        /// Clones an Exercise
        /// </summary>
        /// <param name="id">Id of the Exercise to be cloned</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("exercises/{id}/clone")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "cloneExercise")]
        public async Task<IActionResult> Clone(Guid id, CancellationToken ct)
        {
            var createdExercise = await _exerciseService.CloneAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdExercise.Id }, createdExercise);
        }

        /// <summary>
        /// Sends a new Exercise Notification
        /// </summary>
        /// <remarks>
        /// Creates a new Exercise within an Exercise with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Exercise within the specified Exercise
        /// </remarks>     
        /// <param name="id">The id of the Exercise</param>
        /// <param name="incomingData">The data to create the Exercise with</param>
        /// <param name="ct"></param>
        [HttpPost("exercises/{id}/notifications")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [SwaggerOperation(operationId: "broadcastToExercise")]
        public async Task<IActionResult> Broadcast([FromRoute] Guid id, [FromBody] Notification incomingData, CancellationToken ct)
        {
            if (!incomingData.IsValid())
            {
                throw new ArgumentException(String.Format("Message was NOT sent to exercise {0}", id.ToString()));
            }
            var notification = await _notificationService.PostToExercise(id, incomingData, ct);
            if (notification.ToId != id)
            {
                throw new ForbiddenException("Message was not sent to exercise " + id.ToString());
            }
            await _exerciseHub.Clients.Group(id.ToString()).SendAsync("Reply", notification);
            return Ok("Message was sent to exercise " + id.ToString());
        }

    }
}


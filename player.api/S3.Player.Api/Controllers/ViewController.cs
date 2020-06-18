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
    public class ViewController : BaseController
    {
        private readonly IViewService _viewService;
        private readonly IHubContext<ViewHub> _viewHub;
        private readonly INotificationService _notificationService;
        private readonly IAuthorizationService _authorizationService;

        public ViewController(IViewService viewService, IHubContext<ViewHub> viewHub, INotificationService notificationService, IAuthorizationService authorizationService)
        {
            _viewService = viewService;
            _viewHub = viewHub;
            _notificationService = notificationService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all View in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Views in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>
        /// <returns></returns>
        [HttpGet("views")]
        [ProducesResponseType(typeof(IEnumerable<View>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getViews")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _viewService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Views for a User
        /// </summary>
        /// <remarks>
        /// Returns all Views where the specified User is a member of at least one of it's Teams
        /// <para />
        /// Accessible to a SuperUser or the specified User itself
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/views")]
        [ProducesResponseType(typeof(IEnumerable<View>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserViews")]
        public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _viewService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Views for the current User
        /// </summary>
        /// <remarks>
        /// Returns all Views where the current User is a member of at least one of it's Teams
        /// </remarks>
        [HttpGet("me/views")]
        [ProducesResponseType(typeof(IEnumerable<View>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyViews")]
        public async Task<IActionResult> GetMy(CancellationToken ct)
        {
            var list = await _viewService.GetByUserIdAsync(User.GetId(), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific View by id
        /// </summary>
        /// <remarks>
        /// Returns the View with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified View
        /// </remarks>
        /// <param name="id">The id of the View</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("views/{id}")]
        [ProducesResponseType(typeof(View), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getView")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var view = await _viewService.GetAsync(id, ct);

            if (view == null)
                throw new EntityNotFoundException<View>();

            return Ok(view);
        }

        /// <summary>
        /// Creates a new View
        /// </summary>
        /// <remarks>
        /// Creates a new View with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>
        /// <param name="view">The data to create the View with</param>
        /// <param name="ct"></param>
        [HttpPost("views")]
        [ProducesResponseType(typeof(View), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createView")]
        public async Task<IActionResult> Create([FromBody] View view, CancellationToken ct)
        {
            var createdView = await _viewService.CreateAsync(view, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdView.Id }, createdView);
        }

        /// <summary>
        /// Updates an View
        /// </summary>
        /// <remarks>
        /// Updates an View with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified View
        /// </remarks>
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="view">The updated View values</param>
        /// <param name="ct"></param>
        [HttpPut("views/{id}")]
        [ProducesResponseType(typeof(View), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateView")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] View view, CancellationToken ct)
        {
            var updatedView = await _viewService.UpdateAsync(id, view, ct);
            return Ok(updatedView);
        }

        /// <summary>
        /// Deletes an View
        /// </summary>
        /// <remarks>
        /// Deletes an View with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified View
        /// </remarks>
        /// <param name="id">The id of the View to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("views/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteView")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _viewService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Clones an View
        /// </summary>
        /// <param name="id">Id of the View to be cloned</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("views/{id}/clone")]
        [ProducesResponseType(typeof(View), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "cloneView")]
        public async Task<IActionResult> Clone(Guid id, CancellationToken ct)
        {
            var createdView = await _viewService.CloneAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdView.Id }, createdView);
        }

        /// <summary>
        /// Sends a new View Notification
        /// </summary>
        /// <remarks>
        /// Creates a new View within an View with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin View within the specified View
        /// </remarks>
        /// <param name="id">The id of the View</param>
        /// <param name="incomingData">The data to create the View with</param>
        /// <param name="ct"></param>
        [HttpPost("views/{id}/notifications")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [SwaggerOperation(operationId: "broadcastToView")]
        public async Task<IActionResult> Broadcast([FromRoute] Guid id, [FromBody] Notification incomingData, CancellationToken ct)
        {
            if (!incomingData.IsValid())
            {
                throw new ArgumentException(String.Format("Message was NOT sent to view {0}", id.ToString()));
            }
            var notification = await _notificationService.PostToView(id, incomingData, ct);
            if (notification.ToId != id)
            {
                throw new ForbiddenException("Message was not sent to view " + id.ToString());
            }
            await _viewHub.Clients.Group(id.ToString()).SendAsync("Reply", notification);
            return Ok("Message was sent to view " + id.ToString());
        }

    }
}

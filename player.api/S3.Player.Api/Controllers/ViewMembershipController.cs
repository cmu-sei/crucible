/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Mvc;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.Services;
using S3.Player.Api.ViewModels;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace S3.Player.Api.Controllers
{
    public class ViewMembershipController : BaseController
    {
        private readonly IViewMembershipService _viewMembershipService;

        public ViewMembershipController(IViewMembershipService viewMembershipService)
        {
            _viewMembershipService = viewMembershipService;
        }

        /// <summary>
        /// Gets a specific View Membership by id
        /// </summary>
        /// <remarks>
        /// Returns the View Membership with the id specified
        /// <para />
        /// Accessible to Super Users, View Admins for the memberships' View, or the User that the membership belongs to
        /// </remarks>
        /// <param name="id">The id of the View Membership</param>
        /// <returns></returns>
        [HttpGet("view-memberships/{id}")]
        [ProducesResponseType(typeof(ViewMembership), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getViewMembership")]
        public async Task<IActionResult> Get(Guid id)
        {
            var membership = await _viewMembershipService.GetAsync(id);

            if (membership == null)
                throw new EntityNotFoundException<ViewMembership>();

            return Ok(membership);
        }

        /// <summary>
        /// Gets all View Memberships for a User
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions in the system.
        /// <para />
        /// Accessible to Super Users or the specified User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("/users/{userId}/view-memberships")]
        [ProducesResponseType(typeof(IEnumerable<ViewMembership>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getViewMemberships")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var list = await _viewMembershipService.GetByUserIdAsync(userId);
            return Ok(list);
        }
    }
}

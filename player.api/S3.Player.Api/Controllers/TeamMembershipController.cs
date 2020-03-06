/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
    public class TeamMembershipController : BaseController
    {
        private readonly ITeamMembershipService _teamMembershipService;

        public TeamMembershipController(ITeamMembershipService teamMembershipService)
        {
            _teamMembershipService = teamMembershipService;
        }       

        /// <summary>
        /// Gets a specific Team Membership by id
        /// </summary>
        /// <remarks>
        /// Returns the Team Membership with the id specified
        /// <para />
        /// Accessible to Super Users, Exercise Admins for the membership's Exercise, or the User that the membership belongs to
        /// </remarks>
        /// <param name="id">The id of the Team Membership</param>
        /// <returns></returns>
        [HttpGet("team-memberships/{id}")]
        [ProducesResponseType(typeof(TeamMembership), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTeamMembership")]
        public async Task<IActionResult> Get(Guid id)
        {
            var membership = await _teamMembershipService.GetAsync(id);

            if (membership == null)
                throw new EntityNotFoundException<TeamMembership>();

            return Ok(membership);
        }

        /// <summary>
        /// Gets all Team Memberships for a User by Exercise
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions in the system.
        /// <para />
        /// Accessible to Super Users or the specified User
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("/users/{userId}/exercises/{exerciseId}/team-memberships")]
        [ProducesResponseType(typeof(IEnumerable<TeamMembership>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTeamMemberships")]
        public async Task<IActionResult> GetByExerciseIdForUser(Guid exerciseId, Guid userId)
        {
            var list = await _teamMembershipService.GetByExerciseIdForUserAsync(exerciseId, userId);
            return Ok(list);
        }

        /// <summary>
        /// Updates a Team Membership
        /// </summary>
        /// <remarks>
        /// Updates a Team Membership with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Exercise
        /// </remarks>     
        /// <param name="id">The id of the Team Membership</param>
        /// <param name="form">The updated Team Membership values</param>
        [HttpPut("team-memberships/{id}")]
        [ProducesResponseType(typeof(TeamMembership), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateTeamMembership")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] TeamMembershipForm form)
        {
            var updatedMembership = await _teamMembershipService.UpdateAsync(id, form);
            return Ok(updatedMembership);
        }
    }
}


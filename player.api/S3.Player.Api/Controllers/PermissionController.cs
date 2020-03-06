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
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Gets all Permissions in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(IEnumerable<Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getPermissions")]
        public async Task<IActionResult> Get()
        {
            var list = await _permissionService.GetAsync();
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Permission by id
        /// </summary>
        /// <remarks>
        /// Returns the Permission with the id specified
        /// <para />
        /// Accessible to all authenticated Users
        /// </remarks>
        /// <param name="id">The id of the Permission</param>
        /// <returns></returns>
        [HttpGet("permissions/{id}")]
        [ProducesResponseType(typeof(Permission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getPermission")]
        public async Task<IActionResult> Get(Guid id)
        {
            var permission = await _permissionService.GetAsync(id);

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            return Ok(permission);
        }

        /// <summary>
        /// Creates a new Permission
        /// </summary>
        /// <remarks>
        /// Creates a new Permission with the attributes specified
        /// <para />
        /// An Permission is a top-level resource that can optionally be the parent of an Exercise specific Application resource, which would inherit it's properties
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>   
        [HttpPost("permissions")]
        [ProducesResponseType(typeof(Permission), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createPermission")]
        public async Task<IActionResult> Create([FromBody] PermissionForm form)
        {
            var createdPermission = await _permissionService.CreateAsync(form);
            return CreatedAtAction(nameof(this.Get), new { id = createdPermission.Id }, createdPermission);
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <remarks>
        /// Updates a Permission with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="form">The updated Permission values</param>
        /// <returns></returns>
        [HttpPut("permissions/{id}")]
        [ProducesResponseType(typeof(Permission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updatePermission")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] PermissionForm form)
        {
            var updatedPermission = await _permissionService.UpdateAsync(id, form);
            return Ok(updatedPermission);
        }

        /// <summary>
        /// Deletes an Permission
        /// </summary>
        /// <remarks>
        /// Deletes a Permission with the specified id
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>    
        /// <param name="id">The id of the Permission to delete</param>
        [HttpDelete("permissions/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deletePermission")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _permissionService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Gets a User's permissions for an Exercise
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions for the User on this Exercise
        /// <para />
        /// If the User is a member of the Exercise, this will first use any Permissions on their Primary Team Membership, and then apply any Permissions on the Team itself.
        /// If the User is not a member of the Exercise, the User's top level Permissions will be returned.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("users/{userId}/exercises/{exerciseId}/permissions")]
        [ProducesResponseType(typeof(IEnumerable<Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserExercisePermissions")]
        public async Task<IActionResult> GetByExerciseId(Guid exerciseId, Guid userId)
        {
            var list = await _permissionService.GetByExerciseIdForUserAsync(exerciseId, userId);
            return Ok(list);
        }

        /// <summary>
        /// Gets a User's permissions for the Exercise a given Team is part of
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions for the User on the Exercise that the specified Team belongs to
        /// <para />
        /// If the User is a member of the Exercise, this will first use any Permissions on their Primary Team Membership, and then apply any Permissions on the Team itself.
        /// If the User is not a member of the Exercise, the User's top level Permissions will be returned.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("users/{userId}/teams/{teamId}/permissions")]
        [ProducesResponseType(typeof(IEnumerable<Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserExercisePermissionsByTeam")]
        public async Task<IActionResult> GetByTeamId(Guid teamId, Guid userId)
        {
            var list = await _permissionService.GetByTeamIdForUserAsync(teamId, userId);
            return Ok(list);
        }

        /// <summary>
        /// Adds a Permission to a Role
        /// </summary>
        /// <remarks>
        /// Adds the specified Permission to the specified Role
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="roleId">The id of the Role</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpPost("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "addPermissionToRole")]
        public async Task<IActionResult> AddPermissionToRole(Guid roleId, Guid permissionId)
        {
            await _permissionService.AddToRoleAsync(roleId, permissionId);
            return Ok();
        }

        /// <summary>
        /// Removes a Permission from a Role
        /// </summary>
        /// <remarks>
        /// Removes the specified Permission from the specified Role
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="roleId">The id of the Role</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "removePermissionFromRole")]
        public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            await _permissionService.RemoveFromRoleAsync(roleId, permissionId);
            return Ok();
        }

        /// <summary>
        /// Adds a Permission to a Team
        /// </summary>
        /// <remarks>
        /// Adds the specified Permission to the specified Team
        /// <para />
        /// Accessible to a SuperUser or an Exercise Admin of the Exercise the Team is part of
        /// </remarks>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpPost("teams/{teamId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "addPermissionToTeam")]
        public async Task<IActionResult> AddPermissionToTeam(Guid teamId, Guid permissionId)
        {
            await _permissionService.AddToTeamAsync(teamId, permissionId);
            return Ok();
        }

        /// <summary>
        /// Removes a Permission from a Team
        /// </summary>
        /// <remarks>
        /// Removes the specified Permission from the specified Team
        /// <para />
        /// Accessible to a SuperUser or an Exercise Admin of the Exercise the Team is part of
        /// </remarks>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpDelete("teams/{teamId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "removePermissionFromTeam")]
        public async Task<IActionResult> RemovePermissionFromTeam(Guid teamId, Guid permissionId)
        {
            await _permissionService.RemoveFromTeamAsync(teamId, permissionId);
            return Ok();
        }

        /// <summary>
        /// Adds a Permission to a User
        /// </summary>
        /// <remarks>
        /// Adds the specified Permission to the specified User
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="userId">The id of the User</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpPost("users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "addPermissionToUser")]
        public async Task<IActionResult> AddPermissionToUser(Guid userId, Guid permissionId)
        {
            await _permissionService.AddToUserAsync(userId, permissionId);
            return Ok();
        }

        /// <summary>
        /// Removes a Permission from a User
        /// </summary>
        /// <remarks>
        /// Removes the specified Permission from the specified User
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="userId">The id of the User</param>
        /// <param name="permissionId">The id of the Permission</param>
        /// <returns></returns>
        [HttpDelete("users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "removePermissionFromUser")]
        public async Task<IActionResult> RemovePermissionFromUser(Guid userId, Guid permissionId)
        {
            await _permissionService.RemoveFromUserAsync(userId, permissionId);
            return Ok();
        }
    }
}


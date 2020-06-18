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
using STT = System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using SAVM = Steamfitter.Api.ViewModels;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    public class PermissionController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IAuthorizationService _authorizationService;

        public PermissionController(IPermissionService permissionService, IAuthorizationService authorizationService)
        {
            _permissionService = permissionService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Permission in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Permissions in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getPermissions")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _permissionService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets Permissions for the current user
        /// </summary>
        /// <remarks>
        /// Returns a list of the current user's Permissions.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("permissions/mine")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyPermissions")]
        public async STT.Task<IActionResult> GetMine(CancellationToken ct)
        {
            var list = await _permissionService.GetMineAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets Permissions for the specified user
        /// </summary>
        /// <remarks>
        /// Returns a list of the specified user's Permissions.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("/users/{userId}/permissions")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Permission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getPermissionsByUser")]
        public async STT.Task<IActionResult> GetByUser([FromRoute] Guid userId, CancellationToken ct)
        {
            var list = await _permissionService.GetByUserAsync(userId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Permission by id
        /// </summary>
        /// <remarks>
        /// Returns the Permission with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Permission
        /// </remarks>
        /// <param name="id">The id of the Permission</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("permissions/{id}")]
        [ProducesResponseType(typeof(SAVM.Permission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getPermission")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var permission = await _permissionService.GetAsync(id, ct);

            if (permission == null)
                throw new EntityNotFoundException<SAVM.Permission>();

            return Ok(permission);
        }

        /// <summary>
        /// Creates a new Permission
        /// </summary>
        /// <remarks>
        /// Creates a new Permission with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="permission">The data to create the Permission with</param>
        /// <param name="ct"></param>
        [HttpPost("permissions")]
        [ProducesResponseType(typeof(SAVM.Permission), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createPermission")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.Permission permission, CancellationToken ct)
        {
            permission.CreatedBy = User.GetId();
            var createdPermission = await _permissionService.CreateAsync(permission, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdPermission.Id }, createdPermission);
        }

        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <remarks>
        /// Updates a Permission with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Permission
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="permission">The updated Permission values</param>
        /// <param name="ct"></param>
        [HttpPut("permissions/{id}")]
        [ProducesResponseType(typeof(SAVM.Permission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updatePermission")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.Permission permission, CancellationToken ct)
        {
            permission.ModifiedBy = User.GetId();
            var updatedPermission = await _permissionService.UpdateAsync(id, permission, ct);
            return Ok(updatedPermission);
        }

        /// <summary>
        /// Deletes a Permission
        /// </summary>
        /// <remarks>
        /// Deletes a Permission with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Permission
        /// </remarks>    
        /// <param name="id">The id of the Permission to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("permissions/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deletePermission")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _permissionService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


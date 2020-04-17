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
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;
using Steamfitter.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    public class UserPermissionController : BaseController
    {
        private readonly IUserPermissionService _userPermissionService;
        private readonly IAuthorizationService _authorizationService;

        public UserPermissionController(IUserPermissionService userPermissionService, IAuthorizationService authorizationService)
        {
            _userPermissionService = userPermissionService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all UserPermissions in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the UserPermissions in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("userpermissions")]
        [ProducesResponseType(typeof(IEnumerable<UserPermission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserPermissions")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _userPermissionService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific UserPermission by id
        /// </summary>
        /// <remarks>
        /// Returns the UserPermission with the id specified
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>
        /// <param name="id">The id of the UserPermission</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("userpermissions/{id}")]
        [ProducesResponseType(typeof(UserPermission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserPermission")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var permission = await _userPermissionService.GetAsync(id, ct);

            if (permission == null)
                throw new EntityNotFoundException<UserPermission>();

            return Ok(permission);
        }

        /// <summary>
        /// Creates a new UserPermission
        /// </summary>
        /// <remarks>
        /// Creates a new UserPermission with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>    
        /// <param name="permission">The data to create the UserPermission with</param>
        /// <param name="ct"></param>
        [HttpPost("userpermissions")]
        [ProducesResponseType(typeof(UserPermission), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createUserPermission")]
        public async Task<IActionResult> Create([FromBody] UserPermission permission, CancellationToken ct)
        {
            permission.CreatedBy = User.GetId();
            var createdUserPermission = await _userPermissionService.CreateAsync(permission, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdUserPermission.Id }, createdUserPermission);
        }

        /// <summary>
        /// Deletes a UserPermission
        /// </summary>
        /// <remarks>
        /// Deletes a UserPermission with the specified id
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>    
        /// <param name="id">The id of the UserPermission to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("userpermissions/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteUserPermission")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _userPermissionService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Deletes a UserPermission by user ID and permission ID
        /// </summary>
        /// <remarks>
        /// Deletes a UserPermission with the specified user ID and permission ID
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>    
        /// <param name="userId">ID of a user.</param>
        /// <param name="permissionId">ID of a permission.</param>
        /// <param name="ct"></param>
        [HttpDelete("users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteUserPermissionByIds")]
        public async Task<IActionResult> Delete(Guid userId, Guid permissionId, CancellationToken ct)
        {
            await _userPermissionService.DeleteByIdsAsync(userId, permissionId, ct);
            return NoContent();
        }

    }
}


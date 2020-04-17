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
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caster.Api.Features.UserPermissions
{
    [Route("api/userPermissions")]
    [ApiController]
    [Authorize]
    public class UserPermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserPermissionsController(IMediator mediator) 
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get a single userPermission.
        /// </summary>
        /// <param name="id">ID of an userPermission.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserPermission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetUserPermission")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return Ok(result);
        }
        
        /// <summary>
        /// Get all userPermissions.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<UserPermission>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetAllUserPermissions")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Create a new userPermission.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(typeof(UserPermission), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateUserPermission")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update a userPermission.
        /// </summary>
        /// <param name="id">ID of an userPermission.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserPermission), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "EditUserPermission")]
        public async Task<IActionResult> Edit([FromRoute] Guid id, Edit.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete a userPermission.
        /// </summary>
        /// <param name="id">ID of an userPermission.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteUserPermission")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _mediator.Send(new Delete.Command { Id = id });
            return NoContent();
        }

        /// <summary>
        /// Delete a userPermission by user and permission.
        /// </summary>
        /// <param name="userId">ID of a user.</param>
        /// <param name="permissionId">ID of a permission.</param>
        /// <returns></returns>
        [HttpDelete("/api/users/{userId}/permissions/{permissionId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteUserPermissionByIds")]
        public async Task<IActionResult> DeleteByIds([FromRoute] Guid userId, [FromRoute] Guid permissionId)
        {
            await _mediator.Send(new Delete.Command { UserId = userId, PermissionId = permissionId });
            return NoContent();
        }

    }
}


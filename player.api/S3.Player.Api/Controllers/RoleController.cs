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
    public class RoleController : BaseController
    {
        private readonly IRoleService _RoleService;

        public RoleController(IRoleService RoleService)
        {
            _RoleService = RoleService;
        }

        /// <summary>
        /// Gets all Roles in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Roles in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>
        /// <returns></returns>
        [HttpGet("Roles")]
        [ProducesResponseType(typeof(IEnumerable<Role>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getRoles")]
        public async Task<IActionResult> Get()
        {
            var list = await _RoleService.GetAsync();
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Role by id
        /// </summary>
        /// <remarks>
        /// Returns the Role with the id specified
        /// <para />
        /// Accessible to all authenticated Users
        /// </remarks>
        /// <param name="id">The id of the Role</param>
        /// <returns></returns>
        [HttpGet("Roles/{id}")]
        [ProducesResponseType(typeof(Role), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getRole")]
        public async Task<IActionResult> Get(Guid id)
        {
            var Role = await _RoleService.GetAsync(id);

            if (Role == null)
                throw new EntityNotFoundException<Role>();

            return Ok(Role);
        }

        /// <summary>
        /// Creates a new Role
        /// </summary>
        /// <remarks>
        /// Creates a new Role with the attributes specified
        /// <para />
        /// An Role is a top-level resource that can optionally be the parent of an View specific Application resource, which would inherit it's properties
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        [HttpPost("Roles")]
        [ProducesResponseType(typeof(Role), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createRole")]
        public async Task<IActionResult> Create([FromBody] RoleForm form)
        {
            var createdRole = await _RoleService.CreateAsync(form);
            return CreatedAtAction(nameof(this.Get), new { id = createdRole.Id }, createdRole);
        }

        /// <summary>
        /// Updates a Role
        /// </summary>
        /// <remarks>
        /// Updates a Role with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="form">The updated Role values</param>
        /// <returns></returns>
        [HttpPut("Roles/{id}")]
        [ProducesResponseType(typeof(Role), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateRole")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] RoleForm form)
        {
            var updatedRole = await _RoleService.UpdateAsync(id, form);
            return Ok(updatedRole);
        }

        /// <summary>
        /// Deletes an Role
        /// </summary>
        /// <remarks>
        /// Deletes a Role with the specified id
        /// <para />
        /// Accessible only to a SuperUser
        /// </remarks>
        /// <param name="id">The id of the Role to delete</param>
        [HttpDelete("Roles/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteRole")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _RoleService.DeleteAsync(id);
            return NoContent();
        }
    }
}

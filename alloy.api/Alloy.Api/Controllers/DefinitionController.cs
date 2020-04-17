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
using Swashbuckle.AspNetCore.SwaggerGen;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.Services;
using Alloy.Api.ViewModels;

namespace Alloy.Api.Controllers
{
    public class DefinitionController : BaseController
    {
        private readonly IDefinitionService _definitionService;
        private readonly IAuthorizationService _authorizationService;

        public DefinitionController(IDefinitionService definitionService, IAuthorizationService authorizationService)
        {
            _definitionService = definitionService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Definition in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Definitions in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("definitions")]
        [ProducesResponseType(typeof(IEnumerable<Definition>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDefinitions")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _definitionService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Definition by id
        /// </summary>
        /// <remarks>
        /// Returns the Definition with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Definition
        /// </remarks>
        /// <param name="id">The id of the Definition</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("definitions/{id}")]
        [ProducesResponseType(typeof(Definition), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(operationId: "getDefinition")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var definition = await _definitionService.GetAsync(id, ct);

            if (definition == null)
                throw new EntityNotFoundException<Definition>();

            return Ok(definition);
        }

        /// <summary>
        /// Creates a new Definition
        /// </summary>
        /// <remarks>
        /// Creates a new Definition with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="definition">The data to create the Definition with</param>
        /// <param name="ct"></param>
        [HttpPost("definitions")]
        [ProducesResponseType(typeof(Definition), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createDefinition")]
        public async Task<IActionResult> Create([FromBody] Definition definition, CancellationToken ct)
        {
            var createdDefinition = await _definitionService.CreateAsync(definition, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdDefinition.Id }, createdDefinition);
        }

        /// <summary>
        /// Updates an Definition
        /// </summary>
        /// <remarks>
        /// Updates an Definition with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Definition
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="definition">The updated Definition values</param>
        /// <param name="ct"></param>
        [HttpPut("definitions/{id}")]
        [ProducesResponseType(typeof(Definition), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(operationId: "updateDefinition")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Definition definition, CancellationToken ct)
        {
            var updatedDefinition = await _definitionService.UpdateAsync(id, definition, ct);
            return Ok(updatedDefinition);
        }

        /// <summary>
        /// Deletes an Definition
        /// </summary>
        /// <remarks>
        /// Deletes an Definition with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Definition
        /// </remarks>    
        /// <param name="id">The id of the Definition to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("definitions/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(operationId: "deleteDefinition")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _definitionService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


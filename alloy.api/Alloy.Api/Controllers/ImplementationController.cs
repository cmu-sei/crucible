/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
    public class ImplementationController : BaseController
    {
        private readonly IImplementationService _implementationService;
        private readonly IAuthorizationService _authorizationService;

        public ImplementationController(IImplementationService implementationService, IAuthorizationService authorizationService)
        {
            _implementationService = implementationService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Implementation in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Implementations in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>
        /// <returns></returns>
        [HttpGet("implementations")]
        [ProducesResponseType(typeof(IEnumerable<Implementation>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getImplementations")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _implementationService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Implementations for the indicated Definition
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Implementations for the Definition.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("definitions/{definitionId}/implementations")]
        [ProducesResponseType(typeof(IEnumerable<Implementation>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDefinitionImplementations")]
        public async Task<IActionResult> GetDefinitionImplementations(string definitionId, CancellationToken ct)
        {
            var list = await _implementationService.GetDefinitionImplementationsAsync(Guid.Parse(definitionId), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets the user's Implementations for the indicated Definition
        /// </summary>
        /// <remarks>
        /// Returns a list of the user's Implementations for the Definition.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("definitions/{definitionId}/implementations/mine")]
        [ProducesResponseType(typeof(IEnumerable<Implementation>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyDefinitionImplementations")]
        public async Task<IActionResult> GetMyDefinitionImplementations(string definitionId, CancellationToken ct)
        {
            var list = await _implementationService.GetMyDefinitionImplementationsAsync(Guid.Parse(definitionId), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets the user's Implementations for the indicated Player Exercise Id
        /// </summary>
        /// <remarks>
        /// Returns a list of the user's Implementations for the Exercise.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("exercises/{exerciseId}/implementations/mine")]
        [ProducesResponseType(typeof(IEnumerable<Implementation>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyExerciseImplementations")]
        public async Task<IActionResult> GetMyExerciseImplementations(string exerciseId, CancellationToken ct)
        {
            var list = await _implementationService.GetMyExerciseImplementationsAsync(Guid.Parse(exerciseId), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Implementation by id
        /// </summary>
        /// <remarks>
        /// Returns the Implementation with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Implementation
        /// </remarks>
        /// <param name="id">The id of the Implementation</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("implementations/{id}")]
        [ProducesResponseType(typeof(Implementation), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getImplementation")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var implementation = await _implementationService.GetAsync(id, ct);

            if (implementation == null)
                throw new EntityNotFoundException<Implementation>();

            return Ok(implementation);
        }

        /// <summary>
        /// Creates a new Implementation
        /// </summary>
        /// <remarks>
        /// Creates a new Implementation with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>
        /// <param name="implementation">The data to create the Implementation with</param>
        /// <param name="ct"></param>
        [HttpPost("implementations")]
        [ProducesResponseType(typeof(Implementation), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createImplementation")]
        public async Task<IActionResult> Create([FromBody] Implementation implementation, CancellationToken ct)
        {
            var createdImplementation = await _implementationService.CreateAsync(implementation, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdImplementation.Id }, createdImplementation);
        }

        /// <summary>
        /// Creates a new Implementation from a definition
        /// </summary>
        /// <remarks>
        /// Creates a new Implementation from the specified definition
        /// </remarks>
        /// <param name="definitionId">The ID of the Definition to use to create the Implementation</param>
        /// <param name="ct"></param>
        [HttpPost("definitions/{definitionId}/implementations")]
        [ProducesResponseType(typeof(Implementation), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createImplementationFromDefinition")]
        public async Task<IActionResult> CreateImplementationFromDefinition(string definitionId, CancellationToken ct)
        {
            var createdImplementation = await _implementationService.LaunchImplementationFromDefinitionAsync(Guid.Parse(definitionId), ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdImplementation.Id }, createdImplementation);
        }

        /// <summary>
        /// Updates an Implementation
        /// </summary>
        /// <remarks>
        /// Updates an Implementation with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Implementation
        /// </remarks>
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="implementation">The updated Implementation values</param>
        /// <param name="ct"></param>
        [HttpPut("implementations/{id}")]
        [ProducesResponseType(typeof(Implementation), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateImplementation")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Implementation implementation, CancellationToken ct)
        {
            var updatedImplementation = await _implementationService.UpdateAsync(id, implementation, ct);
            return Ok(updatedImplementation);
        }

        /// <summary>
        /// Deletes an Implementation
        /// </summary>
        /// <remarks>
        /// Deletes an Implementation with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Implementation
        /// </remarks>
        /// <param name="id">The id of the Implementation to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("implementations/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteImplementation")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _implementationService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Ends an Implementation
        /// </summary>
        /// <remarks>
        /// Ends an Implementation with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Implementation
        /// </remarks>
        /// <param name="id">The id of the Implementation to end</param>
        /// <param name="ct"></param>
        [HttpDelete("implementations/{id}/end")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "endImplementation")]
        public async Task<IActionResult> End(Guid id, CancellationToken ct)
        {
            await _implementationService.EndAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Redeploys the Caster Workspace of an Implementation
        /// </summary>
        /// <remarks>
        /// Redeploys the Caster Workspace for the Implementation with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Implementation
        /// </remarks>
        /// <param name="id">The id of the Implementation to redeploy</param>
        /// <param name="ct"></param>
        [HttpPost("implementations/{id}/redeploy")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "redeployImplementation")]
        public async Task<IActionResult> Redeploy(Guid id, CancellationToken ct)
        {
            await _implementationService.RedeployAsync(id, ct);
            return NoContent();
        }

    }
}

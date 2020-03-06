/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
    public class DispatchTaskResultController : BaseController
    {
        private readonly IDispatchTaskResultService _DispatchTaskResultService;
        private readonly IAuthorizationService _authorizationService;

        public DispatchTaskResultController(IDispatchTaskResultService DispatchTaskResultService, IAuthorizationService authorizationService)
        {
            _DispatchTaskResultService = DispatchTaskResultService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all DispatchTaskResult in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the DispatchTaskResults in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTaskResult>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDispatchTaskResults")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTaskResults for a Session
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTaskResults for the specified Session
        /// </remarks>
        /// <returns></returns>
        [HttpGet("sessions/{id}/DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSessionDispatchTaskResults")]
        public async Task<IActionResult> GetBySessionId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetBySessionIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTaskResults for an Exercise
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTaskResults for the specified Exercise
        /// </remarks>
        /// <returns></returns>
        [HttpGet("exercises/{id}/DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseDispatchTaskResults")]
        public async Task<IActionResult> GetByExerciseId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetByExerciseIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual DispatchTaskResults for a User
        /// </summary>
        /// <remarks>
        /// Returns all manual DispatchTaskResults for the specified User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserDispatchTaskResults")]
        public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual DispatchTaskResults for the current User
        /// </summary>
        /// <remarks>
        /// Returns all manual DispatchTaskResults for the current User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("me/DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyDispatchTaskResults")]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetByUserIdAsync(User.GetId(), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTaskResults for a VM
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTaskResults for the specified VM
        /// </remarks>
        /// <returns></returns>
        [HttpGet("vms/{id}/DispatchTaskResults")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getVmDispatchTaskResults")]
        public async Task<IActionResult> GetByVmId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskResultService.GetByVmIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific DispatchTaskResult by id
        /// </summary>
        /// <remarks>
        /// Returns the DispatchTaskResult with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified DispatchTaskResult
        /// </remarks>
        /// <param name="id">The id of the DispatchTaskResult</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("DispatchTaskResults/{id}")]
        [ProducesResponseType(typeof(DispatchTaskResult), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDispatchTaskResult")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var DispatchTaskResult = await _DispatchTaskResultService.GetAsync(id, ct);

            if (DispatchTaskResult == null)
                throw new EntityNotFoundException<DispatchTaskResult>();

            return Ok(DispatchTaskResult);
        }

        /// <summary>
        /// Creates a new DispatchTaskResult
        /// </summary>
        /// <remarks>
        /// Creates a new DispatchTaskResult with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="dispatchTaskResult">The data to create the DispatchTaskResult with</param>
        /// <param name="ct"></param>
        [HttpPost("DispatchTaskResults")]
        [ProducesResponseType(typeof(DispatchTaskResult), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createDispatchTaskResult")]
        public async Task<IActionResult> Create([FromBody] DispatchTaskResult dispatchTaskResult, CancellationToken ct)
        {
            var createdDispatchTaskResult = await _DispatchTaskResultService.CreateAsync(dispatchTaskResult, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdDispatchTaskResult.Id }, createdDispatchTaskResult);
        }

        /// <summary>
        /// Updates an DispatchTaskResult
        /// </summary>
        /// <remarks>
        /// Updates an DispatchTaskResult with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTaskResult
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="dispatchTaskResult">The updated DispatchTaskResult values</param>
        /// <param name="ct"></param>
        [HttpPut("DispatchTaskResults/{id}")]
        [ProducesResponseType(typeof(DispatchTaskResult), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateDispatchTaskResult")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DispatchTaskResult dispatchTaskResult, CancellationToken ct)
        {
            var updatedDispatchTaskResult = await _DispatchTaskResultService.UpdateAsync(id, dispatchTaskResult, ct);
            return Ok(updatedDispatchTaskResult);
        }

        /// <summary>
        /// Deletes an DispatchTaskResult
        /// </summary>
        /// <remarks>
        /// Deletes an DispatchTaskResult with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTaskResult
        /// </remarks>    
        /// <param name="id">The id of the DispatchTaskResult to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("DispatchTaskResults/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteDispatchTaskResult")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _DispatchTaskResultService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


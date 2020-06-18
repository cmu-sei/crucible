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
    public class ResultController : BaseController
    {
        private readonly IResultService _ResultService;
        private readonly IAuthorizationService _authorizationService;

        public ResultController(IResultService ResultService, IAuthorizationService authorizationService)
        {
            _ResultService = ResultService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Result in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Results in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Result>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getResults")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _ResultService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Results for a Scenario
        /// </summary>
        /// <remarks>
        /// Returns all Results for the specified Scenario
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarios/{id}/Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarioResults")]
        public async STT.Task<IActionResult> GetByScenarioId(Guid id, CancellationToken ct)
        {
            var list = await _ResultService.GetByScenarioIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Results for an View
        /// </summary>
        /// <remarks>
        /// Returns all Results for the specified View
        /// </remarks>
        /// <returns></returns>
        [HttpGet("views/{id}/Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getViewResults")]
        public async STT.Task<IActionResult> GetByViewId(Guid id, CancellationToken ct)
        {
            var list = await _ResultService.GetByViewIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual Results for a User
        /// </summary>
        /// <remarks>
        /// Returns all manual Results for the specified User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserResults")]
        public async STT.Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _ResultService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual Results for the current User
        /// </summary>
        /// <remarks>
        /// Returns all manual Results for the current User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("me/Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyResults")]
        public async STT.Task<IActionResult> GetMine(CancellationToken ct)
        {
            var list = await _ResultService.GetByUserIdAsync(User.GetId(), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Results for a VM
        /// </summary>
        /// <remarks>
        /// Returns all Results for the specified VM
        /// </remarks>
        /// <returns></returns>
        [HttpGet("vms/{id}/Results")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getVmResults")]
        public async STT.Task<IActionResult> GetByVmId(Guid id, CancellationToken ct)
        {
            var list = await _ResultService.GetByVmIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Result by id
        /// </summary>
        /// <remarks>
        /// Returns the Result with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Result
        /// </remarks>
        /// <param name="id">The id of the Result</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("Results/{id}")]
        [ProducesResponseType(typeof(SAVM.Result), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getResult")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var Result = await _ResultService.GetAsync(id, ct);

            if (Result == null)
                throw new EntityNotFoundException<SAVM.Result>();

            return Ok(Result);
        }

        /// <summary>
        /// Creates a new Result
        /// </summary>
        /// <remarks>
        /// Creates a new Result with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="result">The data to create the Result with</param>
        /// <param name="ct"></param>
        [HttpPost("Results")]
        [ProducesResponseType(typeof(SAVM.Result), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createResult")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.Result result, CancellationToken ct)
        {
            var createdResult = await _ResultService.CreateAsync(result, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdResult.Id }, createdResult);
        }

        /// <summary>
        /// Updates an Result
        /// </summary>
        /// <remarks>
        /// Updates an Result with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Result
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="result">The updated Result values</param>
        /// <param name="ct"></param>
        [HttpPut("Results/{id}")]
        [ProducesResponseType(typeof(SAVM.Result), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateResult")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.Result result, CancellationToken ct)
        {
            var updatedResult = await _ResultService.UpdateAsync(id, result, ct);
            return Ok(updatedResult);
        }

        /// <summary>
        /// Deletes an Result
        /// </summary>
        /// <remarks>
        /// Deletes an Result with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Result
        /// </remarks>    
        /// <param name="id">The id of the Result to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("Results/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteResult")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _ResultService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


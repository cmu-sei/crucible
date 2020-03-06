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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;
using Steamfitter.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// TODO: we need to figure out authorization for this - but agents checking in won't have access to identity (so as to get a token)
    /// </summary>
    public class ExerciseAgentController : Controller
    {
        private readonly IExerciseAgentService _ExerciseAgentService;
        private readonly IAuthorizationService _authorizationService;

        public ExerciseAgentController(IExerciseAgentService ExerciseAgentService, IAuthorizationService authorizationService)
        {
            _ExerciseAgentService = ExerciseAgentService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all ExerciseAgent in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the ExerciseAgents in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("ExerciseAgents")]
        [ProducesResponseType(typeof(IEnumerable<ExerciseAgent>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseAgents")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _ExerciseAgentService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific ExerciseAgent by id
        /// </summary>
        /// <remarks>
        /// Returns the ExerciseAgent with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified ExerciseAgent
        /// </remarks>
        /// <param name="id">The id of the ExerciseAgent</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("ExerciseAgents/{id}")]
        [ProducesResponseType(typeof(ExerciseAgent), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseAgent")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var ExerciseAgent = await _ExerciseAgentService.GetAsync(id, ct);

            if (ExerciseAgent == null)
                throw new EntityNotFoundException<ExerciseAgent>();

            return Ok(ExerciseAgent);
        }

        /// <summary>
        /// Creates a new ExerciseAgent
        /// </summary>
        /// <remarks>
        /// Creates a new ExerciseAgent with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="exerciseAgent">The data to create the ExerciseAgent with</param>
        /// <param name="ct"></param>
        [HttpPost("ExerciseAgents")]
        [ProducesResponseType(typeof(ExerciseAgent), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createExerciseAgent")]
        public async Task<IActionResult> Create([FromBody] ExerciseAgent exerciseAgent, CancellationToken ct)
        {
            if(exerciseAgent.Id == Guid.Empty)
                exerciseAgent.Id = Guid.NewGuid();
            
            var createdExerciseAgent = await _ExerciseAgentService.CreateAsync(exerciseAgent, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdExerciseAgent.VmWareUuid }, createdExerciseAgent);
        }

        /// <summary>
        /// Updates a ExerciseAgent
        /// </summary>
        /// <remarks>
        /// Updates a ExerciseAgent with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified ExerciseAgent
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="ExerciseAgent">The updated ExerciseAgent values</param>
        /// <param name="ct"></param>
        [HttpPut("ExerciseAgents/{id}")]
        [ProducesResponseType(typeof(ExerciseAgent), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateExerciseAgent")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ExerciseAgent ExerciseAgent, CancellationToken ct)
        {
            var updatedExerciseAgent = await _ExerciseAgentService.UpdateAsync(id, ExerciseAgent, ct);
            return Ok(updatedExerciseAgent);
        }

        /// <summary>
        /// Deletes a ExerciseAgent
        /// </summary>
        /// <remarks>
        /// Deletes a ExerciseAgent with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified ExerciseAgent
        /// </remarks>    
        /// <param name="id">The id of the ExerciseAgent to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("ExerciseAgents/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteExerciseAgent")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _ExerciseAgentService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


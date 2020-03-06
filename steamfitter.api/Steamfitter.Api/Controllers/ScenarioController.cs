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
    public class ScenarioController : BaseController
    {
        private readonly IScenarioService _scenarioService;
        private readonly IAuthorizationService _authorizationService;

        public ScenarioController(IScenarioService scenarioService, IAuthorizationService authorizationService)
        {
            _scenarioService = scenarioService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Scenario in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Scenarios in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("scenarios")]
        [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarios")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _scenarioService.GetAsync(ct);
            return Ok(list);
        }

        // /// <summary>
        // /// Gets all Scenarios for a User
        // /// </summary>
        // /// <remarks>
        // /// Returns all Scenarios where the specified User is a member of at least one of it's Teams
        // /// <para />
        // /// Accessible to a SuperUser or the specified User itself
        // /// </remarks>
        // /// <returns></returns>
        // [HttpGet("users/{id}/scenarios")]
        // [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getUserScenarios")]
        // public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        // {
        //     var list = await _scenarioService.GetByUserIdAsync(id, ct);
        //     return Ok(list);
        // }

        // /// <summary>
        // /// Gets all Scenarios for the current User
        // /// </summary>
        // /// <remarks>
        // /// Returns all Scenarios where the current User is a member of at least one of it's Teams
        // /// <para />
        // /// Accessible only to the current User.
        // /// <para/>
        // /// This is a convenience endpoint and simply returns a 302 redirect to the fully qualified users/{id}/scenarios endpoint 
        // /// </remarks>
        // [HttpGet("me/scenarios")]
        // [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(operationId: "getMyScenarios")]
        // public async Task<IActionResult> GetMy(CancellationToken ct)
        // {
        //     return RedirectToAction(nameof(this.GetByUserId), new { id = User.GetId() });
        // }

        /// <summary>
        /// Gets a specific Scenario by id
        /// </summary>
        /// <remarks>
        /// Returns the Scenario with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Scenario
        /// </remarks>
        /// <param name="id">The id of the Scenario</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("scenarios/{id}")]
        [ProducesResponseType(typeof(Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenario")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var scenario = await _scenarioService.GetAsync(id, ct);

            if (scenario == null)
                throw new EntityNotFoundException<Scenario>();

            return Ok(scenario);
        }

        /// <summary>
        /// Creates a new Scenario
        /// </summary>
        /// <remarks>
        /// Creates a new Scenario with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="scenario">The data to create the Scenario with</param>
        /// <param name="ct"></param>
        [HttpPost("scenarios")]
        [ProducesResponseType(typeof(Scenario), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createScenario")]
        public async Task<IActionResult> Create([FromBody] Scenario scenario, CancellationToken ct)
        {
            scenario.CreatedBy = User.GetId();
            var createdScenario = await _scenarioService.CreateAsync(scenario, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdScenario.Id }, createdScenario);
        }

        /// <summary>
        /// Copies a new Scenario
        /// </summary>
        /// <remarks>
        /// Copies a new Scenario with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="id">The ID of scenario to copy</param>
        /// <param name="ct"></param>
        [HttpPost("scenarios/{id}/copy")]
        [ProducesResponseType(typeof(Scenario), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "copyScenario")]
        public async Task<IActionResult> Copy(Guid id, CancellationToken ct)
        {
            var newScenario = await _scenarioService.CopyAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = newScenario.Id }, newScenario);
        }

        /// <summary>
        /// Updates an Scenario
        /// </summary>
        /// <remarks>
        /// Updates an Scenario with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="scenario">The updated Scenario values</param>
        /// <param name="ct"></param>
        [HttpPut("scenarios/{id}")]
        [ProducesResponseType(typeof(Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateScenario")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Scenario scenario, CancellationToken ct)
        {
            scenario.ModifiedBy = User.GetId();
            var updatedScenario = await _scenarioService.UpdateAsync(id, scenario, ct);
            return Ok(updatedScenario);
        }

        /// <summary>
        /// Deletes an Scenario
        /// </summary>
        /// <remarks>
        /// Deletes an Scenario with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>    
        /// <param name="id">The id of the Scenario to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("scenarios/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteScenario")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _scenarioService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


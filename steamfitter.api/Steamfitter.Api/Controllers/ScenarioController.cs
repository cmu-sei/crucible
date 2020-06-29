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
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using SAVM = Steamfitter.Api.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Steamfitter.Api.Controllers
{
    public class ScenarioController : BaseController
    {
        private readonly IScenarioService _ScenarioService;
        private readonly IAuthorizationService _authorizationService;

        public ScenarioController(IScenarioService ScenarioService, IAuthorizationService authorizationService)
        {
            _ScenarioService = ScenarioService;
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
        [HttpGet("Scenarios")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Scenario>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getScenarios")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _ScenarioService.GetAsync(ct);
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
        // [HttpGet("users/{id}/Scenarios")]
        // [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(OperationId = "getUserScenarios")]
        // public async STT.Task<IActionResult> GetByUserId(int id, CancellationToken ct)
        // {
        //     var list = await _ScenarioService.GetByUserIdAsync(id, ct);
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
        // /// This is a convenience endpoint and simply returns a 302 redirect to the fully qualified users/{id}/Scenarios endpoint 
        // /// </remarks>
        // [HttpGet("me/Scenarios")]
        // [ProducesResponseType(typeof(IEnumerable<Scenario>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(OperationId = "getMyScenarios")]
        // public async STT.Task<IActionResult> GetMy(CancellationToken ct)
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
        [HttpGet("Scenarios/{id}")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getScenario")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var Scenario = await _ScenarioService.GetAsync(id, ct);

            if (Scenario == null)
                throw new EntityNotFoundException<SAVM.Scenario>();

            return Ok(Scenario);
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
        [HttpPost("Scenarios")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "createScenario")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.Scenario scenario, CancellationToken ct)
        {
            var createdScenario = await _ScenarioService.CreateAsync(scenario, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdScenario.Id }, createdScenario);
        }

        /// <summary>
        /// Creates a new Scenario from a ScenarioTemplate
        /// </summary>
        /// <remarks>
        /// Creates a new Scenario from the specified ScenarioTemplate
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="id">The ScenarioTemplate ID to create the Scenario with</param>
        /// <param name="ct"></param>
        [HttpPost("ScenarioTemplates/{id}/Scenarios")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "createScenarioFromScenarioTemplate")]
        public async STT.Task<IActionResult> CreateFromScenarioTemplate(Guid id, CancellationToken ct)
        {
            var createdScenario = await _ScenarioService.CreateFromScenarioTemplateAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdScenario.Id }, createdScenario);
        }

        /// <summary>
        /// Creates a new Scenario from a Scenario
        /// </summary>
        /// <remarks>
        /// Creates a new Scenario from the specified Scenario
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="id">The Scenario ID to copy into a new Scenario</param>
        /// <param name="ct"></param>
        [HttpPost("Scenarios/{id}/Copy")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "copyScenario")]
        public async STT.Task<IActionResult> CopyScenario(Guid id, CancellationToken ct)
        {
            var createdScenario = await _ScenarioService.CreateFromScenarioAsync(id, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdScenario.Id }, createdScenario);
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
        [HttpPut("Scenarios/{id}")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "updateScenario")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.Scenario scenario, CancellationToken ct)
        {
            var updatedScenario = await _ScenarioService.UpdateAsync(id, scenario, ct);
            return Ok(updatedScenario);
        }

        /// <summary>
        /// Start a Scenario
        /// </summary>
        /// <remarks>
        /// Updates a Scenario to active and executes initial Tasks
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>  
        /// <param name="id">The Id of the Scenario to update</param>
        /// <param name="ct"></param>
        [HttpPut("Scenarios/{id}/start")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "startScenario")]
        public async STT.Task<IActionResult> Start([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedScenario = await _ScenarioService.StartAsync(id, ct);
            return Ok(updatedScenario);
        }

        /// <summary>
        /// Pause a Scenario
        /// </summary>
        /// <remarks>
        /// Updates a Scenario to paused
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>  
        /// <param name="id">The Id of the Scenario to update</param>
        /// <param name="ct"></param>
        [HttpPut("Scenarios/{id}/pause")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "pauseScenario")]
        public async STT.Task<IActionResult> Pause([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedScenario = await _ScenarioService.PauseAsync(id, ct);
            return Ok(updatedScenario);
        }

        /// <summary>
        /// Continue a Scenario
        /// </summary>
        /// <remarks>
        /// Updates a Scenario to active
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>  
        /// <param name="id">The Id of the Scenario to update</param>
        /// <param name="ct"></param>
        [HttpPut("Scenarios/{id}/continue")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "continueScenario")]
        public async STT.Task<IActionResult> Continue([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedScenario = await _ScenarioService.ContinueAsync(id, ct);
            return Ok(updatedScenario);
        }

        /// <summary>
        /// End a Scenario
        /// </summary>
        /// <remarks>
        /// Updates a Scenario to ended
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Scenario
        /// </remarks>  
        /// <param name="id">The Id of the Scenario to update</param>
        /// <param name="ct"></param>
        [HttpPut("Scenarios/{id}/end")]
        [ProducesResponseType(typeof(SAVM.Scenario), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "endScenario")]
        public async STT.Task<IActionResult> End([FromRoute] Guid id, CancellationToken ct)
        {
            var updatedScenario = await _ScenarioService.EndAsync(id, ct);
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
        [HttpDelete("Scenarios/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "deleteScenario")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _ScenarioService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


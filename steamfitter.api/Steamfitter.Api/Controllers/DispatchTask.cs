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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Data;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;
using Steamfitter.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    public class DispatchTaskController : BaseController
    {
        private readonly IDispatchTaskService _DispatchTaskService;
        private readonly IAuthorizationService _authorizationService;

        public DispatchTaskController(IDispatchTaskService DispatchTaskService, IAuthorizationService authorizationService)
        {
            _DispatchTaskService = DispatchTaskService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all DispatchTask in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the DispatchTasks in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDispatchTasks")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTasks for a Scenario
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTasks for the specified Scenario
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarios/{id}/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarioDispatchTasks")]
        public async Task<IActionResult> GetByScenarioId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetByScenarioIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTasks for a Session
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTasks for the specified Session
        /// </remarks>
        /// <returns></returns>
        [HttpGet("sessions/{id}/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSessionDispatchTasks")]
        public async Task<IActionResult> GetBySessionId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetBySessionIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTasks for an Exercise
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTasks for the specified Exercise
        /// </remarks>
        /// <returns></returns>
        [HttpGet("exercises/{id}/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getExerciseDispatchTasks")]
        public async Task<IActionResult> GetByExerciseId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetByExerciseIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual DispatchTasks for a User
        /// </summary>
        /// <remarks>
        /// Returns all manual DispatchTasks for the specified User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserDispatchTasks")]
        public async Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTasks for a VM
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTasks for the specified VM
        /// </remarks>
        /// <returns></returns>
        [HttpGet("vms/{id}/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getVmDispatchTasks")]
        public async Task<IActionResult> GetByVmId(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetByVmIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all DispatchTasks for a Trigger Task (Parent)
        /// </summary>
        /// <remarks>
        /// Returns all DispatchTasks for the specified TriggerTask
        /// </remarks>
        /// <returns></returns>
        [HttpGet("DispatchTasks/{id}/subtasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSubtasks")]
        public async Task<IActionResult> GetSubtasks(Guid id, CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetSubtasksAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual DispatchTasks for the current User
        /// </summary>
        /// <remarks>
        /// Returns all manual DispatchTasks for the current User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("me/DispatchTasks")]
        [ProducesResponseType(typeof(IEnumerable<DispatchTask>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyDispatchTasks")]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var list = await _DispatchTaskService.GetByUserIdAsync(User.GetId(), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific DispatchTask by id
        /// </summary>
        /// <remarks>
        /// Returns the DispatchTask with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified DispatchTask
        /// </remarks>
        /// <param name="id">The id of the DispatchTask</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("DispatchTasks/{id}")]
        [ProducesResponseType(typeof(DispatchTask), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getDispatchTask")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var DispatchTask = await _DispatchTaskService.GetAsync(id, ct);
            return Ok(DispatchTask);
        }

        /// <summary>
        /// Creates a new DispatchTask
        /// </summary>
        /// <remarks>
        /// Creates a new DispatchTask with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="dispatchTask">The data to create the DispatchTask with</param>
        /// <param name="ct"></param>
        [HttpPost("DispatchTasks")]
        [ProducesResponseType(typeof(DispatchTask), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createDispatchTask")]
        public async Task<IActionResult> Create([FromBody] DispatchTask dispatchTask, CancellationToken ct)
        {
            var createdDispatchTask = await _DispatchTaskService.CreateAsync(dispatchTask, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdDispatchTask.Id }, createdDispatchTask);
        }

        /// <summary>
        /// Copies a DispatchTask
        /// </summary>
        /// <remarks>
        /// Copies a DispatchTask to the location specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTask
        /// </remarks>  
        /// <param name="id">The Id of the DispatchTask to copy</param>
        /// <param name="newLocation">The Id and type of the new location</param>
        /// <param name="ct"></param>
        [HttpPost("DispatchTasks/{id}/copy")]
        [ProducesResponseType(typeof(DispatchTask[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "copyDispatchTask")]
        public async Task<IActionResult> Copy([FromRoute] Guid id, [FromBody] NewLocation newLocation, CancellationToken ct)
        {
            var taskWithSubtasks = await _DispatchTaskService.CopyAsync(id, newLocation.Id, newLocation.LocationType, ct);
            return Ok(taskWithSubtasks);
        }

        /// <summary>
        /// Creates a new DispatchTask and executes it
        /// </summary>
        /// <remarks>
        /// Creates a new DispatchTask with the attributes specified and executes it
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="dispatchTask">The data to create the DispatchTask with</param>
        /// <param name="ct"></param>
        [HttpPost("DispatchTasks/execute")]
        [ProducesResponseType(typeof(DispatchTaskResult[]), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createAndExecuteDispatchTask")]
        public async Task<IActionResult> CreateAndExecute([FromBody] DispatchTask dispatchTask, CancellationToken ct)
        {
            var resultList = await _DispatchTaskService.CreateAndExecuteAsync(dispatchTask, ct);
            return Ok(resultList);
        }

        /// <summary>
        /// Executes a specific DispatchTask by id
        /// </summary>
        /// <remarks>
        /// Executes the DispatchTask with the id specified
        /// <para />
        /// Accessible to a SuperUser or administrator
        /// </remarks>
        /// <param name="id">The id of the DispatchTask</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("DispatchTasks/{id}/execute")]
        [ProducesResponseType(typeof(DispatchTaskResult[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "executeDispatchTask")]
        public async Task<IActionResult> Execute(Guid id, CancellationToken ct)
        {
            var resultList = await _DispatchTaskService.ExecuteAsync(id, ct);
            return Ok(resultList);
        }

        /// <summary>
        /// Updates a DispatchTask
        /// </summary>
        /// <remarks>
        /// Updates a DispatchTask with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTask
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="dispatchTask">The updated DispatchTask values</param>
        /// <param name="ct"></param>
        [HttpPut("DispatchTasks/{id}")]
        [ProducesResponseType(typeof(DispatchTask), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateDispatchTask")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DispatchTask dispatchTask, CancellationToken ct)
        {
            var updatedDispatchTask = await _DispatchTaskService.UpdateAsync(id, dispatchTask, ct);
            return Ok(updatedDispatchTask);
        }

        /// <summary>
        /// Moves a DispatchTask
        /// </summary>
        /// <remarks>
        /// Moves a DispatchTask to the location specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTask
        /// </remarks>  
        /// <param name="id">The Id of the DispatchTask to move</param>
        /// <param name="newLocation">The Id and type of the new location</param>
        /// <param name="ct"></param>
        [HttpPut("DispatchTasks/{id}/move")]
        [ProducesResponseType(typeof(DispatchTask[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "moveDispatchTask")]
        public async Task<IActionResult> Move([FromRoute] Guid id, [FromBody] NewLocation newLocation, CancellationToken ct)
        {
            var taskWithSubtasks = await _DispatchTaskService.MoveAsync(id, newLocation.Id, newLocation.LocationType, ct);
            return Ok(taskWithSubtasks);
        }

        /// <summary>
        /// Deletes a DispatchTask
        /// </summary>
        /// <remarks>
        /// Deletes a DispatchTask with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified DispatchTask
        /// </remarks>    
        /// <param name="id">The id of the DispatchTask to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("DispatchTasks/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteDispatchTask")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _DispatchTaskService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Gets all possible DispatchTask commands
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the DispatchTask commands.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("DispatchTasks/commands")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getAvailableCommands")]
        public async Task<IActionResult> GetAvailableCommands(CancellationToken ct)
        {
            return Ok(System.IO.File.ReadAllText(@"availableCommands.json"));
        }

    }

    public class NewLocation
    {
        public Guid Id { get; set; }
        public string LocationType { get; set; }
    }
}


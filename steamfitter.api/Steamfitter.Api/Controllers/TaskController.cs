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
using STT = System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Services;
using SAVM = Steamfitter.Api.ViewModels;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    public class TaskController : BaseController
    {
        private readonly ITaskService _TaskService;
        private readonly IAuthorizationService _authorizationService;

        public TaskController(ITaskService TaskService, IAuthorizationService authorizationService)
        {
            _TaskService = TaskService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Task in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Tasks in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTasks")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _TaskService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Tasks for a ScenarioTemplate
        /// </summary>
        /// <remarks>
        /// Returns all Tasks for the specified ScenarioTemplate
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarioTemplates/{id}/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarioTemplateTasks")]
        public async STT.Task<IActionResult> GetByScenarioTemplateId(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetByScenarioTemplateIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Tasks for a Scenario
        /// </summary>
        /// <remarks>
        /// Returns all Tasks for the specified Scenario
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarios/{id}/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getScenarioTasks")]
        public async STT.Task<IActionResult> GetByScenarioId(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetByScenarioIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Tasks for an View
        /// </summary>
        /// <remarks>
        /// Returns all Tasks for the specified View
        /// </remarks>
        /// <returns></returns>
        [HttpGet("views/{id}/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getViewTasks")]
        public async STT.Task<IActionResult> GetByViewId(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetByViewIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual Tasks for a User
        /// </summary>
        /// <remarks>
        /// Returns all manual Tasks for the specified User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("users/{id}/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getUserTasks")]
        public async STT.Task<IActionResult> GetByUserId(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetByUserIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Tasks for a VM
        /// </summary>
        /// <remarks>
        /// Returns all Tasks for the specified VM
        /// </remarks>
        /// <returns></returns>
        [HttpGet("vms/{id}/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getVmTasks")]
        public async STT.Task<IActionResult> GetByVmId(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetByVmIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all Tasks for a Trigger Task (Parent)
        /// </summary>
        /// <remarks>
        /// Returns all Tasks for the specified TriggerTask
        /// </remarks>
        /// <returns></returns>
        [HttpGet("Tasks/{id}/subtasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getSubtasks")]
        public async STT.Task<IActionResult> GetSubtasks(Guid id, CancellationToken ct)
        {
            var list = await _TaskService.GetSubtasksAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all manual Tasks for the current User
        /// </summary>
        /// <remarks>
        /// Returns all manual Tasks for the current User
        /// </remarks>
        /// <returns></returns>
        [HttpGet("me/Tasks")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.Task>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getMyTasks")]
        public async STT.Task<IActionResult> GetMine(CancellationToken ct)
        {
            var list = await _TaskService.GetByUserIdAsync(User.GetId(), ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific Task by id
        /// </summary>
        /// <remarks>
        /// Returns the Task with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified Task
        /// </remarks>
        /// <param name="id">The id of the STT.Task</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("Tasks/{id}")]
        [ProducesResponseType(typeof(SAVM.Task), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getTask")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var Task = await _TaskService.GetAsync(id, ct);
            return Ok(Task);
        }

        /// <summary>
        /// Creates a new Task
        /// </summary>
        /// <remarks>
        /// Creates a new Task with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="task">The data to create the Task with</param>
        /// <param name="ct"></param>
        [HttpPost("Tasks")]
        [ProducesResponseType(typeof(SAVM.Task), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createTask")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.Task task, CancellationToken ct)
        {
            var createdTask = await _TaskService.CreateAsync(task, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdTask.Id }, createdTask);
        }

        /// <summary>
        /// Copies a Task
        /// </summary>
        /// <remarks>
        /// Copies a Task to the location specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Task
        /// </remarks>  
        /// <param name="id">The Id of the Task to copy</param>
        /// <param name="newLocation">The Id and type of the new location</param>
        /// <param name="ct"></param>
        [HttpPost("Tasks/{id}/copy")]
        [ProducesResponseType(typeof(SAVM.Task[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "copyTask")]
        public async STT.Task<IActionResult> Copy([FromRoute] Guid id, [FromBody] NewLocation newLocation, CancellationToken ct)
        {
            var taskWithSubtasks = await _TaskService.CopyAsync(id, newLocation.Id, newLocation.LocationType, ct);
            return Ok(taskWithSubtasks);
        }

        /// <summary>
        /// Creates a new Task and executes it
        /// </summary>
        /// <remarks>
        /// Creates a new Task with the attributes specified and executes it
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="task">The data to create the Task with</param>
        /// <param name="ct"></param>
        [HttpPost("Tasks/execute")]
        [ProducesResponseType(typeof(SAVM.Result[]), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createAndExecuteTask")]
        public async STT.Task<IActionResult> CreateAndExecute([FromBody] SAVM.Task task, CancellationToken ct)
        {
            var resultList = await _TaskService.CreateAndExecuteAsync(task, ct);
            return Ok(resultList);
        }

        /// <summary>
        /// Executes a specific Task by id
        /// </summary>
        /// <remarks>
        /// Executes the Task with the id specified
        /// <para />
        /// Accessible to a SuperUser or administrator
        /// </remarks>
        /// <param name="id">The id of the STT.Task</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("Tasks/{id}/execute")]
        [ProducesResponseType(typeof(SAVM.Result[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "executeTask")]
        public async STT.Task<IActionResult> Execute(Guid id, CancellationToken ct)
        {
            var resultList = await _TaskService.ExecuteAsync(id, ct);
            return Ok(resultList);
        }

        /// <summary>
        /// Updates a Task
        /// </summary>
        /// <remarks>
        /// Updates a Task with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Task
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="task">The updated Task values</param>
        /// <param name="ct"></param>
        [HttpPut("Tasks/{id}")]
        [ProducesResponseType(typeof(SAVM.Task), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateTask")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.Task task, CancellationToken ct)
        {
            var updatedTask = await _TaskService.UpdateAsync(id, task, ct);
            return Ok(updatedTask);
        }

        /// <summary>
        /// Moves a Task
        /// </summary>
        /// <remarks>
        /// Moves a Task to the location specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Task
        /// </remarks>  
        /// <param name="id">The Id of the Task to move</param>
        /// <param name="newLocation">The Id and type of the new location</param>
        /// <param name="ct"></param>
        [HttpPut("Tasks/{id}/move")]
        [ProducesResponseType(typeof(SAVM.Task[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "moveTask")]
        public async STT.Task<IActionResult> Move([FromRoute] Guid id, [FromBody] NewLocation newLocation, CancellationToken ct)
        {
            var taskWithSubtasks = await _TaskService.MoveAsync(id, newLocation.Id, newLocation.LocationType, ct);
            return Ok(taskWithSubtasks);
        }

        /// <summary>
        /// Deletes a Task
        /// </summary>
        /// <remarks>
        /// Deletes a Task with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified Task
        /// </remarks>    
        /// <param name="id">The id of the Task to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("Tasks/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteTask")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _TaskService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Gets all possible Task commands
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Task commands.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("Tasks/commands")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getAvailableCommands")]
        public async STT.Task<IActionResult> GetAvailableCommands(CancellationToken ct)
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


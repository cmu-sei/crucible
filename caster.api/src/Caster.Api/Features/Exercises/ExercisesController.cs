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
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caster.Api.Features.Exercises
{
    [Route("api/exercises")]
    [ApiController]
    [Authorize]
    public class ExercisesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExercisesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get a single exercise.
        /// </summary>
        /// <param name="id">ID of an exercise.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Export a single exercise.
        /// </summary>
        /// <param name="id">ID of an exercise.</param>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet("exercises/{id}/actions/export")]
        [ProducesResponseType(typeof(FileResult), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "ExportExercise")]
        public async Task<IActionResult> Export([FromRoute] Guid id, [FromQuery] Export.Query query)
        {
            query.Id = id;
            var result = await _mediator.Send(query);
            return File(result.Data, result.Type, result.Name);
        }

        /// <summary>
        /// Import an exercise.
        /// </summary>
        /// <param name="id">ID of an exercise.</param>
        /// <param name="command"></param>
        [HttpPost("exercises/{id}/actions/import")]
        [ProducesResponseType(typeof(Import.ImportExerciseResult), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "ImportExercise")]
        public async Task<IActionResult> Import([FromRoute] Guid id, [FromQuery] Import.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Get all exercises.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<Exercise>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetAllExercises")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Create a new exercise.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateExercise")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update an exercise.
        /// </summary>
        /// <param name="id">ID of an exercise.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Exercise), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "EditExercise")]
        public async Task<IActionResult> Edit([FromRoute] Guid id, Edit.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete an exercise.
        /// </summary>
        /// <param name="id">ID of an exercise.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteExercise")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _mediator.Send(new Delete.Command { Id = id });
            return NoContent();
        }
    }
}

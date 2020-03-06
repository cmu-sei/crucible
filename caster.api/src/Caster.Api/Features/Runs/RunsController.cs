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
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caster.Api.Features.Runs
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class RunsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RunsController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get a Run by Id
        /// </summary>
        /// <param name="id">The Id of the Run to retrieve</param>
        /// <param name="query"></param>
        [HttpGet("runs/{id}")]
        [ProducesResponseType(typeof(Run), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetRun")]
        public async Task<IActionResult> Get([FromRoute] Guid id, [FromQuery] Get.Query query)
        {
            query.Id = id;
            var result = await this._mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get a list of Runs for a specified Workspace
        /// </summary>
        /// <param name="workspaceId">The Id of a Workspace</param>
        /// <param name="query"></param>
        [HttpGet("workspaces/{workspaceId}/runs")]
        [ProducesResponseType(typeof(IEnumerable<Run>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetRunsByWorkspaceId")]
        public async Task<IActionResult> GetByWorkspace([FromRoute] Guid workspaceId, [FromQuery] GetByWorkspace.Query query)
        {
            query.WorkspaceId = workspaceId;
            var result = await this._mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new Run
        /// </summary>
        /// <param name="command">The Create command</param>
        /// <returns></returns>
        [HttpPost("runs")]
        [ProducesResponseType(typeof(Run), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateRun")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await this._mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Reject a Run, preventing it from being Applied
        /// </summary>
        /// <param name="id">The Id of the Run to reject</param>
        /// <returns></returns>
        [HttpPost("runs/{id}/actions/reject")]
        [ProducesResponseType(typeof(Run), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "RejectRun")]
        public async Task<IActionResult> Reject([FromRoute] Guid id)
        {
            var result = await this._mediator.Send(new Reject.Command { Id = id });
            return Ok(result);
        }
    }
}


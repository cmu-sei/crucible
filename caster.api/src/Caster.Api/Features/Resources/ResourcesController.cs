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

namespace Caster.Api.Features.Resources
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResourcesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get a single resource in a Workspace.
        /// </summary>
        [HttpGet("workspaces/{workspaceId}/resources/{id}")]
        [ProducesResponseType(typeof(Resource), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetResource")]
        public async Task<IActionResult> Get([FromRoute] Guid workspaceId, [FromRoute] string id, [FromQuery] Get.Query query)
        {
            query.WorkspaceId = workspaceId;
            query.Id = id;
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get all resources in a Workspace.
        /// </summary>
        [HttpGet("workspaces/{workspaceId}/resources")]
        [ProducesResponseType(typeof(Resource[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetResourcesByWorkspace")]
        public async Task<IActionResult> GetByWorkspace([FromRoute] Guid workspaceId)
        {
            var result = await _mediator.Send(new GetByWorkspace.Query { WorkspaceId = workspaceId });
            return Ok(result);
        }

        /// <summary>
        /// Taint selected Resources
        /// </summary>
        [HttpPost("workspaces/{workspaceId}/resources/actions/taint")]
        [ProducesResponseType(typeof(Resource[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "TaintResources")]
        public async Task<IActionResult> Taint([FromRoute] Guid workspaceId, [FromBody] Taint.Command command)
        {
            command.WorkspaceId = workspaceId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Untaint selected Resources
        /// </summary>
        [HttpPost("workspaces/{workspaceId}/resources/actions/untaint")]
        [ProducesResponseType(typeof(Resource[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "UntaintResources")]
        public async Task<IActionResult> Untaint([FromRoute] Guid workspaceId, [FromBody] Taint.Command command)
        {
            command.WorkspaceId = workspaceId;
            command.Untaint = true;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Refresh the Workspace's Resources
        /// </summary>
        [HttpPost("workspaces/{workspaceId}/resources/actions/refresh")]
        [ProducesResponseType(typeof(Resource[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "RefreshResources")]
        public async Task<IActionResult> Refresh([FromRoute] Guid workspaceId)
        {
            var result = await _mediator.Send(new Refresh.Command{ WorkspaceId = workspaceId });
            return Ok(result);
        }
    }
}


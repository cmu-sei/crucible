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

namespace Caster.Api.Features.Hosts
{
    [Route("api/hosts")]
    [ApiController]
    [Authorize]
    public class HostsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HostsController(IMediator mediator) 
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get a single Host.
        /// </summary>
        /// <param name="id">ID of an Host.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Host), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetHost")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return Ok(result);
        }
        
        /// <summary>
        /// Get all Hosts.
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(IEnumerable<Host>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetAllHosts")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Create a new Host.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(typeof(Host), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateHost")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update an Host.
        /// </summary>
        /// <param name="id">ID of an Host.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Host), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "EditHost")]
        public async Task<IActionResult> Edit([FromRoute] Guid id, Edit.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete an Host.
        /// </summary>
        /// <param name="id">ID of an Host.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteHost")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _mediator.Send(new Delete.Command { Id = id });
            return NoContent();
        }
    }
}


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

namespace Caster.Api.Features.Modules
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class ModulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ModulesController(IMediator mediator) 
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get a Module by Id
        /// </summary>
        /// <param name="id">The Id of the Module to retrieve</param>
        [HttpGet("modules/{id}")]
        [ProducesResponseType(typeof(Module), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetModule")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var result = await this._mediator.Send(new Get.Query { Id = Guid.Parse(id) });
            return Ok(result);
        }

        /// <summary>
        /// Get all available Modules
        /// </summary>
        /// <param name="query"></param>
        [HttpGet("modules")]
        [ProducesResponseType(typeof(IEnumerable<Module>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetAllModules")]
        public async Task<IActionResult> GetAll([FromQuery] GetAll.Query query)
        {
            var result = await this._mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new terraform module.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("modules")]
        [ProducesResponseType(typeof(Module), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "CreateTerrraformModule")]
        public async Task<IActionResult> Create(Create.Command command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }

        /// <summary>
        /// Create or update a terraform module from the repository.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("repository/modules")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "CreateTerrraformModuleFromRepository")]
        public async Task<IActionResult> CreateFromRepository(CreateFromRepository.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// Create a terraform module snippet.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("modules/snippet")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "CreateSnippet")]
        public async Task<IActionResult> CreateSnippet(CreateSnippet.Command command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Delete a module.
        /// </summary>
        /// <param name="id">ID of a module.</param>
        /// <returns></returns>
        [HttpDelete("modules/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "DeleteModule")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            await _mediator.Send(new Delete.Command { Id = Guid.Parse(id) });
            return NoContent();
        }

        /// <summary>
        /// Get a Module's Versions
        /// </summary>
        /// <param name="id">The Id of the Module to retrieve</param>
        [HttpGet("modules/{id}/versions")]
        [ProducesResponseType(typeof(IEnumerable<ModuleVersion>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetModuleVersions")]
        public async Task<IActionResult> GetModuleVersions([FromRoute] string id)
        {
            var result = await this._mediator.Send(new GetVersions.Query { ModuleId = Guid.Parse(id) });
            return Ok(result);
        }

    }

}


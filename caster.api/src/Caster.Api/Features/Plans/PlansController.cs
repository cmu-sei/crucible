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
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caster.Api.Features.Plans
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class PlansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlansController(IMediator mediator) 
        {
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get a Plan by Id
        /// </summary>
        /// <param name="id">The Id of the Plan to retrieve</param>
        [HttpGet("plans/{id}")]
        [ProducesResponseType(typeof(Plan), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetPlan")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await this._mediator.Send(new Get.Query { Id = id });
            return Ok(result);
        }

        /// <summary>
        /// Get a Plan by Run Id
        /// </summary>
        /// <param name="runId">The Id of the Run</param>
        [HttpGet("runs/{runId}/plan")]
        [ProducesResponseType(typeof(Plan), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "GetPlanByRunId")]
        public async Task<IActionResult> GetByRun([FromRoute] Guid runId)
        {
            var result = await this._mediator.Send(new GetByRun.Query { RunId = runId });
            return Ok(result);
        }
    }
}


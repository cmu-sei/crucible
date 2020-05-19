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
using System.Threading;
using System.Threading.Tasks;
using Foreman.Api.ViewModels;
using Foreman.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Foreman.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [Produces("application/json")]
    [Route("api/scheduleditems")]
    [ApiController]
    public class ScheduledItems : BaseController
    {
        private ISchedulerService _service;

        public ScheduledItems(ISchedulerService service)
        {
            this._service = service;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("listSchedules")]
        public IActionResult Get(CancellationToken ct)
        {
            return Ok(this._service.List(ct));
        }
        
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("getScheduleByName")]
        public async Task<IActionResult> GetByName([FromRoute] Guid name, string groupName, CancellationToken ct)
        {
            return Ok(this._service.GetByName(name, groupName, ct));
        }
        
        [HttpGet("current")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("getCurrent")]
        public async Task<IActionResult> Current(CancellationToken ct)
        {
            return Ok(this._service.GetCurrent(ct));
        }
        
        [HttpGet("clear")]
        [ProducesResponseType(typeof(NoContentResult), (int)HttpStatusCode.NoContent)]
        [SwaggerOperation("clear")]
        public async Task<IActionResult> Clear(CancellationToken ct)
        {
            this._service.Clear(ct);
            return NoContent();
        }
    }
}

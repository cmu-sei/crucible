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
using System.Threading.Tasks;
using Foreman.Api.ViewModels;
using Foreman.Core.Models;
using Foreman.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Foreman.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    [Produces("application/json")]
    [Route("api/webhooks")]
    [ApiController]
    public class WebHooksController : BaseController
    {
        private readonly IWebHookRepositoryService _service;
        
        //TODO this is a lift and shift from ghosts api — need to move this to the service model we use in s3/BT projects

        public WebHooksController(IWebHookRepositoryService service)
        {
            this._service = service;
        }

        // GET: api/WebHooks
        [HttpGet]
        [ProducesResponseType(typeof(Task<IEnumerable<WebHook>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("listWebHooks")]
        public async Task<IEnumerable<WebHook>> List(CancellationToken ct)
        {
            return await this._service.Get(ct);
        }

        // GET: api/WebHooks/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Task<WebHook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("getWebHookById")]
        public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var webHook = await this._service.GetById(id, ct);

            return webHook == null ? (IActionResult) NotFound() : Ok(webHook);
        }

        // PUT: api/WebHooks/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Task<WebHook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("updateWebHook")]
        public async Task<WebHook> Update(Guid id, WebHook webHook, CancellationToken ct)
        {
            return await this._service.Update(webHook, ct);
        }

        // POST: api/WebHooks
        [HttpPost]
        [ProducesResponseType(typeof(Task<WebHook>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("createWebHook")]
        public async Task<WebHook> Create(WebHook webHook, CancellationToken ct)
        {
            return await this._service.Create(webHook, ct);
        }

        // DELETE: api/WebHooks/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)] 
        [SwaggerOperation("deleteWebHook")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            await this._service.Delete(id, ct);
            return NoContent();
        }
    }
}

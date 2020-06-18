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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.Services;
using Alloy.Api.ViewModels;

namespace Alloy.Api.Controllers
{
    public class EventTemplateController : BaseController
    {
        private readonly IEventTemplateService _eventTemplateService;
        private readonly IAuthorizationService _authorizationService;

        public EventTemplateController(IEventTemplateService eventTemplateService, IAuthorizationService authorizationService)
        {
            _eventTemplateService = eventTemplateService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all EventTemplate in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the EventTemplates in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("eventTemplates")]
        [ProducesResponseType(typeof(IEnumerable<EventTemplate>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getEventTemplates")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _eventTemplateService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific EventTemplate by id
        /// </summary>
        /// <remarks>
        /// Returns the EventTemplate with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified EventTemplate
        /// </remarks>
        /// <param name="id">The id of the EventTemplate</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("eventTemplates/{id}")]
        [ProducesResponseType(typeof(EventTemplate), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(OperationId = "getEventTemplate")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var eventTemplate = await _eventTemplateService.GetAsync(id, ct);

            if (eventTemplate == null)
                throw new EntityNotFoundException<EventTemplate>();

            return Ok(eventTemplate);
        }

        /// <summary>
        /// Creates a new EventTemplate
        /// </summary>
        /// <remarks>
        /// Creates a new EventTemplate with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="eventTemplate">The data to create the EventTemplate with</param>
        /// <param name="ct"></param>
        [HttpPost("eventTemplates")]
        [ProducesResponseType(typeof(EventTemplate), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "createEventTemplate")]
        public async Task<IActionResult> Create([FromBody] EventTemplate eventTemplate, CancellationToken ct)
        {
            var createdEventTemplate = await _eventTemplateService.CreateAsync(eventTemplate, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdEventTemplate.Id }, createdEventTemplate);
        }

        /// <summary>
        /// Updates an EventTemplate
        /// </summary>
        /// <remarks>
        /// Updates an EventTemplate with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified EventTemplate
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="eventTemplate">The updated EventTemplate values</param>
        /// <param name="ct"></param>
        [HttpPut("eventTemplates/{id}")]
        [ProducesResponseType(typeof(EventTemplate), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(OperationId = "updateEventTemplate")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EventTemplate eventTemplate, CancellationToken ct)
        {
            var updatedEventTemplate = await _eventTemplateService.UpdateAsync(id, eventTemplate, ct);
            return Ok(updatedEventTemplate);
        }

        /// <summary>
        /// Deletes an EventTemplate
        /// </summary>
        /// <remarks>
        /// Deletes an EventTemplate with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified EventTemplate
        /// </remarks>    
        /// <param name="id">The id of the EventTemplate to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("eventTemplates/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(OperationId = "deleteEventTemplate")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _eventTemplateService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


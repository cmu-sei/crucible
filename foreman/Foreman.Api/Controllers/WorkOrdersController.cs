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
using Foreman.Core;
using Foreman.Core.Models;
using Foreman.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Foreman.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/workorders")]
    [ApiController]
    public class WorkOrdersController : BaseController
    {
        private readonly IWorkOrderRepositoryService _service;
        
        public WorkOrdersController(IWorkOrderRepositoryService service)
        {
            this._service = service;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(Task<IEnumerable<WorkOrder>>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("listWorkOrders")]
        public Task<IEnumerable<WorkOrder>> List(CancellationToken ct)
        {
            return this._service.List(ct);
        }
        
        [HttpGet("bystatus")]
        [ProducesResponseType(typeof(Task<IEnumerable<WorkOrder>>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("listWorkOrdersByStatus")]
        public Task<IEnumerable<WorkOrder>> ListByStatus(StatusType status, CancellationToken ct)
        {
            return this._service.ListByStatus(status, ct);
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Task<IEnumerable<WorkOrder>>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("getWorkOrderbyId")]
        public Task<WorkOrder> Get(Guid id, CancellationToken ct)
        {
            return this._service.GetById(id, ct);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("CreateWorkOrder")]
        public async Task<WorkOrder> Create(WorkOrder workOrder, CancellationToken ct)
        {
            await this._service.Create(workOrder, ct);
            return workOrder;
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("updateWorkOrder")]
        public async Task<WorkOrder> Update(WorkOrder workOrder, CancellationToken ct)
        {
            await this._service.Update(workOrder, ct);
            return workOrder;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation("deleteWorkOrder")]
        public IActionResult Delete(Guid id, CancellationToken ct)
        {
            this._service.Delete(id, ct);
            return NoContent();
        }
    }
}

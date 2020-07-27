/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Player.Vm.Api.Domain.Vsphere.Models;
using MediatR;

namespace Player.Vm.Api.Features.Vms
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class VmsController : ControllerBase
    {
        private readonly IVmService _vmService;
        private readonly IMediator _mediator;

        public VmsController(IVmService vmService, IMediator mediator)
        {
            _vmService = vmService;
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieve all Vms
        /// </summary>
        /// <returns></returns>
        [HttpGet("vms")]
        [ProducesResponseType(typeof(IEnumerable<Vm>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getAll")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var vms = await _vmService.GetAllAsync(ct);

            return Ok(vms);
        }

        /// <summary>
        /// Retrieve a single Vm by Id
        /// </summary>
        /// <param name="id">The Id of the Vm</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("vms/{id}")]
        [ProducesResponseType(typeof(Vm), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Vm), (int)HttpStatusCode.NotFound)]
        [SwaggerOperation(OperationId = "getVm")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var vm = await _vmService.GetAsync(id, ct);

            if (vm == null)
                return NotFound(vm);

            return Ok(vm);
        }

        /// <summary>
        /// Gets all of the Vms in the specified Team
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="name">An optional search term for the vm's name</param>
        /// <param name="includePersonal"></param>
        /// <param name="onlyMine"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("teams/{teamId}/vms")]
        [ProducesResponseType(typeof(IEnumerable<Vm>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getTeamVms")]
        public async Task<IActionResult> GetByTeamId(Guid teamId, string name, bool? includePersonal, bool? onlyMine, CancellationToken ct)
        {
            var vms = await _vmService.GetByTeamIdAsync(teamId,
                name,
                includePersonal.HasValue ? includePersonal.Value : false,
                onlyMine.HasValue ? onlyMine.Value : false,
                ct);
            return Ok(vms);
        }

        /// <summary>
        /// Gets all of the Vms the calling user has access to in the specified View
        /// </summary>
        /// <param name="viewId">The Id of the View</param>
        /// <param name="name">An optional search term for the vm's name</param>
        /// <param name="includePersonal"></param>
        /// <param name="onlyMine"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("views/{viewId}/vms")]
        [ProducesResponseType(typeof(IEnumerable<Vm>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getViewVms")]
        public async Task<IActionResult> GetByViewId([FromRoute] Guid viewId, [FromQuery] string name, bool? includePersonal, bool? onlyMine, CancellationToken ct)
        {
            var vms = await _vmService.GetByViewIdAsync(viewId,
                name,
                includePersonal.HasValue ? includePersonal.Value : false,
                onlyMine.HasValue ? onlyMine.Value : false,
                ct);
            return Ok(vms);
        }

        /// <summary>
        /// Creates a new Virtual Machine
        /// </summary>
        /// <remarks>
        /// Creates a new Virtual Machine with the attributes specified
        /// <para />
        /// Accessible to a User with management permissions on a team the Virtual Machine will be added to
        /// </remarks>
        /// <param name="form">The data to create the Team with</param>
        /// <param name="ct"></param>
        [HttpPost("vms")]
        [ProducesResponseType(typeof(Vm), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "createVm")]
        public async Task<IActionResult> Create([FromBody] VmCreateForm form, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                throw new InvalidOperationException();

            var createdVm = await _vmService.CreateAsync(form, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdVm.Id }, createdVm);
        }

        /// <summary>
        /// Updates a new Virtual Machine
        /// </summary>
        /// <remarks>
        /// Updates a Virtual Machine with the attributes specified
        /// <para />
        /// Accessible to a User with management permissions on a team the Virtual Machine is in
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="form">The data to update the Virtual Machine with</param>
        /// <param name="ct"></param>
        [HttpPut("vms/{id}")]
        [ProducesResponseType(typeof(Vm), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "updateVm")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] VmUpdateForm form, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                throw new InvalidOperationException();

            var updatedVm = await _vmService.UpdateAsync(id, form, ct);
            return Ok(updatedVm);
        }

        /// <summary>
        /// Deletes a Vm
        /// </summary>
        /// <remarks>
        /// Deletes a Vm with the specified id
        /// <para />
        /// Accessible to a User with management permission on a team the Virtual Machine is in
        /// </remarks>
        /// <param name="id">The id of the Vm</param>
        /// <param name="ct"></param>
        [HttpDelete("vms/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "deleteVm")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _vmService.DeleteAsync(id, ct);
            return NoContent();
        }

        /// <summary>
        /// Adds a Virtual Machine to a Team
        /// </summary>
        /// <remarks>
        /// Creates a new Virtual Machine with the attributes specified
        /// <para />
        /// Accessible to a User with management permissions on a team the Virtual Machine will be added to
        /// </remarks>
        /// <param name="vmId">The id of the Virtual Machine</param>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="ct"></param>
        [HttpPost("teams/{teamId}/vms/{vmId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "addVmToTeam")]
        public async Task<IActionResult> AddToTeam([FromRoute] Guid vmId, [FromRoute] Guid teamId, CancellationToken ct)
        {
            await _vmService.AddToTeamAsync(vmId, teamId, ct);
            return Ok();
        }

        /// <summary>
        /// Removes a Virtual Machine from a Team
        /// </summary>
        /// <remarks>
        /// Removes the specified Virtual Machine from the specified Team
        /// <para />
        /// Accessible to a User with management permissions on a team the Virtual Machine will be added to
        /// </remarks>
        /// <param name="vmId">The id of the Virtual Machine</param>
        /// <param name="teamId">The id of the Team</param>
        /// <param name="ct"></param>
        [HttpDelete("teams/{teamId}/vms/{vmId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "removeVmFromTeam")]
        public async Task<IActionResult> RemoveFromTeam([FromRoute] Guid vmId, [FromRoute] Guid teamId, CancellationToken ct)
        {
            await _vmService.RemoveFromTeamAsync(vmId, teamId, ct);
            return NoContent();
        }

        /// <summary>
        /// Power on multiple Virtual Machines
        /// </summary>
        [HttpPost("vms/actions/power-on")]
        [ProducesResponseType(typeof(BulkPowerOperation.Response), (int)HttpStatusCode.Accepted)]
        [SwaggerOperation(OperationId = "bulkPowerOn")]
        public async Task<IActionResult> BulkPowerOn(BulkPowerOperation.Command command)
        {
            command.Operation = PowerOperation.PowerOn;
            var result = await _mediator.Send(command);
            return Accepted(result);
        }

        /// <summary>
        /// Power off multiple Virtual Machines
        /// </summary>
        [HttpPost("vms/actions/power-off")]
        [ProducesResponseType(typeof(BulkPowerOperation.Response), (int)HttpStatusCode.Accepted)]
        [SwaggerOperation(OperationId = "bulkPowerOff")]
        public async Task<IActionResult> BulkPowerOff(BulkPowerOperation.Command command)
        {
            command.Operation = PowerOperation.PowerOff;
            var result = await _mediator.Send(command);
            return Accepted(result);
        }

        /// <summary>
        /// Shutdown multiple Virtual Machines
        /// </summary>
        [HttpPost("vms/actions/shutdown")]
        [ProducesResponseType(typeof(BulkPowerOperation.Response), (int)HttpStatusCode.Accepted)]
        [SwaggerOperation(OperationId = "bulkShutdown")]
        public async Task<IActionResult> BulkShutdown(BulkPowerOperation.Command command)
        {
            command.Operation = PowerOperation.Shutdown;
            var result = await _mediator.Send(command);
            return Accepted(result);
        }
    }
}

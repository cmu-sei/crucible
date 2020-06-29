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
using NetVimClient;
using Swashbuckle.AspNetCore.Annotations;

namespace Player.Vm.Api.Features.Vsphere
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class VsphereController : Controller
    {
        private readonly IMediator _mediator;

        public VsphereController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieve a single vsphere virtual machine by Id, including a ticket to access it's console
        /// </summary>
        [HttpGet("vms/vsphere/{id}")]
        [ProducesResponseType(typeof(VsphereVirtualMachine), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getVsphereVirtualMachine")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Get.Query { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Power on a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/power-on")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "powerOnVsphereVirtualMachine")]
        public async Task<IActionResult> PowerOn([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new PowerOn.Command { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Power off a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/power-off")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "powerOffVsphereVirtualMachine")]
        public async Task<IActionResult> PowerOff([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new PowerOff.Command { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Reboot a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/reboot")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "rebootVsphereVirtualMachine")]
        public async Task<IActionResult> Reboot([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Reboot.Command { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Shutdown a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/shutdown")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "shutdownVsphereVirtualMachine")]
        public async Task<IActionResult> Shutdown([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new Shutdown.Command { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Get tools status of a vsphere virtual machine
        /// </summary>
        [HttpGet("vms/vsphere/{id}/tools")]
        [ProducesResponseType(typeof(VirtualMachineToolsStatus), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getVsphereVirtualMachineToolsStatus")]
        public async Task<IActionResult> GetToolsStatus([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetToolsStatus.Query { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Change the network of a vsphere virtual machine's network adapter
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/change-network")]
        [ProducesResponseType(typeof(VsphereVirtualMachine), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "changeVsphereVirtualMachineNetwork")]
        public async Task<IActionResult> ChangeNetwork([FromRoute] Guid id, [FromBody] ChangeNetwork.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Json(result);
        }

        /// <summary>
        /// Validate credentials for a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/validate-credentials")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "validateVsphereVirtualMachineCredentials")]
        public async Task<IActionResult> ValidateCredentials([FromRoute] Guid id, [FromBody] ValidateCredentials.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Json(result);
        }

        /// <summary>
        /// Upload a file to a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/upload-file"), DisableRequestSizeLimit]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "uploadFileToVsphereVirtualMachine")]
        public async Task<IActionResult> UploadFile([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new UploadFile.Command
            {
                Id = id,
                Files = Request.Form.Files,
                FilePath = Request.Form["filepath"][0],
                Password = Request.Form["password"][0],
                Username = Request.Form["username"][0]
            });
            return Json(result);
        }

        /// <summary>
        /// Get isos available to be mounted to a vsphere virtual machine
        /// </summary>
        [HttpGet("vms/vsphere/{id}/isos")]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getVsphereVirtualMachineIsos")]
        public async Task<IActionResult> GetIsos([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetIsos.Query { Id = id });
            return Json(result);
        }

        /// <summary>
        /// Mount an iso to a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/mount-iso")]
        [ProducesResponseType(typeof(VsphereVirtualMachine), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "mountVsphereVirtualMachineIso")]
        public async Task<IActionResult> MountIso([FromRoute] Guid id, [FromBody] MountIso.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Json(result);
        }

        /// <summary>
        /// Set the resolution of a vsphere virtual machine
        /// </summary>
        [HttpPost("vms/vsphere/{id}/actions/set-resolution")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "setVsphereVirtualMachineResolution")]
        public async Task<IActionResult> SetResolution([FromRoute] Guid id, [FromBody] SetResolution.Command command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Json(result);
        }
    }
}
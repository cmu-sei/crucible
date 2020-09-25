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
using STT = System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.Services;
using SAVM = Steamfitter.Api.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Steamfitter.Api.Controllers
{
    public class VmCredentialController : BaseController
    {
        private readonly IVmCredentialService _VmCredentialService;

        public VmCredentialController(IVmCredentialService VmCredentialService)
        {
            _VmCredentialService = VmCredentialService;
        }

        /// <summary>
        /// Gets all VmCredentials in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the VmCredentials in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("VmCredentials")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.VmCredential>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getVmCredentials")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _VmCredentialService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all VmCredentials for a ScenarioTemplate
        /// </summary>
        /// <remarks>
        /// Returns all VmCredentials for the specified ScenarioTemplate
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarioTemplates/{id}/VmCredentials")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.VmCredential>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getScenarioTemplateVmCredentials")]
        public async STT.Task<IActionResult> GetByScenarioTemplateId(Guid id, CancellationToken ct)
        {
            var list = await _VmCredentialService.GetByScenarioTemplateIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets all VmCredentials for a Scenario
        /// </summary>
        /// <remarks>
        /// Returns all VmCredentials for the specified Scenario
        /// </remarks>
        /// <returns></returns>
        [HttpGet("scenarios/{id}/VmCredentials")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.VmCredential>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getScenarioVmCredentials")]
        public async STT.Task<IActionResult> GetByScenarioId(Guid id, CancellationToken ct)
        {
            var list = await _VmCredentialService.GetByScenarioIdAsync(id, ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific VmCredential by id
        /// </summary>
        /// <remarks>
        /// Returns the VmCredential with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified VmCredential
        /// </remarks>
        /// <param name="id">The id of the STT.Task</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("VmCredentials/{id}")]
        [ProducesResponseType(typeof(SAVM.VmCredential), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getVmCredential")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var vmCredential = await _VmCredentialService.GetAsync(id, ct);
            return Ok(vmCredential);
        }

        /// <summary>
        /// Creates a new VmCredential
        /// </summary>
        /// <remarks>
        /// Creates a new VmCredential with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="vmCredential">The data to create the VmCredential with</param>
        /// <param name="ct"></param>
        [HttpPost("VmCredentials")]
        [ProducesResponseType(typeof(SAVM.VmCredential), (int)HttpStatusCode.Created)]
        [SwaggerOperation(OperationId = "createVmCredential")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.VmCredential vmCredential, CancellationToken ct)
        {
            var createdVmCredential = await _VmCredentialService.CreateAsync(vmCredential, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdVmCredential.Id }, createdVmCredential);
        }

        /// <summary>
        /// Updates a VmCredential
        /// </summary>
        /// <remarks>
        /// Updates a VmCredential with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified VmCredential
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="vmCredential">The updated VmCredential values</param>
        /// <param name="ct"></param>
        [HttpPut("VmCredentials/{id}")]
        [ProducesResponseType(typeof(SAVM.VmCredential), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "updateVmCredential")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.VmCredential vmCredential, CancellationToken ct)
        {
            var updatedVmCredential = await _VmCredentialService.UpdateAsync(id, vmCredential, ct);
            return Ok(updatedVmCredential);
        }

        /// <summary>
        /// Deletes a VmCredential
        /// </summary>
        /// <remarks>
        /// Deletes a VmCredential with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified VmCredential
        /// </remarks>    
        /// <param name="id">The id of the VmCredential to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("VmCredentials/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "deleteVmCredential")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _VmCredentialService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


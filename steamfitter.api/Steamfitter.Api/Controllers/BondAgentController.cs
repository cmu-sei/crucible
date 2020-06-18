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
using System.Linq;
using System.Net;
using System.Threading;
using STT = System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAVM = Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Steamfitter.Api.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// TODO: we need to figure out authorization for this - but agents checking in won't have access to identity (so as to get a token)
    /// </summary>
    public class BondAgentController : Controller
    {
        private readonly IBondAgentService _BondAgentService;
        private readonly IAuthorizationService _authorizationService;

        public BondAgentController(IBondAgentService BondAgentService, IAuthorizationService authorizationService)
        {
            _BondAgentService = BondAgentService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all BondAgent in the system
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the BondAgents in the system.
        /// <para />
        /// Only accessible to a SuperUser
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("BondAgents")]
        [ProducesResponseType(typeof(IEnumerable<SAVM.BondAgent>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getBondAgents")]
        public async STT.Task<IActionResult> Get(CancellationToken ct)
        {
            var list = await _BondAgentService.GetAsync(ct);
            return Ok(list);
        }

        /// <summary>
        /// Gets a specific BondAgent by id
        /// </summary>
        /// <remarks>
        /// Returns the BondAgent with the id specified
        /// <para />
        /// Accessible to a SuperUser or a User that is a member of a Team within the specified BondAgent
        /// </remarks>
        /// <param name="id">The id of the BondAgent</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("BondAgents/{id}")]
        [ProducesResponseType(typeof(SAVM.BondAgent), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "getBondAgent")]
        public async STT.Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var BondAgent = await _BondAgentService.GetAsync(id, ct);

            if (BondAgent == null)
                throw new EntityNotFoundException<SAVM.BondAgent>();

            return Ok(BondAgent);
        }

        /// <summary>
        /// Creates a new BondAgent
        /// </summary>
        /// <remarks>
        /// Creates a new BondAgent with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or an Administrator
        /// </remarks>    
        /// <param name="bondAgent">The data to create the BondAgent with</param>
        /// <param name="ct"></param>
        [HttpPost("BondAgents")]
        [ProducesResponseType(typeof(SAVM.BondAgent), (int)HttpStatusCode.Created)]
        [SwaggerOperation(operationId: "createBondAgent")]
        public async STT.Task<IActionResult> Create([FromBody] SAVM.BondAgent bondAgent, CancellationToken ct)
        {
            if(bondAgent.Id == Guid.Empty)
                bondAgent.Id = Guid.NewGuid();
            
            var createdBondAgent = await _BondAgentService.CreateAsync(bondAgent, ct);
            return CreatedAtAction(nameof(this.Get), new { id = createdBondAgent.VmWareUuid }, createdBondAgent);
        }

        /// <summary>
        /// Updates a BondAgent
        /// </summary>
        /// <remarks>
        /// Updates a BondAgent with the attributes specified
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified BondAgent
        /// </remarks>  
        /// <param name="id">The Id of the Exericse to update</param>
        /// <param name="BondAgent">The updated BondAgent values</param>
        /// <param name="ct"></param>
        [HttpPut("BondAgents/{id}")]
        [ProducesResponseType(typeof(SAVM.BondAgent), (int)HttpStatusCode.OK)]
        [SwaggerOperation(operationId: "updateBondAgent")]
        public async STT.Task<IActionResult> Update([FromRoute] Guid id, [FromBody] SAVM.BondAgent BondAgent, CancellationToken ct)
        {
            var updatedBondAgent = await _BondAgentService.UpdateAsync(id, BondAgent, ct);
            return Ok(updatedBondAgent);
        }

        /// <summary>
        /// Deletes a BondAgent
        /// </summary>
        /// <remarks>
        /// Deletes a BondAgent with the specified id
        /// <para />
        /// Accessible only to a SuperUser or a User on an Admin Team within the specified BondAgent
        /// </remarks>    
        /// <param name="id">The id of the BondAgent to delete</param>
        /// <param name="ct"></param>
        [HttpDelete("BondAgents/{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(operationId: "deleteBondAgent")]
        public async STT.Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _BondAgentService.DeleteAsync(id, ct);
            return NoContent();
        }

    }
}


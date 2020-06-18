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
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.Services;
using Alloy.Api.ViewModels;
using Alloy.Api.Infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Caster.Api.Models;

namespace Alloy.Api.Controllers
{
    public class CasterController : BaseController
    {
        private readonly ICasterService _casterService;
        private readonly IAuthorizationService _authorizationService;

        public CasterController(ICasterService casterService, IAuthorizationService authorizationService)
        {
            _casterService = casterService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all Directories
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the Directories.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("directories")]
        [ProducesResponseType(typeof(IEnumerable<Directory>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getDirectories")]
        public async Task<IActionResult> GetDirectories(CancellationToken ct)
        {
            var list = await _casterService.GetDirectoriesAsync(ct);
            return Ok(list);
        }

        // /// <summary>
        // /// Gets all workspaces
        // /// </summary>
        // /// <remarks>
        // /// Returns a list of all of Workspaces.
        // /// </remarks>       
        // /// <returns></returns>
        // [HttpGet("workspaces")]
        // [ProducesResponseType(typeof(IEnumerable<Workspace>), (int)HttpStatusCode.OK)]
        // [SwaggerOperation(OperationId = "getWorkspaces")]
        // public async Task<IActionResult> GetWorkspaces(CancellationToken ct)
        // {
        //     var list = await _casterService.GetWorkspacesAsync(ct);
        //     return Ok(list);
        // }

    }

}


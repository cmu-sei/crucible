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
using Steamfitter.Api.Models;

namespace Alloy.Api.Controllers
{
    public class SteamfitterController : BaseController
    {
        private readonly ISteamfitterService _steamfitterService;
        private readonly IAuthorizationService _authorizationService;

        public SteamfitterController(ISteamfitterService steamfitterService, IAuthorizationService authorizationService)
        {
            _steamfitterService = steamfitterService;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets all ScenarioTemplates
        /// </summary>
        /// <remarks>
        /// Returns a list of all of the ScenarioTemplates.
        /// </remarks>       
        /// <returns></returns>
        [HttpGet("scenarioTemplates")]
        [ProducesResponseType(typeof(IEnumerable<ScenarioTemplate>), (int)HttpStatusCode.OK)]
        [SwaggerOperation(OperationId = "getScenarioTemplates")]
        public async Task<IActionResult> GetScenarioTemplates(CancellationToken ct)
        {
            var list = await _steamfitterService.GetScenarioTemplatesAsync(ct);
            return Ok(list);
        }

    }

}


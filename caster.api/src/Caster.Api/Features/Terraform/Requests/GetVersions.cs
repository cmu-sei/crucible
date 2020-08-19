/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Runtime.Serialization;
using Caster.Api.Infrastructure.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Domain.Services;
using System;
using Caster.Api.Data;
using AutoMapper;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Caster.Api.Infrastructure.Options;
using System.Linq;

namespace Caster.Api.Features.Terraform
{
    public class GetVersions
    {
        [DataContract(Name = "GetTerraformVersions")]
        public class Query : IRequest<TerraformVersionsResult>
        {
        }

        public class TerraformVersionsResult
        {
            public string[] Versions { get; set; }
            public string DefaultVersion { get; set; }
        }

        public class Handler : IRequestHandler<Query, TerraformVersionsResult>
        {
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;
            private readonly ITerraformService _terraformService;
            private readonly TerraformOptions _terraformOptions;

            public Handler(
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                ITerraformService terraformService,
                TerraformOptions terraformOptions)
            {
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
                _terraformService = terraformService;
                _terraformOptions = terraformOptions;
            }

            public async Task<TerraformVersionsResult> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var versions = _terraformService.GetVersions();

                return new TerraformVersionsResult
                {
                    Versions = versions.ToArray(),
                    DefaultVersion = _terraformOptions.DefaultVersion
                };
            }
        }
    }
}

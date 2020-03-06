/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Caster.Api.Data;
using AutoMapper;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Exceptions;
using Caster.Api.Infrastructure.Identity;

namespace Caster.Api.Features.Modules
{
    public class CreateSnippet
    {
        [DataContract(Name="CreateSnippetCommand")]
        public class Command : IRequest<string>
        {
            /// <summary>
            /// ID of the Version.
            /// </summary>
            [DataMember]
            public Guid VersionId { get; set; }

            /// <summary>
            /// Name for this instance of the Module.
            /// </summary>
            [DataMember]
            public string ModuleName { get; set; }

            /// <summary>
            /// Variables of the Module.
            /// </summary>
            [DataMember]
            public List<VariableValue> VariableValues { get; set; }

        }

        public class Handler : IRequestHandler<Command, string>
        {
            private readonly CasterContext _db;
            private readonly IMapper _mapper;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;

            public Handler(
                CasterContext db,
                IMapper mapper,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver)
            {
                _db = db;
                _mapper = mapper;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
            }

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var version = await _db.ModuleVersions.FirstOrDefaultAsync(v => v.Id == request.VersionId);
                var snippet = $"module \"{request.ModuleName}\" {{\n" +
                              $"  source = \"git::{version.UrlLink}?ref={version.Name}\"";
                foreach (var variable in request.VariableValues)
                {
                    snippet = $"{snippet}\n  {variable.Name} = \"{variable.Value}\"";
                }
                snippet = snippet + "\n}";

                return snippet;
            }

        }

        public class VariableValue
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}


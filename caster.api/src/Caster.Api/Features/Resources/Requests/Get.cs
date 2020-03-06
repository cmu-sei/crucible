/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using System.Runtime.Serialization;
using Caster.Api.Data;
using System;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using System.Linq;
using Caster.Api.Infrastructure.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Caster.Api.Features.Resources
{
    public class Get
    {
        [DataContract(Name="GetResourceQuery")]
        public class Query : IRequest<Resource>
        {
            /// <summary>
            /// Id of the Workspace that the Resource exists in.
            /// </summary>
            [JsonIgnore]
            public Guid WorkspaceId { get; set; }

            /// <summary>
            /// Id of the Resource.
            /// </summary>
            [JsonIgnore]
            public string Id { get; set; }

            /// <summary>
            /// Type of the Resource.
            /// </summary>
            [DataMember]
            [Required]
            public string Type { get; set; }
        }

        public class Handler : IRequestHandler<Query, Resource>
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

            public async Task<Resource> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var workspace = await _db.Workspaces.FindAsync(request.WorkspaceId);

                if (workspace == null)
                    throw new EntityNotFoundException<Workspace>();

                var state = workspace.GetState();
                var resources = state.GetResources();
                var resource = resources.Where(r => r.Type == request.Type && r.Id == request.Id).FirstOrDefault();
                return _mapper.Map<Resource>(resource, opts => opts.ExcludeMembers());
            }
        }
    }
}


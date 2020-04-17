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
using AutoMapper;
using System.Runtime.Serialization;
using Caster.Api.Data;
using System;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Caster.Api.Infrastructure.Options;
using Caster.Api.Domain.Services;
using Microsoft.Extensions.Logging;
using Caster.Api.Infrastructure.Identity;
using System.Text.Json.Serialization;

namespace Caster.Api.Features.Resources
{
    public class Taint
    {
        [DataContract(Name="TaintResourcesCommand")]
        public class Command : IRequest<Resource[]>
        {
            /// <summary>
            /// ID of the Workspace.
            /// </summary>
            [JsonIgnore]
            public Guid WorkspaceId { get; set; }

            /// <summary>
            /// Untaint the Resources if true
            /// </summary>
            [JsonIgnore]
            public bool Untaint { get; set;}

            /// <summary>
            /// Perform the chosen operation on all Resources in the Workspace if true.
            /// </summary>
            [DataMember]
            public bool SelectAll { get; set;}

            /// <summary>
            /// List of Resource addresses to Taint or Untaint. Ignored if SelectAll is true.
            /// </summary>
            [DataMember]
            public string[] ResourceAddresses { get; set; }
        }

        public class Handler : BaseOperationHandler, IRequestHandler<Command, Resource[]>
        {
            public Handler(
                CasterContext db,
                IMapper mapper,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                TerraformOptions terraformOptions,
                ITerraformService terraformService,
                ILockService lockService,
                ILogger<BaseOperationHandler> logger) :
            base(db, mapper, authorizationService, identityResolver, terraformOptions, terraformService, lockService, logger) {}

            public async Task<Resource[]> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var workspace = await _db.Workspaces
                    .Include(w => w.Directory)
                    .Where(w => w.Id == request.WorkspaceId)
                    .FirstOrDefaultAsync();

                if (workspace == null) throw new EntityNotFoundException<Workspace>();

                string[] addresses = request.ResourceAddresses;

                if (request.SelectAll) {
                    addresses = workspace.GetState().GetResources().Select(r => r.Address).ToArray();
                }

                workspace = await base.PerformOperation(
                    workspace,
                    request.Untaint ? ResourceOperation.untaint : ResourceOperation.taint,
                    addresses);

                return _mapper.Map<Resource[]>(workspace.GetState().GetResources(), opts => opts.ExcludeMembers(nameof(Resource.Attributes)));
            }
        }
    }
}


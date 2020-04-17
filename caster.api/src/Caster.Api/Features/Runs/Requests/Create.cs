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
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Caster.Api.Data;
using AutoMapper;
using System.Runtime.Serialization;
using System.Linq;
using Caster.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Caster.Api.Infrastructure.Exceptions;
using Caster.Api.Domain.Events;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Identity;

namespace Caster.Api.Features.Runs
{
    public class Create
    {
        [DataContract(Name="CreateRunCommand")]
        public class Command : IRequest<Run>
        {
            /// <summary>
            /// The Id of the Workspace to create the Run in
            /// </summary>
            [DataMember]
            public Guid WorkspaceId { get; set; }

            /// <summary>
            /// If true, will create a Run to destroy all resources in the Workspace
            /// </summary>
            [DataMember]
            public bool IsDestroy { get; set; }

            /// <summary>
            /// Optional list of resources to constrain the affects of this Run to
            /// </summary>
            [DataMember]
            public string[] Targets { get; set; }
        }

        public class Handler : IRequestHandler<Command, Run>
        {
            private readonly CasterContext _db;
            private readonly IMapper _mapper;
            private readonly IMediator _mediator;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;
            private readonly ILockService _lockService;

            public Handler(
                CasterContext db,
                IMapper mapper,
                IMediator mediator,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                ILockService lockService)
            {
                _db = db;
                _mapper = mapper;
                _mediator = mediator;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
                _lockService = lockService;
            }

            public async Task<Run> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                await ValidateWorkspace(request.WorkspaceId);

                Domain.Models.Run run = null;

                using (var lockResult = await _lockService.GetWorkspaceLock(request.WorkspaceId).LockAsync(0))
                {
                    if (!lockResult.AcquiredLock)
                        throw new WorkspaceConflictException();

                    if ((await _db.AnyIncompleteRuns(request.WorkspaceId)))
                    {
                        throw new ConflictException("This Workspace's current Run must be rejected or applied before a new one can be created.");
                    }

                    run = await this.DoWork(request);
                }

                await _mediator.Publish(new RunCreated { RunId = run.Id });
                return _mapper.Map<Run>(run);
            }

            private async Task<Domain.Models.Run> DoWork(Command request)
            {
                var run = _mapper.Map<Domain.Models.Run>(request);
                await _db.Runs.AddAsync(run);
                await _db.SaveChangesAsync();
                return run;
            }

            private async Task ValidateWorkspace(Guid workspaceId)
            {
                var workspace = await _db.Workspaces.FindAsync(workspaceId);

                if (workspace == null)
                    throw new EntityNotFoundException<Workspace>();
            }
        }
    }
}


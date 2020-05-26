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
using Caster.Api.Domain.Events;

namespace Caster.Api.Features.Workspaces
{
    public class SetLockingStatus
    {
        [DataContract(Name="SetWorkspaceLockingStatusQuery")]
        public class Command : IRequest<bool>
        {
            public bool Enabled { get; set; }
        }

        public class Handler : IRequestHandler<Command, bool>
        {
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;
            private readonly ILockService _lockService;
            private readonly IMediator _mediator;

            public Handler(
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                ILockService lockService,
                IMediator mediator)
            {
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
                _lockService = lockService;
                _mediator = mediator;
            }

            public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                    throw new ForbiddenException();

                if (request.Enabled)
                {
                    _lockService.EnableWorkspaceLocking();
                }
                else
                {
                    _lockService.DisableWorkspaceLocking();
                }

                var lockingEnabled = _lockService.IsWorkspaceLockingEnabled();
                await _mediator.Publish(new WorkspaceSettingsUpdated(lockingEnabled));

                return lockingEnabled;
            }
        }
    }
}

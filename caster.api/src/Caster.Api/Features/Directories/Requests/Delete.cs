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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Caster.Api.Data;
using System.Runtime.Serialization;
using Caster.Api.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Domain.Events;
using Caster.Api.Features.Directories.Interfaces;

namespace Caster.Api.Features.Directories
{
    public class Delete
    {
        [DataContract(Name="DeleteDirectoryCommand")]
        public class Command : IRequest, IDirectoryDeleteRequest
        {
            public Guid Id { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly CasterContext _db;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;

            public Handler(
                CasterContext db,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                IMediator mediator)
            {
                _db = db;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var directory = await _db.Directories.FindAsync(request.Id);

                if (directory == null)
                    throw new EntityNotFoundException<Directory>();

                var workspaces = await CheckForResources(directory);

                if (workspaces.Any())
                {
                    string errorMessage = "Cannot delete this Directory due to existing Resources in the following Workspaces:";

                    foreach (var workspace in workspaces)
                    {
                        errorMessage += $"\n Name: {workspace.Name}, Id: {workspace.Id} in Directory: {workspace.Directory.Name}, {workspace.DirectoryId}";
                    }

                    throw new ConflictException(errorMessage);
                }

                _db.Directories.Remove(directory);
                await _db.SaveChangesAsync(cancellationToken);
            }

            private async Task<Domain.Models.Workspace[]> CheckForResources(Domain.Models.Directory directory)
            {
                var directories = await _db.Directories
                    .GetChildren(directory, true)
                    .Include(d => d.Workspaces)
                    .ToArrayAsync();

                List<Domain.Models.Workspace> workspaces = new List<Domain.Models.Workspace>();

                foreach (var workspace in directories.SelectMany(d => d.Workspaces))
                {
                    if (workspace.GetState().GetResources().Any())
                    {
                        workspaces.Add(workspace);
                    }
                }

                return workspaces.ToArray();
            }
        }
    }
}


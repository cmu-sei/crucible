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
using Microsoft.EntityFrameworkCore;
using Caster.Api.Infrastructure.Exceptions;
using Caster.Api.Domain.Models;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Features.Directories.Interfaces;

namespace Caster.Api.Features.Directories
{
    public class Create
    {
        [DataContract(Name="CreateDirectoryCommand")]
        public class Command : IRequest<Directory>, IDirectoryUpdateRequest
        {
            /// <summary>
            /// Name of the directory.
            /// </summary>
            [DataMember]
            public string Name { get; set; }

            /// <summary>
            /// ID of the project this directory is under.
            /// </summary>
            [DataMember]
            public Guid ProjectId { get; set; }

            /// <summary>
            /// ID of the directory this directory is under.
            /// </summary>
            [DataMember]
            public Guid? ParentId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Directory>
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

            public async Task<Directory> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                await ValidateProject(request.ProjectId);

                var directory = _mapper.Map<Domain.Models.Directory>(request);
                await SetPath(directory);

                await _db.Directories.AddAsync(directory, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<Directory>(directory);
            }

            private async Task ValidateProject(Guid projectId)
            {
                var project = await _db.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

                if (project == null)
                    throw new EntityNotFoundException<Project>();
            }

            private async Task SetPath(Domain.Models.Directory directory)
            {
                directory.Id = Guid.NewGuid();

                if (!directory.ParentId.HasValue)
                {
                    directory.SetPath();
                }
                else
                {
                    var parentDirectory = await _db.Directories.FindAsync(directory.ParentId);

                    if (parentDirectory == null)
                        throw new EntityNotFoundException<Directory>("Parent Directory not found");

                    if (parentDirectory.ProjectId != directory.ProjectId)
                        throw new ConflictException("Parent and child Directories must be in the same Project");

                    directory.SetPath(parentDirectory.Path);
                }
            }
        }
    }
}

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
using Caster.Api.Infrastructure.Exceptions;
using Caster.Api.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Extensions;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Features.Files.Interfaces;

namespace Caster.Api.Features.Files
{
    public class Create
    {
        [DataContract(Name="CreateFileCommand")]
        public class Command : FileUpdateRequest, IRequest<File>
        {
            /// <summary>
            /// Name of the file.
            /// </summary>
            [DataMember]
            public string Name { get; set; }

            /// <summary>
            /// ID of the directory this file is under.
            /// </summary>
            [DataMember]
            public Guid DirectoryId { get; set; }

            /// <summary>
            /// An optional Workspace to assign this File to
            /// </summary>
            [DataMember]
            public Guid? WorkspaceId { get; set; }

            /// <summary>
            /// The full contents of the file.
            /// </summary>
            [DataMember]
            public override string Content { get; set; }
        }

        public class Handler : IRequestHandler<Command, File>
        {
            private readonly CasterContext _db;
            private readonly IMapper _mapper;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;
            private readonly IGetFileQuery _fileQuery;
            private readonly IIdentityResolver _identityResolver;

            public Handler(
                CasterContext db,
                IMapper mapper,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                IGetFileQuery fileQuery)
            {
                _db = db;
                _mapper = mapper;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
                _fileQuery = fileQuery;
                _identityResolver = identityResolver;
            }

            public async Task<File> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                await ValidateEntities(request.DirectoryId, request.WorkspaceId);

                var file = _mapper.Map<Domain.Models.File>(request);
                file.Save(_user.GetId(), isAdmin: (await _identityResolver.IsAdminAsync()), bypassLock: true);

                await _db.Files.AddAsync(file, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                return await _fileQuery.ExecuteAsync(file.Id);
            }

            private async Task ValidateEntities(Guid directoryId, Guid? workspaceId)
            {
                var directory = await _db.Directories.FindAsync(directoryId);

                if (directory == null)
                    throw new EntityNotFoundException<Directory>();

                if (workspaceId.HasValue)
                {
                    var workspace = await _db.Workspaces.FindAsync(workspaceId.Value);

                    if (workspace == null)
                        throw new EntityNotFoundException<Workspace>();
                }
            }
        }
    }
}

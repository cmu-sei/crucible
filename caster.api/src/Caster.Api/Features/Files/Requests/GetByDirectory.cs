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
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Caster.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using Caster.Api.Infrastructure.Exceptions;
using Caster.Api.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;
using System.Text.Json.Serialization;

namespace Caster.Api.Features.Files
{
    public class GetByDirectory
    {
        [DataContract(Name="GetFilesByDirectoryQuery")]
        public class Query : IRequest<File[]>
        {
            [JsonIgnore]
            public Guid DirectoryId { get; set; }

            /// <summary>
            /// Whether or not to retrieve file content.
            /// </summary>
            [DataMember]
            public bool IncludeContent { get; set; }

            /// <summary>
            /// Whether or not to retrieve deleted files.
            /// </summary>
            [DataMember]
            public bool IncludeDeleted { get; set; }
        }

        public class Handler : IRequestHandler<Query, File[]>
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

            public async Task<File[]> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                await ValidateDirectory(request.DirectoryId);

                return await _db.Files
                    .GetAll(
                        configurationProvider: _mapper.ConfigurationProvider,
                        directoryId: request.DirectoryId,
                        includeDeleted: request.IncludeDeleted,
                        includeContent: request.IncludeContent)
                    .ToArrayAsync();
            }

            private async Task ValidateDirectory(Guid directoryId)
            {
                var directory = await _db.Directories.FindAsync(directoryId);

                if (directory == null)
                    throw new EntityNotFoundException<Directory>();
            }
        }
    }
}


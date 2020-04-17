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
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Extensions;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Features.Files.Interfaces;
using System.Text.Json.Serialization;

namespace Caster.Api.Features.Files
{
    public class Rename
    {
        [DataContract(Name="RenameFileCommand")]
        public class Command : FileUpdateRequest, IRequest<File>, IFileCommand
        {
            [JsonIgnore]
            public Guid Id { get; set; }

            /// <summary>
            /// New Name for the file.
            /// </summary>
            [DataMember]
            public string Name { get; set; }

            [JsonIgnore]
            public override string Content { get; set; }
        }

        public class Handler : FileCommandHandler, IRequestHandler<Command, File>
        {
            private Command _request { get; set; }

            public Handler(
                CasterContext db,
                IMapper mapper,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                ILockService lockService,
                IGetFileQuery fileQuery)
                : base(db, mapper, authorizationService, identityResolver, lockService, fileQuery) {}

            public async Task<File> Handle(Command request, CancellationToken cancellationToken)
            {
                _request = request;
                return await base.Handle(request, cancellationToken);
            }

            protected override async Task PerformOperation(Domain.Models.File file)
            {
                var isAdmin = await _identityResolver.IsAdminAsync();
                var userId = _user.GetId();
                var isNotAlreadyLocked = (userId != file.LockedById);

                if(isNotAlreadyLocked)
                {
                    file.Lock(userId, isAdmin);
                }

                try
                {
                    file.Name = _request.Name;
                    file.Save(userId, isAdmin);
                }
                finally
                {
                    if(isNotAlreadyLocked)
                    {
                        file.Unlock(userId);
                    }
                }
            }
        }
    }
}


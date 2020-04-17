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
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Infrastructure.Extensions;
using Caster.Api.Features.Files.Interfaces;

namespace Caster.Api.Features.Files
{
    public class Lock
    {
        [DataContract(Name="LockFileCommand")]
        public class Command : FileMetadataUpdateRequest, IRequest<File>, IFileCommand
        {
            public Guid Id { get; set; }
        }

        public class Handler : FileCommandHandler, IRequestHandler<Command, File>
        {
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
                return await base.Handle(request, cancellationToken);
            }

            protected override async Task PerformOperation(Domain.Models.File file)
            {
                file.Lock(_user.GetId(), isAdmin: (await _identityResolver.IsAdminAsync()));
            }
        }
    }
}


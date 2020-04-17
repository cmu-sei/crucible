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
using System.Threading.Tasks;
using Caster.Api.Data;
using AutoMapper;
using Caster.Api.Infrastructure.Exceptions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Identity;
using System.Threading;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Extensions;

namespace Caster.Api.Features.Files
{
    public interface IFileCommand
    {
        Guid Id { get; set; }
    }

    public abstract class FileCommandHandler
    {
        protected readonly CasterContext _db;
        protected readonly IMapper _mapper;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly ClaimsPrincipal _user;
        protected readonly ILockService _lockService;
        protected readonly IGetFileQuery _fileQuery;
        protected readonly IIdentityResolver _identityResolver;

        public FileCommandHandler(
            CasterContext db,
            IMapper mapper,
            IAuthorizationService authorizationService,
            IIdentityResolver identityResolver,
            ILockService lockService,
            IGetFileQuery fileQuery)
        {
            _db = db;
            _mapper = mapper;
            _authorizationService = authorizationService;
            _user = identityResolver.GetClaimsPrincipal();
            _lockService = lockService;
            _fileQuery = fileQuery;
            _identityResolver = identityResolver;
        }

        protected async Task<File> Handle(IFileCommand request, CancellationToken cancellationToken)
        {
            await this.Authorize();

            using (var lockResult = await _lockService.GetFileLock(request.Id).LockAsync(0))
            {
                if (!lockResult.AcquiredLock)
                    throw new FileConflictException();

                var file = await _db.Files.FindAsync(request.Id);

                if (file == null)
                    throw new EntityNotFoundException<File>();

                await this.PerformOperation(file);

                await _db.SaveChangesAsync(cancellationToken);
            }

            return await _fileQuery.ExecuteAsync(request.Id);
        }

        protected virtual async Task Authorize() {
            if (!(await _authorizationService.AuthorizeAsync(
                _user, null, new ContentDeveloperRequirement())).Succeeded)
            {
                throw new ForbiddenException();
            }
        }

        protected abstract Task PerformOperation(Domain.Models.File file);

    }
}


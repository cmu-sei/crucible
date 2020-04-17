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
using AutoMapper;
using Caster.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using Caster.Api.Infrastructure.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;
using System.Text.Json.Serialization;
using Caster.Api.Domain.Services;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using System.Linq;
using Caster.Api.Domain.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Caster.Api.Domain.Events;
using Caster.Api.Data.Extensions;

namespace Caster.Api.Features.Exercises
{
    public class Import
    {
        [DataContract(Name="ImportExerciseCommand")]
        public class Command : IRequest<ImportExerciseResult>
        {
            [JsonIgnore]
            public Guid Id { get; set; }

            [DataMember]
            public IFormFile Archive { get; set; }

            [DataMember]
            public bool PreserveIds { get; set; }
        }

        public class ImportValidator : AbstractValidator<Command> {
            public ImportValidator() {
                RuleFor(x => x.Archive)
                    .NotNull().Must(BeAValidArchiveType)
                    .WithMessage($"File extension must be one of {string.Join(", ", ArchiveTypeHelpers.GetValidExtensions())}");
            }

            private bool BeAValidArchiveType(IFormFile file)
            {
                var isValid = false;

                foreach (var extension in ArchiveTypeHelpers.GetValidExtensions())
                {
                    if (file.FileName.ToLower().EndsWith(extension))
                    {
                        isValid = true;
                    }
                }

                return isValid;
            }
        }

        public class ImportExerciseResult
        {
            /// <summary>
            /// A list of Files that were unable to be updated because
            /// they were locked or the current user does not have permission to lock them
            /// </summary>
            public string[] LockedFiles { get; set; }
        }

        public class Handler : IRequestHandler<Command, ImportExerciseResult>
        {
            private readonly CasterContext _db;
            private readonly IMapper _mapper;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;
            private readonly IArchiveService _archiveService;
            private readonly IImportService _importService;
            private readonly IMediator _mediator;

            public Handler(
                CasterContext db,
                IMapper mapper,
                IAuthorizationService authorizationService,
                IIdentityResolver identityResolver,
                IArchiveService archiveService,
                IImportService importService,
                IMediator mediator)
            {
                _db = db;
                _mapper = mapper;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
                _archiveService = archiveService;
                _importService = importService;
                _mediator = mediator;
            }

            public async Task<ImportExerciseResult> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                    throw new ForbiddenException();

                var exercise =  await _db.Exercises
                    .Include(e => e.Directories)
                        .ThenInclude(d => d.Workspaces)
                    .Include(e => e.Directories)
                        .ThenInclude(d => d.Files)
                    .SingleOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

                if (exercise == null)
                    throw new EntityNotFoundException<Exercise>();

                Domain.Models.Exercise extractedExercise;

                using (var memStream = new System.IO.MemoryStream())
                {
                    await request.Archive.CopyToAsync(memStream, cancellationToken);
                    memStream.Position = 0;
                    extractedExercise = _archiveService.ExtractExercise(memStream, request.Archive.FileName);
                }

                var importResult = await _importService.ImportExercise(exercise, extractedExercise, request.PreserveIds);

                var entries = _db.GetUpdatedEntries();
                await _db.SaveChangesAsync(cancellationToken);
                await this.PublishEvents(entries);

                return _mapper.Map<ImportExerciseResult>(importResult);
            }

            private async Task PublishEvents(EntityEntry[] entries)
            {
                foreach (var entry in entries)
                {
                    var evt = entry.ToEvent();

                    if (evt != null)
                    {
                        await _mediator.Publish(evt);
                    }
                }
            }
        }
    }
}

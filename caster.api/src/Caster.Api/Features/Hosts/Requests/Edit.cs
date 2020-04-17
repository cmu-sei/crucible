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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Principal;
using Caster.Api.Infrastructure.Authorization;
using Caster.Api.Infrastructure.Identity;

namespace Caster.Api.Features.Hosts
{
    public class Edit
    {
        [DataContract(Name="EditHostCommand")]
        public class Command : IRequest<Host>
        {
            public Guid Id { get; set; }

                        /// <summary>
            /// Name of the Host
            /// </summary>
            [DataMember]
            public string Name { get; set; }

            /// <summary>
            /// Name of the datastore to use with this Host
            /// </summary>
            [DataMember]
            public string Datastore { get; set; }

            /// <summary>
            /// Maximum number of mahcines to deploy to this Host
            /// </summary>
            [DataMember]
            public int MaximumMachines { get; set; }

            /// <summary>
            /// True if this Host should be selected for deployments
            /// </summary>
            [DataMember]
            public bool Enabled { get; set; }

            /// <summary>
            /// True if this Host should not be automatically selected for deployments and only used for development purposes, manually
            /// </summary>
            [DataMember]
            public bool Development { get; set; }

            /// <summary>
            /// The Id of the Exercise to assign this Host to
            /// </summary>
            [DataMember]
            public Guid? ExerciseId { get; set; }
        }

        public class Handler : IRequestHandler<Command, Host>
        {
            private readonly CasterContext _db;
            private readonly IMapper _mapper;
            private readonly IAuthorizationService _authorizationService;
            private readonly ClaimsPrincipal _user;

            public Handler(CasterContext db, IMapper mapper, IAuthorizationService authorizationService, IIdentityResolver identityResolver)
            {
                _db = db;
                _mapper = mapper;
                _authorizationService = authorizationService;
                _user = identityResolver.GetClaimsPrincipal();
            }

            public async Task<Host> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                    throw new ForbiddenException();

                var host = await _db.Hosts.FindAsync(request.Id);

                if (host == null)
                    throw new EntityNotFoundException<Host>();

                _mapper.Map(request, host);
                await _db.SaveChangesAsync();
                return _mapper.Map<Host>(host);
            }
        }
    }
}


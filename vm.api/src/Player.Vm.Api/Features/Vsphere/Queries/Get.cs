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
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Player.Vm.Api.Infrastructure.Exceptions;
using AutoMapper;
using Player.Vm.Api.Domain.Vsphere.Services;
using Player.Vm.Api.Features.Vms;
using Microsoft.Extensions.Logging;
using Player.Vm.Api.Domain.Vsphere.Options;
using System.Security.Claims;
using System.Security.Principal;
using Player.Vm.Api.Infrastructure.Extensions;
using S3.Player.Api;
using System.Linq;

namespace Player.Vm.Api.Features.Vsphere
{
    public class Get
    {
        [DataContract(Name = "GetVsphereVirtualMachineQuery")]
        public class Query : IRequest<VsphereVirtualMachine>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
        }

        public class Handler : BaseHandler, IRequestHandler<Query, VsphereVirtualMachine>
        {
            private readonly IVmService _vmService;
            private readonly IMapper _mapper;
            private readonly IVsphereService _vsphereService;
            private readonly ILogger<Get> _logger;
            private readonly VsphereOptions _vsphereOptions;
            private readonly ClaimsPrincipal _user;
            private readonly IS3PlayerApiClient _playerClient;

            public Handler(
                IVmService vmService,
                IMapper mapper,
                IVsphereService vsphereService,
                ILogger<Get> logger,
                VsphereOptions vsphereOptions,
                IPrincipal user,
                IS3PlayerApiClient playerClient) :
                base(mapper, vsphereService)
            {
                _vmService = vmService;
                _mapper = mapper;
                _vsphereService = vsphereService;
                _logger = logger;
                _vsphereOptions = vsphereOptions;
                _user = user as ClaimsPrincipal;
                _playerClient = playerClient;
            }

            public async Task<VsphereVirtualMachine> Handle(Query request, CancellationToken cancellationToken)
            {
                var vm = await _vmService.GetAsync(request.Id, cancellationToken);

                if (vm == null)
                    throw new EntityNotFoundException<VsphereVirtualMachine>();

                var vsphereVirtualMachine = await base.GetVsphereVirtualMachine(vm);

                await LogAccess(vm);

                return vsphereVirtualMachine;
            }

            private async Task LogAccess(Vms.Vm vm)
            {
                if (_vsphereOptions.LogConsoleAccess && _logger.IsEnabled(LogLevel.Information))
                {
                    var team = (await _playerClient.GetUserViewTeamsAsync(vm.ViewId, _user.GetId()))
                        .FirstOrDefault(t => t.IsPrimary.Value);

                    _logger.LogInformation(new EventId(1),
                        $"User {_user.GetName()} ({_user.GetId()}) in Team {team.Name} ({team.Id}) accessed console of {vm.Name} ({vm.Id})");
                }
            }
        }
    }
}
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
using Player.Vm.Api.Domain.Vsphere.Extensions;
using Player.Vm.Api.Domain.Services;
using System.Security.Principal;

namespace Player.Vm.Api.Features.Vsphere
{
    public class ChangeNetwork
    {
        [DataContract(Name = "ChangeVsphereVirtualMachineNetwork")]
        public class Command : IRequest<VsphereVirtualMachine>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
            public string Adapter { get; set; }
            public string Network { get; set; }
        }

        public class Handler : BaseHandler, IRequestHandler<Command, VsphereVirtualMachine>
        {
            private readonly IVsphereService _vsphereService;
            private readonly IVmService _vmService;
            private readonly IMapper _mapper;
            private readonly IPlayerService _playerService;

            public Handler(
                IVsphereService vsphereService,
                IVmService vmService,
                IMapper mapper,
                IPlayerService playerService,
                IPrincipal principal) :
                base(mapper, vsphereService, playerService, principal)
            {
                _vsphereService = vsphereService;
                _vmService = vmService;
                _mapper = mapper;
                _playerService = playerService;
            }

            public async Task<VsphereVirtualMachine> Handle(Command request, CancellationToken cancellationToken)
            {
                var vm = await _vmService.GetAsync(request.Id, cancellationToken);

                if (vm == null)
                    throw new EntityNotFoundException<VsphereVirtualMachine>();

                if (!(await _playerService.CanManageTeamsAsync(vm.TeamIds, false, cancellationToken)))
                    throw new ForbiddenException("You do not have permission to change networks on this vm.");

                await _vsphereService.ReconfigureVm(request.Id, Feature.net, request.Adapter, request.Network);

                return await base.GetVsphereVirtualMachine(vm, cancellationToken);
            }
        }
    }
}
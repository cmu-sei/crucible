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
using Player.Vm.Api.Domain.Services;
using System.Security.Principal;
using System.Security.Claims;
using Player.Vm.Api.Infrastructure.Extensions;

namespace Player.Vm.Api.Features.Vsphere
{
    public class BaseHandler
    {
        private readonly IMapper _mapper;
        private readonly IVsphereService _vsphereService;
        private readonly IPlayerService _playerService;
        private readonly Guid _userId;

        public BaseHandler(
            IMapper mapper,
            IVsphereService vsphereService,
            IPlayerService playerService,
            IPrincipal principal)
        {
            _mapper = mapper;
            _vsphereService = vsphereService;
            _playerService = playerService;
            _userId = (principal as ClaimsPrincipal).GetId();
        }

        protected async Task<VsphereVirtualMachine> GetVsphereVirtualMachine(Features.Vms.Vm vm, CancellationToken cancellationToken)
        {
            var domainMachine = await _vsphereService.GetMachineById(vm.Id);

            if (domainMachine == null)
                throw new EntityNotFoundException<VsphereVirtualMachine>();

            var vsphereVirtualMachine = _mapper.Map<VsphereVirtualMachine>(domainMachine);
            var canManage = await _playerService.CanManageTeamsAsync(vm.TeamIds, false, cancellationToken);

            vsphereVirtualMachine.Ticket = await _vsphereService.GetConsoleUrl(domainMachine); ;
            vsphereVirtualMachine.NetworkCards = await _vsphereService.GetNicOptions(
                id: vm.Id,
                canManage: canManage,
                allowedNetworks: vm.AllowedNetworks,
                machine: domainMachine);

            // copy vm properties
            vsphereVirtualMachine = _mapper.Map(vm, vsphereVirtualMachine);
            vsphereVirtualMachine.CanAccessNicConfiguration = canManage;
            vsphereVirtualMachine.IsOwner = vsphereVirtualMachine.UserId == _userId;

            return vsphereVirtualMachine;
        }
    }
}
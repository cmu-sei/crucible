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
using NetVimClient;

namespace Player.Vm.Api.Features.Vsphere
{
    public class GetToolsStatus
    {
        [DataContract(Name = "GetVsphereVirtualMachineToolsStatus")]
        public class Query : IRequest<VirtualMachineToolsStatus>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, VirtualMachineToolsStatus>
        {
            private readonly IVsphereService _vsphereService;
            private readonly IVmService _vmService;

            public Handler(
                IVsphereService vsphereService,
                IVmService vmService)
            {
                _vsphereService = vsphereService;
                _vmService = vmService;
            }

            public async Task<VirtualMachineToolsStatus> Handle(Query request, CancellationToken cancellationToken)
            {
                var vm = await _vmService.GetAsync(request.Id, cancellationToken);

                if (vm == null)
                    throw new EntityNotFoundException<VsphereVirtualMachine>();

                return await _vsphereService.GetVmToolsStatus(request.Id);
            }
        }
    }
}
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
using Player.Vm.Api.Domain.Vsphere.Services;
using Player.Vm.Api.Features.Vms;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Player.Vm.Api.Features.Vsphere
{
    public class UploadFile
    {
        [DataContract(Name = "UploadFileToVsphereVirtualMachine")]
        public class Command : IRequest<string>
        {
            [JsonIgnore]
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string FilePath { get; set; }
            public IFormFileCollection Files { get; set; }
        }

        public class Handler : IRequestHandler<Command, string>
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

            public async Task<string> Handle(Command request, CancellationToken cancellationToken)
            {
                var vm = await _vmService.GetAsync(request.Id, cancellationToken);

                if (vm == null)
                    throw new EntityNotFoundException<VsphereVirtualMachine>();

                foreach (var formFile in request.Files)
                {
                    using (Stream fileStream = formFile.OpenReadStream())
                    {
                        try
                        {
                            await _vsphereService.UploadFileToVm(
                                request.Id,
                                request.Username,
                                request.Password,
                                string.Format("{0}{1}", request.FilePath, formFile.FileName),
                                fileStream);
                        }
                        catch (Exception ex)
                        {
                            throw new BadRequestException(ex.Message);
                        }
                    }
                }

                return "Files were successfully uploaded.";
            }
        }
    }
}
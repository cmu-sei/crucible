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
using Player.Vm.Api.Infrastructure.Exceptions;
using Player.Vm.Api.Domain.Vsphere.Services;
using Player.Vm.Api.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Player.Vm.Api.Domain.Vsphere.Models;
using System.Text.Json.Serialization;
using Player.Vm.Api.Domain.Models;
using Player.Vm.Api.Domain.Services;
using System.Collections.Generic;
using Player.Vm.Api.Features.Shared.Interfaces;

namespace Player.Vm.Api.Features.Vms
{
    public class BulkPowerOperation
    {
        [DataContract(Name = "BulkPowerOperation")]
        public class Command : IRequest<Response>, ICheckTasksRequest
        {
            public Guid[] Ids { get; set; }

            [JsonIgnore]
            public PowerOperation Operation { get; set; }
        }

        [DataContract(Name = "BulkPowerOperationResponse")]
        public class Response
        {
            public Guid[] Accepted { get; set; }

            // TODO: Change key to Guid when System.Text.Json
            // adds support for non-string Dictionary keys (.NET 5?)
            public Dictionary<string, string> Errors { get; set; }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly IVsphereService _vsphereService;
            private readonly IPlayerService _playerService;
            private readonly VmContext _dbContext;

            public Handler(
                IVsphereService vsphereService,
                IPlayerService playerService,
                VmContext dbContext)
            {
                _vsphereService = vsphereService;
                _playerService = playerService;
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var errorsDict = new Dictionary<Guid, string>();
                var acceptedList = new List<Guid>();

                var vms = await _dbContext.Vms
                    .Include(x => x.VmTeams)
                    .Where(x => request.Ids.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                foreach (var id in request.Ids)
                {
                    var vm = vms.Where(x => x.Id == id).FirstOrDefault();

                    if (vm == null)
                    {
                        errorsDict.Add(id, "Virtual Machine Not Found");
                    }
                    else if (vm.PowerState == PowerState.Unknown)
                    {
                        errorsDict.Add(id, "Unsupported Operation");
                    }
                    else if (!(await _playerService.CanAccessTeamsAsync(vm.VmTeams.Select(x => x.TeamId), cancellationToken)))
                    {
                        errorsDict.Add(id, "Unauthorized");
                    }
                    else
                    {
                        acceptedList.Add(id);
                    }
                }

                foreach (var vm in vms.Where(x => acceptedList.Contains(x.Id)))
                {
                    vm.HasPendingTasks = true;
                }

                await _dbContext.SaveChangesAsync();

                if (request.Operation == PowerOperation.Shutdown)
                {
                    var results = await _vsphereService.BulkShutdown(request.Ids);

                    errorsDict = errorsDict
                        .Concat(results)
                        .ToLookup(x => x.Key, x => x.Value)
                        .ToDictionary(x => x.Key, g => g.First());
                }
                else
                {
                    await _vsphereService.BulkPowerOperation(request.Ids, request.Operation);
                }

                return new Response
                {
                    Accepted = acceptedList.ToArray(),
                    Errors = errorsDict.Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key.ToString(), y => y.Value)
                };
            }
        }
    }
}
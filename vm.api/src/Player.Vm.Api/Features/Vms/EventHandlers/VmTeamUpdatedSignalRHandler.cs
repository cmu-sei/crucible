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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Player.Vm.Api.Data;
using Player.Vm.Api.Domain.Events;
using Player.Vm.Api.Domain.Services;
using Player.Vm.Api.Hubs;

namespace Player.Vm.Api.Features.Vms.EventHandlers
{
    public class VmTeamCreatedSignalRHandler : VmBaseSignalRHandler, INotificationHandler<EntityCreated<Domain.Models.VmTeam>>
    {
        public VmTeamCreatedSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub) : base(db, mapper, viewService, vmHub) { }

        public async Task Handle(EntityCreated<Domain.Models.VmTeam> notification, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            if (notification.Entity.Vm == null)
            {
                if (_db.Entry(notification.Entity).State == EntityState.Detached)
                {
                    _db.Attach(notification.Entity);
                }

                notification.Entity.Vm = await _db.Vms
                    .Where(x => x.Id == notification.Entity.VmId)
                    .Include(x => x.VmTeams)
                    .FirstOrDefaultAsync();
            }

            if (notification.Entity.Vm == null)
            {
                return;
            }

            var vm = _mapper.Map<Vm>(notification.Entity.Vm);
            var viewId = await _viewService.GetViewIdForTeam(notification.Entity.TeamId, cancellationToken);

            if (viewId.HasValue)
            {
                foreach (var teamId in notification.Entity.Vm.VmTeams.Select(x => x.TeamId))
                {
                    if (teamId != notification.Entity.TeamId)
                    {
                        var vId = await _viewService.GetViewIdForTeam(teamId, cancellationToken);

                        // if this vm was already on a team in the same view, don't notify that view again
                        if (vId.HasValue && vId.Value == viewId.Value)
                        {
                            viewId = null;
                            break;
                        }
                    }
                }

                if (viewId.HasValue)
                {
                    tasks.Add(_vmHub.Clients.Group(viewId.ToString()).SendAsync(VmHubMethods.VmCreated, vm, cancellationToken));
                }
            }

            tasks.Add(_vmHub.Clients.Group(notification.Entity.TeamId.ToString()).SendAsync(VmHubMethods.VmCreated, vm, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }

    public class VmTeamDeletedSignalRHandler : VmBaseSignalRHandler, INotificationHandler<EntityDeleted<Domain.Models.VmTeam>>
    {
        public VmTeamDeletedSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub) : base(db, mapper, viewService, vmHub) { }

        public async Task Handle(EntityDeleted<Domain.Models.VmTeam> notification, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            if (notification.Entity.Vm == null)
            {
                if (_db.Entry(notification.Entity).State == EntityState.Detached)
                {
                    _db.Attach(notification.Entity);
                }

                notification.Entity.Vm = await _db.Vms
                    .Where(x => x.Id == notification.Entity.VmId)
                    .Include(x => x.VmTeams)
                    .FirstOrDefaultAsync();
            }

            if (notification.Entity.Vm != null)
            {
                var vm = _mapper.Map<Vm>(notification.Entity.Vm);
                var viewId = await _viewService.GetViewIdForTeam(notification.Entity.TeamId, cancellationToken);

                if (viewId.HasValue)
                {
                    foreach (var teamId in notification.Entity.Vm.VmTeams.Select(x => x.TeamId))
                    {
                        if (teamId != notification.Entity.TeamId)
                        {
                            var vId = await _viewService.GetViewIdForTeam(teamId, cancellationToken);

                            // if this vm is still on a team in the same view, don't notify that view
                            if (vId.HasValue && vId.Value == viewId.Value)
                            {
                                viewId = null;
                                break;
                            }
                        }
                    }

                    if (viewId.HasValue)
                    {
                        tasks.Add(_vmHub.Clients.Group(viewId.ToString()).SendAsync(VmHubMethods.VmDeleted, notification.Entity.VmId, cancellationToken));
                    }
                }
            }

            tasks.Add(_vmHub.Clients.Group(notification.Entity.TeamId.ToString()).SendAsync(VmHubMethods.VmDeleted, notification.Entity.VmId, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}

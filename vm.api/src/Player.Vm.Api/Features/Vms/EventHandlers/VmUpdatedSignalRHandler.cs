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
using Player.Vm.Api.Infrastructure.Extensions;

namespace Player.Vm.Api.Features.Vms.EventHandlers
{
    public class VmBaseSignalRHandler
    {
        protected readonly VmContext _db;
        protected readonly IMapper _mapper;
        protected readonly IViewService _viewService;
        protected readonly IHubContext<VmHub> _vmHub;

        public VmBaseSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub)
        {
            _db = db;
            _mapper = mapper;
            _viewService = viewService;
            _vmHub = vmHub;
        }

        protected async Task<Guid[]> GetGroups(Domain.Models.Vm vm, CancellationToken cancellationToken)
        {
            var groupIds = new List<Guid>();

            foreach (var teamId in vm.VmTeams.Select(x => x.TeamId))
            {
                var viewId = await _viewService.GetViewIdForTeam(teamId, cancellationToken);

                if (viewId.HasValue && !groupIds.Any(v => v == viewId.Value))
                {
                    groupIds.Add(viewId.Value);
                }

                groupIds.Add(teamId);
            }

            return groupIds.ToArray();
        }

        protected async Task HandleCreateOrUpdate(
            Domain.Models.Vm vmEntity,
            string method,
            string[] modifiedProperties,
            CancellationToken cancellationToken)
        {
            if (!vmEntity.TeamsLoaded)
            {
                if (_db.Entry(vmEntity).State == EntityState.Detached)
                {
                    _db.Attach(vmEntity);
                }

                await _db.Entry(vmEntity)
                    .Collection(x => x.VmTeams)
                    .LoadAsync(cancellationToken);
            }

            var groupIds = await this.GetGroups(vmEntity, cancellationToken);
            var vm = _mapper.Map<Vm>(vmEntity);
            var tasks = new List<Task>();

            foreach (var groupId in groupIds)
            {
                tasks.Add(_vmHub.Clients.Group(groupId.ToString()).SendAsync(method, vm, modifiedProperties, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }

    public class VmCreatedSignalRHandler : VmBaseSignalRHandler, INotificationHandler<EntityCreated<Domain.Models.Vm>>
    {
        public VmCreatedSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub) : base(db, mapper, viewService, vmHub) { }

        public async Task Handle(EntityCreated<Domain.Models.Vm> notification, CancellationToken cancellationToken)
        {
            await base.HandleCreateOrUpdate(notification.Entity, VmHubMethods.VmCreated, null, cancellationToken);
        }
    }

    public class VmUpdatedSignalRHandler : VmBaseSignalRHandler, INotificationHandler<EntityUpdated<Domain.Models.Vm>>
    {
        public VmUpdatedSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub) : base(db, mapper, viewService, vmHub) { }

        public async Task Handle(EntityUpdated<Domain.Models.Vm> notification, CancellationToken cancellationToken)
        {
            await base.HandleCreateOrUpdate(
                notification.Entity,
                VmHubMethods.VmUpdated,
                notification.ModifiedProperties.Select(x => x.TitleCaseToCamelCase()).ToArray(),
                cancellationToken);
        }
    }

    public class VmDeletedSignalRHandler : VmBaseSignalRHandler, INotificationHandler<EntityDeleted<Domain.Models.Vm>>
    {
        public VmDeletedSignalRHandler(
            VmContext db,
            IMapper mapper,
            IViewService viewService,
            IHubContext<VmHub> vmHub) : base(db, mapper, viewService, vmHub)
        {
        }

        public async Task Handle(EntityDeleted<Domain.Models.Vm> notification, CancellationToken cancellationToken)
        {
            var groupIds = await base.GetGroups(notification.Entity, cancellationToken);
            var tasks = new List<Task>();

            foreach (var groupId in groupIds)
            {
                tasks.Add(_vmHub.Clients.Group(groupId.ToString()).SendAsync(VmHubMethods.VmDeleted, notification.Entity.Id, cancellationToken));
            }

            // If a vm was deleted without loading it's teams, just send the notification to everyone.
            // It's only an id, so no data leakage.
            if (!groupIds.Any())
            {
                tasks.Add(_vmHub.Clients.All.SendAsync(VmHubMethods.VmDeleted, notification.Entity.Id, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
    }
}

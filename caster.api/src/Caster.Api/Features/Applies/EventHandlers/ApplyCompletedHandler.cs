/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caster.Api.Data;
using Caster.Api.Domain.Events;
using Caster.Api.Domain.Models;
using Caster.Api.Domain.Services;
using MediatR;

namespace Caster.Api.Features.Applies.EventHandlers
{
    public class ApplyCompletedHandler : INotificationHandler<ApplyCompleted>
    {
        private readonly IPlayerSyncService _playerSyncService;
        private readonly CasterContext _dbContext;

        public ApplyCompletedHandler(IPlayerSyncService playerSyncService, CasterContext dbContext)
        {
            _playerSyncService = playerSyncService;
            _dbContext = dbContext;
        }

        public async Task Handle(ApplyCompleted notification, CancellationToken cancellationToken)
        {
            await _playerSyncService.AddAsync(notification.Workspace.Id);
            await this.ProcessRemovedResources(notification.Workspace);
        }

        private async Task ProcessRemovedResources(Workspace workspace)
        {
            var removedResources = workspace.GetRemovedResources();
            var resourcesToSync = removedResources
                .Where(r => r.GetTeamId().HasValue)
                .Select(r => new RemovedResource { Id = r.Id });

            await _dbContext.RemovedResources.AddRangeAsync(resourcesToSync);
            await _dbContext.SaveChangesAsync();

            if (resourcesToSync.Any())
                _playerSyncService.CheckRemovedResources();
        }
    }
}

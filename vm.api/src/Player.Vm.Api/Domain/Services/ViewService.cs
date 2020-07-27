/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Player.Vm.Api.Infrastructure.Options;
using S3.Player.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Player.Vm.Api.Domain.Services
{
    public interface IViewService
    {
        Task<Guid?> GetViewIdForTeam(Guid teamId, CancellationToken ct);
        Task<Guid[]> GetViewIdsForTeams(IEnumerable<Guid> teamIds, CancellationToken ct);
    }

    public class ViewService : IViewService, IDisposable
    {
        private readonly IS3PlayerApiClient _playerApiClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ViewService> _logger;

        public ViewService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ClientOptions clientOptions,
            ILogger<ViewService> logger)
        {
            _cache = cache;
            _logger = logger;

            var playerUri = new Uri(clientOptions.urls.playerApi);
            var httpClient = httpClientFactory.CreateClient("player-admin");
            httpClient.BaseAddress = playerUri;
            var playerApiClient = new S3PlayerApiClient(httpClient, true);
            playerApiClient.BaseUri = playerUri;
            _playerApiClient = playerApiClient;
        }


        public async Task<Guid?> GetViewIdForTeam(Guid teamId, CancellationToken ct)
        {
            Guid? viewId;

            if (!_cache.TryGetValue(teamId, out viewId))
            {
                try
                {
                    var team = await _playerApiClient.GetTeamAsync(teamId, ct);
                    viewId = team.ViewId;

                    _cache.Set(teamId, viewId, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(12)));
                }
                catch (Exception ex)
                {
                    viewId = null;
                    _logger.LogError(ex, $"Error Getting ViewId for TeamId: {teamId}");
                }
            }

            return viewId;
        }

        public async Task<Guid[]> GetViewIdsForTeams(IEnumerable<Guid> teamIds, CancellationToken ct)
        {
            var viewIds = new List<Guid>();

            foreach (var teamId in teamIds)
            {
                var viewId = await this.GetViewIdForTeam(teamId, ct);

                if (viewId.HasValue && !viewIds.Any(x => x == viewId.Value))
                {
                    viewIds.Add(viewId.Value);
                }
            }

            return viewIds.ToArray();
        }

        public void Dispose()
        {
            _playerApiClient.Dispose();
        }
    }
}

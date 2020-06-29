/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Http;
using Player.Vm.Api.Infrastructure.Extensions;
using S3.Player.Api;
using S3.Player.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Player.Vm.Api.Domain.Services
{
    public interface IPlayerService
    {
        Task<bool> IsSystemAdmin(CancellationToken ct);
        Task<bool> CanManageTeamsAsync(IEnumerable<Guid> teamIds, bool all, CancellationToken ct);
        Task<bool> CanManageTeamAsync(Guid teamId, CancellationToken ct);
        Task<bool> CanAccessTeamsAsync(IEnumerable<Guid> teamIds, CancellationToken ct);
        Task<bool> CanAccessTeamAsync(Guid teamId, CancellationToken ct);
        Task<IEnumerable<Team>> GetTeamsByViewIdAsync(Guid viewId, CancellationToken ct);
        Task<Guid> GetPrimaryTeamByViewIdAsync(Guid viewId, CancellationToken ct);
        Guid GetCurrentViewId();
    }

    public class PlayerService : IPlayerService
    {
        private readonly IS3PlayerApiClient _s3PlayerApiClient;
        private readonly Guid _userId;
        private Guid _currentViewId;
        private Dictionary<Guid, Team> _teamCache;

        public PlayerService(IHttpContextAccessor httpContextAccessor, IS3PlayerApiClient s3PlayerApiClient)
        {
            _teamCache = new Dictionary<Guid, Team>();
            _userId = httpContextAccessor.HttpContext.User.GetId();
            _s3PlayerApiClient = s3PlayerApiClient;
        }

        public async Task<bool> IsSystemAdmin(CancellationToken ct)
        {
            var user = await _s3PlayerApiClient.GetUserAsync(_userId);

            return user.IsSystemAdmin.Value;
        }

        public async Task<bool> CanManageTeamsAsync(IEnumerable<Guid> teamIds, bool all, CancellationToken ct)
        {
            var teamDict = new Dictionary<Guid, bool>();

            foreach (var teamId in teamIds)
            {
                teamDict.Add(teamId, false);

                try
                {
                    Team team;

                    if (!_teamCache.TryGetValue(teamId, out team))
                    {
                        team = await GetTeamById(teamId);

                        if (team == null)
                            continue;

                        _teamCache.Add(teamId, team);
                    }

                    _currentViewId = team.ViewId.Value;

                    if (team.CanManage.Value)
                    {
                        teamDict[teamId] = true;

                        if (!all)
                            return true;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return !teamDict.Values.Any(v => v == false);
        }

        public async Task<bool> CanManageTeamAsync(Guid teamId, CancellationToken ct)
        {
            return await CanManageTeamsAsync(new List<Guid> { teamId }, true, ct);
        }

        public async Task<bool> CanAccessTeamsAsync(IEnumerable<Guid> teamIds, CancellationToken ct)
        {
            foreach (var teamId in teamIds)
            {
                try
                {
                    Team team;

                    if (!_teamCache.TryGetValue(teamId, out team))
                    {
                        team = await GetTeamById(teamId);

                        if (team == null)
                            continue;

                        _teamCache.Add(teamId, team);
                    }

                    _currentViewId = team.ViewId.Value;

                    if (team.CanManage.Value || team.IsPrimary.Value)
                        return true;
                }
                catch (Exception ex)
                {
                }
            }

            return false;
        }

        public async Task<bool> CanAccessTeamAsync(Guid teamId, CancellationToken ct)
        {
            return await CanAccessTeamsAsync(new List<Guid> { teamId }, ct);
        }

        public async Task<IEnumerable<Team>> GetTeamsByViewIdAsync(Guid viewId, CancellationToken ct)
        {
            _currentViewId = viewId;

            var teams = await _s3PlayerApiClient.GetUserViewTeamsAsync(viewId, _userId, ct);

            foreach (Team team in teams)
            {
                if (!_teamCache.ContainsKey(team.Id.Value))
                {
                    _teamCache.Add(team.Id.Value, team);
                }
            }

            return teams.Where(t => (t.IsPrimary.HasValue && t.IsPrimary.Value) || t.CanManage.Value);
        }

        public async Task<Guid> GetPrimaryTeamByViewIdAsync(Guid viewId, CancellationToken ct)
        {
            _currentViewId = viewId;
            var teams = await _s3PlayerApiClient.GetUserViewTeamsAsync(viewId, _userId, ct);

            foreach (Team team in teams)
            {
                if (!_teamCache.ContainsKey(team.Id.Value))
                {
                    _teamCache.Add(team.Id.Value, team);
                }
            }

            return teams
                .Where(t => t.IsPrimary.Value)
                .Select(t => t.Id.Value)
                .FirstOrDefault();
        }

        public Guid GetCurrentViewId()
        {
            return _currentViewId;
        }

        public async Task<Team> GetTeamById(Guid id)
        {
            try
            {
                return await _s3PlayerApiClient.GetTeamAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

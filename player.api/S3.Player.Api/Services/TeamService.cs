/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace S3.Player.Api.Services
{
    public interface ITeamService
    {
        Task<IEnumerable<ViewModels.Team>> GetAsync(CancellationToken ct);
        Task<ViewModels.Team> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ViewModels.Team>> GetByViewIdAsync(Guid viewId, CancellationToken ct);
        Task<IEnumerable<ViewModels.Team>> GetByViewIdForUserAsync(Guid viewId, Guid userId, CancellationToken ct);
        Task<ViewModels.Team> CreateAsync(Guid viewId, TeamForm form, CancellationToken ct);
        Task<ViewModels.Team> UpdateAsync(Guid id, TeamForm form, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
        Task<ViewModels.Team> SetPrimaryAsync(Guid teamId, Guid userId, CancellationToken ct);
    }

    public class TeamService : ITeamService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private IUserClaimsService _claimsService;


        public TeamService(PlayerContext context, IPrincipal user, IAuthorizationService authorizationService, IMapper mapper, IUserClaimsService claimsService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<IEnumerable<Team>> GetAsync(CancellationToken ct)
        {
             if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Teams.ProjectTo<TeamDTO>().ToListAsync(ct);
            return _mapper.Map<IEnumerable<Team>>(items);
        }

        public async Task<IEnumerable<Team>> GetByViewIdAsync(Guid viewId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(viewId))).Succeeded)
                throw new ForbiddenException();

            var viewExists = _context.Views
                .Where(e => e.Id == viewId)
                .DeferredAny()
                .FutureValue();

            var teamsQuery = _context.Teams
                .Where(e => e.ViewId == viewId)
                .ProjectTo<TeamDTO>();
                //.Future();

            if (!(await viewExists.ValueAsync()))
                throw new EntityNotFoundException<View>();

            var teams = await teamsQuery.ToListAsync();

            return _mapper.Map<IEnumerable<Team>>(teams);
        }

        public async Task<IEnumerable<Team>> GetByViewIdForUserAsync(Guid viewId, Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserOrViewAdminRequirement(viewId, userId))).Succeeded)
                throw new ForbiddenException();

            var viewExists = _context.Views
                .Where(e => e.Id == viewId)
                .DeferredAny()
                .FutureValue();

            var userExists = _context.Users
                .Where(u => u.Id == userId)
                .DeferredAny()
                .FutureValue();

            //QueryFutureEnumerable<TeamDTO> teamQuery;
            IQueryable<TeamDTO> teamQuery;

            if ((await _authorizationService.AuthorizeAsync(await _claimsService.GetClaimsPrincipal(userId, true), null, new ViewAdminRequirement(viewId))).Succeeded)
            {
                teamQuery = _context.Teams
                    .Where(t => t.ViewId == viewId)
                    .ProjectTo<TeamDTO>();
                    //.Future();
            }
            else
            {
                teamQuery = _context.TeamMemberships
                .Where(x => x.UserId == userId && x.Team.ViewId == viewId)
                .Select(x => x.Team)
                .Distinct()
                .ProjectTo<TeamDTO>();
                //.Future();
            }

            var teams = await teamQuery.ToListAsync();

            if (!(await userExists.ValueAsync()))
                throw new EntityNotFoundException<User>();

            if (!(await viewExists.ValueAsync()))
                throw new EntityNotFoundException<View>();

            return _mapper.Map<IEnumerable<Team>>(teams);
        }

        public async Task<Team> GetAsync(Guid id, CancellationToken ct)
        {
            var team = await _context.Teams
                .ProjectTo<TeamDTO>()
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            if(team != null)
            {
                if (!(await _authorizationService.AuthorizeAsync(_user, null, new TeamAccessRequirement(team.ViewId, id))).Succeeded)
                    throw new ForbiddenException();
            }

            return _mapper.Map<Team>(team);
        }

        public async Task<Team> CreateAsync(Guid viewId, ViewModels.TeamForm form, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(viewId))).Succeeded)
                throw new ForbiddenException();

            var viewEntity = await _context.Views.SingleOrDefaultAsync(e => e.Id == viewId, ct);

            if (viewEntity == null)
                throw new EntityNotFoundException<View>();

            var teamEntity = _mapper.Map<TeamEntity>(form);

            viewEntity.Teams.Add(teamEntity);
            await _context.SaveChangesAsync(ct);

            var team = await GetAsync(teamEntity.Id, ct);
            return _mapper.Map<Team>(team);
        }

        public async Task<Team> UpdateAsync(Guid id, ViewModels.TeamForm form, CancellationToken ct)
        {
            var teamToUpdate = await _context.Teams.SingleOrDefaultAsync(t => t.Id == id, ct);

            if (teamToUpdate == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(teamToUpdate.ViewId))).Succeeded)
                throw new ForbiddenException();

            _mapper.Map(form, teamToUpdate);

            _context.Teams.Update(teamToUpdate);
            await _context.SaveChangesAsync(ct);

            var team = await GetAsync(id, ct);
            return _mapper.Map<Team>(team);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var teamToDelete = await _context.Teams.SingleOrDefaultAsync(t => t.Id == id, ct);

            if (teamToDelete == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(teamToDelete.ViewId))).Succeeded)
                throw new ForbiddenException();

            _context.Teams.Remove(teamToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<Team> SetPrimaryAsync(Guid teamId, Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded)
                throw new ForbiddenException("You can only change your own Primary Team.");

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new TeamMemberRequirement(teamId))).Succeeded)
                throw new ForbiddenException("You can only change your Primary Team to a Team that you are a member of");

            var teamEntity = await _context.Teams.SingleOrDefaultAsync(t => t.Id == teamId, ct);
            var viewMembership = await _context.ViewMemberships
                .Include(m => m.TeamMemberships)
                .SingleOrDefaultAsync(m => m.ViewId == teamEntity.ViewId && m.UserId == userId);

            var teamMembership = viewMembership.TeamMemberships.Where(m => m.TeamId == teamId).FirstOrDefault();

            viewMembership.PrimaryTeamMembershipId = teamMembership.Id;
            _context.ViewMemberships.Update(viewMembership);
            await _context.SaveChangesAsync(ct);

            await _claimsService.RefreshClaims(userId);

            var team = await GetAsync(teamId, ct);
            return _mapper.Map<Team>(team);
        }
    }
}

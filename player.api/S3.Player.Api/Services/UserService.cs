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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Extensions;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.ViewModels;
using Z.EntityFramework.Plus;

namespace S3.Player.Api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ViewModels.User>> GetAsync(CancellationToken ct);
        Task<IEnumerable<ViewModels.User>> GetByTeamAsync(Guid teamId, CancellationToken ct);
        Task<IEnumerable<ViewModels.User>> GetByViewAsync(Guid viewId, CancellationToken ct);
        Task<ViewModels.User> GetAsync(Guid id, CancellationToken ct);
        Task<ViewModels.User> CreateAsync(ViewModels.User user, CancellationToken ct);
        Task<ViewModels.User> UpdateAsync(Guid id, ViewModels.User user, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
        Task<bool> AddToTeamAsync(Guid teamId, Guid userId, CancellationToken ct);
        Task<bool> RemoveFromTeamAsync(Guid teamId, Guid userId, CancellationToken ct);
    }

    public class UserService : IUserService
    {
        private readonly PlayerContext _context;
        private readonly ClaimsPrincipal _user;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserClaimsService _userClaimsService;

        public UserService(PlayerContext context, IPrincipal user, IAuthorizationService authorizationService, IUserClaimsService userClaimsService)
        {
            _context = context;
            _user = user as ClaimsPrincipal;
            _authorizationService = authorizationService;
            _userClaimsService = userClaimsService;
        }

        public async Task<IEnumerable<ViewModels.User>> GetAsync(CancellationToken ct)
        {
            if(!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Users.ProjectTo<ViewModels.User>().ToArrayAsync(ct);
            return items;
        }

        public async Task<IEnumerable<ViewModels.User>> GetByTeamAsync(Guid teamId, CancellationToken ct)
        {
            var teamQuery = _context.Teams
                .Where(t => t.Id == teamId)
                .Future();

            var userQuery = _context.TeamMemberships
                .Where(t => t.TeamId == teamId)
                .Select(m => m.User)
                .Distinct()
                .ProjectTo<User>();
                //.Future();

            var team = (await teamQuery.ToListAsync()).FirstOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new TeamAccessRequirement(team.ViewId, team.Id))).Succeeded)
                throw new ForbiddenException();

            return await userQuery.ToListAsync();
        }

        public async Task<IEnumerable<ViewModels.User>> GetByViewAsync(Guid viewId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(viewId))).Succeeded)
                throw new ForbiddenException();

            var viewQuery = _context.Views
                .Where(e => e.Id == viewId)
                .Future();

            var view = (await viewQuery.ToListAsync()).FirstOrDefault();

            if (view == null)
                throw new EntityNotFoundException<View>();

            var users = _context.ViewMemberships
                .Where(m => m.ViewId == viewId)
                .Select(m => m.User)
                .Distinct()
                .ProjectTo<User>();

            return await users.ToListAsync();
        }

        public async Task<ViewModels.User> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Users.ProjectTo<ViewModels.User>().SingleOrDefaultAsync(o => o.Id == id, ct);
            return item;
        }

        public async Task<ViewModels.User> CreateAsync(ViewModels.User user, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var userEntity = Mapper.Map<UserEntity>(user);

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(user.Id, ct);
        }

        public async Task<ViewModels.User> UpdateAsync(Guid id, ViewModels.User user, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            // Don't allow changing your own Id
            if (id == _user.GetId() && id != user.Id)
            {
                throw new ForbiddenException("You cannot change your own Id");
            }

            var userToUpdate = await _context.Users.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (userToUpdate == null)
                throw new EntityNotFoundException<User>();

            Mapper.Map(user, userToUpdate);

            _context.Users.Update(userToUpdate);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            if (id == _user.GetId())
            {
                throw new ForbiddenException("You cannot delete your own account");
            }

            var userToDelete = await _context.Users.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (userToDelete == null)
                throw new EntityNotFoundException<User>();

            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> AddToTeamAsync(Guid teamId, Guid userId, CancellationToken ct)
        {
            var teamQuery = _context.Teams.Where(t => t.Id == teamId).Future();
            var userExists = _context.Users.Where(u => u.Id == userId).DeferredAny().FutureValue();

            var viewIdQuery = _context.Teams.Where(t => t.Id == teamId).Select(t => t.ViewId);

            var viewMembershipQuery = _context.ViewMemberships
                .Where(x => x.UserId == userId && viewIdQuery.Contains(x.ViewId))
                .Future();

            var team = (await teamQuery.ToListAsync()).SingleOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if(!(await userExists.ValueAsync()))
                throw new EntityNotFoundException<User>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(team.ViewId))).Succeeded)
                throw new ForbiddenException();

            var viewMembership = (await viewMembershipQuery.ToListAsync()).FirstOrDefault();

            bool setPrimary = false;
            if (viewMembership == null)
            {
                viewMembership = new ViewMembershipEntity { ViewId = team.ViewId, UserId = userId };
                _context.ViewMemberships.Add(viewMembership);
                await _context.SaveChangesAsync(ct);
                setPrimary = true;
            }

            var teamMembership = new TeamMembershipEntity { ViewMembershipId = viewMembership.Id, UserId = userId, TeamId = teamId };

            if (setPrimary)
            {
                viewMembership.PrimaryTeamMembership = teamMembership;
            }

            _context.TeamMemberships.Add(teamMembership);

            await _context.SaveChangesAsync(ct);
            await _userClaimsService.RefreshClaims(userId);

            return true;
        }

        public async Task<bool> RemoveFromTeamAsync(Guid teamId, Guid userId, CancellationToken ct)
        {
            var teamQuery = _context.Teams.Where(t => t.Id == teamId).Future();
            var userExists = _context.Users.Where(u => u.Id == userId).DeferredAny().FutureValue();
            var teamMembershipQuery = _context.TeamMemberships.Include(m => m.Team).Where(m => m.UserId == userId).Future();

            var team = (await teamQuery.ToListAsync()).SingleOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (!(await userExists.ValueAsync()))
                throw new EntityNotFoundException<User>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(team.ViewId))).Succeeded)
                throw new ForbiddenException();

            var teamMemberships = await teamMembershipQuery.ToListAsync();
            var teamMembership = teamMemberships.SingleOrDefault(tu => tu.TeamId == teamId);

            if (teamMembership != null)
            {
                var viewMembership = _context.ViewMemberships.SingleOrDefault(eu => eu.UserId == userId && eu.ViewId == team.ViewId);

                if (teamMemberships.Where(m => m.Team.ViewId == team.ViewId).Count() == 1)
                {
                    _context.TeamMemberships.Remove(teamMembership);
                    viewMembership.PrimaryTeamMembershipId = null;
                    await _context.SaveChangesAsync();

                    _context.ViewMemberships.Remove(viewMembership);
                }
                else if (viewMembership.PrimaryTeamMembershipId == teamMembership.Id)
                {
                    // Set a new primary Team if we are deleting the current one
                    Guid newPrimaryTeamMembershipId = teamMemberships.Where(m => m.Team.ViewId == team.ViewId && m.TeamId != teamId).FirstOrDefault().Id;
                    viewMembership.PrimaryTeamMembershipId = newPrimaryTeamMembershipId;
                    _context.ViewMemberships.Update(viewMembership);
                    await _context.SaveChangesAsync(ct);

                    _context.TeamMemberships.Remove(teamMembership);
                }
                else
                {
                    _context.TeamMemberships.Remove(teamMembership);
                }

                await _context.SaveChangesAsync(ct);
            }

            await _userClaimsService.RefreshClaims(userId);
            return true;
        }
    }
}

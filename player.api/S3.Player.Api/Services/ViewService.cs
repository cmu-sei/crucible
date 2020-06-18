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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Extensions;
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

namespace S3.Player.Api.Services
{
    public interface IViewService
    {
        Task<IEnumerable<ViewModels.View>> GetAsync(CancellationToken ct);
        Task<ViewModels.View> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ViewModels.View>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ViewModels.View> CreateAsync(ViewModels.View view, CancellationToken ct);
        Task<View> CloneAsync(Guid id, CancellationToken ct);
        Task<ViewModels.View> UpdateAsync(Guid id, ViewModels.View view, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class ViewService : IViewService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private IUserClaimsService _claimsService;

        public ViewService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user, IMapper mapper, IUserClaimsService claimsService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<IEnumerable<ViewModels.View>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Views
                .ToListAsync(ct);

            return _mapper.Map<IEnumerable<View>>(items);
        }

        public async Task<ViewModels.View> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(_user, null, new ViewMemberRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Views
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<View>(item);
        }

        public async Task<IEnumerable<ViewModels.View>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded)
                throw new ForbiddenException();

            var user = await _context.Users
                .Include(u => u.ViewMemberships)
                    .ThenInclude(em => em.View)
                .Where(u => u.Id == userId)
                .SingleOrDefaultAsync(ct);

            if (user == null)
                throw new EntityNotFoundException<User>();

            var views = user.ViewMemberships.Select(x => x.View);

            return _mapper.Map<IEnumerable<ViewModels.View>>(views);
        }

        public async Task<ViewModels.View> CreateAsync(ViewModels.View view, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewCreationRequirement())).Succeeded)
                throw new ForbiddenException();

            var viewEntity = Mapper.Map<ViewEntity>(view);

            var viewAdminPermission = await _context.Permissions
                .Where(p => p.Key == PlayerClaimTypes.ViewAdmin.ToString())
                .FirstOrDefaultAsync(ct);

            if (viewAdminPermission == null)
                throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ViewAdmin.ToString()} Permission not found.");

            var userId = _user.GetId();

            // Create an Admin team with the caller as a member
            var teamEntity = new TeamEntity() { Name = "Admin" };
            teamEntity.Permissions.Add(new TeamPermissionEntity() { Permission = viewAdminPermission });

            var viewMembershipEntity = new ViewMembershipEntity { View = viewEntity, UserId = userId };
            viewEntity.Teams.Add(teamEntity);
            viewEntity.Memberships.Add(viewMembershipEntity);

            _context.Views.Add(viewEntity);
            await _context.SaveChangesAsync(ct);

            var teamMembershipEntity = new TeamMembershipEntity { Team = teamEntity, UserId = userId, ViewMembership = viewMembershipEntity };
            viewMembershipEntity.PrimaryTeamMembership = teamMembershipEntity;
            _context.TeamMemberships.Add(teamMembershipEntity);
            _context.ViewMemberships.Update(viewMembershipEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(viewEntity.Id, ct);
        }

        public async Task<View> CloneAsync(Guid idToBeCloned, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewCreationRequirement())).Succeeded)
                throw new ForbiddenException();

            var view = await _context.Views
                .Include(o => o.Teams)
                    .ThenInclude(o => o.Applications)
                .Include(o => o.Teams)
                    .ThenInclude(o => o.Permissions)
                .Include(o => o.Applications)
                    .ThenInclude(o => o.Template)
                .SingleOrDefaultAsync(o => o.Id == idToBeCloned, ct);

            var newView = view.Clone();
            newView.Name = $"Clone of {newView.Name}";

            //copy view applications
            foreach (var application in view.Applications)
            {
                var newApplication = application.Clone();
                newView.Applications.Add(newApplication);
            }

            //copy teams
            foreach (var team in view.Teams)
            {
                var newTeam = team.Clone();

                //copy team applications
                foreach (var applicationInstance in team.Applications)
                {
                    var newApplicationInstance = applicationInstance.Clone();

                    var application = view.Applications.FirstOrDefault(o => o.Id == applicationInstance.ApplicationId);
                    var newApplication = newView.Applications.FirstOrDefault(o => application != null && o.GetName() == application.GetName());

                    newApplicationInstance.Application = newApplication;

                    newTeam.Applications.Add(newApplicationInstance);
                }

                //copy team permissions
                foreach (var permission in team.Permissions)
                {
                    var newPermission = new TeamPermissionEntity(newTeam.Id, permission.PermissionId);
                    newTeam.Permissions.Add(newPermission);
                }

                newView.Teams.Add(newTeam);
            }

            _context.Add(newView);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map<View>(newView);
        }

        public async Task<ViewModels.View> UpdateAsync(Guid id, ViewModels.View view, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var viewToUpdate = await _context.Views.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (viewToUpdate == null)
                throw new EntityNotFoundException<View>();

            Mapper.Map(view, viewToUpdate);

            _context.Views.Update(viewToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(viewToUpdate, view);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var viewToDelete = await _context.Views.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (viewToDelete == null)
                throw new EntityNotFoundException<View>();

            _context.Views.Remove(viewToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }
    }
}

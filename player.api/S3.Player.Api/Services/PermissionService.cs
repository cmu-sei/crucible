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
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetAsync();
        Task<Permission> GetAsync(Guid id);
        Task<IEnumerable<Permission>> GetByViewIdForUserAsync(Guid viewId, Guid userId);
        Task<IEnumerable<Permission>> GetByTeamIdForUserAsync(Guid teamId, Guid userId);
        Task<Permission> CreateAsync(PermissionForm form);
        Task<Permission> UpdateAsync(Guid id, PermissionForm form);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AddToRoleAsync(Guid roleId, Guid permissionId);
        Task<bool> RemoveFromRoleAsync(Guid roleId, Guid permissionId);
        Task<bool> AddToTeamAsync(Guid teamId, Guid permissionId);
        Task<bool> RemoveFromTeamAsync(Guid teamId, Guid permissionId);
        Task<bool> AddToUserAsync(Guid userId, Guid permissionId);
        Task<bool> RemoveFromUserAsync(Guid userId, Guid permissionId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;

        public PermissionService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
        }

        public async Task<IEnumerable<Permission>> GetAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Permissions
                .ProjectTo<ViewModels.Permission>()
                .ToListAsync();

            return items;
        }

        public async Task<Permission> GetAsync(Guid id)
        {
            var item = await _context.Permissions
                .ProjectTo<Permission>()
                .SingleOrDefaultAsync(o => o.Id == id);

            return item;
        }

        public async Task<Permission> CreateAsync(PermissionForm form)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var permissionEntity = Mapper.Map<PermissionEntity>(form);

            _context.Permissions.Add(permissionEntity);
            await _context.SaveChangesAsync();

            return Mapper.Map<Permission>(permissionEntity);
        }

        public async Task<Permission> UpdateAsync(Guid id, PermissionForm form)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var permissionToUpdate = await _context.Permissions.SingleOrDefaultAsync(v => v.Id == id);

            if (permissionToUpdate == null)
                throw new EntityNotFoundException<Permission>();

            if (permissionToUpdate.ReadOnly)
                throw new ForbiddenException("Cannot update a Read-Only Permission");

            Mapper.Map(form, permissionToUpdate);

            _context.Permissions.Update(permissionToUpdate);
            await _context.SaveChangesAsync();

            return Mapper.Map<Permission>(permissionToUpdate);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var permissionToDelete = await _context.Permissions.SingleOrDefaultAsync(t => t.Id == id);

            if (permissionToDelete == null)
                throw new EntityNotFoundException<Permission>();

            if (permissionToDelete.ReadOnly)
                throw new ForbiddenException("Cannot delete a Read-Only Permission");

            _context.Permissions.Remove(permissionToDelete);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Permission>> GetByViewIdForUserAsync(Guid viewId, Guid userId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserOrViewAdminRequirement(viewId, userId))).Succeeded)
                throw new ForbiddenException();

            var userQuery = _context.Users
                .Include(u => u.Role.Permissions)
                .Where(u => u.Id == userId);

            UserEntity user = (await userQuery.ToListAsync()).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException<User>();

            return await GetPermissions(viewId, user);
        }

        public async Task<IEnumerable<Permission>> GetByTeamIdForUserAsync(Guid teamId, Guid userId)
        {
            var userQuery = _context.Users
                .Include(u => u.Role.Permissions)
                .Where(u => u.Id == userId)
                .Future();

            var teamQuery = _context.Teams
                .Where(t => t.Id == teamId)
                .Future();

            UserEntity user = (await userQuery.ToListAsync()).FirstOrDefault();
            TeamEntity team = (await teamQuery.ToListAsync()).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException<User>();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserOrViewAdminRequirement(team.ViewId, userId))).Succeeded)
                throw new ForbiddenException();

            return await GetPermissions(team.ViewId, user);
        }

        private async Task<IEnumerable<Permission>> GetPermissions(Guid viewId, UserEntity user)
        {
            var viewMembershipQuery = _context.ViewMemberships
                .Include(x => x.PrimaryTeamMembership)
                .ThenInclude(m => m.Role)
                .ThenInclude(r => r.Permissions)
                .ThenInclude(p => p.Permission)
                .Include(x => x.PrimaryTeamMembership)
                .ThenInclude(m => m.Team)
                .ThenInclude(t => t.Role)
                .ThenInclude(r => r.Permissions)
                .ThenInclude(p => p.Permission)
                .Include(x => x.PrimaryTeamMembership)
                .ThenInclude(m => m.Team.Permissions)
                .ThenInclude(p => p.Permission)
                .Where(x => x.ViewId == viewId && x.UserId == user.Id);
            //.Future() // TODO: Doesn't load all includes - bug in library?

            ViewMembershipEntity membership = (await viewMembershipQuery.ToListAsync()).FirstOrDefault();

            List<PermissionEntity> permissions = new List<PermissionEntity>();

            if (membership != null)
            {
                if (membership.PrimaryTeamMembership != null)
                {
                    permissions.Add(new PermissionEntity {Key = "TeamMember", Value = membership.PrimaryTeamMembership.TeamId.ToString()});

                    if (membership.PrimaryTeamMembership.Role != null)
                    {
                        permissions.AddRange(membership.PrimaryTeamMembership.Role.Permissions.Select(x => x.Permission));
                    }

                    if (membership.PrimaryTeamMembership.Team != null)
                    {
                        if (membership.PrimaryTeamMembership.Team.Role != null)
                        {
                            permissions.AddRange(membership.PrimaryTeamMembership.Team.Role.Permissions.Select(x => x.Permission));
                        }

                        if (membership.PrimaryTeamMembership.Team.Permissions.Any())
                        {
                            permissions.AddRange(membership.PrimaryTeamMembership.Team.Permissions.Select(x => x.Permission));
                        }
                    }
                }
            }
            else
            {
                if (user.Role != null)
                {
                    permissions.AddRange(user.Role.Permissions.Select(x => x.Permission));
                }
            }

            return Mapper.Map<IEnumerable<Permission>>(permissions);
        }

        public async Task<bool> AddToRoleAsync(Guid roleId, Guid permissionId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var roleQuery = _context.Roles
                .Where(r => r.Id == roleId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var role = (await roleQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();

            if (role == null)
                throw new EntityNotFoundException<Role>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            role.Permissions.Add(new RolePermissionEntity(roleId, permissionId));
            _context.Roles.Update(role);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromRoleAsync(Guid roleId, Guid permissionId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var roleQuery = _context.Roles
                .Where(r => r.Id == roleId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var rolePermissionQuery = _context.RolePermissions
                .Where(x => x.RoleId == roleId && x.PermissionId == permissionId)
                .Future();

            var role = (await roleQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();
            var rolePermission = (await rolePermissionQuery.ToListAsync()).FirstOrDefault();

            if (role == null)
                throw new EntityNotFoundException<Role>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            if (rolePermission != null)
            {
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> AddToTeamAsync(Guid teamId, Guid permissionId)
        {
            var teamQuery = _context.Teams
                .Where(t => t.Id == teamId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var team = (await teamQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(team.ViewId))).Succeeded)
                throw new ForbiddenException();

            team.Permissions.Add(new TeamPermissionEntity(teamId, permissionId));
            _context.Teams.Update(team);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromTeamAsync(Guid teamId, Guid permissionId)
        {
            var teamQuery = _context.Teams
                .Where(t => t.Id == teamId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var teamPermissionQuery = _context.TeamPermissions
                .Where(x => x.TeamId == teamId && x.PermissionId == permissionId)
                .Future();

            var team = (await teamQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();
            var teamPermission = (await teamPermissionQuery.ToListAsync()).FirstOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(team.ViewId))).Succeeded)
                throw new ForbiddenException();

            if (teamPermission != null)
            {
                _context.TeamPermissions.Remove(teamPermission);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> AddToUserAsync(Guid userId, Guid permissionId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var userQuery = _context.Users
                .Where(t => t.Id == userId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var user = (await userQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException<User>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            user.Permissions.Add(new UserPermissionEntity(userId, permissionId));
            _context.Users.Update(user);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromUserAsync(Guid userId, Guid permissionId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var userQuery = _context.Users
                .Where(t => t.Id == userId)
                .Future();

            var permissionQuery = _context.Permissions
                .Where(p => p.Id == permissionId)
                .Future();

            var userPermissionQuery = _context.UserPermissions
                .Where(x => x.UserId == userId && x.PermissionId == permissionId)
                .Future();

            var user = (await userQuery.ToListAsync()).FirstOrDefault();
            var permission = (await permissionQuery.ToListAsync()).FirstOrDefault();
            var userPermission = (await userPermissionQuery.ToListAsync()).FirstOrDefault();

            if (user == null)
                throw new EntityNotFoundException<User>();

            if (permission == null)
                throw new EntityNotFoundException<Permission>();

            if (userPermission != null)
            {
                if (permission.Key == PlayerClaimTypes.SystemAdmin.ToString() && userId == _user.GetId())
                    throw new ForbiddenException($"You cannot remove the {PlayerClaimTypes.SystemAdmin.ToString()} permission from yourself.");

                _context.UserPermissions.Remove(userPermission);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}

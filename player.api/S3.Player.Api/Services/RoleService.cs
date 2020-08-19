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
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAsync();
        Task<Role> GetAsync(Guid id);
        Task<Role> GetAsync(string name);
        Task<Role> CreateAsync(RoleForm form);
        Task<Role> UpdateAsync(Guid id, RoleForm form);
        Task<bool> DeleteAsync(Guid id);
    }

    public class RoleService : IRoleService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;

        public RoleService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
        }

        public async Task<IEnumerable<Role>> GetAsync()
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Roles
                .ProjectTo<Role>()
                .ToListAsync();

            return items;
        }

        public async Task<Role> GetAsync(Guid id)
        {
            var item = await _context.Roles
                .ProjectTo<Role>()
                .SingleOrDefaultAsync(o => o.Id == id);

            return item;
        }

        public async Task<Role> GetAsync(string name)
        {
            var item = await _context.Roles
                .ProjectTo<Role>()
                .SingleOrDefaultAsync(o => o.Name == name);
            
            if (item == null)
            {
                throw new EntityNotFoundException<Role>();
            }
            
            return item;
        }

        public async Task<Role> CreateAsync(RoleForm form)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            // Ensure role with this name does not already exist
            var role = await _context.Roles
                .ProjectTo<Role>()
                .SingleOrDefaultAsync(o => o.Name == form.Name);
            if (role != null)
            {
                throw new ConflictException("A role with that name already exists.");
            }

            var roleEntity = Mapper.Map<RoleEntity>(form);

            _context.Roles.Add(roleEntity);
            await _context.SaveChangesAsync();

            return Mapper.Map<Role>(roleEntity);
        }

        public async Task<Role> UpdateAsync(Guid id, RoleForm form)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var roleToUpdate = await _context.Roles.SingleOrDefaultAsync(v => v.Id == id);

            if (roleToUpdate == null)
                throw new EntityNotFoundException<Role>();

            Mapper.Map(form, roleToUpdate);

            _context.Roles.Update(roleToUpdate);
            await _context.SaveChangesAsync();

            return await GetAsync(roleToUpdate.Id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var roleToDelete = await _context.Roles.SingleOrDefaultAsync(t => t.Id == id);

            if (roleToDelete == null)
                throw new EntityNotFoundException<Role>();

            _context.Roles.Remove(roleToDelete);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

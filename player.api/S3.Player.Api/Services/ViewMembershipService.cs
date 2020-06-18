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
    public interface IViewMembershipService
    {
        Task<ViewMembership> GetAsync(Guid id);
        Task<IEnumerable<ViewMembership>> GetByUserIdAsync(Guid userId);
    }

    public class ViewMembershipService : IViewMembershipService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;

        public ViewMembershipService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
        }

        public async Task<ViewMembership> GetAsync(Guid id)
        {
            var item = await _context.ViewMemberships
                .ProjectTo<ViewMembership>()
                .SingleOrDefaultAsync(o => o.Id == id);

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserOrViewAdminRequirement(item.ViewId, item.UserId))).Succeeded)
                throw new ForbiddenException();

            return item;
        }

        public async Task<IEnumerable<ViewMembership>> GetByUserIdAsync(Guid userId)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded)
                throw new ForbiddenException();

            var userExists = _context.Users
                .Where(u => u.Id == userId)
                .DeferredAny()
                .FutureValue();

            var membershipQuery = _context.ViewMemberships
                .Where(m => m.UserId == userId)
                .ProjectTo<ViewMembership>()
                .Future();

            if (!(await userExists.ValueAsync()))
                throw new EntityNotFoundException<User>();

            return await membershipQuery.ToListAsync();
        }
    }
}

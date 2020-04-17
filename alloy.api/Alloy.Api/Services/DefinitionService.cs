/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Alloy.Api.Data;
using Alloy.Api.Data.Models;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Authorization;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.ViewModels;

namespace Alloy.Api.Services
{
    public interface IDefinitionService
    {
        Task<IEnumerable<ViewModels.Definition>> GetAsync(CancellationToken ct);
        Task<ViewModels.Definition> GetAsync(Guid id, CancellationToken ct);
        // Task<IEnumerable<ViewModels.Definition>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ViewModels.Definition> CreateAsync(ViewModels.Definition definition, CancellationToken ct);
        Task<ViewModels.Definition> UpdateAsync(Guid id, ViewModels.Definition definition, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class DefinitionService : IDefinitionService
    {
        private readonly AlloyContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ILogger<DefinitionService> _logger;
        private readonly IUserClaimsService _claimsService;

        public DefinitionService(
            AlloyContext context,
            IAuthorizationService authorizationService,
            IPrincipal user,
            IMapper mapper,
            ILogger<DefinitionService> logger,
            IUserClaimsService claimsService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _logger = logger;
            _claimsService = claimsService;
        }

        /// <summary>
        /// Get all definitions
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>Definitions</returns>
        public async Task<IEnumerable<ViewModels.Definition>> GetAsync(CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new BasicRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            List<DefinitionEntity> items;
            if ((await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded ||
                (await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded)
            {
                items = await _context.Definitions.ToListAsync(ct);
            }
            else
            {
                items = await _context.Definitions.Where(d => d.IsPublished).ToListAsync(ct);
            }

            return _mapper.Map<IEnumerable<Definition>>(items);
        }

        /// <summary>
        /// Get a single Definition
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="ct"></param>
        /// <returns>The Definition</returns>
        public async Task<ViewModels.Definition> GetAsync(Guid id, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new BasicRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Definitions
                .SingleOrDefaultAsync(o => o.Id == id, ct);
            if (!item.IsPublished &&
                !(  (await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded ||
                    (await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded))
                throw new ForbiddenException();

            return _mapper.Map<Definition>(item);
        }

        /// <summary>
        /// Create a Definition
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ViewModels.Definition> CreateAsync(ViewModels.Definition definition, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded)
            {
                throw new ForbiddenException();
            }

            definition.CreatedBy = user.GetId();
            var definitionEntity = Mapper.Map<DefinitionEntity>(definition);

            _context.Definitions.Add(definitionEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(definitionEntity.Id, ct);
        }

        /// <summary>
        /// update the Definition
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="definition">the new information</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<ViewModels.Definition> UpdateAsync(Guid id, ViewModels.Definition definition, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            var definitionEntity = await GetTheDefinitionAsync(id, true, true, ct);
            definition.ModifiedBy = user.GetId();
            Mapper.Map(definition, definitionEntity);

            _context.Definitions.Update(definitionEntity);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(definitionEntity, definition);
        }

        /// <summary>
        /// delete the definition
        /// </summary>
        /// <param name="id">Guid</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var definitionEntity = await GetTheDefinitionAsync(id, true, true, ct);

            if (definitionEntity == null)
                throw new EntityNotFoundException<Definition>();

            _context.Definitions.Remove(definitionEntity);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        private async Task<DefinitionEntity> GetTheDefinitionAsync(Guid definitionId, bool mustBeOwner, bool mustBeContentDeveloper, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            var isContentDeveloper = (await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded;
            var isSystemAdmin = (await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded;
            if (mustBeContentDeveloper && !isContentDeveloper && !isSystemAdmin)
            {
                _logger.LogInformation($"User {user.GetId()} is not a content developer.");
                throw new ForbiddenException();
            }

            var definitionEntity = await _context.Definitions.SingleOrDefaultAsync(v => v.Id == definitionId, ct);

            if (definitionEntity == null)
            {
                _logger.LogError($"Definition {definitionId} was not found.");
                throw new EntityNotFoundException<Definition>();
            }
            else if (mustBeOwner && definitionEntity.CreatedBy != user.GetId() && !isSystemAdmin)
            {
                _logger.LogError($"User {user.GetId()} is not permitted to access Definition {definitionId}.");
                throw new ForbiddenException($"User {user.GetId()} is not permitted to access Definition {definitionId}.");
            }

            return definitionEntity;
        }

    }
}


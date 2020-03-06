/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
using Microsoft.Extensions.Options;
using Alloy.Api.Data;
using Alloy.Api.Data.Models;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Authorization;
using Alloy.Api.Infrastructure.Exceptions;
using Alloy.Api.Infrastructure.Options;
using Alloy.Api.ViewModels;

namespace Alloy.Api.Services
{
    public interface IImplementationService
    {
        Task<IEnumerable<Implementation>> GetAsync(CancellationToken ct);
        Task<IEnumerable<Implementation>> GetDefinitionImplementationsAsync(Guid definitionId, CancellationToken ct);
        Task<IEnumerable<Implementation>> GetMyDefinitionImplementationsAsync(Guid definitionId, CancellationToken ct);
        Task<Implementation> GetAsync(Guid id, CancellationToken ct);
        Task<Implementation> CreateAsync(Implementation implementation, CancellationToken ct);
        Task<Implementation> LaunchImplementationFromDefinitionAsync(Guid definitionId, CancellationToken ct);
        Task<Implementation> UpdateAsync(Guid id, Implementation implementation, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
        Task<Implementation> EndAsync(Guid implementationId, CancellationToken ct);
    }

    public class ImplementationService : IImplementationService
    {
        private readonly AlloyContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ICasterService _casterService;
        private readonly IPlayerService _playerService;
        private readonly ISteamfitterService _steamfitterService;
        private readonly IAlloyImplementationQueue _alloyImplementationQueue;
        private readonly ILogger<ImplementationService> _logger;
        private readonly IOptionsMonitor<ResourceOptions> _resourceOptions;
        private readonly IUserClaimsService _claimsService;

        public ImplementationService(
            AlloyContext context,
            IAuthorizationService authorizationService,
            IPrincipal user,
            IMapper mapper,
            IPlayerService playerService,
            ISteamfitterService steamfitterService,
            ICasterService casterService, 
            IAlloyImplementationQueue alloyBackgroundService,
            ILogger<ImplementationService> logger,
            IOptionsMonitor<ResourceOptions> resourceOptions,
            IUserClaimsService claimsService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _casterService = casterService;
            _playerService = playerService;
            _steamfitterService = steamfitterService;
            _alloyImplementationQueue = alloyBackgroundService;
            _logger = logger;
            _resourceOptions = resourceOptions;
            _claimsService = claimsService;
        }

        public async Task<IEnumerable<Implementation>> GetAsync(CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            IEnumerable<ImplementationEntity> items = null;
            if ((await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded)
            {
                items = await _context.Implementations.ToListAsync(ct);
            }
            else
            {
                items = await _context.Implementations.Where(x => x.CreatedBy == user.GetId()).ToListAsync(ct);
            }
            
            return _mapper.Map<IEnumerable<Implementation>>(items);
        }

        public async Task<IEnumerable<Implementation>> GetDefinitionImplementationsAsync(Guid definitionId, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded)
            {
                _logger.LogInformation($"User {user.GetId()} is not a content developer.");
                throw new ForbiddenException();
            }

            var items = await _context.Implementations
                .Where(x => x.DefinitionId == definitionId)
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<Implementation>>(items);
        }

        public async Task<IEnumerable<Implementation>> GetMyDefinitionImplementationsAsync(Guid definitionId, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new BasicRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Implementations
                .Where(x => x.UserId == user.GetId() && x.DefinitionId == definitionId)
                .ToListAsync(ct);
            
            return _mapper.Map<IEnumerable<Implementation>>(items);
        }

        public async Task<Implementation> GetAsync(Guid id, CancellationToken ct)
        {
            var item = await GetTheImplementationAsync(id, true, false, ct);

            return _mapper.Map<Implementation>(item);
        }

        public async Task<Implementation> CreateAsync(Implementation implementation, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            implementation.CreatedBy = user.GetId();
            var implementationEntity = Mapper.Map<ImplementationEntity>(implementation);

            _context.Implementations.Add(implementationEntity);
            await _context.SaveChangesAsync(ct);

            return  Mapper.Map<Implementation>(implementationEntity);
        }

        public async Task<Implementation> LaunchImplementationFromDefinitionAsync(Guid definitionId, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if (!(await _authorizationService.AuthorizeAsync(user, null, new BasicRightsRequirement())).Succeeded)
                throw new ForbiddenException();
            // check for resource limitations
            if (!(await ResourcesAreAvailableAsync(definitionId, ct)))
            {
                throw new Exception($"The appropriate resources are not available to create an implementation from the Definition {definitionId}.");
            }
            // create the implementation from the definition
            var implementationEntity = await CreateImplementationEntityAsync(definitionId, ct);
            // add the implementation to the implementation queue for AlloyBackgrounsService to process.
            _alloyImplementationQueue.Add(implementationEntity);
            return Mapper.Map<Implementation>(implementationEntity);
        }

        public async Task<Implementation> UpdateAsync(Guid id, Implementation implementation, CancellationToken ct)
        {
            var implementationEntity = await GetTheImplementationAsync(id, true, true, ct);
            implementation.ModifiedBy = _user.GetId();
            Mapper.Map(implementation, implementationEntity);

            _context.Implementations.Update(implementationEntity);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(implementationEntity, implementation);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var implementationEntity = await GetTheImplementationAsync(id, true, true, ct);
            _context.Implementations.Remove(implementationEntity);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<Implementation> EndAsync(Guid implementationId, CancellationToken ct)
        {
            try
            {
                var implementationEntity = await GetTheImplementationAsync(implementationId, true, false, ct);
                if (implementationEntity.EndDate != null)
                {
                    var msg = $"Implementation {implementationEntity.Id} has already been ended";
                    _logger.LogError(msg);
                    throw new Exception(msg);
                }
                implementationEntity.EndDate = DateTime.UtcNow;
                implementationEntity.Status = ImplementationStatus.Ending;
                implementationEntity.InternalStatus = InternalImplementationStatus.EndQueued;
                await _context.SaveChangesAsync(ct);
                // add the implementation to the implementation queue for AlloyBackgrounsService to process the caster destroy.
                _alloyImplementationQueue.Add(implementationEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ending Implementation {implementationId}.", ex);
                throw;
            }

            return await GetAsync(implementationId, ct);
        }

        private async Task<ImplementationEntity> CreateImplementationEntityAsync(Guid definitionId, CancellationToken ct)
        {
            _logger.LogInformation($"For Definition {definitionId}, Create Implementation.");
            var userId = _user.GetId();
            var username = _user.Claims.First(c => c.Type.ToLower() == "name").Value;
            var implementationEntity = new ImplementationEntity() {
                CreatedBy = userId,
                UserId = userId,
                Username = username,
                DefinitionId = definitionId,
                Status = ImplementationStatus.Creating,
                InternalStatus = InternalImplementationStatus.LaunchQueued
            };

            _context.Implementations.Add(implementationEntity);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation($"Implementation {implementationEntity.Id} created for Definition {definitionId}.");

            return implementationEntity;
        }

        private async Task<bool> ResourcesAreAvailableAsync(Guid definitionId, CancellationToken ct)
        {
            var resourcesAvailable = true;
            // check to see if this user already has this Definition Implemented
            var notActiveStatuses = new List<ImplementationStatus>() {
                ImplementationStatus.Failed, ImplementationStatus.Ended,
                ImplementationStatus.Expired};
            var items = await _context.Implementations
                .Where(x => x.UserId == _user.GetId() && x.DefinitionId == definitionId && !notActiveStatuses.Contains(x.Status))
                .ToListAsync(ct);
            resourcesAvailable = !items.Any();
            if (!resourcesAvailable)
            {
                _logger.LogError($"User {_user.GetId()} already has an active Implementation for Definition {definitionId}.");
                throw new Exception($"User {_user.GetId()} already has an active Implementation for Definition {definitionId}.");
            }
            {
                // check to see if this user has too many Implementations started
                items = await _context.Implementations
                    .Where(x => x.UserId == _user.GetId() && !notActiveStatuses.Contains(x.Status))
                    .ToListAsync(ct);
                if(!(await _authorizationService.AuthorizeAsync(_user, null, new SystemAdminRightsRequirement())).Succeeded)
                {
                    var upperLimit = _resourceOptions.CurrentValue.MaxImplementationsForBasicUser;
                    resourcesAvailable = items.Count() < upperLimit;
                    if (!resourcesAvailable)
                    {
                        _logger.LogError($"User {_user.GetId()} already has {upperLimit} Implementations active.");
                        throw new Exception($"User {_user.GetId()} already has {upperLimit} Implementations active.");
                    }
                }
            }

            return resourcesAvailable;
        }

        private async Task<ImplementationEntity> GetTheImplementationAsync(Guid implementationId, bool mustBeOwner, bool mustBeContentDeveloper, CancellationToken ct)
        {
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            var isContentDeveloper = (await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded;
            var isSystemAdmin = (await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded;
            if (mustBeContentDeveloper && !isSystemAdmin && !isContentDeveloper)
            {
                _logger.LogInformation($"User {user.GetId()} is not a content developer.");
                throw new ForbiddenException($"User {user.GetId()} is not a content developer.");
            }

            var implementationEntity = await _context.Implementations.SingleOrDefaultAsync(v => v.Id == implementationId, ct);

            if (implementationEntity == null)
            {
                _logger.LogError($"Implementation {implementationId} was not found.");
                throw new EntityNotFoundException<Definition>();
            }
            else if (mustBeOwner && implementationEntity.CreatedBy != user.GetId() && !isSystemAdmin)
            {
                _logger.LogError($"User {user.GetId()} is not permitted to access Implementation {implementationId}.");
                throw new ForbiddenException($"User {user.GetId()} is not permitted to access Implementation {implementationId}.");
            }

            return implementationEntity;
        }

    }
}


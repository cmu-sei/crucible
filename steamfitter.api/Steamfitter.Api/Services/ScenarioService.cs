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
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IScenarioService
    {
        Task<IEnumerable<ViewModels.Scenario>> GetAsync(CancellationToken ct);
        Task<ViewModels.Scenario> GetAsync(Guid id, CancellationToken ct);
        // Task<IEnumerable<ViewModels.Scenario>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ViewModels.Scenario> CreateAsync(ViewModels.Scenario scenario, CancellationToken ct);
        Task<ViewModels.Scenario> CopyAsync(Guid id, CancellationToken ct);
        Task<ViewModels.Scenario> UpdateAsync(Guid id, ViewModels.Scenario scenario, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class ScenarioService : IScenarioService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDispatchTaskService _dispatchTaskService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;

        public ScenarioService(SteamfitterContext context, IAuthorizationService authorizationService, IDispatchTaskService dispatchTaskService, IPrincipal user, IMapper mapper)
        {
            _context = context;
            _authorizationService = authorizationService;
            _dispatchTaskService = dispatchTaskService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ViewModels.Scenario>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Scenarios
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<Scenario>>(items);
        }

        public async Task<ViewModels.Scenario> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Scenarios
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<Scenario>(item);
        }

        public async Task<ViewModels.Scenario> CreateAsync(ViewModels.Scenario scenario, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            scenario.DateCreated = DateTime.UtcNow;
            var scenarioEntity = Mapper.Map<ScenarioEntity>(scenario);

            //TODO: add permissions
            // var scenarioAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.ScenarioAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (scenarioAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ScenarioAdmin.ToString()} Permission not found.");

            _context.Scenarios.Add(scenarioEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(scenarioEntity.Id, ct);
        }

        public async Task<ViewModels.Scenario> CopyAsync(Guid oldScenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var oldScenarioEntity = _context.Scenarios.Find(oldScenarioId);
            if (oldScenarioEntity == null)
                throw new EntityNotFoundException<Scenario>($"Scenario {oldScenarioId} was not found.");

            var newScenarioEntity = new ScenarioEntity() {
                CreatedBy = _user.GetId(),
                Name = $"Copy of {oldScenarioEntity.Name}",
                Description = oldScenarioEntity.Description,
                DurationHours = oldScenarioEntity.DurationHours
            };

            _context.Scenarios.Add(newScenarioEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the DispatchTasks, including children
            var oldDispatchTaskEntityIds = _context.DispatchTasks.Where(dt => dt.ScenarioId == oldScenarioId && dt.TriggerTaskId == null).Select(s => s.Id);
            foreach (var oldDispatchTaskEntityId in oldDispatchTaskEntityIds)
            {
                await _dispatchTaskService.CopyAsync(oldDispatchTaskEntityId, newScenarioEntity.Id, "scenario", ct);
            }

            return await GetAsync(newScenarioEntity.Id, ct);
        }

        public async Task<ViewModels.Scenario> UpdateAsync(Guid id, ViewModels.Scenario scenario, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioToUpdate = await _context.Scenarios.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioToUpdate == null)
                throw new EntityNotFoundException<Scenario>();

            scenario.CreatedBy = scenarioToUpdate.CreatedBy;
            scenario.DateCreated = scenarioToUpdate.DateCreated;
            scenario.DateModified = DateTime.UtcNow;
            Mapper.Map(scenario, scenarioToUpdate);

            _context.Scenarios.Update(scenarioToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(scenarioToUpdate, scenario);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioToDelete = await _context.Scenarios.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioToDelete == null)
                throw new EntityNotFoundException<Scenario>();

            _context.Scenarios.Remove(scenarioToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

    }
}


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
using STT = System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Hubs;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using SAVM = Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IScenarioTemplateService
    {
        STT.Task<IEnumerable<ViewModels.ScenarioTemplate>> GetAsync(CancellationToken ct);
        STT.Task<ViewModels.ScenarioTemplate> GetAsync(Guid id, CancellationToken ct);
        // STT.Task<IEnumerable<ViewModels.ScenarioTemplate>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        STT.Task<ViewModels.ScenarioTemplate> CreateAsync(ViewModels.ScenarioTemplate scenarioTemplate, CancellationToken ct);
        STT.Task<ViewModels.ScenarioTemplate> CopyAsync(Guid id, CancellationToken ct);
        STT.Task<ViewModels.ScenarioTemplate> UpdateAsync(Guid id, ViewModels.ScenarioTemplate scenarioTemplate, CancellationToken ct);
        STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class ScenarioTemplateService : IScenarioTemplateService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITaskService _taskService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly IHubContext<EngineHub> _engineHub;

        public ScenarioTemplateService(
            SteamfitterContext context,
            IAuthorizationService authorizationService,
            ITaskService taskService,
            IPrincipal user,
            IMapper mapper,
            IHubContext<EngineHub> engineHub)
        {
            _context = context;
            _authorizationService = authorizationService;
            _taskService = taskService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _engineHub = engineHub;
        }

        public async STT.Task<IEnumerable<ViewModels.ScenarioTemplate>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.ScenarioTemplates
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<SAVM.ScenarioTemplate>>(items);
        }

        public async STT.Task<ViewModels.ScenarioTemplate> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.ScenarioTemplates
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<SAVM.ScenarioTemplate>(item);
        }

        public async STT.Task<ViewModels.ScenarioTemplate> CreateAsync(ViewModels.ScenarioTemplate scenarioTemplate, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            scenarioTemplate.DateCreated = DateTime.UtcNow;
            var scenarioTemplateEntity = _mapper.Map<ScenarioTemplateEntity>(scenarioTemplate);

            //TODO: add permissions
            // var scenarioTemplateAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.ScenarioTemplateAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (scenarioTemplateAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ScenarioTemplateAdmin.ToString()} Permission not found.");

            _context.ScenarioTemplates.Add(scenarioTemplateEntity);
            await _context.SaveChangesAsync(ct);
            scenarioTemplate = await GetAsync(scenarioTemplateEntity.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioTemplateCreated, scenarioTemplate);

            return scenarioTemplate;
        }

        public async STT.Task<ViewModels.ScenarioTemplate> CopyAsync(Guid oldScenarioTemplateId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var oldScenarioTemplateEntity = _context.ScenarioTemplates.Find(oldScenarioTemplateId);
            if (oldScenarioTemplateEntity == null)
                throw new EntityNotFoundException<SAVM.ScenarioTemplate>($"ScenarioTemplate {oldScenarioTemplateId} was not found.");

            var newScenarioTemplateEntity = new ScenarioTemplateEntity() {
                CreatedBy = _user.GetId(),
                Name = $"{oldScenarioTemplateEntity.Name} - {_user.Claims.FirstOrDefault(c => c.Type == "name").Value}",
                Description = oldScenarioTemplateEntity.Description,
                DurationHours = oldScenarioTemplateEntity.DurationHours
            };

            _context.ScenarioTemplates.Add(newScenarioTemplateEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the Tasks, including children
            var oldTaskEntityIds = _context.Tasks.Where(dt => dt.ScenarioTemplateId == oldScenarioTemplateId && dt.TriggerTaskId == null).Select(s => s.Id).ToList();
            foreach (var oldTaskEntityId in oldTaskEntityIds)
            {
                await _taskService.CopyAsync(oldTaskEntityId, newScenarioTemplateEntity.Id, "scenarioTemplate", ct);
            }

            var newScenarioTemplate = await GetAsync(newScenarioTemplateEntity.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioTemplateCreated, newScenarioTemplate);

            return newScenarioTemplate;
        }

        public async STT.Task<ViewModels.ScenarioTemplate> UpdateAsync(Guid id, ViewModels.ScenarioTemplate scenarioTemplate, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioTemplateToUpdate = await _context.ScenarioTemplates.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioTemplateToUpdate == null)
                throw new EntityNotFoundException<SAVM.ScenarioTemplate>();

            scenarioTemplate.CreatedBy = scenarioTemplateToUpdate.CreatedBy;
            scenarioTemplate.DateCreated = scenarioTemplateToUpdate.DateCreated;
            scenarioTemplate.DateModified = DateTime.UtcNow;
            _mapper.Map(scenarioTemplate, scenarioTemplateToUpdate);

            _context.ScenarioTemplates.Update(scenarioTemplateToUpdate);
            await _context.SaveChangesAsync(ct);

            scenarioTemplate = await GetAsync(scenarioTemplateToUpdate.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioTemplateUpdated, scenarioTemplate);

            return scenarioTemplate;
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioTemplateToDelete = await _context.ScenarioTemplates.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioTemplateToDelete == null)
                throw new EntityNotFoundException<SAVM.ScenarioTemplate>();

            _context.ScenarioTemplates.Remove(scenarioTemplateToDelete);
            await _context.SaveChangesAsync(ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioTemplateDeleted, id);

            return true;
        }

    }
}


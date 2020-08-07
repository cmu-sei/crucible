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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Hubs;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using SAVM = Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IScenarioService
    {
        STT.Task<IEnumerable<ViewModels.Scenario>> GetAsync(CancellationToken ct);
        STT.Task<ViewModels.Scenario> GetAsync(Guid Id, CancellationToken ct);
        STT.Task<ViewModels.Scenario> GetMineAsync(CancellationToken ct);
        STT.Task<ViewModels.Scenario> CreateAsync(ViewModels.Scenario Scenario, CancellationToken ct);
        STT.Task<ViewModels.Scenario> CreateFromScenarioTemplateAsync(Guid scenarioTemplateId, CancellationToken ct);
        STT.Task<ViewModels.Scenario> CreateFromScenarioAsync(Guid scenarioId, CancellationToken ct);
        STT.Task<ViewModels.Scenario> UpdateAsync(Guid Id, ViewModels.Scenario Scenario, CancellationToken ct);
        STT.Task<bool> DeleteAsync(Guid Id, CancellationToken ct);
        STT.Task<ViewModels.Scenario> StartAsync(Guid Id, CancellationToken ct);
        STT.Task<ViewModels.Scenario> PauseAsync(Guid Id, CancellationToken ct);
        STT.Task<ViewModels.Scenario> ContinueAsync(Guid Id, CancellationToken ct);
        STT.Task<ViewModels.Scenario> EndAsync(Guid Id, CancellationToken ct);
    }

    public class ScenarioService : IScenarioService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ITaskService _taskService;
        private readonly IStackStormService _stackstormService;
        private readonly IHubContext<EngineHub> _engineHub;

        public ScenarioService(SteamfitterContext context,
                                IAuthorizationService authorizationService,
                                IPrincipal user,
                                IMapper mapper,
                                ITaskService taskService,
                                IStackStormService stackstormService,
                                IHubContext<EngineHub> engineHub)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _taskService = taskService;
            _stackstormService = stackstormService;
            _engineHub = engineHub;
        }

        public async STT.Task<IEnumerable<ViewModels.Scenario>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Scenarios
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<SAVM.Scenario>>(items);
        }

        public async STT.Task<ViewModels.Scenario> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Scenarios
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<SAVM.Scenario>(item);
        }

        public async STT.Task<ViewModels.Scenario> GetMineAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Scenarios
                .SingleOrDefaultAsync(o => o.Id == _user.GetId(), ct);
            if (item == null)
            {
                var createdDate = DateTime.UtcNow;
                var id = _user.GetId();
                var name = _user.Claims.FirstOrDefault(c => c.Type == "name").Value;
                item = new ScenarioEntity() {
                    Id = id,
                    Name = $"{name} User Scenario",
                    Description = "Personal Task Builder Scenario",
                    StartDate = createdDate,
                    EndDate = createdDate.AddYears(100),
                    Status = ScenarioStatus.active,
                    OnDemand = false,
                    DateCreated = createdDate,
                    DateModified = createdDate,
                    CreatedBy = id,
                    ModifiedBy = id
                };
                _context.Scenarios.Add(item);
                await _context.SaveChangesAsync(ct);
            }

            return _mapper.Map<SAVM.Scenario>(item);
        }

        public async STT.Task<ViewModels.Scenario> CreateAsync(ViewModels.Scenario scenario, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            scenario.DateCreated = DateTime.UtcNow;
            scenario.CreatedBy = _user.GetId();
            var scenarioEntity = _mapper.Map<ScenarioEntity>(scenario);

            //TODO: add permissions
            // var ScenarioAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.ScenarioAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (ScenarioAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ScenarioAdmin.ToString()} Permission not found.");

            _context.Scenarios.Add(scenarioEntity);
            await _context.SaveChangesAsync(ct);
            scenario = await GetAsync(scenarioEntity.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioCreated, scenario);

            return scenario;
        }

        public async STT.Task<ViewModels.Scenario> CreateFromScenarioTemplateAsync(Guid scenarioTemplateId, CancellationToken ct)
        {
            var scenarioTemplateEntity = _context.ScenarioTemplates.Find(scenarioTemplateId);
            if (scenarioTemplateEntity == null)
                throw new EntityNotFoundException<SAVM.ScenarioTemplate>($"ScenarioTemplate {scenarioTemplateId} was not found.");

            var scenarioEntity = new ScenarioEntity() {
                CreatedBy = _user.GetId(),
                Name = scenarioTemplateEntity.Name,
                Description = scenarioTemplateEntity.Description,
                OnDemand = true,
                ScenarioTemplateId = scenarioTemplateId
            };
            var durationHours = scenarioTemplateEntity.DurationHours != null ? (int)scenarioTemplateEntity.DurationHours : 720;
            scenarioEntity.EndDate = scenarioEntity.StartDate.AddHours(durationHours);
            _context.Scenarios.Add(scenarioEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the Tasks
            var oldTaskEntities = _context.Tasks.Where(dt => dt.ScenarioTemplateId == scenarioTemplateId && dt.TriggerTaskId == null).ToList();
            // copy the PARENT Tasks
            foreach (var oldTaskEntity in oldTaskEntities)
            {
                await _taskService.CopyAsync(oldTaskEntity.Id, scenarioEntity.Id, "scenario", ct);
            }

            var scenario = _mapper.Map<SAVM.Scenario>(scenarioEntity);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioCreated, scenario);

            return scenario;
        }

        public async STT.Task<ViewModels.Scenario> CreateFromScenarioAsync(Guid oldScenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var oldScenarioEntity = _context.Scenarios.Find(oldScenarioId);
            if (oldScenarioEntity == null)
                throw new EntityNotFoundException<SAVM.Scenario>($"Scenario {oldScenarioId} was not found.");

            var newScenarioEntity = new ScenarioEntity() {
                CreatedBy = _user.GetId(),
                Name = $"{oldScenarioEntity.Name} - {_user.Claims.FirstOrDefault(c => c.Type == "name").Value}",
                Description = oldScenarioEntity.Description,
                OnDemand = true,
                ScenarioTemplateId = oldScenarioEntity.ScenarioTemplateId
            };

            _context.Scenarios.Add(newScenarioEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the Tasks
            var oldTaskEntities = _context.Tasks.Where(dt => dt.ScenarioId == oldScenarioId && dt.TriggerTaskId == null).ToList();
            // copy the PARENT Tasks
            foreach (var oldTaskEntity in oldTaskEntities)
            {
                await _taskService.CopyAsync(oldTaskEntity.Id, newScenarioEntity.Id, "scenario", ct);
            }

            var scenario = await GetAsync(newScenarioEntity.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioCreated, scenario);

            return scenario;
        }

        public async STT.Task<ViewModels.Scenario> UpdateAsync(Guid id, ViewModels.Scenario scenario, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioToUpdate = await _context.Scenarios.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioToUpdate == null)
                throw new EntityNotFoundException<SAVM.Scenario>();

            scenario.DateCreated = scenarioToUpdate.DateCreated;
            scenario.CreatedBy = scenarioToUpdate.CreatedBy;
            scenario.DateModified = DateTime.UtcNow;
            scenario.ModifiedBy = _user.GetId();
            _mapper.Map(scenario, scenarioToUpdate);

            _context.Scenarios.Update(scenarioToUpdate);
            await _context.SaveChangesAsync(ct);

            var updatedScenario =  _mapper.Map(scenarioToUpdate, scenario);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioUpdated, updatedScenario);

            return updatedScenario;
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioToDelete = await _context.Scenarios.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (scenarioToDelete == null)
                throw new EntityNotFoundException<SAVM.Scenario>();

            _context.Scenarios.Remove(scenarioToDelete);
            await _context.SaveChangesAsync(ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioDeleted, id);

            return true;
        }

        public async STT.Task<ViewModels.Scenario> StartAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenario = await _context.Scenarios
                .SingleAsync(o => o.Id == id, ct);
            var dateTimeStart = DateTime.UtcNow;
            scenario.DateModified = dateTimeStart;
            scenario.ModifiedBy = _user.GetId();
            scenario.StartDate = dateTimeStart;
            scenario.Status = ScenarioStatus.active;

            _context.Scenarios.Update(scenario);
            await _context.SaveChangesAsync(ct);
            // TODO:  create a better way to do this that doesn't require getting ALL of the VM's
            // We just need to grab all of the VM's from the scenario view
            await _stackstormService.GetStackstormVms();
            var tasks = await _taskService.GetByScenarioIdAsync(scenario.Id, ct);
            foreach (var task in tasks)
            {
                if (task.TriggerTaskId is null && task.TriggerCondition != TaskTrigger.Manual)
                {
                    await _taskService.ExecuteAsync(task.Id, ct);
                }
            }

            var updatedScenario = _mapper.Map<SAVM.Scenario>(scenario);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioUpdated, updatedScenario);

            return updatedScenario;
        }

        public async STT.Task<ViewModels.Scenario> PauseAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async STT.Task<ViewModels.Scenario> ContinueAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async STT.Task<ViewModels.Scenario> EndAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenario = await _context.Scenarios
                .SingleAsync(o => o.Id == id, ct);
            var endDateTime = DateTime.UtcNow;
            scenario.DateModified = endDateTime;
            scenario.ModifiedBy = _user.GetId();
            scenario.Status = ScenarioStatus.ended;
            scenario.EndDate = endDateTime;

            _context.Scenarios.Update(scenario);
            await _context.SaveChangesAsync(ct);

            var updatedScenario = _mapper.Map<SAVM.Scenario>(scenario);
            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioUpdated, updatedScenario);

            return updatedScenario;
        }

    }
}


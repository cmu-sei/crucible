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
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using STT = System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Hubs;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Infrastructure.Options;
using SAVM = Steamfitter.Api.ViewModels;
using System.Net.Http.Headers;

namespace Steamfitter.Api.Services
{
    public interface ITaskService
    {
        STT.Task<IEnumerable<SAVM.Task>> GetAsync(CancellationToken ct);
        STT.Task<SAVM.Task> GetAsync(Guid Id, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetByScenarioTemplateIdAsync(Guid scenarioTemplateId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetByViewIdAsync(Guid viewId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetByVmIdAsync(Guid vmId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> GetSubtasksAsync(Guid triggerTaskId, CancellationToken ct);
        STT.Task<SAVM.Task> CreateAsync(SAVM.Task Task, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Result>> CreateAndExecuteAsync(SAVM.Task task, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Result>> ExecuteAsync(Guid id, CancellationToken ct);
        STT.Task<SAVM.Task> UpdateAsync(Guid Id, SAVM.Task Task, CancellationToken ct);
        STT.Task<bool> DeleteAsync(Guid Id, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> CopyAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct);
        STT.Task<SAVM.Task> CreateFromResultAsync(Guid resultId, Guid? newLocationId, string newLocationType, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> MoveAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct);
    }

    public class TaskService : ITaskService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly IStackStormService _stackStormService;
        private readonly VmTaskProcessingOptions _options;
        private readonly IResultService _resultService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TaskService> _logger;
        private readonly IPlayerService _playerService;
        private readonly IPlayerVmService _playerVmService;
        private readonly bool _isHttps;
        private readonly IHubContext<EngineHub> _engineHub;
        private readonly ITaskExecutionQueue _taskExecutionQueue;

        public TaskService(
            SteamfitterContext context,
            IAuthorizationService authorizationService,
            IPrincipal user,
            IMapper mapper,
            IStackStormService stackStormService,
            IOptions<VmTaskProcessingOptions> options,
            IResultService resultService,
            ILogger<TaskService> logger,
            IPlayerService playerService,
            IPlayerVmService playerVmService,
            IHttpClientFactory httpClientFactory,
            ClientOptions clientSettings,
            IHubContext<EngineHub> engineHub,
            ITaskExecutionQueue taskexecutionQueue)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _stackStormService = stackStormService;
            _options = options.Value;
            _resultService = resultService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _playerService = playerService;
            _playerVmService = playerVmService;
            _isHttps = clientSettings.urls.playerApi.ToLower().StartsWith("https:");
            _engineHub = engineHub;
            _taskExecutionQueue = taskexecutionQueue;
        }

        public async STT.Task<IEnumerable<SAVM.Task>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Tasks.ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<SAVM.Task>>(items);
        }

        public async STT.Task<SAVM.Task> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Tasks
                .SingleOrDefaultAsync(o => o.Id == id, ct);
            if (item == null)
                throw new EntityNotFoundException<SAVM.Task>();

            return _mapper.Map<SAVM.Task>(item);
        }

        public async STT.Task<IEnumerable<SAVM.Task>> GetByScenarioTemplateIdAsync(Guid scenarioTemplateId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var Tasks = _context.Tasks.Where(x => x.ScenarioTemplateId == scenarioTemplateId);

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<IEnumerable<SAVM.Task>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var Tasks = _context.Tasks.Where(x => x.ScenarioId == scenarioId);

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<IEnumerable<SAVM.Task>> GetByViewIdAsync(Guid viewId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioIdList = _context.Scenarios.Where(s => s.ViewId == viewId).Select(s => s.Id.ToString()).ToList();
            var Tasks = _context.Tasks.Where(dt => scenarioIdList.Contains(dt.ScenarioId.ToString()));

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<IEnumerable<SAVM.Task>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            // the user's personal scenario used in Task Builder is the user ID.
            var Tasks = _context.Tasks.Where(dt => dt.UserId == userId && dt.ScenarioTemplateId == null && (dt.ScenarioId == null || dt.ScenarioId == userId));

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<IEnumerable<SAVM.Task>> GetByVmIdAsync(Guid vmId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmIdList = _context.Results.Where(r => r.VmId == vmId).Select(r => r.Id.ToString()).ToList();
            var Tasks = _context.Tasks.Where(dt => vmIdList.Contains(dt.Id.ToString()));

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<IEnumerable<SAVM.Task>> GetSubtasksAsync(Guid triggerTaskId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var Tasks = _context.Tasks.Where(x => x.TriggerTaskId == triggerTaskId);

            return _mapper.Map<IEnumerable<SAVM.Task>>(Tasks);
        }
        
        public async STT.Task<SAVM.Task> CreateAsync(SAVM.Task task, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            var vmListCount = task.VmList != null ? task.VmList.Count : 0;
            if (vmListCount > 0)
            {
                if (task.VmMask != "")
                    throw new InvalidOperationException("A Task cannot have BOTH a VmMask and a VmList!");
                // convert the list of vm guids into a comma separated string and save it in VmMask
                var vmIdString = "";
                foreach (var vmId in task.VmList)
                {
                    vmIdString = vmIdString + vmId + ",";
                }
                task.VmMask = vmIdString.Remove(vmIdString.Count() - 1);
            }
            if (task.ActionParameters.Keys.Any(key => key == "Moid"))
            {
                task.ActionParameters["Moid"] = "{moid}";
            }
            task.DateCreated = DateTime.UtcNow;
            task.CreatedBy = _user.GetId();
            task.UserId = _user.GetId();
            task.Iterations = task.Iterations > 0 ? task.Iterations : 1;
            task.CurrentIteration = 0;
            var taskEntity = _mapper.Map<TaskEntity>(task);

            _context.Tasks.Add(taskEntity);
            await _context.SaveChangesAsync(ct);
            task = await GetAsync(taskEntity.Id, ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskCreated, task);

            return task;
        }

        public async STT.Task<IEnumerable<SAVM.Result>> CreateAndExecuteAsync(SAVM.Task task, CancellationToken ct)
        {
            // create the TaskEntity
            task = await CreateAsync(task, ct);
            // execute the TaskEntity.  Authorization is null, because there can't be any subtasks on a CreateAndExecute.
            var resultList = await ExecuteAsync(task.Id, ct);

            return resultList;
        }

        public async STT.Task<IEnumerable<SAVM.Result>> ExecuteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskToExecute = await _context.Tasks.SingleOrDefaultAsync(v => v.Id == id, ct);
            taskToExecute.CurrentIteration = 0;
            await _context.SaveChangesAsync();
            taskToExecute.UserId = _user.GetId();
            _taskExecutionQueue.Add(taskToExecute);

            return  new List<SAVM.Result>();
        }

        public async STT.Task<SAVM.Task> UpdateAsync(Guid id, SAVM.Task task, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskToUpdate = await _context.Tasks.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (taskToUpdate == null)
                throw new EntityNotFoundException<SAVM.Task>();

            var vmListCount = task.VmList != null ? task.VmList.Count : 0;
            if (vmListCount > 0)
            {
                if (task.VmMask != "")
                    throw new InvalidOperationException("A Task cannot have BOTH a VmMask and a VmList!");
                // convert the list of vm guids into a comma separated string and save it in VmMask
                var vmIdString = "";
                foreach (var vmId in task.VmList)
                {
                    vmIdString = vmIdString + vmId + ",";
                }
                task.VmMask = vmIdString.Remove(vmIdString.Count() - 1);
            }
            task.CreatedBy = taskToUpdate.CreatedBy;
            task.DateCreated = taskToUpdate.DateCreated;
            task.DateModified = DateTime.UtcNow;
            task.ModifiedBy = _user.GetId();
            _mapper.Map(task, taskToUpdate);

            _context.Tasks.Update(taskToUpdate);
            await _context.SaveChangesAsync(ct);
            var updatedTask = _mapper.Map(taskToUpdate, task);
            updatedTask.VmList = null;
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskUpdated, updatedTask);

            return updatedTask;
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskToDelete = await _context.Tasks.SingleOrDefaultAsync(v => v.Id == id, ct);
            if (taskToDelete == null)
                throw new EntityNotFoundException<SAVM.Task>();

            _context.Tasks.Remove(taskToDelete);
            await _context.SaveChangesAsync(ct);
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskDeleted, id);

            return true;
        }

        public async STT.Task<IEnumerable<SAVM.Task>> CopyAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct)
        {
            // check user authorization
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            var items = await CopyTaskAsync(id, newLocationId, newLocationType, ct);

            return  _mapper.Map<IEnumerable<SAVM.Task>>(items);
        }

        public async STT.Task<IEnumerable<SAVM.Task>> MoveAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct)
        {
            // check user authorization
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            var items = await MoveTaskAsync(id, newLocationId, newLocationType, ct);

            return _mapper.Map<IEnumerable<SAVM.Task>>(items);
        }

        public async STT.Task<SAVM.Task> CreateFromResultAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct)
        {
            // check user authorization
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            // check for existing result
            var resultEntity = await _context.Results.SingleAsync(v => v.Id == id, ct);
            if (resultEntity == null)
                throw new EntityNotFoundException<STT.Task>();
            // determine where the new Task goes
            Guid? triggerTaskId = null;
            Guid? scenarioTemplateId = null;
            Guid? scenarioId = null;
            switch (newLocationType)
            {
                case "task":
                    triggerTaskId = newLocationId;
                    var newLocationTaskEntity = await _context.Tasks.SingleAsync(v => v.Id == triggerTaskId, ct);
                    scenarioTemplateId = newLocationTaskEntity.ScenarioTemplateId;
                    scenarioId = newLocationTaskEntity.ScenarioId;
                    break;
                case "scenarioTemplate":
                    scenarioTemplateId = newLocationId;
                    break;
                case "scenario":
                    scenarioId = newLocationId;
                    break;
                default:
                    break;
            }
            // create the new Task
            var newTaskEntity =  new TaskEntity();
            newTaskEntity.ScenarioTemplate = null;
            newTaskEntity.ScenarioTemplateId = scenarioTemplateId;
            newTaskEntity.Scenario = null;
            newTaskEntity.ScenarioId = scenarioId;
            newTaskEntity.TriggerTask = null;
            newTaskEntity.TriggerTaskId = triggerTaskId;
            newTaskEntity.TriggerCondition = TaskTrigger.Manual;
            newTaskEntity.Name = "New Task";
            newTaskEntity.Description = "Created from old execution";
            newTaskEntity.UserId = _user.GetId();
            newTaskEntity.Action = resultEntity.Action;
            newTaskEntity.VmMask = resultEntity.VmId.ToString();
            newTaskEntity.ApiUrl = resultEntity.ApiUrl;
            newTaskEntity.InputString = resultEntity.InputString;
            newTaskEntity.ExpectedOutput = resultEntity.ExpectedOutput;
            newTaskEntity.ExpirationSeconds = resultEntity.ExpirationSeconds;
            newTaskEntity.DelaySeconds = 0;
            newTaskEntity.IntervalSeconds = 0;
            newTaskEntity.Iterations = 1;
            newTaskEntity.IterationTermination = TaskIterationTermination.IterationCount;
            newTaskEntity.CurrentIteration = 0;
            newTaskEntity.CreatedBy = _user.GetId();
            newTaskEntity.ModifiedBy = _user.GetId();
            newTaskEntity.DateCreated = DateTime.UtcNow;
            newTaskEntity.DateModified = newTaskEntity.DateCreated;
            // save new task to the database
            _context.Tasks.Add(newTaskEntity);
            await _context.SaveChangesAsync();
            var newTask = _mapper.Map<SAVM.Task>(newTaskEntity);
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskCreated, newTask);

            return  _mapper.Map<SAVM.Task>(newTask);
        }

        private async STT.Task<IEnumerable<TaskEntity>> CopyTaskAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct)
        {
            // check for existing task
            var existingTaskEntity = await _context.Tasks.SingleAsync(v => v.Id == id, ct);
            if (existingTaskEntity == null)
                throw new EntityNotFoundException<STT.Task>();
            // determine where the copy goes
            Guid? triggerTaskId = null;
            Guid? scenarioTemplateId = null;
            Guid? scenarioId = null;
            switch (newLocationType)
            {
                case "task":
                    triggerTaskId = newLocationId;
                    var newLocationTaskEntity = await _context.Tasks.SingleAsync(v => v.Id == triggerTaskId, ct);
                    if (await PreventAddingToSelfAsync(existingTaskEntity.Id, newLocationTaskEntity, ct))
                    {
                        throw new Exception("Cannot copy a Task underneath itself!");
                    }
                    scenarioTemplateId = newLocationTaskEntity.ScenarioTemplateId;
                    scenarioId = newLocationTaskEntity.ScenarioId;
                    break;
                case "scenarioTemplate":
                    scenarioTemplateId = newLocationId;
                    break;
                case "scenario":
                    scenarioId = newLocationId;
                    break;
                default:
                    break;
            }
            // create the copy
            var newTaskEntity = _mapper.Map<TaskEntity, TaskEntity>(existingTaskEntity);
            // set the new task relationships
            newTaskEntity.ScenarioTemplate = null;
            newTaskEntity.ScenarioTemplateId = scenarioTemplateId;
            newTaskEntity.Scenario = null;
            newTaskEntity.ScenarioId = scenarioId;
            newTaskEntity.TriggerTask = null;
            newTaskEntity.TriggerTaskId = triggerTaskId;
            newTaskEntity.UserId = _user.GetId();
            newTaskEntity.CreatedBy = _user.GetId();
            newTaskEntity.ModifiedBy = _user.GetId();
            newTaskEntity.DateCreated = DateTime.UtcNow;
            newTaskEntity.DateModified = newTaskEntity.DateCreated;
            // save new task to the database
            _context.Tasks.Add(newTaskEntity);
            await _context.SaveChangesAsync();
            var newTask = _mapper.Map<SAVM.Task>(newTaskEntity);
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskCreated, newTask);
            // return the new task with all of its new subtasks
            var entities = new List<TaskEntity>();
            entities.Add(newTaskEntity);
            entities.AddRange(await CopySubTasks(id, newTaskEntity.Id, ct));

            return entities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> CopySubTasks(Guid oldTaskEntityId, Guid newTaskEntityId, CancellationToken ct)
        {
            var oldSubTaskEntityIds = _context.Tasks.Where(dt => dt.TriggerTaskId == oldTaskEntityId).Select(dt => dt.Id).ToList();
            var subEntities = new List<TaskEntity>();
            foreach (var oldSubTaskEntityId in oldSubTaskEntityIds)
            {
                var newEntities = await CopyTaskAsync(oldSubTaskEntityId, newTaskEntityId, "task", ct);
                subEntities.AddRange(newEntities);
            }

            return subEntities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> MoveTaskAsync(Guid id, Guid? newLocationId, string newLocationType, CancellationToken ct)
        {
            // check for existing task
            var existingTaskEntity = await _context.Tasks.SingleAsync(v => v.Id == id, ct);
            if (existingTaskEntity == null)
                throw new EntityNotFoundException<STT.Task>();
            // determine where the copy goes
            existingTaskEntity.TriggerTaskId = null;
            existingTaskEntity.ScenarioTemplateId = null;
            existingTaskEntity.ScenarioId = null;
            switch (newLocationType)
            {
                case "task":
                    var newLocationTaskEntity = await _context.Tasks.SingleAsync(v => v.Id == newLocationId, ct);
                    if (await PreventAddingToSelfAsync(existingTaskEntity.Id, newLocationTaskEntity, ct))
                    {
                        throw new Exception("Cannot move a Task underneath itself!");
                    }
                    existingTaskEntity.TriggerTaskId = newLocationId;
                    existingTaskEntity.ScenarioTemplateId = newLocationTaskEntity.ScenarioTemplateId;
                    existingTaskEntity.ScenarioId = newLocationTaskEntity.ScenarioId;
                    break;
                case "scenarioTemplate":
                    existingTaskEntity.ScenarioTemplateId = newLocationId;
                    break;
                case "scenario":
                    existingTaskEntity.ScenarioId = newLocationId;
                    break;
                default:
                    break;
            }
            await _context.SaveChangesAsync();
            var movedTask = _mapper.Map<SAVM.Task>(existingTaskEntity);
            _engineHub.Clients.All.SendAsync(EngineMethods.TaskUpdated, movedTask);
            var entities = new List<TaskEntity>();
            entities.Add(existingTaskEntity);
            entities.AddRange(await MoveSubTasks(existingTaskEntity.Id, ct));

            return entities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> MoveSubTasks(Guid taskEntityId, CancellationToken ct)
        {
            var subTaskEntityIds = _context.Tasks.Where(dt => dt.TriggerTaskId == taskEntityId).Select(dt => dt.Id).ToList();
            var subEntities = new List<TaskEntity>();
            foreach (var subTaskEntityId in subTaskEntityIds)
            {
                var newEntities = await MoveTaskAsync(subTaskEntityId, taskEntityId, "task", ct);
                subEntities.AddRange(newEntities);
            }

            return subEntities;
        }

        private async STT.Task<bool> PreventAddingToSelfAsync(Guid existingId, TaskEntity newLocationEntity, CancellationToken ct)
        {
            // walk up the dispatch task family tree to make sure the existing task is not on it
            // a null parentId means we hit the top
            var parentId = newLocationEntity.TriggerTaskId;
            var wouldAddToSelf = false;
            while (!wouldAddToSelf && parentId != null)
            {
                wouldAddToSelf = (parentId == existingId);
                parentId = (await _context.Tasks.SingleAsync(v => v.Id == parentId, ct)).TriggerTaskId;
            }
            return wouldAddToSelf;
        }

    }

}


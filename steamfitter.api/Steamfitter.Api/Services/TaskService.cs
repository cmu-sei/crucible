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
using System.Threading;
using STT = System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Infrastructure.Options;
using SAVM = Steamfitter.Api.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        STT.Task<IEnumerable<SAVM.Task>> CopyAsync(Guid id, Guid newLocationId, string newLocationType, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.Task>> MoveAsync(Guid id, Guid newLocationId, string newLocationType, CancellationToken ct);
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
            ClientOptions clientSettings)
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

            var Tasks = _context.Tasks.Where(dt => dt.UserId == userId && dt.ScenarioTemplateId == null && dt.ScenarioId == null);

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
            if (task.VmMask != "" && vmListCount > 0)
                throw new InvalidOperationException("A Task cannot have BOTH a VmMask and a VmList!");
            // convert the list of vm guids into a comma separated string and save it in VmMask
            if (vmListCount > 0)
            {
                var vmIdString = "";
                foreach (var vmId in task.VmList)
                {
                    vmIdString = vmIdString + vmId + ",";
                }
                task.VmMask = vmIdString.Remove(vmIdString.Count() - 1);
            }
            task.DateCreated = DateTime.UtcNow;
            task.CreatedBy = _user.GetId();
            task.UserId = _user.GetId();
            var taskEntity = Mapper.Map<TaskEntity>(task);

            _context.Tasks.Add(taskEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(taskEntity.Id, ct);
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
            await VerifyTaskToExecuteAsync(taskToExecute, ct);
            var resultEntityList = _context.Results.Where(dtr => dtr.TaskId == taskToExecute.Id && dtr.Status == Data.TaskStatus.sent).ToList();
            // schedule the task, if it has a delay and hasn't already been scheduled
            if (taskToExecute.DelaySeconds > 0 && !resultEntityList.Any())
            {
                // create Results required (at least one but one for each VM)
                resultEntityList = await CreateResultsAsync(taskToExecute, ct);
                if (resultEntityList.Where(r => r.Status == Data.TaskStatus.error).Count() == 0) 
                {
                    var wasPassedOn = false;
                    // schedule the task with Foreman
                    wasPassedOn = await ScheduleTask(taskToExecute);
                    if (wasPassedOn)
                    {
                        resultEntityList = _context.Results.Where(dtr => dtr.TaskId == taskToExecute.Id && dtr.Status == Data.TaskStatus.pending).ToList();
                        resultEntityList.ForEach(dtr => dtr.Status = Data.TaskStatus.sent);
                        await _context.SaveChangesAsync(ct);
                    }
                    else
                    {
                        _context.Results.RemoveRange(resultEntityList);
                        var resultEntity = NewResultEntity(taskToExecute);
                        resultEntity.Status = Data.TaskStatus.error;
                        resultEntity.ActualOutput = "Error scheduling task!";
                        await _context.Results.AddAsync(resultEntity);
                        await _context.SaveChangesAsync(ct);
                        resultEntityList.Add(resultEntity);
                    }
                }
            }
            else
            {
                // create Results required (at least one but one for each VM)
                if (resultEntityList.Any())
                {
                    // this was a scheduled task, so just change result statuses to pending
                    resultEntityList.ForEach(dtr => dtr.Status = Data.TaskStatus.pending);
                    await _context.SaveChangesAsync(ct);
                }
                else
                {
                    resultEntityList = await CreateResultsAsync(taskToExecute, ct);
                }
                if (resultEntityList.Where(r => r.Status == Data.TaskStatus.error).Count() == 0)
                {
                    // determine where to execute the task, execute it, process result
                    Data.TaskStatus overallStatus;
                    switch (taskToExecute.ApiUrl)
                    {
                        case "stackstorm":
                        {
                            overallStatus = await ExecuteStackstormTaskAsync(taskToExecute, resultEntityList, ct);
                            break;
                        }
                        case "player":
                        case "vm":
                        case "caster":
                        default:
                        {
                            var message = $"API ({taskToExecute.ApiUrl}) is not currently implemented";
                            _logger.LogError(message);
                            throw new NotImplementedException(message);
                        }
                    }

                    // Process the subtasks
                    var subtaskEntityList = GetSubtasksToExecute(taskToExecute.Id, overallStatus);
                    var allSubtasksWereProcessed = await ProcessSubtasks(subtaskEntityList, ct);
                    if (!allSubtasksWereProcessed)
                    {
                        var message = $"One or more child tasks were not processed correctly for Task {taskToExecute.Id}";
                        _logger.LogError(message);
                        throw new ApplicationException(message);
                    }
                }
            }

            return _mapper.Map(resultEntityList, new List<SAVM.Result>());
        }

        public async STT.Task<SAVM.Task> UpdateAsync(Guid id, SAVM.Task task, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskToUpdate = await _context.Tasks.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (taskToUpdate == null)
                throw new EntityNotFoundException<SAVM.Task>();

            task.CreatedBy = taskToUpdate.CreatedBy;
            task.DateCreated = taskToUpdate.DateCreated;
            task.DateModified = DateTime.UtcNow;
            task.ModifiedBy = _user.GetId();
            Mapper.Map(task, taskToUpdate);

            _context.Tasks.Update(taskToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(taskToUpdate, task);
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var TaskToDelete = await _context.Tasks.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (TaskToDelete == null)
                throw new EntityNotFoundException<SAVM.Task>();

            _context.Tasks.Remove(TaskToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async STT.Task<IEnumerable<SAVM.Task>> CopyAsync(Guid id, Guid newLocationId, string newLocationType, CancellationToken ct)
        {
            // check user authorization
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            var items = await CopyTaskAsync(id, newLocationId, newLocationType, ct);

            return  _mapper.Map<IEnumerable<SAVM.Task>>(items);
        }

        public async STT.Task<IEnumerable<SAVM.Task>> MoveAsync(Guid id, Guid newLocationId, string newLocationType, CancellationToken ct)
        {
            // check user authorization
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            var items = await MoveTaskAsync(id, newLocationId, newLocationType, ct);

            return _mapper.Map<IEnumerable<SAVM.Task>>(items);
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
            var newTaskEntity = Mapper.Map<TaskEntity, TaskEntity>(existingTaskEntity);
            // set the new task relationships
            newTaskEntity.ScenarioTemplate = null;
            newTaskEntity.ScenarioTemplateId = scenarioTemplateId;
            newTaskEntity.Scenario = null;
            newTaskEntity.ScenarioId = scenarioId;
            newTaskEntity.CreatedBy = _user.GetId();
            newTaskEntity.TriggerTaskId = triggerTaskId;
            // save new task to the database
            _context.Tasks.Add(newTaskEntity);
            await _context.SaveChangesAsync();
            // return the new task with all of its new subtasks
            var entities = new List<TaskEntity>();
            entities.Add(newTaskEntity);
            entities.AddRange(await CopySubTasks(id, newTaskEntity, ct));

            return entities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> CopySubTasks(Guid oldTaskEntityId, TaskEntity newTaskEntity, CancellationToken ct)
        {
            var oldSubTaskEntityIds = _context.Tasks.Where(dt => dt.TriggerTaskId == oldTaskEntityId).Select(dt => dt.Id);
            var subEntities = new List<TaskEntity>();
            foreach (var oldSubTaskEntityId in oldSubTaskEntityIds)
            {
                var newEntities = await CopyTaskAsync(oldSubTaskEntityId, newTaskEntity.Id, "task", ct);
                subEntities.AddRange(newEntities);
            }

            return subEntities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> MoveTaskAsync(Guid id, Guid newLocationId, string newLocationType, CancellationToken ct)
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
            var entities = new List<TaskEntity>();
            entities.Add(existingTaskEntity);
            entities.AddRange(await MoveSubTasks(existingTaskEntity, ct));

            return entities;
        }

        private async STT.Task<IEnumerable<TaskEntity>> MoveSubTasks(TaskEntity taskEntity, CancellationToken ct)
        {
            var subTaskEntityIds = _context.Tasks.Where(dt => dt.TriggerTaskId == taskEntity.Id).Select(dt => dt.Id);
            var subEntities = new List<TaskEntity>();
            foreach (var subTaskEntityId in subTaskEntityIds)
            {
                var newEntities = await MoveTaskAsync(subTaskEntityId, taskEntity.Id, "task", ct);
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

        private async STT.Task VerifyTaskToExecuteAsync(TaskEntity taskToExecute, CancellationToken ct)
        {
            if (taskToExecute == null)
            {
                throw new EntityNotFoundException<SAVM.Task>();
            }
            else if (taskToExecute.ScenarioTemplateId != null)
            {
                throw new ArgumentException($"Task {taskToExecute.Id} is part of a scenarioTemplate {taskToExecute.ScenarioTemplateId} and cannot be executed.");
            }
            else if (taskToExecute.ScenarioId != null)
            {
                await VerifyScenarioTaskAsync(taskToExecute, ct);
            }
            else
            {
                VerifyManualTaskAsync(taskToExecute);
            }
        }

        private async STT.Task<List<ResultEntity>> CreateResultsAsync(TaskEntity taskToExecute, CancellationToken ct)
        {
            var resultEntities = new List<ResultEntity>();
            var viewIdList = new List<Guid?>();
            var scenarioEntity = taskToExecute.ScenarioId == null ? null : _context.Scenarios.First(s => s.Id == taskToExecute.ScenarioId);
            if (taskToExecute.VmMask.Count() == 0)
            {
                // this task has no VM's associated. Create one result entity.  Used to send a command to an API, etc.
                var resultEntity = NewResultEntity(taskToExecute);
                resultEntities.Add(resultEntity);
            }
            else
            {
                if (scenarioEntity != null)
                {
                    // A scenario is assigned to a specific view
                    viewIdList.Add(scenarioEntity.ViewId);
                }
                else
                {
                    // for an ad-hoc task, get the views that the user has access to
                    viewIdList.AddRange((await _playerService.GetViewsAsync(ct)).Select(x => x.Id).ToList());
                }
                // at this point, the VmMask could contain an actual mask, or a comma separated list of VM ID's
                var vmMaskList = taskToExecute.VmMask.Split(",").ToList();
                var vmIdList = new List<Guid>();
                var vmList = new List<S3.VM.Api.Models.Vm>();
                // create the ID list, if the mask is a list of Guid's. If not Guid's, vmIdList will end up empty.
                foreach (var mask in vmMaskList)
                {
                    Guid vmId;
                    if (Guid.TryParse(mask, out vmId))
                    {
                        vmIdList.Add(vmId);
                    }
                }
                // determine if VM's should match by Name or ID
                var matchName = vmIdList.Count() == 0;
                // create a VmList from the view ID list and the VmMask matching criteria
                foreach (var viewId in viewIdList)
                {
                    try
                    {
                        var viewVms = await _playerVmService.GetViewVmsAsync((Guid)viewId, ct);
                        foreach (var vm in viewVms)
                        {
                            if ((!matchName && vmIdList.Contains((Guid)vm.Id)) || (matchName && vm.Name.ToLower().Contains(taskToExecute.VmMask.ToLower())))
                            {
                                vmList.Add(vm);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        _logger.LogDebug($"CreateResultsAsync - No VM's found in view {viewId}");
                    }
                }
                // make sure we are only matching a SINGLE view
                var distinctViewIds = vmList.Select(v => (Guid)v.ViewId).Distinct();
                if (distinctViewIds.Count()  == 1)
                {
                    // create a result for each matched VM
                    foreach (var vm in vmList)
                    {
                        var resultEntity = NewResultEntity(taskToExecute);
                        resultEntity.VmId = vm.Id;
                        resultEntity.VmName = vm.Name;
                        resultEntities.Add(resultEntity);
                    }
                }
                else
                {
                    // Houston, we've had a problem.  Create a single result to show the error
                    var resultEntity = NewResultEntity(taskToExecute);
                    if (distinctViewIds.Count() == 0)
                    {
                        // The problem is that NO VM's matched
                        resultEntity.ActualOutput = $"No matched VMs!  VM Mask: {taskToExecute.VmMask}.";
                    }
                    else
                    {
                        // The problem is that VM's matched from multiple views
                        var matchedVms = string.Join(",", vmList.Select(v => $"{v.Name}(viewId:{v.ViewId})"));
                        resultEntity.ActualOutput = $"Matched VMs in more than one view!  VM Mask: {taskToExecute.VmMask}.  Matched VMs: {matchedVms}.";
                    }
                    _logger.LogError($"CreateResultsAsync - {resultEntity.ActualOutput}");
                    resultEntity.Status = Data.TaskStatus.error;
                    resultEntities.Add(resultEntity);
                }
            }
            await _context.Results.AddRangeAsync(resultEntities);
            await _context.SaveChangesAsync(ct);
            return resultEntities;
        }

        private async STT.Task VerifyScenarioTaskAsync(TaskEntity taskToExecute, CancellationToken ct)
        {
            var scenario = await _context.Scenarios.FindAsync(taskToExecute.ScenarioId);
            if (scenario.Status != ScenarioStatus.active)
            {
                var message = $"Task {taskToExecute.Id} is part of scenario {scenario.Id}, which is not currently active.  A Scenario must be active in order to execute its Task.";
                _logger.LogInformation(message);
                throw new ArgumentException(message);
            }
            else{
                // TODO: add scenario specific restraints on whether or not this task can run
            }
        }

        private void VerifyManualTaskAsync(TaskEntity taskToExecute)
        {
            var results = _context.Results.Where(dtr => dtr.TaskId == taskToExecute.Id && dtr.Status == Data.TaskStatus.pending);
            if (results.Any())
            {
                var message = $"Task {taskToExecute.Id} has a pending result {results.First().Id}, so it cannot be manually executed.";
                _logger.LogInformation(message);
                throw new ArgumentException(message);
            }
        }

        private async STT.Task<Data.TaskStatus> ExecuteStackstormTaskAsync(TaskEntity taskToExecute, List<ResultEntity> resultEntityList, CancellationToken ct)
        {
            var overallStatus = Data.TaskStatus.succeeded;
            foreach (var resultEntity in resultEntityList)
            {
                STT.Task<string> task = null;
                resultEntity.InputString = resultEntity.InputString.Replace("VmGuid", "Moid").Replace("{moid}", resultEntity.VmId.ToString());
                resultEntity.VmName = _stackStormService.GetVmName((Guid)resultEntity.VmId);
                switch (taskToExecute.Action)
                {
                    case TaskAction.guest_file_read:
                    {
                        task = STT.Task.Run(() => _stackStormService.GuestReadFile(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.guest_process_run:
                    {
                        task = STT.Task.Run(() => _stackStormService.GuestCommand(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_power_on:
                    {
                        task = STT.Task.Run(() => _stackStormService.VmPowerOn(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_power_off:
                    {
                        task = STT.Task.Run(() => _stackStormService.VmPowerOff(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_create_from_template:
                    {
                        task = STT.Task.Run(() => _stackStormService.CreateVmFromTemplate(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_remove:
                    {
                        task = STT.Task.Run(() => _stackStormService.VmRemove(resultEntity.InputString));
                        break;
                    }
                    default:
                    {
                        var message = $"Task Action {taskToExecute.Action} has not been implemented.";
                        _logger.LogError(message);
                        resultEntity.Status = Data.TaskStatus.failed;
                        resultEntity.StatusDate = DateTime.UtcNow;
                        break;
                    }
                }

                if (task.Wait(TimeSpan.FromSeconds(resultEntity.ExpirationSeconds)))
                {
                    resultEntity.ActualOutput = task.Result.ToString();
                    resultEntity.Status = ProcessResult(resultEntity, ct);
                }
                else
                {
                    resultEntity.ActualOutput = task.Result.ToString();
                    resultEntity.Status = Data.TaskStatus.expired;
                }
                resultEntity.StatusDate = DateTime.UtcNow;
                if (resultEntity.Status != Data.TaskStatus.succeeded)
                {
                    if (overallStatus != Data.TaskStatus.failed)
                    {
                        overallStatus = resultEntity.Status;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return overallStatus;
        }

        private Data.TaskStatus ProcessResult(ResultEntity resultEntity, CancellationToken ct)
        {
            if (resultEntity.Status == Data.TaskStatus.pending)
            {
                CheckSuccess(resultEntity);
            }
            return resultEntity.Status;
        }

        private void CheckSuccess(ResultEntity resultEntity)
        {
            if (resultEntity.ActualOutput.ToLower().Contains(resultEntity.ExpectedOutput.ToLower()))
            {
                resultEntity.Status = Data.TaskStatus.succeeded;
            }
            else
            {
                resultEntity.Status = Data.TaskStatus.failed;
            }

        }

        private IEnumerable<TaskEntity> GetSubtasksToExecute(Guid executedTaskId, Data.TaskStatus executedTaskStatus)
        {
            var subtaskEntities = _context.Tasks.Where(t => t.TriggerTaskId == executedTaskId);
            if (subtaskEntities.Any())
            {
                // filter the sutaskEntities based on the trigger task's status
                switch (executedTaskStatus)
                {
                    case Data.TaskStatus.succeeded:
                    {
                        subtaskEntities = subtaskEntities.Where(s => s.TriggerCondition == TaskTrigger.Success || s.TriggerCondition == TaskTrigger.Completion);
                        break;
                    }
                    case Data.TaskStatus.failed:
                    {
                        subtaskEntities = subtaskEntities.Where(s => s.TriggerCondition == TaskTrigger.Failure || s.TriggerCondition == TaskTrigger.Completion);
                        break;
                    }
                    case Data.TaskStatus.expired:
                    {
                        subtaskEntities = subtaskEntities.Where(s => s.TriggerCondition == TaskTrigger.Expiration);
                        break;
                    }
                    default:
                    {
                        // Any other status (cancellation in particular) should not launch subtasks
                        return new List<TaskEntity>();
                    }
                }
            }

            return subtaskEntities;
        }

        private async STT.Task<bool> ProcessSubtasks(IEnumerable<TaskEntity> subtaskEntityList, CancellationToken ct)
        {
            var allWentWell = true;
            // Handle subtasks, but don't wait for them
            foreach (var subtaskEntity in subtaskEntityList)
            {
                var wasPassedOn = false;
                if (subtaskEntity.DelaySeconds > 0)
                {
                    wasPassedOn = await ScheduleTask(subtaskEntity);
                    if (!wasPassedOn)
                    {
                        var resultEntity = NewResultEntity(subtaskEntity);
                        resultEntity.Status = Data.TaskStatus.error;
                        await _context.Results.AddAsync(resultEntity);
                        await _context.SaveChangesAsync(ct);
                    }
                }
                else
                {
                    wasPassedOn = await ExecuteSubtask(subtaskEntity.Id);
                }
                allWentWell = allWentWell && wasPassedOn;
            }

            return allWentWell;
        }

        private async STT.Task<bool> ExecuteSubtask(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            var baseAddress = _isHttps ? CurrentHttpContext.AppBaseUrl.ToLower().Replace("http:", "https:") : CurrentHttpContext.AppBaseUrl;
            client.BaseAddress = new Uri(baseAddress);
            var relativeAddress = $"Tasks/{id}/execute";
            client.DefaultRequestHeaders.Add("Authorization", new List<string>(){CurrentHttpContext.Authorization});
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var executeResponse = await client.PostAsync(relativeAddress, new StringContent(""));
            if (!executeResponse.IsSuccessStatusCode)
            {
                _logger.LogError(@"Error posting child task to " + client.BaseAddress + relativeAddress + 
                    "\nchild request: " + client.DefaultRequestHeaders.ToString() +
                    "\ninitial request: " + CurrentHttpContext.Authorization +
                    "\nStatusCode: " + executeResponse.StatusCode.ToString() +
                    "\nContent: " + executeResponse.Content.ToString() +
                    "\nReasonPhrase: " + executeResponse.ReasonPhrase.ToString() +
                    "\nRequestMessage: " + executeResponse.RequestMessage.ToString() +
                    "\nHeaders: " + executeResponse.Headers.ToString()
                );
            }

            return executeResponse.IsSuccessStatusCode;
        }

        private async STT.Task<bool> ScheduleTask(TaskEntity taskEntity)
        {
            var startTime = DateTime.UtcNow.AddSeconds(taskEntity.DelaySeconds).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            var endTime = DateTime.UtcNow.AddSeconds(taskEntity.DelaySeconds + (taskEntity.Iterations * taskEntity.IntervalSeconds)).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            var workorder = new {
                groupName = "steamfitter",
                start = startTime,
                end = endTime,
                job = "WebHook",
                webhookid = _options.ForemanWebhookId,
                woTriggers = new[] {
                    new {
                        groupName = "steamfitter",
                        interval = taskEntity.IntervalSeconds
                    }
                },
                woParams = new [] {
                    new {
                        name = "[task.id]",
                        value = taskEntity.Id
                    }
                }
            };
            var jsonString = JsonConvert.SerializeObject(workorder);
            jsonString = jsonString.Replace("woTriggers", "triggers").Replace("woParams", "params");
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_options.ForemanUrl);
            var relativeAddress = $"api/WorkOrders/";
            client.DefaultRequestHeaders.Add("authorization", new List<string>(){CurrentHttpContext.Authorization});
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json-patch+json");
            try
            {
                var scheduleResponse = await client.PostAsync(relativeAddress, httpContent);
                if (!scheduleResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error scheduling Task {taskEntity.Id}! Response was {scheduleResponse.StatusCode}: {scheduleResponse.ReasonPhrase} - {scheduleResponse.RequestMessage}");
                }
                return scheduleResponse.IsSuccessStatusCode; 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scheduling Task {taskEntity.Id}! {ex.Message}.  Check to make sure that the Foreman Task Scheduler is up and running.");
                return false;
            }

        }

        /// <summary>
        /// This method adds a ResultEntity to _context, but does NOT save the changes
        /// </summary>
        /// <param name="taskEntity"></param>
        /// <returns>the new ResultEntity</returns>
        private ResultEntity NewResultEntity(TaskEntity taskEntity)
        {
            var expirationSeconds = taskEntity.ExpirationSeconds;
            if (expirationSeconds <= 0) expirationSeconds = _options.TaskProcessMaxWaitSeconds;
            var resultEntity = new ResultEntity()
                {
                    TaskId = taskEntity.Id,
                    ApiUrl = taskEntity.ApiUrl,
                    InputString = taskEntity.InputString,
                    ExpirationSeconds = expirationSeconds,
                    Iterations = taskEntity.Iterations,
                    IntervalSeconds = taskEntity.IntervalSeconds,
                    Status = Data.TaskStatus.pending,
                    ExpectedOutput = taskEntity.ExpectedOutput,
                    SentDate = DateTime.UtcNow,
                    StatusDate = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };

            return resultEntity;
        }
    }
}


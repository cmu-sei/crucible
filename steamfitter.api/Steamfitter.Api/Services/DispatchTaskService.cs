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
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Steamfitter.Api.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Steamfitter.Api.Services
{
    public interface IDispatchTaskService
    {
        Task<IEnumerable<ViewModels.DispatchTask>> GetAsync(CancellationToken ct);
        Task<ViewModels.DispatchTask> GetAsync(Guid Id, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetByVmIdAsync(Guid vmId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTask>> GetSubtasksAsync(Guid triggerTaskId, CancellationToken ct);
        Task<ViewModels.DispatchTask> CreateAsync(ViewModels.DispatchTask DispatchTask, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> CreateAndExecuteAsync(ViewModels.DispatchTask dispatchTask, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> ExecuteAsync(Guid id, CancellationToken ct);
        Task<ViewModels.DispatchTask> UpdateAsync(Guid Id, ViewModels.DispatchTask DispatchTask, CancellationToken ct);
        Task<bool> DeleteAsync(Guid Id, CancellationToken ct);
    }

    public class DispatchTaskService : IDispatchTaskService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly IStackStormService _stackStormService;
        private readonly VmTaskProcessingOptions _options;
        private readonly IDispatchTaskResultService _dispatchTaskResultService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DispatchTaskService> _logger;
        private readonly IPlayerVmService _playerVmService;

        public DispatchTaskService(
            SteamfitterContext context, 
            IAuthorizationService authorizationService, 
            IPrincipal user, 
            IMapper mapper,
            IStackStormService stackStormService,
            IOptions<VmTaskProcessingOptions> options,
            IDispatchTaskResultService dispatchTaskResultService,
            ILogger<DispatchTaskService> logger,
            IPlayerVmService playerVmService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _stackStormService = stackStormService;
            _options = options.Value;
            _dispatchTaskResultService = dispatchTaskResultService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _playerVmService = playerVmService;
        }

        public async Task<IEnumerable<ViewModels.DispatchTask>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.DispatchTasks.ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<DispatchTask>>(items);
        }

        public async Task<ViewModels.DispatchTask> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.DispatchTasks
                .SingleOrDefaultAsync(o => o.Id == id, ct);
            if (item == null)
                throw new EntityNotFoundException<DispatchTask>();

            return _mapper.Map<DispatchTask>(item);
        }

        public async Task<IEnumerable<ViewModels.DispatchTask>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTasks = _context.DispatchTasks.Where(x => x.ScenarioId == scenarioId);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTask>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTasks = _context.DispatchTasks.Where(x => x.SessionId == sessionId);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTask>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var sessionIdList = _context.Sessions.Where(s => s.ExerciseId == exerciseId).Select(s => s.Id.ToString()).ToList();
            var DispatchTasks = _context.DispatchTasks.Where(dt => sessionIdList.Contains(dt.SessionId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTask>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTasks = _context.DispatchTasks.Where(dt => dt.UserId == userId && dt.ScenarioId == null && dt.SessionId == null);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTask>> GetByVmIdAsync(Guid vmId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmIdList = _context.DispatchTaskResults.Where(r => r.VmId == vmId).Select(r => r.Id.ToString()).ToList();
            var DispatchTasks = _context.DispatchTasks.Where(dt => vmIdList.Contains(dt.Id.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTask>> GetSubtasksAsync(Guid triggerTaskId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTasks = _context.DispatchTasks.Where(x => x.TriggerTaskId == triggerTaskId);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTask>>(DispatchTasks);
        }
        
        public async Task<ViewModels.DispatchTask> CreateAsync(ViewModels.DispatchTask dispatchTask, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();
            if (dispatchTask.VmMask != "" && dispatchTask.VmList.Count > 0)
                throw new InvalidOperationException("A DispatchTask cannot have BOTH a VmMask and a VmList!");
            // convert the list of vm guids into a comma separated string and save it in VmMask
            if (dispatchTask.VmList.Count() > 0)
            {
                var vmIdString = "";
                foreach (var vmId in dispatchTask.VmList)
                {
                    vmIdString = vmIdString + vmId + ",";
                }
                dispatchTask.VmMask = vmIdString.Remove(vmIdString.Count() - 1);
            }
            dispatchTask.DateCreated = DateTime.UtcNow;
            dispatchTask.CreatedBy = _user.GetId();
            dispatchTask.UserId = _user.GetId();
            var dispatchTaskEntity = Mapper.Map<DispatchTaskEntity>(dispatchTask);

            _context.DispatchTasks.Add(dispatchTaskEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(dispatchTaskEntity.Id, ct);
        }

        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> CreateAndExecuteAsync(ViewModels.DispatchTask dispatchTask, CancellationToken ct)
        {
            // create the DispatchTaskEntity
            dispatchTask = await CreateAsync(dispatchTask, ct);
            // execute the DispatchTaskEntity.  Authorization is null, because there can't be any subtasks on a CreateAndExecute.
            var dispatchTaskResultList = await ExecuteAsync(dispatchTask.Id, ct);

            return dispatchTaskResultList;
        }

        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> ExecuteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskToExecute = await _context.DispatchTasks.SingleOrDefaultAsync(v => v.Id == id, ct);
            await VerifyDispatchTaskToExecuteAsync(dispatchTaskToExecute, ct);

            // create DispatchTaskResults required (at least one but one for each VM)
            await CreateDispatchTaskResultsAsync(dispatchTaskToExecute, ct);
            var dispatchTaskResultEntityList = _context.DispatchTaskResults.Where(dtr => dtr.DispatchTaskId == dispatchTaskToExecute.Id && dtr.Status == Data.TaskStatus.pending).ToList();

            Data.TaskStatus overallStatus;
            // determine where to execute the task, execute it, process result
            switch (dispatchTaskToExecute.ApiUrl)
            {
                case "stackstorm":
                {
                    overallStatus = await ExecuteStackstormTaskAsync(dispatchTaskToExecute, dispatchTaskResultEntityList, ct);
                    break;
                }
                case "player":
                case "vm":
                case "caster":
                default:
                {
                    var message = $"API ({dispatchTaskToExecute.ApiUrl}) is not currently implemented";
                    _logger.LogError(message);
                    throw new NotImplementedException(message);
                }
            }

            // Process the subtasks
            var subtaskEntityList = GetSubtasksToExecute(dispatchTaskToExecute.Id, overallStatus);
            var allSubtasksWereProcessed = await ProcessSubtasks(subtaskEntityList, ct);
            if (!allSubtasksWereProcessed)
            {
                var message = $"One or more child tasks were not processed correctly for DispatchTask {dispatchTaskToExecute.Id}";
                _logger.LogError(message);
                throw new ApplicationException(message);
            }

            return _mapper.Map(dispatchTaskResultEntityList, new List<ViewModels.DispatchTaskResult>());
        }

        public async Task<ViewModels.DispatchTask> UpdateAsync(Guid id, ViewModels.DispatchTask dispatchTask, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskToUpdate = await _context.DispatchTasks.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (dispatchTaskToUpdate == null)
                throw new EntityNotFoundException<DispatchTask>();

            dispatchTask.CreatedBy = dispatchTaskToUpdate.CreatedBy;
            dispatchTask.DateCreated = dispatchTaskToUpdate.DateCreated;
            dispatchTask.DateModified = DateTime.UtcNow;
            dispatchTask.ModifiedBy = _user.GetId();
            Mapper.Map(dispatchTask, dispatchTaskToUpdate);

            _context.DispatchTasks.Update(dispatchTaskToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(dispatchTaskToUpdate, dispatchTask);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTaskToDelete = await _context.DispatchTasks.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (DispatchTaskToDelete == null)
                throw new EntityNotFoundException<DispatchTask>();

            _context.DispatchTasks.Remove(DispatchTaskToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        private async Task VerifyDispatchTaskToExecuteAsync(DispatchTaskEntity dispatchTaskToExecute, CancellationToken ct)
        {
            if (dispatchTaskToExecute == null)
            {
                throw new EntityNotFoundException<DispatchTask>();
            }
            else if (dispatchTaskToExecute.ScenarioId != null)
            {
                throw new ArgumentException($"DispatchTask {dispatchTaskToExecute.Id} is part of a scenario {dispatchTaskToExecute.ScenarioId} and cannot be executed.");
            }
            else if (dispatchTaskToExecute.SessionId != null)
            {
                await VerifySessionTaskAsync(dispatchTaskToExecute, ct);
            }
            else
            {
                VerifyManualTaskAsync(dispatchTaskToExecute);
            }
        }

        private async Task CreateDispatchTaskResultsAsync(DispatchTaskEntity dispatchTaskToExecute, CancellationToken ct)
        {
            var dispatchTaskResultEntities = new List<DispatchTaskResultEntity>();
            var sessionEntity = dispatchTaskToExecute.SessionId == null ? null : _context.Sessions.First(s => s.Id == dispatchTaskToExecute.SessionId);
            if (sessionEntity != null && sessionEntity.ExerciseId != null)
            {
                // if this task has a Session associated to an Exercise, create a VmList from the VmMask
                var exerciseVms = await _playerVmService.GetExerciseVmsAsync((Guid)sessionEntity.ExerciseId, ct);
                foreach (var vm in exerciseVms)
                {
                    if (vm.Name.ToLower().Contains(dispatchTaskToExecute.VmMask.ToLower()))
                    {
                        var dispatchTaskResultEntity = NewDispatchTaskResultEntity(dispatchTaskToExecute);
                        dispatchTaskResultEntity.VmId = vm.Id;
                        dispatchTaskResultEntity.VmName = vm.Name;
                        dispatchTaskResultEntities.Add(dispatchTaskResultEntity);
                    }
                }
            }
            else if (dispatchTaskToExecute.VmMask.Count() > 0)
            {
                var vmIdList = dispatchTaskToExecute.VmMask.Split(",").ToList().ConvertAll(Guid.Parse);
                // if this task has no Session check the VmMask for a list of VM ID's
                foreach (var id in vmIdList)
                {
                    var dispatchTaskResultEntity = NewDispatchTaskResultEntity(dispatchTaskToExecute);
                    dispatchTaskResultEntity.VmId = id;
                    dispatchTaskResultEntities.Add(dispatchTaskResultEntity);
                }
            }
            else
            {
                // this task has no VM's associated. Create one result entity
                var dispatchTaskResultEntity = NewDispatchTaskResultEntity(dispatchTaskToExecute);
                dispatchTaskResultEntities.Add(dispatchTaskResultEntity);
            }
            await _context.DispatchTaskResults.AddRangeAsync(dispatchTaskResultEntities);
            await _context.SaveChangesAsync(ct);

            return;
        }

        private async Task VerifySessionTaskAsync(DispatchTaskEntity dispatchTaskToExecute, CancellationToken ct)
        {
            var session = await _context.Sessions.FindAsync(dispatchTaskToExecute.SessionId);
            if (session.Status != SessionStatus.active)
            {
                var message = $"DispatchTask {dispatchTaskToExecute.Id} is part of session {session.Id}, which is not currently active.  A Session must be active in order to execute its DispatchTask.";
                _logger.LogInformation(message);
                throw new ArgumentException(message);
            }
            else{
                // TODO: add session specific restraints on whether or not this task can run
            }
        }

        private void VerifyManualTaskAsync(DispatchTaskEntity dispatchTaskToExecute)
        {
            var dispatchTaskResults = _context.DispatchTaskResults.Where(dtr => dtr.DispatchTaskId == dispatchTaskToExecute.Id && dtr.Status == Data.TaskStatus.pending);
            if (dispatchTaskResults.Any())
            {
                var message = $"DispatchTask {dispatchTaskToExecute.Id} has a pending result {dispatchTaskResults.First().Id}, so it cannot be manually executed.";
                _logger.LogInformation(message);
                throw new ArgumentException(message);
            }
        }

        private async Task<Data.TaskStatus> ExecuteStackstormTaskAsync(DispatchTaskEntity dispatchTaskToExecute, List<DispatchTaskResultEntity> resultEntityList, CancellationToken ct)
        {
            var overallStatus = Data.TaskStatus.succeeded;
            foreach (var resultEntity in resultEntityList)
            {
                Task<string> task = null;
                resultEntity.InputString = resultEntity.InputString.Replace("VmGuid", "Moid").Replace("{moid}", resultEntity.VmId.ToString());
                resultEntity.VmName = _stackStormService.GetVmName((Guid)resultEntity.VmId);
                switch (dispatchTaskToExecute.Action)
                {
                    case TaskAction.guest_file_read:
                    {
                        task = Task.Run(() => _stackStormService.GuestReadFile(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.guest_process_run:
                    {
                        task = Task.Run(() => _stackStormService.GuestCommand(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_power_on:
                    {
                        task = Task.Run(() => _stackStormService.VmPowerOn(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_power_off:
                    {
                        task = Task.Run(() => _stackStormService.VmPowerOff(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_create_from_template:
                    {
                        task = Task.Run(() => _stackStormService.CreateVmFromTemplate(resultEntity.InputString));
                        break;
                    }
                    case TaskAction.vm_hw_remove:
                    {
                        task = Task.Run(() => _stackStormService.VmRemove(resultEntity.InputString));
                        break;
                    }
                    default:
                    {
                        var message = $"Task Action {dispatchTaskToExecute.Action} has not been implemented.";
                        _logger.LogError(message);
                        resultEntity.Status = Data.TaskStatus.failed;
                        resultEntity.StatusDate = DateTime.UtcNow;
                        break;
                    }
                }

                if (task.Wait(TimeSpan.FromSeconds(resultEntity.ExpirationSeconds)))
                {
                    resultEntity.ActualOutput = task.Result.ToString();
                    resultEntity.Status = ProcessDispatchTaskResult(resultEntity, ct);
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

        private Data.TaskStatus ProcessDispatchTaskResult(DispatchTaskResultEntity dispatchTaskResultEntity, CancellationToken ct)
        {
            if (dispatchTaskResultEntity.Status == Data.TaskStatus.pending)
            {
                CheckSuccess(dispatchTaskResultEntity);
            }
            return dispatchTaskResultEntity.Status;
        }

        private void CheckSuccess(DispatchTaskResultEntity dispatchTaskResultEntity)
        {
            if (dispatchTaskResultEntity.ActualOutput.ToLower().Contains(dispatchTaskResultEntity.ExpectedOutput.ToLower()))
            {
                dispatchTaskResultEntity.Status = Data.TaskStatus.succeeded;
            }
            else
            {
                dispatchTaskResultEntity.Status = Data.TaskStatus.failed;
            }

        }

        private IEnumerable<DispatchTaskEntity> GetSubtasksToExecute(Guid executedTaskId, Data.TaskStatus executedTaskStatus)
        {
            var subtaskEntities = _context.DispatchTasks.Where(t => t.TriggerTaskId == executedTaskId);
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
                        return new List<DispatchTaskEntity>();
                    }
                }
            }

            return subtaskEntities;
        }

        private async Task<bool> ProcessSubtasks(IEnumerable<DispatchTaskEntity> subtaskEntityList, CancellationToken ct)
        {
            var allWentWell = true;
            // Handle subtasks, but don't wait for them
            foreach (var subtaskEntity in subtaskEntityList)
            {
                var wasPassedOn = false;
                if (subtaskEntity.DelaySeconds > 0)
                {
                    wasPassedOn = await ScheduleSubtask(subtaskEntity);
                    if (!wasPassedOn)
                    {
                        var resultEntity = NewDispatchTaskResultEntity(subtaskEntity);
                        resultEntity.Status = Data.TaskStatus.error;
                        await _context.DispatchTaskResults.AddAsync(resultEntity);
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

        private async Task<bool> ExecuteSubtask(Guid id)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(CurrentHttpContext.AppBaseUrl);
            var relativeAddress = $"DispatchTasks/{id}/execute/";
            client.DefaultRequestHeaders.Add("authorization", new List<string>(){CurrentHttpContext.Authorization});
            var executeResponse = await client.PostAsync(relativeAddress, new StringContent(""));

            return executeResponse.IsSuccessStatusCode;
        }

        private async Task<bool> ScheduleSubtask(DispatchTaskEntity subtaskEntity)
        {
            var startTime = DateTime.UtcNow.AddSeconds(subtaskEntity.DelaySeconds).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            var endTime = DateTime.UtcNow.AddSeconds(subtaskEntity.DelaySeconds + (subtaskEntity.Iterations * subtaskEntity.IntervalSeconds)).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            var workorder = new {
                groupName = "steamfitter",
                start = startTime,
                end = endTime,
                job = "WebHook",
                webhookid = _options.ForemanWebhookId,
                woTriggers = new[] {
                    new {
                        groupName = "steamfitter",
                        interval = subtaskEntity.IntervalSeconds
                    }
                },
                woParams = new [] {
                    new {
                        name = "[task.id]",
                        value = subtaskEntity.Id
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
                    _logger.LogError($"Error scheduling DispatchTask {subtaskEntity.Id}! Response was {scheduleResponse.StatusCode}: {scheduleResponse.ReasonPhrase} - {scheduleResponse.RequestMessage}");
                }
                return scheduleResponse.IsSuccessStatusCode; 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scheduling DispatchTask {subtaskEntity.Id}! {ex.Message}.  Check to make sure that the Foreman DispatchTask Scheduler is up and running.");
                return false;
            }

        }

        /// <summary>
        /// This method adds a DispatchTaskResultEntity to _context, but does NOT save the changes
        /// </summary>
        /// <param name="dispatchTaskEntity"></param>
        /// <returns>the new DispatchTaskResultEntity</returns>
        private DispatchTaskResultEntity NewDispatchTaskResultEntity(DispatchTaskEntity dispatchTaskEntity)
        {
            var expirationSeconds = dispatchTaskEntity.ExpirationSeconds;
            if (expirationSeconds <= 0) expirationSeconds = _options.DispatchTaskProcessMaxWaitSeconds;
            var dispatchTaskResultEntity = new DispatchTaskResultEntity()
                {
                    DispatchTaskId = dispatchTaskEntity.Id,
                    ApiUrl = dispatchTaskEntity.ApiUrl,
                    InputString = dispatchTaskEntity.InputString,
                    ExpirationSeconds = expirationSeconds,
                    Iterations = dispatchTaskEntity.Iterations,
                    IntervalSeconds = dispatchTaskEntity.IntervalSeconds,
                    Status = Data.TaskStatus.pending,
                    ExpectedOutput = dispatchTaskEntity.ExpectedOutput,
                    SentDate = DateTime.UtcNow,
                    StatusDate = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };

            return dispatchTaskResultEntity;
        }
    }
}


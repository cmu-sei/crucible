/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using IdentityModel.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Hubs;
using Steamfitter.Api.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using STT = System.Threading.Tasks;
using S3.VM.Api;

namespace Steamfitter.Api.Services
{
    public interface ITaskExecutionService : IHostedService
    {
    }

    public class TaskExecutionService : ITaskExecutionService
    {
        private readonly ILogger<TaskExecutionService> _logger;
        private readonly IOptionsMonitor<Infrastructure.Options.VmTaskProcessingOptions> _vmTaskProcessingOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITaskExecutionQueue _taskExecutionQueue;
        private readonly IMapper _mapper;
        private readonly IHubContext<EngineHub> _engineHub;
        private readonly IStackStormService _stackStormService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<Infrastructure.Options.ClientOptions> _clientOptions;

        public TaskExecutionService(
            ILogger<TaskExecutionService> logger,
            IOptionsMonitor<Infrastructure.Options.VmTaskProcessingOptions> vmTaskProcessingOptions,
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            IHubContext<EngineHub> engineHub,
            IStackStormService stackStormService,
            ITaskExecutionQueue taskExecutionQueue,
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<Infrastructure.Options.ClientOptions> clientOptions)
        {
            _logger = logger;
            _vmTaskProcessingOptions = vmTaskProcessingOptions;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _engineHub = engineHub;
            _stackStormService = stackStormService;
            _taskExecutionQueue = taskExecutionQueue;
            _httpClientFactory = httpClientFactory;
            _clientOptions = clientOptions;
        }

        public STT.Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Run();

            return STT.Task.CompletedTask;
        }

        public STT.Task StopAsync(CancellationToken cancellationToken)
        {
            return STT.Task.CompletedTask;
        }

        /// <summary>
        /// Bootstraps (loads data) Tasks that were in process when this api encounters a stop/start cycle
        /// </summary>
        private async STT.Task Bootstrap()
        {
            _logger.LogInformation($"TaskExecutionService is starting Bootstrap.");
            var bootstrapComplete = false;
            while (!bootstrapComplete)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        using (var steamfitterContext = scope.ServiceProvider.GetRequiredService<SteamfitterContext>())
                        {
                            // get tasks that are currently "in process"
                            var queuedTaskEntities = steamfitterContext.Tasks
                                .Where(task => task.Results.Any(result => result.Status == TaskStatus.queued)).Include(task => task.Results);

                            if (queuedTaskEntities.Any())
                            {
                                _logger.LogDebug($"TaskExecutionService is queueing {queuedTaskEntities.Count()} Tasks.");
                                foreach (var taskEntity in queuedTaskEntities)
                                {
                                    var userId = taskEntity.Results.Where(result => result.Status == TaskStatus.queued).First().CreatedBy;
                                    taskEntity.UserId = userId;
                                    _taskExecutionQueue.Add(taskEntity);
                                    _logger.LogDebug($"TaskExecutionService is queueing Task {taskEntity.Id}.");
                                }
                            }
                        }
                    }
                    bootstrapComplete = true;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError("Exception encountered in TaskExecutionService Bootstrap.", ex);
                    await STT.Task.Delay(new TimeSpan(0, 0, _vmTaskProcessingOptions.CurrentValue.HealthCheckSeconds));
                }
            }
            _logger.LogInformation("TaskExecutionService Bootstrap complete.");
        }

        private async STT.Task Run()
        {
            await Bootstrap();

            await STT.Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        _logger.LogDebug("The TaskExecutionService is ready to process tasks.");
                        // _implementatioQueue is a BlockingCollection, so this loop will sleep if nothing is in the queue
                        var taskEntity = _taskExecutionQueue.Take(new CancellationToken());
                        // process the taskEntity on a new thread
                        // When adding a Task to the TaskExecutionQueue, the UserId MUST be changed to the current UserId, so that all results can be assigned to the correct user
                        var newThread = new Thread(ProcessTheTask);
                        newThread.Start(taskEntity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception encountered in TaskExecutionService Run loop.", ex);
                    }
                }
            });
        }

        private async void ProcessTheTask(Object taskEntityAsObject)
        {
            var ct = new CancellationToken();
            var taskEntity = taskEntityAsObject == null ? (TaskEntity)null : (TaskEntity)taskEntityAsObject;
            _logger.LogDebug($"Processing Task '{taskEntity.Name}' ({taskEntity.Id}).");
            // When adding a Task to the TaskExecutionQueue, the UserId MUST be changed to the current UserId, so that all results can be assigned to the correct user
            var userId = taskEntity.UserId != null ? (Guid)taskEntity.UserId : new Guid();
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                using (var steamfitterContext = scope.ServiceProvider.GetRequiredService<SteamfitterContext>())
                // using (var engineHub = scope.ServiceProvider.GetRequiredService<EngineHub>())
                {
                    {
                        var taskToExecute = await steamfitterContext.Tasks.SingleOrDefaultAsync(v => v.Id == taskEntity.Id, ct);
                        await VerifyTaskToExecuteAsync(taskToExecute, steamfitterContext, ct);
                        // get any queued results (means this task was going to execute, but the API crashed before it could)
                        var resultEntityList = steamfitterContext.Results.Where(dtr => dtr.TaskId == taskToExecute.Id && dtr.Status == Data.TaskStatus.queued).ToList();
                        // determine the delay required for this task execution
                        var delaySeconds = 0;
                        if ((resultEntityList.Any() && taskToExecute.CurrentIteration == 1) || (!resultEntityList.Any() && taskToExecute.CurrentIteration == 0))
                        {
                            delaySeconds = taskToExecute.DelaySeconds;
                        }
                        else
                        {
                            delaySeconds = taskToExecute.IntervalSeconds;
                        }
                        if (resultEntityList.Any())
                        {
                            var now = DateTime.UtcNow;
                            foreach (var resultEntity in resultEntityList)
                            {
                                resultEntity.StatusDate = now;
                            }
                            await steamfitterContext.SaveChangesAsync();
                            var currentWait = DateTime.UtcNow.Subtract(resultEntityList[0].DateCreated).TotalSeconds;
                            delaySeconds = currentWait < delaySeconds ? delaySeconds - (int)currentWait : 0;
                        }
                        else
                        {
                            var scenarioEntity = taskToExecute.ScenarioId == null ? null : steamfitterContext.Scenarios.First(s => s.Id == taskToExecute.ScenarioId);
                            resultEntityList = await CreateResultsAsync(taskToExecute, scenarioEntity, userId, ct);
                            await steamfitterContext.Results.AddRangeAsync(resultEntityList);
                            taskToExecute.CurrentIteration++;
                            await steamfitterContext.SaveChangesAsync(ct);
                            _engineHub.Clients.All.SendAsync(EngineMethods.ResultsUpdated, _mapper.Map<IEnumerable<ViewModels.Result>>(resultEntityList));
                        }
                        // make sure there were no errors creating the results before continuing
                        if (resultEntityList.Where(r => r.Status == Data.TaskStatus.error).Count() == 0)
                        {
                            // delay if necessary
                            if (delaySeconds > 0)
                            {
                                await STT.Task.Delay(new TimeSpan(0, 0, delaySeconds));
                            }
                            // ACTUALLY execute the task and process results
                            var overallStatus = await ProcessTaskAsync(taskToExecute, resultEntityList, steamfitterContext, ct);

                            // Start the next Task (iteration or subtask)
                            if (IsAnotherIteration(taskToExecute, overallStatus))
                            {
                                // Process the next Iteration
                                _taskExecutionQueue.Add(taskToExecute);
                            }
                            else
                            {
                                // Process the subtasks
                                var subtaskEntityList = GetSubtasksToExecute(taskToExecute.Id, steamfitterContext, overallStatus);
                                foreach (var subtaskEntity in subtaskEntityList)
                                {
                                    _taskExecutionQueue.Add(subtaskEntity);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error processing Task {taskEntity.Id}", ex);
            }
        }

        private async STT.Task<bool> VerifyTaskToExecuteAsync(TaskEntity taskToExecute, SteamfitterContext steamfitterContext, CancellationToken ct)
        {
            if (taskToExecute == null || taskToExecute.ScenarioTemplateId != null)
            {
                var message = $"Task {taskToExecute.Id} is part of scenario template, so it cannot be executed.";
                _logger.LogDebug(message);
                return false;
            }
            else if (taskToExecute.ScenarioId != null)
            {
                var scenario = await steamfitterContext.Scenarios.FindAsync(taskToExecute.ScenarioId);
                if (scenario.Status != ScenarioStatus.active)
                {
                    var message = $"Task {taskToExecute.Id} is part of scenario {scenario.Id}, which is not currently active.  A Scenario must be active in order to execute its Task.";
                    _logger.LogDebug(message);
                    return false;
                }
                else{
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private bool IsAnotherIteration(TaskEntity task, Data.TaskStatus taskStatus)
        {
            // task.Iterations is always the hard stop
            var isAnotherIteration = task.CurrentIteration < task.Iterations;
            if (isAnotherIteration)
            {
                // still another iteration, unless the termination status has been achieved
                switch (task.IterationTermination)
                {
                    case Data.TaskIterationTermination.UntilSuccess:
                        isAnotherIteration = taskStatus != Data.TaskStatus.succeeded;
                        break;
                    case Data.TaskIterationTermination.UntilFailure:
                        isAnotherIteration = taskStatus != Data.TaskStatus.failed;
                        break;
                    default:
                        break;
                }
            }
            return isAnotherIteration;
        }

        private async STT.Task<List<ResultEntity>> CreateResultsAsync(TaskEntity taskToExecute, ScenarioEntity scenarioEntity, Guid userId, CancellationToken ct)
        {
            var resultEntities = new List<ResultEntity>();
            Guid? viewId = null;
            if (taskToExecute.VmMask.Count() == 0)
            {
                // this task has no VM's associated. Create one result entity.  Used to send a command to an API, etc.
                var resultEntity = NewResultEntity(taskToExecute, userId);
                resultEntities.Add(resultEntity);
            }
            else
            {
                // A scenario is assigned to a specific view
                viewId = scenarioEntity.ViewId;
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
                // create a VmList from the view ID and the VmMask matching criteria
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        S3VmApiClient vmApiClient = null;
                        var tokenResponse = await ApiClientsExtensions.GetToken(scope);
                        vmApiClient = RefreshClient(vmApiClient, tokenResponse, ct);
                        var viewVms = await VmApiExtensions.GetViewVmsAsync(vmApiClient, (Guid)viewId, ct);
                        foreach (var vm in viewVms)
                        {
                            if ((!matchName && vmIdList.Contains((Guid)vm.Id)) || (matchName && vm.Name.ToLower().Contains(taskToExecute.VmMask.ToLower())))
                            {
                                vmList.Add(vm);
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    _logger.LogDebug($"CreateResultsAsync - No VM's found in view {viewId}");
                }
                // make sure we are only matching a SINGLE view
                if (vmList.Count() > 0)
                {
                    // create a result for each matched VM
                    foreach (var vm in vmList)
                    {
                        var resultEntity = NewResultEntity(taskToExecute, userId);
                        resultEntity.VmId = vm.Id;
                        resultEntity.VmName = vm.Name;
                        resultEntities.Add(resultEntity);
                    }
                }
                else
                {
                    // Houston, we've had a problem.  Create a single result to show the error
                    var resultEntity = NewResultEntity(taskToExecute, userId);
                    if (viewId != null)
                    {
                        // The problem is that NO VM's matched
                        resultEntity.ActualOutput = $"No matched VMs!  VM Mask: {taskToExecute.VmMask}.";
                    }
                    else if (scenarioEntity != null)
                    {
                        // The problem is that this task did not have a scenario or a viewId
                        resultEntity.ActualOutput = $"There was no view associated with this task's scenario!  {taskToExecute.Name} ({taskToExecute.Id})";
                    }
                    else
                    {
                        // The problem is that this task did not have a scenario or a viewId
                        resultEntity.ActualOutput = $"There was no scenario associated with this task!  {taskToExecute.Name} ({taskToExecute.Id})";
                    }
                    _logger.LogError($"CreateResultsAsync - {resultEntity.ActualOutput}");
                    resultEntity.Status = Data.TaskStatus.error;
                    resultEntity.StatusDate = DateTime.UtcNow;
                    resultEntities.Add(resultEntity);
                }
            }
            return resultEntities;
        }

        private async STT.Task<Data.TaskStatus> ProcessTaskAsync(TaskEntity taskToExecute, List<ResultEntity> resultEntityList, SteamfitterContext steamfitterContext, CancellationToken ct)
        {
            var overallStatus = Data.TaskStatus.succeeded;
            var tasks = new List<STT.Task<string>>();
            var xref = new Dictionary<int, ResultEntity>();
            foreach (var resultEntity in resultEntityList)
            {
                resultEntity.InputString = resultEntity.InputString.Replace("{moid}", resultEntity.VmId.ToString());
                resultEntity.VmName = _stackStormService.GetVmName((Guid)resultEntity.VmId);
                resultEntity.Status = TaskStatus.pending;
                resultEntity.StatusDate = DateTime.UtcNow;
                // if no expiration is set, us the maximum allowed by the TaskProcessMaxWaitSeconds setting
                resultEntity.ExpirationSeconds = resultEntity.ExpirationSeconds <= 0 ? _vmTaskProcessingOptions.CurrentValue.TaskProcessMaxWaitSeconds : resultEntity.ExpirationSeconds;
                var task = RunTask(taskToExecute, resultEntity, ct);
                tasks.Add(task);
                xref[task.Id] = resultEntity;
                await steamfitterContext.SaveChangesAsync();
                _engineHub.Clients.All.SendAsync(EngineMethods.ResultUpdated, _mapper.Map<ViewModels.Result>(resultEntity));
            }
            foreach (var bucket in AsCompletedBuckets(tasks))
            {
                try 
                {
                    var task = await bucket;
                    var resultEntity = xref[task.Id];
                    resultEntity.ActualOutput = task.Result == null ? "" : task.Result.ToString();
                    resultEntity.Status = ProcessResult(resultEntity, ct);
                    resultEntity.StatusDate = DateTime.UtcNow;
                    if (resultEntity.Status != Data.TaskStatus.succeeded)
                    {
                        if (overallStatus != Data.TaskStatus.failed)
                        {
                            overallStatus = resultEntity.Status;
                        }
                    }
                    await steamfitterContext.SaveChangesAsync();
                    _engineHub.Clients.All.SendAsync(EngineMethods.ResultUpdated, _mapper.Map<ViewModels.Result>(resultEntity));
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("the executing task was cancelled");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("the executing task caused an exception", ex);
                }
            }
            return overallStatus;
        }

        private STT.Task<string> RunTask(TaskEntity taskToExecute, ResultEntity resultEntity, CancellationToken ct)
        {
            STT.Task<string> task = null;
            switch (taskToExecute.ApiUrl)
            {
                case "stackstorm": // _stackStormService
                {
                    switch (taskToExecute.Action)
                    {
                        case TaskAction.guest_file_read:
                        {
                            task = STT.Task.Run(() => _stackStormService.GuestReadFile(resultEntity.InputString));
                            break;
                        }
                        case TaskAction.guest_file_upload_content:
                        {
                            task = STT.Task.Run(() => _stackStormService.GuestFileUploadContent(resultEntity.InputString));
                            break;
                        }
                        case TaskAction.guest_process_run:
                        {
                            task = STT.Task.Run(() => _stackStormService.GuestCommand(resultEntity.InputString));
                            break;
                        }
                        case TaskAction.guest_process_run_fast:
                        {
                            task = STT.Task.Run(() => _stackStormService.GuestCommandFast(resultEntity.InputString));
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
                            var message = $"Stackstorm Action {taskToExecute.Action} has not been implemented.";
                            _logger.LogError(message);
                            resultEntity.Status = Data.TaskStatus.failed;
                            resultEntity.StatusDate = DateTime.UtcNow;
                            break;
                        }
                    }
                    break;
                }
                case "vm": // _playerVmService
                {
                    switch (taskToExecute.Action)
                    {
                        case TaskAction.guest_file_write:
                        default:
                        {
                            var message = $"Player VM API Action {taskToExecute.Action} has not been implemented.";
                            _logger.LogError(message);
                            resultEntity.Status = Data.TaskStatus.failed;
                            resultEntity.StatusDate = DateTime.UtcNow;
                            break;
                        }
                    }
                    break;
                }
                case "player":
                case "caster":
                default:
                {
                    var message = $"API ({taskToExecute.ApiUrl}) is not currently implemented";
                    _logger.LogError(message);
                    throw new NotImplementedException(message);
                }
            }

            return task;
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
            resultEntity.StatusDate = DateTime.UtcNow;
        }

        private IEnumerable<TaskEntity> GetSubtasksToExecute(Guid executedTaskId, SteamfitterContext steamfitterContext, Data.TaskStatus executedTaskStatus)
        {
            var subtaskEntities = steamfitterContext.Tasks.Where(t => t.TriggerTaskId == executedTaskId);
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

        private ResultEntity NewResultEntity(TaskEntity taskEntity, Guid userId)
        {
            var expirationSeconds = taskEntity.ExpirationSeconds;
            if (expirationSeconds <= 0) expirationSeconds = _vmTaskProcessingOptions.CurrentValue.TaskProcessMaxWaitSeconds;
            var resultEntity = new ResultEntity()
                {
                    TaskId = taskEntity.Id,
                    ApiUrl = taskEntity.ApiUrl,
                    Action = taskEntity.Action,
                    InputString = taskEntity.InputString,
                    ExpirationSeconds = expirationSeconds,
                    Iterations = taskEntity.Iterations,
                    CurrentIteration = taskEntity.CurrentIteration > 0 ? taskEntity.CurrentIteration : 1,
                    IntervalSeconds = taskEntity.IntervalSeconds,
                    Status = Data.TaskStatus.queued,
                    ExpectedOutput = taskEntity.ExpectedOutput,
                    SentDate = DateTime.UtcNow,
                    StatusDate = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = userId
                };

            return resultEntity;
        }
        public static STT.Task<STT.Task<T>> [] AsCompletedBuckets<T>(IEnumerable<STT.Task<T>> tasks)
        {
            var inputTasks = tasks.ToList();

            var buckets = new STT.TaskCompletionSource<STT.Task<T>>[inputTasks.Count];
            var results = new STT.Task<STT.Task<T>>[buckets.Length];
            for (int i = 0; i < buckets.Length; i++) 
            {
                buckets[i] = new STT.TaskCompletionSource<STT.Task<T>>();
                results[i] = buckets[i].Task;
            }

            int nextTaskIndex = -1;
            Action<STT.Task<T>> continuation = completed =>
            {
                var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            };

            foreach (var inputTask in inputTasks)
                inputTask.ContinueWith(continuation, CancellationToken.None, STT.TaskContinuationOptions.ExecuteSynchronously, STT.TaskScheduler.Default);

            return results;
        }

        private S3VmApiClient RefreshClient(S3VmApiClient clientObject, TokenResponse tokenResponse, CancellationToken ct)
        {
            // TODO: check for token expiration also
            if (clientObject == null)
            {
                clientObject = VmApiExtensions.GetVmApiClient(_httpClientFactory, _clientOptions.CurrentValue.urls.vmApi, tokenResponse);
            }

            return clientObject;
        }

    }

}

/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NetVimClient;
using Player.Vm.Api.Domain.Vsphere.Options;
using Player.Vm.Api.Domain.Vsphere.Extensions;
using Player.Vm.Api.Domain.Vsphere.Models;
using Player.Vm.Api.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Player.Vm.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Nito.AsyncEx;
using Player.Vm.Api.Infrastructure.Extensions;

namespace Player.Vm.Api.Domain.Vsphere.Services
{
    public interface ITaskService
    {
        void CheckTasks();
    }

    public class TaskService : BackgroundService, ITaskService
    {
        private readonly IHubContext<ProgressHub> _progressHub;
        private readonly ILogger<TaskService> _logger;
        private VsphereOptions _options;
        private readonly IOptionsMonitor<VsphereOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private VmContext _dbContext;

        private IConnectionService _connectionService;
        private IMachineStateService _machineStateService;
        private VimPortTypeClient _client;
        private ServiceContent _sic;
        private ManagedObjectReference _props;
        private ConcurrentDictionary<string, List<Notification>> _runningTasks = new ConcurrentDictionary<string, List<Notification>>();
        private AsyncAutoResetEvent _resetEvent = new AsyncAutoResetEvent(false);
        private bool _tasksPending = false;


        public TaskService(
                IOptionsMonitor<VsphereOptions> optionsMonitor,
                ILogger<TaskService> logger,
                IHubContext<ProgressHub> progressHub,
                IConnectionService connectionService,
                IMachineStateService machineStateService,
                IServiceProvider serviceProvider
            )
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _progressHub = progressHub;
            _connectionService = connectionService;
            _serviceProvider = serviceProvider;
            _machineStateService = machineStateService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _options = _optionsMonitor.CurrentValue;
                    _client = _connectionService.GetClient();
                    _sic = _connectionService.GetServiceContent();
                    _props = _connectionService.GetProps();
                    _tasksPending = false;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _dbContext = scope.ServiceProvider.GetRequiredService<VmContext>();
                        await processTasks();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception in TaskService");
                }

                var intervalMilliseconds = _tasksPending ?
                    _options.ReCheckTaskProgressIntervalMilliseconds :
                    _options.CheckTaskProgressIntervalMilliseconds;

                await _resetEvent.WaitAsync(new TimeSpan(0, 0, 0, 0, intervalMilliseconds));
            }
        }

        public void CheckTasks()
        {
            _resetEvent.Set();
        }

        private async Task processTasks()
        {
            await getRecentTasks();
            foreach (var vmTasks in _runningTasks)
            {
                try
                {
                    await this._progressHub.Clients.Group(vmTasks.Key).SendAsync("Progress", vmTasks.Value);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex.Message);
                }
            }
        }

        private async Task getRecentTasks()
        {
            if (_sic != null && _sic.taskManager != null)
            {
                var pendingVms = await _dbContext.Vms
                    .Include(x => x.VmTeams)
                    .Where(x => x.HasPendingTasks)
                    .ToArrayAsync();

                var stillPendingVmIds = new List<Guid>();

                PropertyFilterSpec[] filters = createPFSForRecentTasks(_sic.taskManager);
                RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(_props, filters);
                _runningTasks.Clear();
                var forceCheckMachineState = false;

                foreach (var task in response.returnval)
                {
                    try
                    {
                        var vmRef = ((ManagedObjectReference)task.GetProperty("info.entity")).Value;
                        var vmId = _connectionService.GetVmIdByRef(vmRef);
                        var broadcastTime = DateTime.UtcNow.ToString();
                        var taskId = task.GetProperty("info.key") != null ? task.GetProperty("info.key").ToString() : "";
                        var taskType = task.GetProperty("info.descriptionId") != null ? task.GetProperty("info.descriptionId").ToString() : "";
                        var progress = task.GetProperty("info.progress") != null ? task.GetProperty("info.progress").ToString() : "";
                        var state = task.GetProperty("info.state") != null ? task.GetProperty("info.state").ToString() : "";
                        var notification = new Notification()
                        {
                            broadcastTime = DateTime.UtcNow.ToString(),
                            taskId = taskId,
                            taskName = taskType.Replace("VirtualMachine.", ""),
                            taskType = taskType,
                            progress = progress,
                            state = state
                        };

                        if (vmId.HasValue)
                        {
                            var id = vmId.Value.ToString();
                            var vmTasks = _runningTasks.ContainsKey(id) ? _runningTasks[id] : new List<Notification>();
                            vmTasks.Add(notification);
                            _runningTasks.AddOrUpdate(id, vmTasks, (k, v) => (v = vmTasks));
                        }


                        if ((state == TaskInfoState.queued.ToString() || state == TaskInfoState.running.ToString()))
                        {
                            _tasksPending = true;

                            if (vmId.HasValue)
                            {
                                stillPendingVmIds.Add(vmId.Value);
                            }
                        }

                        if (state == TaskInfoState.success.ToString() &&
                            (this.GetPowerTaskTypes().Contains(taskType)))
                        {
                            if (vmId.HasValue)
                            {
                                forceCheckMachineState = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex.Message);
                    }
                }

                foreach (var vm in pendingVms)
                {
                    if (!stillPendingVmIds.Contains(vm.Id))
                    {
                        vm.HasPendingTasks = false;
                    }
                }

                var vmsToUpdate = await _dbContext.Vms
                    .Include(x => x.VmTeams)
                    .Where(x => stillPendingVmIds.Contains(x.Id))
                    .ToArrayAsync();

                foreach (var vm in vmsToUpdate)
                {
                    vm.HasPendingTasks = true;
                }

                await _dbContext.SaveChangesAsync();

                if (forceCheckMachineState)
                {
                    _machineStateService.CheckState();
                }
            }
        }

        private string[] GetPowerTaskTypes()
        {
            return new string[]
            {
                "VirtualMachine.powerOff",
                "VirtualMachine.powerOn",
            };
        }


        private PropertyFilterSpec[] createPFSForRecentTasks(ManagedObjectReference taskManagerRef)
        {
            PropertySpec pSpec = new PropertySpec();
            pSpec.all = false;
            pSpec.type = "Task";
            pSpec.pathSet = new String[]
                {
                    "info.entity",
                    "info.descriptionId",
                    "info.name",
                    "info.state",
                    "info.progress",
                    "info.cancelled",
                    "info.error",
                    "info.key"
                };

            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = taskManagerRef;
            oSpec.skip = false;
            oSpec.skipSpecified = true;

            TraversalSpec tSpec = new TraversalSpec();
            tSpec.type = "TaskManager";
            tSpec.path = "recentTask";
            tSpec.skip = false;


            oSpec.selectSet = new SelectionSpec[] { tSpec };

            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            pfSpec.propSet = new PropertySpec[] { pSpec };
            pfSpec.objectSet = new ObjectSpec[] { oSpec };

            return new PropertyFilterSpec[] { pfSpec };
        }
    }
}
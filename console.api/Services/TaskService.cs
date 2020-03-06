/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.Vm.Console.Extensions;
using S3.Vm.Console.Hubs;
using S3.Vm.Console.Models;
using S3.Vm.Console.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NetVimClient;

namespace S3.Vm.Console.Services
{
    public class TaskService : IHostedService
    {
        private readonly IHubContext<ProgressHub> _progressHub;
        private readonly ILogger<TaskService> _logger;
        private VmOptions _options;

        private IConnectionService _connectionService;
        private VimPortTypeClient _client;
        private ServiceContent _sic;
        private ManagedObjectReference _props;
        private ConcurrentDictionary<string, List<Notification>> _runningTasks = new ConcurrentDictionary<string, List<Notification>>();


        public TaskService(
                IOptions<VmOptions> options,
                ILogger<TaskService> logger,
                IHubContext<ProgressHub> progressHub,
                IConnectionService connectionService
            )
        {
            _options = options.Value;
            _logger = logger;
            _progressHub = progressHub;
            _connectionService = connectionService;
        }

        public System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            Run();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private async void Run()
        {
            while (true)
            {
                _client = _connectionService.GetClient();
                _sic = _connectionService.GetServiceContent();
                _props = _connectionService.GetProps();
                await processTasks();
                await Task.Delay(new TimeSpan(0, 0, 0, 0, _options.CheckTaskProgressIntervalMilliseconds));
            }
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
                PropertyFilterSpec[] filters = createPFSForRecentTasks(_sic.taskManager);
                RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(_props, filters);
                _runningTasks.Clear();
                foreach (var task in response.returnval)
                {
                    try
                    {
                        var vmString = _connectionService.GetVmGuidByName(((ManagedObjectReference)task.GetProperty("info.entity")).Value).ToString();
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
                        var vmTasks = _runningTasks.ContainsKey(vmString) ? _runningTasks[vmString] : new List<Notification>();
                        vmTasks.Add(notification);
                        _runningTasks.AddOrUpdate(vmString, vmTasks, (k, v) => (v = vmTasks));
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex.Message);
                    }
                }
            }
        }


        private PropertyFilterSpec[] createPFSForRecentTasks(ManagedObjectReference taskManagerRef) 
        {
            PropertySpec pSpec = new PropertySpec();
            pSpec.all= false;
            pSpec.type="Task";
            pSpec.pathSet= new String[]
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
            oSpec.skip= false;
            oSpec.skipSpecified = true;
                
            TraversalSpec tSpec = new TraversalSpec();
            tSpec.type="TaskManager";
            tSpec.path="recentTask";
            tSpec.skip= false;
                    
                
            oSpec.selectSet=new SelectionSpec[]{tSpec};      
                
            PropertyFilterSpec pfSpec = new PropertyFilterSpec();      
            pfSpec.propSet=new PropertySpec[]{pSpec};      
            pfSpec.objectSet=new ObjectSpec[]{oSpec};
                
            return new PropertyFilterSpec[]{pfSpec};
        }


    }
}



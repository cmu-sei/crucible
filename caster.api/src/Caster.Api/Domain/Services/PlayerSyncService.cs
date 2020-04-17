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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Caster.Api.Data;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S3.VM.Api;
using S3.VM.Api.Models;

namespace Caster.Api.Domain.Services
{
    public interface IPlayerSyncService : IHostedService
    {
        Task AddAsync(Guid workspaceId);
        void CheckRemovedResources();
    }

    public class PlayerSyncService : IPlayerSyncService
    {
        private class CancellationSync
        {
            public Object Lock { get; set; } 
            public CancellationTokenSource Source { get; set; }
        }

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptionsMonitor<PlayerOptions> _playerOptions;
        private readonly ILogger<PlayerSyncService> _logger;

        ActionBlock<Guid> _jobQueue;        
        ConcurrentDictionary<Guid, Object> _locks = new ConcurrentDictionary<Guid, object>();
        ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationTokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        AutoResetEvent _reset = new AutoResetEvent(false);

        public PlayerSyncService(IServiceScopeFactory serviceScopeFactory, IOptionsMonitor<PlayerOptions> playerOptions, ILogger<PlayerSyncService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;           
            _playerOptions = playerOptions; 
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = ExecuteAsync();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _jobQueue.Complete();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async Task AddAsync(Guid workspaceId)
        {
            await _jobQueue.SendAsync(workspaceId);
        }

        public void CheckRemovedResources()
        {
            _reset.Set();
        }

        private async Task ExecuteAsync()
        {
            new Thread(new ThreadStart(this.SyncRemovedResources)).Start();

            _jobQueue = new ActionBlock<Guid>(
                async workspaceId => await ProcessItem(workspaceId), 
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = _playerOptions.CurrentValue.MaxParallelism
                }
            );        

            // Get workspaces that need to be synced since last app shutdown
            foreach (var item in (await this.GetItemsToProcess()))
            {
                await _jobQueue.SendAsync(item);
            }

            _jobQueue.Completion.Wait();
        }

        private async Task<Guid[]> GetItemsToProcess()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // Get all Workspaces that have 1 or more Runs created after it's LastSyncTime
                var dbContext = scope.ServiceProvider.GetRequiredService<CasterContext>();

                var workspaceDict = await dbContext.Workspaces
                    .AsNoTracking()
                    .ToDictionaryAsync(x => x.Id, x => x.LastSynced);

                var runs = await dbContext.Runs
                    .AsNoTracking()
                    .GroupBy(x => x.WorkspaceId)
                    .Select(g => new {
                        WorkspaceId = g.Key,
                        LastRun = g.Max(r => r.CreatedAt)
                    })
                    .ToArrayAsync();

                var workspaceIds = new List<Guid>();

                foreach (var kvp in workspaceDict)
                {
                    var run = runs.Where(r => r.WorkspaceId == kvp.Key).FirstOrDefault();

                    if (run != null && Nullable.Compare(run.LastRun, kvp.Value) > 0)
                    {
                        workspaceIds.Add(kvp.Key);
                    }
                }

                return workspaceIds.ToArray();
            }
        }

        private async Task ProcessItem(Guid workspaceId)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    CancellationToken ct;

                    var workspaceLock = _locks.GetOrAdd(workspaceId, x => {
                        return new Object();
                    });

                    lock(workspaceLock)
                    {
                        CancellationTokenSource tokenSource;

                        if (_cancellationTokens.ContainsKey(workspaceId))
                        {
                            tokenSource = _cancellationTokens[workspaceId];

                            if (!tokenSource.IsCancellationRequested)
                                tokenSource.Cancel();
                        }
                    
                        tokenSource = new CancellationTokenSource();
                        _cancellationTokens[workspaceId] = tokenSource;
                        ct = tokenSource.Token;
                    }

                    await SyncWorkspace(workspaceId, scope, ct);
                }
            }
            catch(TaskCanceledException ex)
            {
                // expected to happen if another threads comes in to sync the same workspace
                _logger.LogTrace($"{typeof(TaskCanceledException).Name} when syncing workspace with Id {workspaceId}", ex);
            }
            catch(Exception ex)
            {          
                _logger.LogError($"Exception trying to sync workspace with Id {workspaceId}", ex);
            }
        }

        private async Task SyncWorkspace(Guid workspaceId, IServiceScope scope, CancellationToken ct)
        {        
            var errorMessages = new List<string>();

            var dbContext = scope.ServiceProvider.GetRequiredService<CasterContext>();
            var client = scope.ServiceProvider.GetRequiredService<IS3VmApiClient>();
            var playerOptions = scope.ServiceProvider.GetRequiredService<PlayerOptions>();

            var workspace = await dbContext.Workspaces.SingleOrDefaultAsync(x => x.Id == workspaceId, ct);

            if (workspace == null)
                return;
             
            var state = workspace.GetState();
            var previousState = workspace.GetStateBackup();

            var resources = state.GetResources();
            var previousResources = previousState.GetResources();

            // Get all vms from vm api
            VmSummary[] virtualMachines = null;
            
            try
            {
                virtualMachines = (await client.GetAllAsync(ct)).ToArray();
            }             
            catch(ApiErrorException ex)
            {
                errorMessages.Add($"Error retrieving Virtual Machines: {ex.Body.Title}");
            }
            catch(Exception ex)
            {
                errorMessages.Add($"Error retrieving Virtual Machines: {ex.Message}");
            }

            if (virtualMachines != null)
            {
                foreach (var resource in resources.Where(r => r.IsVirtualMachine())) 
                {
                    var vm = virtualMachines.Where(v => v.Id.ToString() == resource.Id).FirstOrDefault();
                    var teamId = resource.GetTeamId();
                    var url = playerOptions.VmConsoleUrl.Replace("{id}", resource.Id);

                    if (vm == null)
                    {                    
                        if (teamId.HasValue)
                        {
                            // Create vm
                            VmCreateForm form = new VmCreateForm
                            {
                                Name = resource.Name,
                                TeamIds = new List<Guid?>{teamId},
                                Id = new Guid(resource.Id),
                                Url = url
                            };

                            errorMessages.Add(await CallWrapperAsync((async () => await client.CreateVmAsync(form, ct)), $"Error Adding {form.Name} ({form.Id})"));
                        }                        
                    }
                    else
                    {                    
                        if (!teamId.HasValue)
                        {
                            errorMessages.Add(await CallWrapperAsync((async () => await client.DeleteVmAsync(vm.Id.Value, ct)), $"Error Removing {vm.Name} ({vm.Id})"));
                        }
                        else
                        {
                            // Check if team or properties have changed
                            if (!vm.TeamIds.Contains(teamId))
                            {
                                errorMessages.Add(await CallWrapperAsync((async () => await client.AddVmToTeamAsync(vm.Id.Value, teamId.Value, ct)), $"Error Adding {vm.Name} ({vm.Id}) to Team {teamId}"));
                            }
                            
                            foreach (var oldTeamId in vm.TeamIds.Where(t => t != teamId))
                            {
                                errorMessages.Add(await CallWrapperAsync((async () => await client.RemoveVmFromTeamAsync(vm.Id.Value, oldTeamId.Value, ct)), $"Error Removing {vm.Name} ({vm.Id}) from Team {oldTeamId}"));
                            }

                            if (vm.Name != resource.Name ||
                                vm.Url != url)
                            {
                                // TODO: Use PATCH when implemented in Vm Api
                                VmUpdateForm form = new VmUpdateForm
                                {
                                    Name = resource.Name,
                                    Url = url,
                                };

                                errorMessages.Add(await CallWrapperAsync((async () => await client.UpdateVmAsync(vm.Id.Value, form, ct)), $"Error Updating {vm.Name} ({vm.Id}"));
                            }
                        }                    
                    }
                }
            }                  
                         
            workspace.LastSynced = DateTime.UtcNow;
            workspace.SyncErrors = errorMessages.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            await dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Wrap calls to Vm API so we don't have to have the same large try/catch block around each of them
        /// </summary>
        /// <param name="func">The function to wrap</param>
        /// <param name="errorString">The error string that any Exception message will be added to in the event of an Exception</param>
        /// <param name="delete">Set to true if func is a call to Delete a Vm so we can ignore 404 errors</param>
        /// <returns>The full error message on failure, null on success</returns>
        private async Task<string> CallWrapperAsync(Func<Task> func, string errorString, bool delete = false)
        {
            string errorMessage = null;

            try
            {
                await func();
            }
            catch(ApiErrorException ex)
            {
                // Ignore delete error if vm doesn't exist
                if (!(delete && ex.Body.Status == (int)HttpStatusCode.NotFound && ex.Body.Title == "Vm not found"))
                    errorMessage = $"{errorString}: {ex.Body.Title}";
            }
            catch(TaskCanceledException ex)
            {
                // Expected if cancellationToken is triggered. Don't need to handle here.
                throw ex;
            }
            catch(Exception ex)
            {
                errorMessage = $"{errorString}: {ex.Message}";
            }

            return errorMessage;
        }

        private async void SyncRemovedResources()
        {
            while (true)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CasterContext>();
                        var client = scope.ServiceProvider.GetRequiredService<IS3VmApiClient>();

                        var removedResources = await dbContext.RemovedResources.ToArrayAsync();
                        var confirmedDeleted = new List<RemovedResource>();

                        foreach (var removedResource in removedResources)
                        {
                            bool deleteConfirmed = false;

                            try
                            {
                                await client.DeleteVmAsync(new Guid(removedResource.Id));
                                deleteConfirmed = true;
                            }                            
                            catch(ApiErrorException ex)
                            {
                                // TODO: Use instance field when it is implemented in VmApi to confirm
                                if(ex.Body.Status == (int)HttpStatusCode.NotFound && ex.Body.Title == "Vm not found")
                                {
                                    deleteConfirmed = true;
                                }                                
                            }

                            if (deleteConfirmed)
                            {
                                confirmedDeleted.Add(removedResource);
                            }
                        }

                        if (confirmedDeleted.Any())
                        {
                            dbContext.RemovedResources.RemoveRange(confirmedDeleted);
                            await dbContext.SaveChangesAsync();
                        }                        
                    }

                    _reset.WaitOne(new TimeSpan(0, 0, _playerOptions.CurrentValue.RemoveLoopSeconds));
                }
                catch(Exception ex)
                {
                    _logger.LogError("Exception trying to sync deleted resources", ex);
                }
            }
        }
    }
}

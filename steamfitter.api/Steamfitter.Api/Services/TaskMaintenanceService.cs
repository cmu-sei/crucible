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
    public interface ITaskMaintenanceService : IHostedService
    {
    }

    public class TaskMaintenanceService : ITaskMaintenanceService
    {
        private readonly ILogger<TaskMaintenanceService> _logger;
        private readonly IOptionsMonitor<Infrastructure.Options.VmTaskProcessingOptions> _vmTaskProcessingOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly IHubContext<EngineHub> _engineHub;

        public TaskMaintenanceService(
            ILogger<TaskMaintenanceService> logger,
            IOptionsMonitor<Infrastructure.Options.VmTaskProcessingOptions> vmTaskProcessingOptions,
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            IHubContext<EngineHub> engineHub)
        {
            _logger = logger;
            _vmTaskProcessingOptions = vmTaskProcessingOptions;
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _engineHub = engineHub;
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

        private async STT.Task Run()
        {
            await STT.Task.Run(async () =>
            {
                _logger.LogDebug("The TaskMaintenanceService is ready to process tasks.");
                while (true)
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var task1 = ExpireTasks(scope);
                            var task2 = EndScenarios(scope);
                            await STT.Task.WhenAll(task1, task2);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception encountered in TaskMaintenanceService Run loop.", ex);
                    }
                    var delaySeconds = _vmTaskProcessingOptions.CurrentValue.ExpirationCheckSeconds > 0 ? _vmTaskProcessingOptions.CurrentValue.ExpirationCheckSeconds : 60;
                    await STT.Task.Delay(new TimeSpan(0, 0, _vmTaskProcessingOptions.CurrentValue.ExpirationCheckSeconds));
                }
            });
        }

        private async STT.Task ExpireTasks(IServiceScope scope)
        {
            using (var steamfitterContext = scope.ServiceProvider.GetRequiredService<SteamfitterContext>())
            {
                // get results that are currently pending
                var now = DateTime.UtcNow;
                var pendingResultEntities = steamfitterContext.Results.Where(result => result.Status == TaskStatus.pending).ToList();
                if (pendingResultEntities.Any())
                {
                    foreach (var resultEntity in pendingResultEntities)
                    {
                        // set expired results status to expired
                        if (now.Subtract(resultEntity.StatusDate.AddSeconds(resultEntity.ExpirationSeconds)).TotalSeconds >= 0)
                        {
                            resultEntity.Status = TaskStatus.expired;
                            resultEntity.StatusDate = now;
                            await steamfitterContext.SaveChangesAsync();
                            _engineHub.Clients.All.SendAsync(EngineMethods.ResultUpdated, _mapper.Map<ViewModels.Result>(resultEntity));
                            _logger.LogDebug($"TaskMaintenanceService expired Result {resultEntity.Id}.");
                        }
                    }
                }
            }
        }

        private async STT.Task EndScenarios(IServiceScope scope)
        {
            using (var steamfitterContext = scope.ServiceProvider.GetRequiredService<SteamfitterContext>())
            {
                // get scenarios that are currently active
                var now = DateTime.UtcNow;
                var activeScenarioEntities = steamfitterContext.Scenarios.Where(scenario => scenario.Status == ScenarioStatus.active).ToList();
                if (activeScenarioEntities.Any())
                {
                    foreach (var scenarioEntity in activeScenarioEntities)
                    {
                        // set expired scenario status to ended
                        if (now.Subtract(scenarioEntity.EndDate).TotalSeconds >= 0)
                        {
                            scenarioEntity.Status = ScenarioStatus.ended;
                            await steamfitterContext.SaveChangesAsync();
                            _engineHub.Clients.All.SendAsync(EngineMethods.ScenarioUpdated, _mapper.Map<ViewModels.Scenario>(scenarioEntity));
                            _logger.LogDebug($"TaskMaintenanceService ended Scenario {scenarioEntity.Id}.");
                        }
                    }
                }
            }
        }

    }

}

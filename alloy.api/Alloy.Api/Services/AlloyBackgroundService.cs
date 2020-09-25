/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Alloy.Api.Data;
using Alloy.Api.Data.Models;
using Alloy.Api.Infrastructure.Extensions;
using Alloy.Api.Infrastructure.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Caster.Api;
using IdentityModel.Client;
using Steamfitter.Api;
using S3.Player.Api;

namespace Alloy.Api.Services
{
    public interface IAlloyBackgroundService : IHostedService
    {
    }

    public class AlloyBackgroundService : IAlloyBackgroundService
    {
        private readonly ILogger<AlloyBackgroundService> _logger;
        private readonly IOptionsMonitor<Infrastructure.Options.ClientOptions> _clientOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAlloyEventQueue _eventQueue;
        private readonly IHttpClientFactory _httpClientFactory;

        public AlloyBackgroundService(
                ILogger<AlloyBackgroundService> logger,
                IOptionsMonitor<Infrastructure.Options.ClientOptions> clientOptions,
                IServiceScopeFactory scopeFactory,
                IAlloyEventQueue eventQueue,
                IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _clientOptions = clientOptions;
            _scopeFactory = scopeFactory;
            _eventQueue = eventQueue;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Run();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Bootstraps (loads data) Events that were in process when this api encounters a stop/start cycle
        /// </summary>
        private async Task Bootstrap()
        {
            _logger.LogInformation($"AlloyBackgroundService is starting Bootstrap.");
            var bootstrapComplete = false;
            while (!bootstrapComplete)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        using (var alloyContext = scope.ServiceProvider.GetRequiredService<AlloyContext>())
                        {
                            // get event entities that are currently "in process"
                            var eventEntities = alloyContext.Events
                                .Where(o => o.Status != EventStatus.Active &&
                                    o.Status != EventStatus.Failed &&
                                    o.Status != EventStatus.Ended &&
                                    o.Status != EventStatus.Expired);

                            if (eventEntities.Any())
                            {
                                _logger.LogDebug($"AlloyBackgroundService is queueing {eventEntities.Count()} Events.");
                                foreach (var eventEntity in eventEntities)
                                {
                                    _eventQueue.Add(eventEntity);
                                    _logger.LogDebug($"AlloyBackgroundService is queueing Event {eventEntity.Id}.");
                                }
                            }
                        }
                    }
                    bootstrapComplete = true;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError("Exception encountered in AlloyBackgroundService Bootstrap.", ex);
                    await Task.Delay(new TimeSpan(0, 0, _clientOptions.CurrentValue.BackgroundTimerIntervalSeconds));
                }
            }
            _logger.LogInformation("AlloyBackgroundService Bootstrap complete.");
        }

        private async Task Run()
        {
            await Bootstrap();

            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        _logger.LogDebug("The AlloyBackgroundService is ready to process events.");
                        // _implementatioQueue is a BlockingCollection, so this loop will sleep if nothing is in the queue
                        var eventEntity = _eventQueue.Take(new CancellationToken());
                        // process the eventEntity on a new thread
                        var newThread = new Thread(ProcessTheEvent);
                        newThread.Start(eventEntity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception encountered in AlloyBackgroundService Run loop.", ex);
                    }
                }
            });
        }

        private async void ProcessTheEvent(Object eventEntityAsObject)
        {
            var ct = new CancellationToken();
            var eventEntity = eventEntityAsObject == null ? (EventEntity)null : (EventEntity)eventEntityAsObject;
            _logger.LogDebug($"Processing Event {eventEntity.Id} for status '{eventEntity.Status}'.");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    using (var alloyContext = scope.ServiceProvider.GetRequiredService<AlloyContext>())
                    {
                        var retryCount = 0;
                        var resourceCount = int.MaxValue;
                        var resourceRetryCount = 0;
                        // get the alloy context entities required
                        eventEntity = alloyContext.Events.First(x => x.Id == eventEntity.Id);
                        var eventTemplateEntity = alloyContext.EventTemplates.First(x => x.Id == eventEntity.EventTemplateId);
                        // get the auth token
                        var tokenResponse = await ApiClientsExtensions.GetToken(scope);
                        CasterApiClient casterApiClient = null;
                        S3PlayerApiClient playerApiClient = null;
                        SteamfitterApiClient steamfitterApiClient = null;
                        // LOOP until this thread's process is complete
                        while (eventEntity.Status == EventStatus.Creating ||
                            eventEntity.Status == EventStatus.Planning ||
                            eventEntity.Status == EventStatus.Applying ||
                            eventEntity.Status == EventStatus.Ending)
                        {
                            // the updateTheEntity flag is used to indicate if the event entity state should be updated at the end of this loop
                            var updateTheEntity = false;
                            // each time through the loop, one state (case) is handled based on Status and InternalStatus.  This allows for retries of a failed state.
                            switch (eventEntity.Status)
                            {
                                // the "Creating" status means we are creating the initial player view, steamfitter scenario and caster workspace
                                case EventStatus.Creating:
                                    {
                                        switch (eventEntity.InternalStatus)
                                        {
                                            case InternalEventStatus.LaunchQueued:
                                            case InternalEventStatus.CreatingView:
                                                {
                                                    if (eventTemplateEntity.ViewId == null)
                                                    {
                                                        eventEntity.InternalStatus = InternalEventStatus.CreatingScenario;
                                                        updateTheEntity = true;
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                            eventEntity.InternalStatus = InternalEventStatus.CreatingView;
                                                            var viewId = await PlayerApiExtensions.CreatePlayerViewAsync(playerApiClient, eventEntity, (Guid)eventTemplateEntity.ViewId, ct);
                                                            if (viewId != null)
                                                            {
                                                                eventEntity.ViewId = viewId;
                                                                eventEntity.InternalStatus = InternalEventStatus.CreatingScenario;
                                                                updateTheEntity = true;
                                                            }
                                                            else
                                                            {
                                                                retryCount++;
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger.LogError($"Error creating the player view for Event {eventEntity.Id}.", ex);
                                                            retryCount++;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.CreatingScenario:
                                                {
                                                    if (eventTemplateEntity.ScenarioTemplateId == null)
                                                    {
                                                        eventEntity.InternalStatus = InternalEventStatus.CreatingWorkspace;
                                                        updateTheEntity = true;
                                                    }
                                                    else
                                                    {
                                                        steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                        var scenario = await SteamfitterApiExtensions.CreateSteamfitterScenarioAsync(steamfitterApiClient, eventEntity, (Guid)eventTemplateEntity.ScenarioTemplateId, ct);
                                                        if (scenario != null)
                                                        {
                                                            eventEntity.ScenarioId = scenario.Id;
                                                            eventEntity.InternalStatus = InternalEventStatus.CreatingWorkspace;
                                                            updateTheEntity = true;
                                                        }
                                                        else
                                                        {
                                                            retryCount++;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.CreatingWorkspace:
                                                {
                                                    if (eventTemplateEntity.DirectoryId == null)
                                                    {
                                                        // There is no Caster directory, so start the scenario
                                                        var launchDate = DateTime.UtcNow;
                                                        eventEntity.Name = eventTemplateEntity.Name;
                                                        eventEntity.Description = eventTemplateEntity.Description;
                                                        eventEntity.LaunchDate = launchDate;
                                                        eventEntity.ExpirationDate = launchDate.AddHours(eventTemplateEntity.DurationHours);
                                                        eventEntity.Status = EventStatus.Applying;
                                                        eventEntity.InternalStatus = InternalEventStatus.StartingScenario;
                                                        updateTheEntity = true;
                                                    }
                                                    else
                                                    {
                                                        var varsFileContent = "";
                                                        if (eventEntity.ViewId != null)
                                                        {
                                                            playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                            varsFileContent = await CasterApiExtensions.GetCasterVarsFileContentAsync(eventEntity, playerApiClient, ct);
                                                        }
                                                        casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                        var workspaceId = await CasterApiExtensions.CreateCasterWorkspaceAsync(casterApiClient, eventEntity, (Guid)eventTemplateEntity.DirectoryId, varsFileContent, eventTemplateEntity.UseDynamicHost, ct);
                                                        if (workspaceId != null)
                                                        {
                                                            eventEntity.WorkspaceId = workspaceId;
                                                            eventEntity.InternalStatus = InternalEventStatus.PlanningLaunch;
                                                            eventEntity.Status = EventStatus.Planning;
                                                            updateTheEntity = true;
                                                        }
                                                        else
                                                        {
                                                            retryCount++;
                                                        }
                                                    }
                                                    break;
                                                }
                                            default:
                                                {
                                                    _logger.LogError($"Invalid status for Event {eventEntity.Id}: {eventEntity.Status} - {eventEntity.InternalStatus}");
                                                    eventEntity.Status = EventStatus.Failed;
                                                    updateTheEntity = true;
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                // the "Planning" state means that caster is planning a run
                                case EventStatus.Planning:
                                    {
                                        switch (eventEntity.InternalStatus)
                                        {
                                            case InternalEventStatus.PlanningLaunch:
                                            case InternalEventStatus.PlanningRedeploy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    var runId = await CasterApiExtensions.CreateRunAsync(eventEntity, casterApiClient, false, ct);
                                                    if (runId != null)
                                                    {
                                                        eventEntity.RunId = runId;
                                                        updateTheEntity = true;

                                                        switch (eventEntity.InternalStatus)
                                                        {
                                                            case InternalEventStatus.PlanningLaunch:
                                                                eventEntity.InternalStatus = InternalEventStatus.PlannedLaunch;
                                                                break;
                                                            case InternalEventStatus.PlanningRedeploy:
                                                                eventEntity.InternalStatus = InternalEventStatus.PlannedRedeploy;
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.PlannedLaunch:
                                            case InternalEventStatus.PlannedRedeploy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    updateTheEntity = await CasterApiExtensions.WaitForRunToBePlannedAsync(eventEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterPlanningMaxWaitMinutes, ct);
                                                    if (updateTheEntity)
                                                    {
                                                        eventEntity.Status = EventStatus.Applying;

                                                        switch (eventEntity.InternalStatus)
                                                        {
                                                            case InternalEventStatus.PlannedLaunch:
                                                                eventEntity.InternalStatus = InternalEventStatus.ApplyingLaunch;
                                                                break;
                                                            case InternalEventStatus.PlannedRedeploy:
                                                                eventEntity.InternalStatus = InternalEventStatus.ApplyingRedeploy;
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            default:
                                                {
                                                    _logger.LogError($"Invalid status for Event {eventEntity.Id}: {eventEntity.Status} - {eventEntity.InternalStatus}");
                                                    eventEntity.Status = EventStatus.Failed;
                                                    updateTheEntity = true;
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                // the "Applying" state means caster is applying a run (deploying VM's, etc.)
                                case EventStatus.Applying:
                                    {
                                        switch (eventEntity.InternalStatus)
                                        {
                                            case InternalEventStatus.ApplyingLaunch:
                                            case InternalEventStatus.ApplyingRedeploy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    updateTheEntity = await CasterApiExtensions.ApplyRunAsync(eventEntity, casterApiClient, ct);
                                                    if (updateTheEntity)
                                                    {
                                                        switch (eventEntity.InternalStatus)
                                                        {
                                                            case InternalEventStatus.ApplyingLaunch:
                                                                eventEntity.InternalStatus = InternalEventStatus.AppliedLaunch;
                                                                break;
                                                            case InternalEventStatus.ApplyingRedeploy:
                                                                eventEntity.InternalStatus = InternalEventStatus.AppliedRedeploy;
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.AppliedLaunch:
                                            case InternalEventStatus.AppliedRedeploy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    updateTheEntity = await CasterApiExtensions.WaitForRunToBeAppliedAsync(eventEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterDeployMaxWaitMinutes, ct);
                                                    if (updateTheEntity)
                                                    {
                                                        switch (eventEntity.InternalStatus)
                                                        {
                                                            case InternalEventStatus.AppliedLaunch:
                                                                eventEntity.InternalStatus = InternalEventStatus.StartingScenario;
                                                                break;
                                                            case InternalEventStatus.AppliedRedeploy:
                                                                eventEntity.Status = EventStatus.Active;
                                                                eventEntity.InternalStatus = InternalEventStatus.Launched;
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.StartingScenario:
                                                {
                                                    // start the steamfitter scenario, if there is one
                                                    if (eventEntity.ScenarioId != null)
                                                    {
                                                        steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                        updateTheEntity = await SteamfitterApiExtensions.StartSteamfitterScenarioAsync(steamfitterApiClient, (Guid)eventEntity.ScenarioId, ct);
                                                    }
                                                    else
                                                    {
                                                        updateTheEntity = true;
                                                    }
                                                    // moving on means that Launch is now complete
                                                    if (updateTheEntity)
                                                    {
                                                        var launchDate = DateTime.UtcNow;
                                                        eventEntity.Name = eventTemplateEntity.Name;
                                                        eventEntity.Description = eventTemplateEntity.Description;
                                                        eventEntity.LaunchDate = launchDate;
                                                        eventEntity.ExpirationDate = launchDate.AddHours(eventTemplateEntity.DurationHours);
                                                        eventEntity.Status = EventStatus.Active;
                                                        eventEntity.InternalStatus = InternalEventStatus.Launched;
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            default:
                                                {
                                                    _logger.LogError($"Invalid status for Event {eventEntity.Id}: {eventEntity.Status} - {eventEntity.InternalStatus}");
                                                    eventEntity.Status = EventStatus.Failed;
                                                    updateTheEntity = true;
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                                // the "Ending" state means all entities are being torn down
                                case EventStatus.Ending:
                                    {
                                        switch (eventEntity.InternalStatus)
                                        {
                                            case InternalEventStatus.EndQueued:
                                            case InternalEventStatus.PlanningDestroy:
                                                {
                                                    if (eventEntity.WorkspaceId != null)
                                                    {
                                                        casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                        var runId = await CasterApiExtensions.CreateRunAsync(eventEntity, casterApiClient, true, ct);
                                                        if (runId != null)
                                                        {
                                                            eventEntity.RunId = runId;
                                                            eventEntity.InternalStatus = InternalEventStatus.PlannedDestroy;
                                                            updateTheEntity = true;
                                                        }
                                                        else
                                                        {
                                                            retryCount++;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        eventEntity.InternalStatus = InternalEventStatus.DeletingView;
                                                        updateTheEntity = true;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.PlannedDestroy:
                                                {
                                                    if (eventEntity.LastLaunchInternalStatus == InternalEventStatus.PlannedLaunch)
                                                    {
                                                        // This is an edge case.  The previous plan during a launch failed therefore there is nothing
                                                        // to destroy however the Workspace needs deleted.
                                                        eventEntity.InternalStatus = InternalEventStatus.DeletingWorkspace;
                                                    }
                                                    else
                                                    {
                                                        casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                        updateTheEntity = await CasterApiExtensions.WaitForRunToBePlannedAsync(eventEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterPlanningMaxWaitMinutes, ct);
                                                        if (updateTheEntity)
                                                        {
                                                            eventEntity.InternalStatus = InternalEventStatus.ApplyingDestroy;
                                                        }
                                                        else
                                                        {
                                                            retryCount++;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.ApplyingDestroy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    updateTheEntity = await CasterApiExtensions.ApplyRunAsync(eventEntity, casterApiClient, ct);
                                                    if (updateTheEntity)
                                                    {
                                                        eventEntity.InternalStatus = InternalEventStatus.AppliedDestroy;
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.AppliedDestroy:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    await CasterApiExtensions.WaitForRunToBeAppliedAsync(eventEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterDestroyMaxWaitMinutes, ct);
                                                    // all conditions in this case require an event entity update
                                                    updateTheEntity = true;
                                                    // make sure that the run successfully deleted the resources
                                                    var count = (await casterApiClient.GetResourcesByWorkspaceAsync((Guid)eventEntity.WorkspaceId, ct)).Count();
                                                    eventEntity.RunId = null;
                                                    if (count == 0)
                                                    {
                                                        // resources deleted, so continue to delete the workspace
                                                        eventEntity.InternalStatus = InternalEventStatus.DeletingWorkspace;
                                                    }
                                                    else
                                                    {
                                                        if (count < resourceCount)
                                                        {
                                                            // still some resources, but making progress, try the whole process again
                                                            eventEntity.InternalStatus = InternalEventStatus.PlanningDestroy;
                                                            resourceRetryCount = 0;
                                                        }
                                                        else
                                                        {
                                                            // still some resources and not making progress. Check max retries.
                                                            if (resourceRetryCount < _clientOptions.CurrentValue.ApiClientEndFailureMaxRetries)
                                                            {
                                                                // try the whole process again after a wait
                                                                eventEntity.InternalStatus = InternalEventStatus.PlanningDestroy;
                                                                resourceRetryCount++;
                                                                Thread.Sleep(TimeSpan.FromMinutes(_clientOptions.CurrentValue.CasterDestroyRetryDelayMinutes));
                                                            }
                                                            else
                                                            {
                                                                // the caster workspace resources could not be destroyed
                                                                eventEntity.InternalStatus = InternalEventStatus.FailedDestroy;
                                                                eventEntity.Status = EventStatus.Failed;
                                                            }

                                                        }
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.DeletingWorkspace:
                                                {
                                                    casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                    updateTheEntity = await CasterApiExtensions.DeleteCasterWorkspaceAsync(eventEntity, casterApiClient, tokenResponse, ct);
                                                    if (updateTheEntity)
                                                    {
                                                        eventEntity.WorkspaceId = null;
                                                        eventEntity.InternalStatus = InternalEventStatus.DeletingView;
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.DeletingView:
                                                {
                                                    if (eventEntity.ViewId != null)
                                                    {
                                                        playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                        updateTheEntity = await PlayerApiExtensions.DeletePlayerViewAsync(_clientOptions.CurrentValue.urls.playerApi, eventEntity.ViewId, playerApiClient, ct);
                                                    }
                                                    else
                                                    {
                                                        updateTheEntity = true;
                                                    }
                                                    if (updateTheEntity)
                                                    {
                                                        eventEntity.ViewId = null;
                                                        eventEntity.InternalStatus = InternalEventStatus.DeletingScenario;
                                                    }
                                                    break;
                                                }
                                            case InternalEventStatus.DeletingScenario:
                                                {
                                                    if (eventEntity.ScenarioId != null)
                                                    {
                                                        steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                        updateTheEntity = await SteamfitterApiExtensions.EndSteamfitterScenarioAsync(_clientOptions.CurrentValue.urls.steamfitterApi, eventEntity.ScenarioId, steamfitterApiClient, ct);
                                                    }
                                                    else
                                                    {
                                                        updateTheEntity = true;
                                                    }
                                                    if (updateTheEntity)
                                                    {
                                                        eventEntity.ScenarioId = null;
                                                        eventEntity.Status = EventStatus.Ended;
                                                        eventEntity.InternalStatus = InternalEventStatus.Ended;
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                    break;
                                                }

                                            default:
                                                {
                                                    _logger.LogError($"Invalid status for Event {eventEntity.Id}: {eventEntity.Status} - {eventEntity.InternalStatus}");
                                                    eventEntity.Status = EventStatus.Failed;
                                                    updateTheEntity = true;
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            // check for exceeding the max number of retries
                            if (!updateTheEntity)
                            {
                                if ((eventEntity.Status == EventStatus.Creating ||
                                     eventEntity.Status == EventStatus.Planning ||
                                     eventEntity.Status == EventStatus.Applying) &&
                                    retryCount >= _clientOptions.CurrentValue.ApiClientLaunchFailureMaxRetries &&
                                    _clientOptions.CurrentValue.ApiClientLaunchFailureMaxRetries > 0)
                                {
                                    _logger.LogError($"While launching the retry count exceeded for Event {eventEntity.Id}, with status of {eventEntity.Status} - {eventEntity.InternalStatus}");
                                    eventEntity.LastLaunchStatus = eventEntity.Status;
                                    eventEntity.LastLaunchInternalStatus = eventEntity.InternalStatus;
                                    eventEntity.FailureCount++;
                                    eventEntity.Status = EventStatus.Ending;
                                    eventEntity.InternalStatus = InternalEventStatus.EndQueued;
                                    updateTheEntity = true;
                                }
                                else if (eventEntity.Status == EventStatus.Ending &&
                                    retryCount >= _clientOptions.CurrentValue.ApiClientEndFailureMaxRetries &&
                                    _clientOptions.CurrentValue.ApiClientEndFailureMaxRetries > 0)
                                {
                                    _logger.LogError($"While ending the retry count exceeded for Event {eventEntity.Id}, with status of {eventEntity.Status} - {eventEntity.InternalStatus}");
                                    eventEntity.LastEndStatus = eventEntity.Status;
                                    eventEntity.LastEndInternalStatus = eventEntity.InternalStatus;
                                    eventEntity.FailureCount++;
                                    eventEntity.Status = EventStatus.Failed;
                                    updateTheEntity = true;
                                }
                                else
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(_clientOptions.CurrentValue.ApiClientRetryIntervalSeconds));
                                }

                            }
                            // update the entity in the context, if we are moving on
                            if (updateTheEntity)
                            {
                                retryCount = 0;
                                eventEntity.StatusDate = DateTime.UtcNow;
                                await alloyContext.SaveChangesAsync(ct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing event {eventEntity.Id}", ex);
            }
        }

        private S3PlayerApiClient RefreshClient(S3PlayerApiClient clientObject, TokenResponse tokenResponse, CancellationToken ct)
        {
            // TODO: check for token expiration also
            if (clientObject == null)
            {
                clientObject = PlayerApiExtensions.GetPlayerApiClient(_httpClientFactory, _clientOptions.CurrentValue.urls.playerApi, tokenResponse);
            }

            return clientObject;
        }

        private SteamfitterApiClient RefreshClient(SteamfitterApiClient clientObject, TokenResponse tokenResponse, CancellationToken ct)
        {
            // TODO: check for token expiration also
            if (clientObject == null)
            {
                clientObject = SteamfitterApiExtensions.GetSteamfitterApiClient(_httpClientFactory, _clientOptions.CurrentValue.urls.steamfitterApi, tokenResponse);
            }

            return clientObject;
        }

        private CasterApiClient RefreshClient(CasterApiClient clientObject, TokenResponse tokenResponse, CancellationToken ct)
        {
            // TODO: check for token expiration also
            if (clientObject == null)
            {
                clientObject = CasterApiExtensions.GetCasterApiClient(_httpClientFactory, _clientOptions.CurrentValue.urls.casterApi, tokenResponse);
            }

            return clientObject;
        }

    }
}

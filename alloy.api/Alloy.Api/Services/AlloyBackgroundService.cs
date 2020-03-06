/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Caster.Api;
using Caster.Api.Models;
using IdentityModel.Client;
using Steamfitter.Api;
using Steamfitter.Api.Models;
using S3.Player.Api;
using S3.Player.Api.Models;

namespace Alloy.Api.Services
{
    public interface IAlloyBackgroundService : IHostedService
    {
    }

    public class AlloyBackgroundService : IAlloyBackgroundService
    {
        private readonly ILogger<AlloyBackgroundService> _logger;
        private readonly IOptionsMonitor<ClientOptions> _clientOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAlloyImplementationQueue _implementationQueue;
        private readonly IHttpClientFactory _httpClientFactory;

        public AlloyBackgroundService(
                ILogger<AlloyBackgroundService> logger,
                IOptionsMonitor<ClientOptions> clientOptions,
                IServiceScopeFactory scopeFactory,
                IAlloyImplementationQueue implementationQueue,
                IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _clientOptions = clientOptions;
            _scopeFactory = scopeFactory;
            _implementationQueue = implementationQueue;
            _httpClientFactory = httpClientFactory;
        }

        public System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            Bootstrap();
            
            _ = Run();
            
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        /// <summary>
        /// Bootstraps (loads data) Implementations that were in process when this api encounters a stop/start cycle 
        /// </summary>
        private void Bootstrap()
        {
             _logger.LogInformation($"AlloyBackgroundService is starting bootstrap.");
            using (var scope = _scopeFactory.CreateScope())
            {
                using (var alloyContext = scope.ServiceProvider.GetRequiredService<AlloyContext>())
                {
                    // get implementation entities that are currently "in process"
                    var implementationEntities = alloyContext.Implementations
                        .Where(o => o.Status != ImplementationStatus.Active &&
                            o.Status != ImplementationStatus.Failed &&
                            o.Status != ImplementationStatus.Ended &&
                            o.Status != ImplementationStatus.Expired);

                    if (implementationEntities.Any())
                    {
                        _logger.LogInformation($"AlloyBackgroundService is queueing {implementationEntities.Count()} Implementations.");
                        foreach (var implementationEntity in implementationEntities)
                        {
                            _implementationQueue.Add(implementationEntity);
                            _logger.LogInformation($"AlloyBackgroundService is queueing Implementation {implementationEntity.Id}.");
                        }
                    }
                }
            }
            _logger.LogInformation($"AlloyBackgroundService bootstrap complete.");
        }

        private async Task Run()
        {
            await Task.Run(() => 
            {
                while (true)
                {
                    try
                    {
                        _logger.LogInformation("The AlloyBackgroundService is ready to process implementations.");
                        // _implementatioQueue is a BlockingCollection, so this loop will sleep if nothing is in the queue
                        var implementationEntity = _implementationQueue.Take(new CancellationToken());
                        // process the implementationEntity on a new thread
                        var newThread = new Thread(ProcessTheImplementation);
                        newThread.Start(implementationEntity);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception encountered in AlloyBackgroundService loop", ex);
                    }
                }
            });            
        }

        private async void ProcessTheImplementation(Object implementationEntityAsObject)
        {           
            var ct = new CancellationToken();
            var implementationEntity = implementationEntityAsObject == null ? (ImplementationEntity)null : (ImplementationEntity)implementationEntityAsObject;
            _logger.LogInformation($"Processing Implementation {implementationEntity.Id} for status '{implementationEntity.Status}'.");

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
                        implementationEntity = alloyContext.Implementations.First(x => x.Id == implementationEntity.Id);
                        var definitionEntity = alloyContext.Definitions.First(x => x.Id == implementationEntity.DefinitionId);
                        // get the auth token
                        var tokenResponse = await ApiClientsExtensions.GetToken(scope);
                        CasterApiClient casterApiClient = null;
                        S3PlayerApiClient playerApiClient = null;
                        SteamfitterApiClient steamfitterApiClient = null;
                        // LOOP until this thread's process is complete
                        while (implementationEntity.Status == ImplementationStatus.Creating ||
                            implementationEntity.Status == ImplementationStatus.Planning ||
                            implementationEntity.Status == ImplementationStatus.Applying ||
                            implementationEntity.Status == ImplementationStatus.Ending)
                        {
                            // the updateTheEntity flag is used to indicate if the implementation entity state should be updated at the end of this loop
                            var updateTheEntity = false;
                            // each time through the loop, one state (case) is handled based on Status and InternalStatus.  This allows for retries of a failed state.
                            switch (implementationEntity.Status)
                            {
                                // the "Creating" status means we are creating the initial player exercise, steamfitter session and caster workspace
                                case ImplementationStatus.Creating:
                                {
                                    switch (implementationEntity.InternalStatus)
                                    {
                                        case InternalImplementationStatus.LaunchQueued:
                                        case InternalImplementationStatus.CreatingExercise:
                                        {
                                            if (definitionEntity.ExerciseId == null)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.CreatingSession;
                                                updateTheEntity = true;
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                    var exerciseId = await PlayerApiExtensions.CreatePlayerExerciseAsync(playerApiClient, implementationEntity, (Guid)definitionEntity.ExerciseId, ct);
                                                    if (exerciseId != null)
                                                    {
                                                        implementationEntity.ExerciseId = exerciseId;
                                                        implementationEntity.InternalStatus = InternalImplementationStatus.CreatingSession;
                                                        updateTheEntity = true;
                                                    }
                                                    else
                                                    {
                                                        retryCount++;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError($"Error creating the player exercise for Implementation {implementationEntity.Id}.", ex);
                                                    retryCount++;
                                                }
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.CreatingSession:
                                        {
                                            if (definitionEntity.ScenarioId == null)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.CreatingWorkspace;
                                                updateTheEntity = true;
                                            }
                                            else
                                            {
                                                steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                var session = await SteamfitterApiExtensions.CreateSteamfitterSessionAsync(steamfitterApiClient, implementationEntity, (Guid)definitionEntity.ScenarioId, ct);
                                                if (session != null)
                                                {
                                                    implementationEntity.SessionId = session.Id;
                                                    implementationEntity.InternalStatus = InternalImplementationStatus.CreatingWorkspace;
                                                    updateTheEntity = true;
                                                }
                                                else
                                                {
                                                    retryCount++;
                                                }
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.CreatingWorkspace:
                                        {
                                            if (definitionEntity.DirectoryId == null)
                                            {
                                                // There is no Caster directory, so start the session
                                                var launchDate = DateTime.UtcNow;
                                                implementationEntity.Name = definitionEntity.Name;
                                                implementationEntity.Description = definitionEntity.Description;
                                                implementationEntity.LaunchDate = launchDate;
                                                implementationEntity.ExpirationDate = launchDate.AddHours(definitionEntity.DurationHours);
                                                implementationEntity.Status = ImplementationStatus.Applying;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.StartingSession;
                                                updateTheEntity = true;
                                            }
                                            else
                                            {
                                                var varsFileContent = "";
                                                if (implementationEntity.ExerciseId != null)
                                                {
                                                    playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                    varsFileContent = await CasterApiExtensions.GetCasterVarsFileContentAsync(implementationEntity, playerApiClient, ct);
                                                }
                                                casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                var workspaceId = await CasterApiExtensions.CreateCasterWorkspaceAsync(casterApiClient, implementationEntity, (Guid)definitionEntity.DirectoryId, varsFileContent, ct);
                                                if (workspaceId != null)
                                                {
                                                    implementationEntity.WorkspaceId = workspaceId;
                                                    implementationEntity.InternalStatus = InternalImplementationStatus.PlanningLaunch;
                                                    implementationEntity.Status = ImplementationStatus.Planning;
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
                                            _logger.LogError($"Invalid status for Implementation {implementationEntity.Id}: {implementationEntity.Status} - {implementationEntity.InternalStatus}");
                                            implementationEntity.Status = ImplementationStatus.Failed;
                                            updateTheEntity = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                                // the "Planning" state means that caster is planning a run
                                case ImplementationStatus.Planning:
                                {
                                    switch (implementationEntity.InternalStatus)
                                    {
                                        case InternalImplementationStatus.PlanningLaunch:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            var runId = await CasterApiExtensions.CreateRunAsync(implementationEntity, casterApiClient, false, ct);
                                            if (runId != null)
                                            {
                                                implementationEntity.RunId = runId;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.PlannedLaunch;
                                                updateTheEntity = true;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.PlannedLaunch:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.WaitForRunToBePlannedAsync(implementationEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterPlanningMaxWaitMinutes, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.ApplyingLaunch;
                                                implementationEntity.Status = ImplementationStatus.Applying;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            _logger.LogError($"Invalid status for Implementation {implementationEntity.Id}: {implementationEntity.Status} - {implementationEntity.InternalStatus}");
                                            implementationEntity.Status = ImplementationStatus.Failed;
                                            updateTheEntity = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                                // the "Applying" state means caster is applying a run (deploying VM's, etc.)
                                case ImplementationStatus.Applying:
                                {
                                    switch (implementationEntity.InternalStatus)
                                    {
                                        case InternalImplementationStatus.ApplyingLaunch:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.ApplyRunAsync(implementationEntity, casterApiClient, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.AppliedLaunch;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.AppliedLaunch:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.WaitForRunToBeAppliedAsync(implementationEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterDeployMaxWaitMinutes, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.StartingSession;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.StartingSession:
                                        {
                                            // start the steamfitter session, if there is one
                                            if (implementationEntity.SessionId != null)
                                            {
                                                steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                updateTheEntity = await SteamfitterApiExtensions.StartSteamfitterSessionAsync(steamfitterApiClient, (Guid)implementationEntity.SessionId, ct);
                                            }
                                            else
                                            {
                                                updateTheEntity = true;
                                            }
                                            // moving on means that Launch is now complete
                                            if (updateTheEntity)
                                            {
                                                var launchDate = DateTime.UtcNow;
                                                implementationEntity.Name = definitionEntity.Name;
                                                implementationEntity.Description = definitionEntity.Description;
                                                implementationEntity.LaunchDate = launchDate;
                                                implementationEntity.ExpirationDate = launchDate.AddHours(definitionEntity.DurationHours);
                                                implementationEntity.Status = ImplementationStatus.Active;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.Launched;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            _logger.LogError($"Invalid status for Implementation {implementationEntity.Id}: {implementationEntity.Status} - {implementationEntity.InternalStatus}");
                                            implementationEntity.Status = ImplementationStatus.Failed;
                                            updateTheEntity = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                                // the "Ending" state means all entities are being torn down
                                case ImplementationStatus.Ending:
                                {
                                    switch (implementationEntity.InternalStatus)
                                    {
                                        case InternalImplementationStatus.EndQueued:
                                        case InternalImplementationStatus.DeletingExercise:
                                        {
                                            if (implementationEntity.ExerciseId != null)
                                            {
                                                playerApiClient = RefreshClient(playerApiClient, tokenResponse, ct);
                                                updateTheEntity = await PlayerApiExtensions.DeletePlayerExerciseAsync(_clientOptions.CurrentValue.urls.playerApi, implementationEntity.ExerciseId, playerApiClient, ct);
                                            }
                                            else
                                            {
                                                updateTheEntity = true;
                                            }
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.ExerciseId = null;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.DeletingSession;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.DeletingSession:
                                        {
                                            if (implementationEntity.SessionId != null)
                                            {
                                                steamfitterApiClient = RefreshClient(steamfitterApiClient, tokenResponse, ct);
                                                updateTheEntity = await SteamfitterApiExtensions.EndSteamfitterSessionAsync(_clientOptions.CurrentValue.urls.steamfitterApi, implementationEntity.SessionId, steamfitterApiClient, ct);
                                            }
                                            else
                                            {
                                                updateTheEntity = true;
                                            }
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.SessionId = null;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.PlanningDestroy;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.PlanningDestroy:
                                        {
                                            if (implementationEntity.WorkspaceId != null)
                                            {
                                                casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                                var runId = await CasterApiExtensions.CreateRunAsync(implementationEntity, casterApiClient, true, ct);
                                                if (runId != null)
                                                {
                                                    implementationEntity.RunId = runId;
                                                    implementationEntity.InternalStatus = InternalImplementationStatus.PlannedDestroy;
                                                    updateTheEntity = true;
                                                }
                                                else
                                                {
                                                    retryCount++;
                                                }
                                            }
                                            else
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.Ended;
                                                implementationEntity.Status = ImplementationStatus.Ended;
                                                updateTheEntity = true;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.PlannedDestroy:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.WaitForRunToBePlannedAsync(implementationEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterPlanningMaxWaitMinutes, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.ApplyingDestroy;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.ApplyingDestroy:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.ApplyRunAsync(implementationEntity, casterApiClient, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.InternalStatus = InternalImplementationStatus.AppliedDestroy;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.AppliedDestroy:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            await CasterApiExtensions.WaitForRunToBeAppliedAsync(implementationEntity, casterApiClient, _clientOptions.CurrentValue.CasterCheckIntervalSeconds, _clientOptions.CurrentValue.CasterDestroyMaxWaitMinutes, ct);
                                            // all conditions in this case require an implementation entity update
                                            updateTheEntity = true;
                                            // make sure that the run successfully deleted the resources
                                            var count = (await casterApiClient.GetResourcesByWorkspaceAsync((Guid)implementationEntity.WorkspaceId, ct)).Count();
                                            implementationEntity.RunId = null;
                                            if (count == 0)
                                            {
                                                // resources deleted, so continue to delete the workspace
                                                implementationEntity.InternalStatus = InternalImplementationStatus.DeletingWorkspace;
                                            }
                                            else
                                            {
                                                if (count < resourceCount)
                                                {
                                                    // still some resources, but making progress, try the whole process again
                                                    implementationEntity.InternalStatus = InternalImplementationStatus.PlanningDestroy;
                                                    resourceRetryCount = 0;
                                                }
                                                else
                                                {
                                                    // still some resources and not making progress. Check max retries.
                                                    if (resourceRetryCount < _clientOptions.CurrentValue.ApiClientFailureMaxRetries)
                                                    {
                                                        // try the whole process again after a wait
                                                        implementationEntity.InternalStatus = InternalImplementationStatus.PlanningDestroy;
                                                        resourceRetryCount++;
                                                        Thread.Sleep(TimeSpan.FromMinutes(_clientOptions.CurrentValue.CasterDestroyRetryDelayMinutes));
                                                    }
                                                    else
                                                    {
                                                        // the caster workspace resources could not be destroyed
                                                        implementationEntity.InternalStatus = InternalImplementationStatus.FailedDestroy;
                                                        implementationEntity.Status = ImplementationStatus.Failed;
                                                    }
                                                    
                                                }
                                            }
                                            break;
                                        }
                                        case InternalImplementationStatus.DeletingWorkspace:
                                        {
                                            casterApiClient = RefreshClient(casterApiClient, tokenResponse, ct);
                                            updateTheEntity = await CasterApiExtensions.DeleteCasterWorkspaceAsync(implementationEntity, casterApiClient, tokenResponse, ct);
                                            if (updateTheEntity)
                                            {
                                                implementationEntity.WorkspaceId = null;
                                                implementationEntity.Status = ImplementationStatus.Ended;
                                                implementationEntity.InternalStatus = InternalImplementationStatus.Ended;
                                            }
                                            else
                                            {
                                                retryCount++;
                                            }
                                            break;
                                        }
                                        default:
                                        {
                                            _logger.LogError($"Invalid status for Implementation {implementationEntity.Id}: {implementationEntity.Status} - {implementationEntity.InternalStatus}");
                                            implementationEntity.Status = ImplementationStatus.Failed;
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
                                if (retryCount >= _clientOptions.CurrentValue.ApiClientFailureMaxRetries && _clientOptions.CurrentValue.ApiClientFailureMaxRetries > 0)
                                {
                                    _logger.LogError($"Retry count exceeded for Implementation {implementationEntity.Id}, with status of {implementationEntity.Status} - {implementationEntity.InternalStatus}");
                                    implementationEntity.Status = ImplementationStatus.Failed;
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
                                implementationEntity.StatusDate = DateTime.UtcNow;
                                await alloyContext.SaveChangesAsync(ct);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error processing implementation {implementationEntity.Id}", ex);
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



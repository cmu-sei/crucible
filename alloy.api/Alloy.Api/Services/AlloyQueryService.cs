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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alloy.Api.Data;
using Alloy.Api.Infrastructure.Options;
using Alloy.Api.ViewModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api;

namespace Alloy.Api.Services
{
    public interface IAlloyQueryService : IHostedService, IDisposable
    {
    }

    /// <summary>
    /// This class watches for expired implementations to process.
    /// These would not necessarily be in a queue at expiration time and therefore need to be queried.
    /// This class could be modified to monitor *any* entity that needs to be queried periodically.
    /// </summary>
    public class AlloyQueryService : IAlloyQueryService
    {
        private readonly ILogger<AlloyQueryService> _logger;
        private readonly IOptionsMonitor<ClientOptions> _clientOptions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAlloyImplementationQueue _implementationQueue;
        private readonly int _minimumIntervalSeconds = 30;
        
        private Timer _timer;

        public AlloyQueryService(
            ILogger<AlloyQueryService> logger,
            IOptionsMonitor<ClientOptions> clientOptions,
            IServiceScopeFactory scopeFactory,
            IAlloyImplementationQueue implementationQueue
        )
        {
            _logger = logger;
            _clientOptions = clientOptions;
            _scopeFactory = scopeFactory;
            _implementationQueue = implementationQueue;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var intervalInSeconds = Math.Max(_clientOptions.CurrentValue.BackgroundTimerIntervalSeconds, _minimumIntervalSeconds);
            
            _logger.LogInformation("AlloyQueryService is starting.");
            
            _timer = new Timer(Run, null, TimeSpan.Zero,TimeSpan.FromSeconds(intervalInSeconds));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AlloyQueryService is stopping.");
            
            _timer?.Change(Timeout.Infinite, 0);
            
            return Task.CompletedTask;
        }

        private async void Run(object state)
        {
            _logger.LogInformation("AlloyQueryService is working.");

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    using (var alloyContext = scope.ServiceProvider.GetRequiredService<AlloyContext>())
                    {
                        var currentDateTime = DateTime.UtcNow;
                        var expiredImplementationEntities = alloyContext.Implementations.Where(o => 
                            o.EndDate == null &&
                            o.ExpirationDate < currentDateTime).ToList();

                        if (expiredImplementationEntities.Any())
                        {
                            _logger.LogInformation($"AlloyQueryService is processing {expiredImplementationEntities.Count()} expired Implementations.");
                            foreach (var implementationEntity in expiredImplementationEntities)
                            {
                                implementationEntity.EndDate = DateTime.UtcNow;
                                implementationEntity.Status = ImplementationStatus.Ending;
                                implementationEntity.InternalStatus = InternalImplementationStatus.EndQueued;
                                implementationEntity.RunId = null;
                                await alloyContext.SaveChangesAsync();
                                 _logger.LogInformation($"AlloyQueryService is queueing {implementationEntity.Id}.");
                                _implementationQueue.Add(implementationEntity);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception encountered in AlloyQueryService loop");
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

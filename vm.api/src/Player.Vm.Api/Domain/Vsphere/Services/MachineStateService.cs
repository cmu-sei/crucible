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
using System.Threading;
using System.Threading.Tasks;
using Player.Vm.Api.Domain.Vsphere.Options;
using Microsoft.Extensions.DependencyInjection;
using Player.Vm.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Concurrent;
using NetVimClient;
using System.Collections.Generic;
using Player.Vm.Api.Domain.Vsphere.Models;
using Microsoft.Extensions.Caching.Memory;
using Player.Vm.Api.Domain.Models;
using Nito.AsyncEx;
using Player.Vm.Api.Infrastructure.Extensions;

namespace Player.Vm.Api.Domain.Vsphere.Services
{
    public interface IMachineStateService
    {
        void CheckState();
    }

    public class MachineStateService : BackgroundService, IMachineStateService
    {
        private readonly ILogger<MachineStateService> _logger;
        private VsphereOptions _options;
        private readonly IOptionsMonitor<VsphereOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;
        private VmContext _dbContext;
        private IVsphereService _vsphereService;
        private readonly IConnectionService _connectionService;
        private AsyncAutoResetEvent _resetEvent = new AsyncAutoResetEvent(false);
        private DateTime _lastCheckedTime = DateTime.UtcNow;

        public MachineStateService(
                IOptionsMonitor<VsphereOptions> optionsMonitor,
                ILogger<MachineStateService> logger,
                IMemoryCache cache,
                IConnectionService connectionService,
                IServiceProvider serviceProvider
            )
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
            _connectionService = connectionService;
            _serviceProvider = serviceProvider;
        }

        public void CheckState()
        {
            _resetEvent.Set();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        InitScope(scope);
                        var events = await GetEvents();
                        await ProcessEvents(events);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, $"Exception in {nameof(MachineStateService)}");
                }

                await _resetEvent.WaitAsync(
                    new TimeSpan(0, 0, 0, _options.CheckTaskProgressIntervalMilliseconds),
                    cancellationToken);
            }
        }

        private void InitScope(IServiceScope scope)
        {
            _dbContext = scope.ServiceProvider.GetRequiredService<VmContext>();
            _vsphereService = scope.ServiceProvider.GetRequiredService<IVsphereService>();
            _options = _optionsMonitor.CurrentValue;
        }

        private async Task<IEnumerable<Event>> GetEvents()
        {
            var now = DateTime.UtcNow;
            var events = await _vsphereService.GetEvents(GetFilterSpec(_lastCheckedTime));
            _lastCheckedTime = now;
            return events;
        }

        private EventFilterSpec GetFilterSpec(DateTime beginTime)
        {
            var filterSpec = new EventFilterSpec()
            {
                time = new EventFilterSpecByTime()
                {
                    beginTime = beginTime,
                    beginTimeSpecified = true
                },
                eventTypeId = new string[]
                {
                    nameof(VmPoweredOnEvent),
                    nameof(DrsVmPoweredOnEvent),
                    nameof(VmPoweredOffEvent),
                }
            };

            return filterSpec;
        }

        private async Task ProcessEvents(IEnumerable<Event> events)
        {
            var eventDict = new Dictionary<Guid, Event>();

            if (!events.Any())
            {
                return;
            }

            var filteredEvents = events.GroupBy(x => x.vm.vm)
                .Select(g => g.OrderByDescending(l => l.createdTime).First())
                .ToArray();

            foreach (var evt in filteredEvents)
            {
                var id = _connectionService.GetVmIdByRef(evt.vm.vm.Value);

                if (id.HasValue)
                {
                    eventDict.Add(id.Value, evt);
                }
            }

            var vms = await _dbContext.Vms
                .Include(x => x.VmTeams)
                .Where(x => eventDict.Select(y => y.Key).Contains(x.Id))
                .ToListAsync();

            foreach (var vm in vms)
            {
                Event evt;
                if (eventDict.TryGetValue(vm.Id, out evt))
                {
                    var type = evt.GetType();

                    if (new Type[] { typeof(VmPoweredOnEvent), typeof(DrsVmPoweredOnEvent) }.Contains(type))
                    {
                        vm.PowerState = PowerState.On;
                    }
                    else if (type == typeof(VmPoweredOffEvent))
                    {
                        vm.PowerState = PowerState.Off;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
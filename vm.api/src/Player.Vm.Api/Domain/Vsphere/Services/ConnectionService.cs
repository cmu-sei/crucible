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
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NetVimClient;
using Player.Vm.Api.Domain.Vsphere.Models;
using Player.Vm.Api.Domain.Vsphere.Extensions;
using Player.Vm.Api.Domain.Vsphere.Options;
using Microsoft.Extensions.DependencyInjection;
using Player.Vm.Api.Data;
using Microsoft.EntityFrameworkCore;
using Player.Vm.Api.Domain.Models;

namespace Player.Vm.Api.Domain.Vsphere.Services
{
    public interface IConnectionService
    {
        VimPortTypeClient GetClient();
        ServiceContent GetServiceContent();
        UserSession GetSession();
        ManagedObjectReference GetProps();
        ManagedObjectReference GetMachineById(Guid id);
        Guid GetVmGuidByName(string name);
        List<Network> GetNetworksByHost(string hostReference);
        Network GetNetworkByReference(string networkReference);
        Network GetNetworkByName(string networkName);
        Datastore GetDatastoreByName(string dsName);
    }

    public class ConnectionService : BackgroundService, IConnectionService
    {
        private readonly ILogger<ConnectionService> _logger;
        private VsphereOptions _options;
        private readonly IOptionsMonitor<VsphereOptions> _optionsMonitor;
        private readonly IServiceProvider _serviceProvider;

        private VimPortTypeClient _client;
        private ServiceContent _sic;
        private UserSession _session;
        private ManagedObjectReference _props;

        public ConcurrentDictionary<Guid, ManagedObjectReference> _machineCache = new ConcurrentDictionary<Guid, ManagedObjectReference>();
        public ConcurrentDictionary<string, List<Network>> _networkCache = new ConcurrentDictionary<string, List<Network>>();
        public ConcurrentDictionary<string, Datastore> _datastoreCache = new ConcurrentDictionary<string, Datastore>();
        public ConcurrentDictionary<string, Guid> _vmGuids = new ConcurrentDictionary<string, Guid>();

        public ConnectionService(
                IOptionsMonitor<VsphereOptions> vsphereOptionsMonitor,
                ILogger<ConnectionService> logger,
                IServiceProvider serviceProvider
            )
        {
            _options = vsphereOptionsMonitor.CurrentValue;
            _optionsMonitor = vsphereOptionsMonitor;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public VimPortTypeClient GetClient()
        {
            return _client;
        }

        public ServiceContent GetServiceContent()
        {
            return _sic;
        }

        public UserSession GetSession()
        {
            return _session;
        }

        public ManagedObjectReference GetProps()
        {
            return _props;
        }

        public ManagedObjectReference GetMachineById(Guid id)
        {
            ManagedObjectReference machineReference = null;
            _machineCache.TryGetValue(id, out machineReference);
            return machineReference;
        }

        public Guid GetVmGuidByName(string name)
        {
            Guid uuid;
            _vmGuids.TryGetValue(name, out uuid);
            return uuid;
        }

        public List<Network> GetNetworksByHost(string hostReference)
        {
            List<Network> networks;
            _networkCache.TryGetValue(hostReference, out networks);

            if (networks == null)
                networks = new List<Network>();

            return networks;
        }

        public Network GetNetworkByReference(string networkReference)
        {
            return _networkCache.Values.SelectMany(x => x).Where(n => n.Reference == networkReference).FirstOrDefault();
        }

        public Network GetNetworkByName(string networkName)
        {
            return _networkCache.Values.SelectMany(x => x).Where(n => n.Name == networkName).FirstOrDefault();
        }

        public Datastore GetDatastoreByName(string dsName)
        {
            Datastore datastore = null;
            _datastoreCache.TryGetValue(dsName, out datastore);
            return datastore;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int count = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Starting Connect Loop at {DateTime.UtcNow}");

                    _options = _optionsMonitor.CurrentValue;

                    await Connect();

                    if (count == 0)
                    {
                        await LoadCache();
                    }

                    if (++count == _options.LoadCacheAfterIterations)
                        count = 0;

                    _logger.LogInformation($"Finished Connect Loop at {DateTime.UtcNow} with {_machineCache.Count()} Machines");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception encountered in ConnectionService loop");
                    count = 0;
                }

                await Task.Delay(new TimeSpan(0, 0, _options.ConnectionRetryIntervalSeconds));
            }
        }

        #region Cache Setup

        private async Task LoadCache()
        {
            var plan = new TraversalSpec
            {
                name = "FolderTraverseSpec",
                type = "Folder",
                path = "childEntity",
                selectSet = new SelectionSpec[] {

                    new TraversalSpec()
                    {
                        type = "Datacenter",
                        path = "networkFolder",
                        selectSet = new SelectionSpec[] {
                            new SelectionSpec {
                                name = "FolderTraverseSpec"
                            }
                        }
                    },

                    new TraversalSpec()
                    {
                        type = "Datacenter",
                        path = "vmFolder",
                        selectSet = new SelectionSpec[] {
                            new SelectionSpec {
                                name = "FolderTraverseSpec"
                            }
                        }
                    },

                    new TraversalSpec()
                    {
                        type = "Datacenter",
                        path = "datastore",
                        selectSet = new SelectionSpec[] {
                            new SelectionSpec {
                                name = "FolderTraverseSpec"
                            }
                        }
                    },

                    new TraversalSpec()
                    {
                        type = "Folder",
                        path = "childEntity",
                        selectSet = new SelectionSpec[] {
                            new SelectionSpec {
                                name = "FolderTraverseSpec"
                            }
                        }
                    },
                }
            };

            var props = new PropertySpec[]
            {
                new PropertySpec
                {
                    type = "DistributedVirtualSwitch",
                    pathSet = new string[] { "name", "uuid", "config.uplinkPortgroup" }
                },

                new PropertySpec
                {
                    type = "DistributedVirtualPortgroup",
                    pathSet = new string[] { "name", "host", "config.distributedVirtualSwitch" }
                },

                new PropertySpec
                {
                    type = "Network",
                    pathSet = new string[] { "name", "host" }
                },

                new PropertySpec
                {
                    type = "VirtualMachine",
                    pathSet = new string[] { "name", "config.uuid", "summary.runtime.powerState" }
                },

                new PropertySpec
                {
                    type = "Datastore",
                    pathSet = new string[] { "name", "browser" }
                }
            };

            ObjectSpec objectspec = new ObjectSpec();
            objectspec.obj = _sic.rootFolder;
            objectspec.selectSet = new SelectionSpec[] { plan };

            PropertyFilterSpec filter = new PropertyFilterSpec();
            filter.propSet = props;
            filter.objectSet = new ObjectSpec[] { objectspec };

            PropertyFilterSpec[] filters = new PropertyFilterSpec[] { filter };

            _logger.LogInformation($"Starting RetrieveProperties at {DateTime.UtcNow}");
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(_props, filters);
            _logger.LogInformation($"Finished RetrieveProperties at {DateTime.UtcNow}");

            _logger.LogInformation($"Starting LoadMachineCache at {DateTime.UtcNow}");
            await LoadMachineCache(response.returnval.FindType("VirtualMachine"));
            _logger.LogInformation($"Finished LoadMachineCache at {DateTime.UtcNow}");

            _logger.LogInformation($"Starting LoadNetworkCache at {DateTime.UtcNow}");
            LoadNetworkCache(
                response.returnval.FindType("DistributedVirtualSwitch"),
                response.returnval.Where(o => o.obj.type.EndsWith("Network") || o.obj.type.EndsWith("DistributedVirtualPortgroup")).ToArray());
            _logger.LogInformation($"Finished LoadNetworkCache at {DateTime.UtcNow}");

            _logger.LogInformation($"Starting LoadDatastoreCache at {DateTime.UtcNow}");
            LoadDatastoreCache(response.returnval.FindType("Datastore"));
            _logger.LogInformation($"Finished LoadDatastoreCache at {DateTime.UtcNow}");
        }

        private async Task LoadMachineCache(NetVimClient.ObjectContent[] virtualMachines)
        {
            IEnumerable<Guid> existingMachineIds = _machineCache.Keys;
            List<Guid> currentMachineIds = new List<Guid>();
            Dictionary<Guid, VsphereVirtualMachine> vsphereVirtualMachines = new Dictionary<Guid, VsphereVirtualMachine>();

            foreach (var vm in virtualMachines)
            {
                string name = string.Empty;

                try
                {
                    name = vm.GetProperty("name") as string;

                    var idObj = vm.GetProperty("config.uuid");

                    if (idObj == null)
                    {
                        _logger.LogError($"Unable to load machine {name} - {vm.obj.Value}. Invalid UUID");
                        continue;
                    }

                    var toolsStatus = vm.GetProperty("summary.guest.toolsStatus") as Nullable<VirtualMachineToolsStatus>;
                    VirtualMachineToolsStatus vmToolsStatus = VirtualMachineToolsStatus.toolsNotRunning;
                    if (toolsStatus != null)
                    {
                        vmToolsStatus = toolsStatus.Value;
                    }

                    var guid = Guid.Parse(idObj as string);
                    var virtualMachine = new VsphereVirtualMachine
                    {
                        //HostReference = ((ManagedObjectReference)vm.GetProperty("summary.runtime.host")).Value,
                        Id = guid,
                        Name = name,
                        Reference = vm.obj,
                        State = (VirtualMachinePowerState)vm.GetProperty("summary.runtime.powerState") == VirtualMachinePowerState.poweredOn ? "on" : "off",
                        VmToolsStatus = vmToolsStatus,
                    };

                    vsphereVirtualMachines.Add(virtualMachine.Id, virtualMachine);

                    _machineCache.AddOrUpdate(virtualMachine.Id, virtualMachine.Reference, (k, v) => (v = virtualMachine.Reference));
                    currentMachineIds.Add(virtualMachine.Id);
                    _vmGuids.AddOrUpdate(vm.obj.Value, guid, (k, v) => (v = guid));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error refreshing Virtual Machine {name} - {vm.obj.Value}");
                }
            }

            foreach (Guid existingId in existingMachineIds.Except(currentMachineIds))
            {
                if (_machineCache.TryRemove(existingId, out ManagedObjectReference stale))
                {
                    _logger.LogDebug($"removing stale cache entry {stale.Value}");
                }
            }

            await UpdateVms(vsphereVirtualMachines);
        }

        private async Task UpdateVms(Dictionary<Guid, VsphereVirtualMachine> vsphereVirtualMachines)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<VmContext>();
                var vms = await dbContext.Vms.ToArrayAsync();

                foreach (var vm in vms)
                {
                    VsphereVirtualMachine vsphereVirtualMachine;

                    if (vsphereVirtualMachines.TryGetValue(vm.Id, out vsphereVirtualMachine))
                    {
                        var powerState = vsphereVirtualMachine.State == "on" ? PowerState.On : PowerState.Off;
                        vm.PowerState = powerState;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }

        private void LoadNetworkCache(NetVimClient.ObjectContent[] distributedSwitches, NetVimClient.ObjectContent[] networks)
        {
            Dictionary<string, List<Network>> networkCache = new Dictionary<string, List<Network>>();
            IEnumerable<string> existingHosts = _networkCache.Keys;
            List<string> currentHosts = new List<string>();

            foreach (var net in networks)
            {
                string name = null;

                try
                {
                    name = net.GetProperty("name") as string;
                    Network network = null;

                    if (net.obj.type == "Network")
                    {
                        network = new Network
                        {
                            IsDistributed = false,
                            Name = name,
                            SwitchId = null
                        };
                    }
                    else if (net.obj.type == "DistributedVirtualPortgroup")
                    {
                        var dSwitchReference = net.GetProperty("config.distributedVirtualSwitch") as ManagedObjectReference;
                        var dSwitch = distributedSwitches.Where(x => x.obj.Value == dSwitchReference.Value).FirstOrDefault();

                        if (dSwitch != null)
                        {
                            var uplinkPortgroups = dSwitch.GetProperty("config.uplinkPortgroup") as ManagedObjectReference[];
                            if (uplinkPortgroups.Select(x => x.Value).Contains(net.obj.Value))
                            {
                                // Skip uplink portgroups
                                continue;
                            }
                            else
                            {
                                network = new Network
                                {
                                    IsDistributed = true,
                                    Name = name,
                                    SwitchId = dSwitch.GetProperty("uuid") as string,
                                    Reference = net.obj.Value
                                };
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Unexpected type for Network {name}: {net.obj.type}");
                        continue;
                    }

                    if (network != null)
                    {
                        foreach (var host in net.GetProperty("host") as ManagedObjectReference[])
                        {
                            string hostReference = host.Value;

                            if (!networkCache.ContainsKey(hostReference))
                                networkCache.Add(hostReference, new List<Network>());

                            networkCache[hostReference].Add(network);

                            if (!currentHosts.Contains(hostReference))
                            {
                                currentHosts.Add(hostReference);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error refreshing Network {name} - {net.obj.Value}");
                }
            }

            foreach (var kvp in networkCache)
            {
                _networkCache.AddOrUpdate(kvp.Key, kvp.Value, (k, v) => (v = kvp.Value));
            }

            foreach (string existingHost in existingHosts.Except(currentHosts))
            {
                if (_networkCache.TryRemove(existingHost, out List<Network> stale))
                {
                    _logger.LogDebug($"removing stale network cache entry for Host {existingHost}");
                }
            }
        }

        private void LoadDatastoreCache(NetVimClient.ObjectContent[] rawDatastores)
        {
            IEnumerable<string> cachedDatastoreNames = _datastoreCache.Keys;
            List<string> activeDatastoreNames = new List<string>();
            Dictionary<string, Datastore> datastores = new Dictionary<string, Datastore>();
            foreach (var rawDatastore in rawDatastores)
            {
                try
                {
                    Datastore datastore = new Datastore
                    {
                        Name = rawDatastore.GetProperty("name").ToString(),
                        Reference = rawDatastore.obj,
                        Browser = rawDatastore.GetProperty("browser") as ManagedObjectReference
                    };
                    _datastoreCache.TryAdd(rawDatastore.GetProperty("name").ToString(), datastore);
                    activeDatastoreNames.Add(datastore.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error refreshing Datastore {rawDatastore.obj.Value}");
                }
            }

            // clean cache of non-active datastores
            foreach (var dsName in cachedDatastoreNames)
            {
                if (!activeDatastoreNames.Contains(dsName))
                {
                    _logger.LogDebug($"removing stale datastore cache entry {dsName}");
                    _datastoreCache.Remove(dsName, out Datastore stale);
                }
            }
        }

        #endregion

        #region Connection Handling

        private async Task Connect()
        {
            // check whether session is expiring
            if (_session != null && (DateTime.Compare(DateTime.UtcNow, _session.lastActiveTime.AddMinutes(_options.ConnectionRefreshIntervalMinutes)) >= 0))
            {
                _logger.LogDebug("Connect():  Session is more than 20 minutes old");

                // renew session because it expires at 30 minutes (maybe 120 minutes on newer vc)
                _logger.LogInformation($"Connect():  renewing connection to {_options.Host}...[{_options.Username}]");
                try
                {
                    var client = new VimPortTypeClient(VimPortTypeClient.EndpointConfiguration.VimPort, $"https://{_options.Host}/sdk");
                    var sic = await client.RetrieveServiceContentAsync(new ManagedObjectReference { type = "ServiceInstance", Value = "ServiceInstance" });
                    var props = sic.propertyCollector;
                    var session = await client.LoginAsync(sic.sessionManager, _options.Username, _options.Password, null);

                    var oldClient = _client;
                    _client = client;
                    _sic = sic;
                    _props = props;
                    _session = session;

                    await oldClient.CloseAsync();
                    oldClient.Dispose();
                }
                catch (Exception ex)
                {
                    // no connection: Failed with Object reference not set to an instance of an object
                    _logger.LogError(0, ex, $"Connect():  Failed with " + ex.Message);
                    _logger.LogError(0, ex, $"Connect():  User: " + _options.Username);
                    Disconnect();
                }
            }

            if (_client != null && _client.State == CommunicationState.Opened)
            {
                _logger.LogDebug("Connect():  CommunicationState.Opened");
                ServiceContent sic = _sic;
                UserSession session = _session;
                bool isNull = false;

                if (_sic == null)
                {
                    sic = await ConnectToHost(_client);
                    isNull = true;
                }

                if (_session == null)
                {
                    session = await ConnectToSession(_client, sic);
                    isNull = true;
                }

                if (isNull)
                {
                    _session = session;
                    _props = sic.propertyCollector;
                    _sic = sic;
                }

                try
                {
                    var x = await _client.RetrieveServiceContentAsync(new ManagedObjectReference { type = "ServiceInstance", Value = "ServiceInstance" });
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking vcenter connection. Disconnecting.");
                    Disconnect();
                }
            }

            if (_client != null && _client.State == CommunicationState.Faulted)
            {
                _logger.LogDebug($"Connect():  https://{_options.Host}/sdk CommunicationState is Faulted.");
                Disconnect();
            }

            if (_client == null)
            {
                try
                {
                    _logger.LogDebug($"Connect():  Instantiating client https://{_options.Host}/sdk");
                    var client = new VimPortTypeClient(VimPortTypeClient.EndpointConfiguration.VimPort, $"https://{_options.Host}/sdk");
                    _logger.LogDebug($"Connect():  client: [{_client}]");

                    var sic = await ConnectToHost(client);
                    var session = await ConnectToSession(client, sic);

                    _session = session;
                    _props = sic.propertyCollector;
                    _sic = sic;
                    _client = client;
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, $"Connect():  Failed with " + ex.Message);
                }
            }
        }

        private async Task<ServiceContent> ConnectToHost(VimPortTypeClient client)
        {
            _logger.LogInformation($"Connect():  Connecting to {_options.Host}...");
            var sic = await client.RetrieveServiceContentAsync(new ManagedObjectReference { type = "ServiceInstance", Value = "ServiceInstance" });
            return sic;
        }

        private async Task<UserSession> ConnectToSession(VimPortTypeClient client, ServiceContent sic)
        {
            _logger.LogInformation($"Connect():  logging into {_options.Host}...[{_options.Username}]");
            var session = await client.LoginAsync(sic.sessionManager, _options.Username, _options.Password, null);
            _logger.LogInformation($"Connect():  Session created.");
            return session;
        }

        public void Disconnect()
        {
            _logger.LogInformation($"Disconnect()");
            _client.Dispose();
            _client = null;
            _sic = null;
            _session = null;
        }

        #endregion
    }
}

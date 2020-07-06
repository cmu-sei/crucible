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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NetVimClient;
using AutoMapper;
using Player.Vm.Api.Domain.Vsphere.Options;
using Player.Vm.Api.Domain.Vsphere.Models;
using Player.Vm.Api.Domain.Vsphere.Extensions;

namespace Player.Vm.Api.Domain.Vsphere.Services
{
    public interface IVsphereService
    {
        Task<VsphereVirtualMachine> GetMachineById(Guid id);
        Task<string> GetConsoleUrl(VsphereVirtualMachine machine);
        Task<NicOptions> GetNicOptions(Guid id, bool canManage, List<string> allowedNetworks, VsphereVirtualMachine machine);
        Task<string> PowerOnVm(Guid id);
        Task<string> PowerOffVm(Guid id);
        Task<string> RebootVm(Guid id);
        Task<string> ShutdownVm(Guid id);
        Task<TaskInfo> ReconfigureVm(Guid id, Feature feature, string label, string newvalue);
        Task<VirtualMachineToolsStatus> GetVmToolsStatus(Guid id);
        Task<string> UploadFileToVm(Guid id, string username, string password, string filepath, Stream fileStream);
        Task<IEnumerable<string>> GetIsos(string viewId, string teamId);
        Task<string> SetResolution(Guid id, int width, int height);
    }

    public class VsphereService : IVsphereService
    {
        private VsphereOptions _options;
        private RewriteHostOptions _rewriteHostOptions;

        private readonly ILogger<VsphereService> _logger;
        int _pollInterval = 1000;
        List<NetworkMOR> _networks;

        private VimPortTypeClient _client;
        private ServiceContent _sic;
        ManagedObjectReference _props;
        private readonly IConfiguration _configuration;
        private readonly IConnectionService _connectionService;
        private readonly IMapper _mapper;

        public VsphereService(
                VsphereOptions options,
                IOptions<RewriteHostOptions> rewriteHostOptions,
                ILogger<VsphereService> logger,
                IConfiguration configuration,
                IConnectionService connectionService,
                IMapper mapper
            )
        {
            _options = options;
            _rewriteHostOptions = rewriteHostOptions.Value;

            _logger = logger;
            _connectionService = connectionService;
            _client = connectionService.GetClient();
            _sic = connectionService.GetServiceContent();
            _props = connectionService.GetProps();
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<string> GetConsoleUrl(VsphereVirtualMachine machine)
        {
            if (machine.State == "off")
            {
                return null;
            }

            return await GetConsoleUrl(machine.Id, machine.Reference);
        }

        public async Task<string> GetConsoleUrl(Guid id, ManagedObjectReference vmReference)
        {
            VirtualMachineTicket ticket = null;
            string url = null;

            try
            {
                ticket = await _client.AcquireTicketAsync(vmReference, "webmks");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get ticket for vm {id}");
                return url;
            }

            string host = string.Empty;

            // ticket.host is null when using esxi instead of vcenter
            if (ticket.host != null)
                host = ticket.host;
            else
                host = _options.Host;

            if (_rewriteHostOptions.RewriteHost)
            {
                url = $"wss://{_rewriteHostOptions.RewriteHostUrl}/ticket/{ticket.ticket}?{_rewriteHostOptions.RewriteHostQueryParam}={host}";
            }
            else
            {
                url = url = $"wss://{host}/ticket/{ticket.ticket}";
            }

            _logger.LogDebug($"Returning url: {url}");

            return url;
        }

        //string uuid = "50303a87-2f5b-6d13-2211-a33556ba6e7f";
        public async Task<string> GetConsoleUrl(Guid uuid)
        {
            ManagedObjectReference vmReference = null;

            _logger.LogDebug($"Aquiring webmks ticket for vm {uuid}");

            vmReference = await GetVm(uuid);

            if (vmReference == null)
                return null;

            return await GetConsoleUrl(uuid, vmReference);
        }

        public async Task<ManagedObjectReference> GetVm(Guid id)
        {
            ManagedObjectReference vmReference = _connectionService.GetMachineById(id);

            if (vmReference == null && _client != null && _sic != null)
            {
                try
                {
                    vmReference = await _client.FindByUuidAsync(_sic.searchIndex, null, id.ToString(), true, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, $"Failed to get reference for " + id);
                    if (_client == null)
                    {
                        _logger.LogError(0, ex, $"_client is null");
                    }
                    if (_sic == null)
                    {
                        _logger.LogError(0, ex, $"_sic is null");
                    }
                    _logger.LogError(0, ex, $"Failed with " + ex.Message);
                    // should determine the cause
                }
            }

            return vmReference;
        }

        public async Task<string> PowerOnVm(Guid id)
        {
            _logger.LogDebug($"Power on vm {id} requested");

            ManagedObjectReference vmReference = null;
            ManagedObjectReference task;
            string state = null;

            vmReference = await GetVm(id);

            if (vmReference == null)
            {
                _logger.LogDebug($"Could not get vm reference");
                return state;
            }
            state = await GetPowerState(id);

            if (state == "on")
            {
                state = "already running";
                _logger.LogDebug($"Returning state: {state}");
                return state;
            }

            try
            {
                task = await _client.PowerOnVM_TaskAsync(vmReference, null);

                // TaskInfo info = await WaitForVimTask(task);
                // if (info.state == TaskInfoState.success) {
                //     state = "started";
                // }
                // else
                // {
                //     throw new Exception(info.error.localizedMessage);
                // }

                state = "poweron submitted";
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Failed to send power on " + id);
                state = "poweron error";
            }

            state = "poweron submitted";

            _logger.LogDebug($"Returning state: {state}");

            return state;
        }

        public async Task<string> PowerOffVm(Guid id)
        {
            ManagedObjectReference vmReference = null;
            ManagedObjectReference task;
            string state = null;

            vmReference = await GetVm(id);

            if (vmReference == null)
            {
                _logger.LogDebug($"Could not get vm reference");
                return state;
            }

            state = await GetPowerState(id);

            _logger.LogDebug($"Power off vm {id} requested");

            if (state == "off")
            {
                state = "already off";
                _logger.LogDebug($"Returning state: {state}");
                return state;
            }

            try
            {
                task = await _client.PowerOffVM_TaskAsync(vmReference);

                // TaskInfo info = await WaitForVimTask(task);
                // if (info.state == TaskInfoState.success) {
                //     State = VmPowerState.off;
                //     state = "stopped";
                // }
                // else
                // {
                //     throw new Exception(info.error.localizedMessage);
                // }
                state = "poweroff submitted";
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Failed to send power off " + id);
                state = "poweroff error";
            }

            _logger.LogDebug($"Returning state: {state}");

            return state;
        }

        public async Task<string> RebootVm(Guid id)
        {
            // need to wait vm to poweroff before telling it to poweron

            ManagedObjectReference vmReference = null;
            ManagedObjectReference task;

            vmReference = await GetVm(id);

            if (vmReference == null)
            {
                _logger.LogDebug($"Could not get vm reference");
                return "error";
            }

            if (await GetPowerState(id) == "error")
            {
                return "error";
            }

            try
            {
                task = await _client.PowerOffVM_TaskAsync(vmReference);

                TaskInfo info = await WaitForVimTask(task);
                if (info.state != TaskInfoState.success)
                {
                    return "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Failed to send power off " + id);
                return "error";
            }

            return await PowerOnVm(id);
        }

        public async Task<string> ShutdownVm(Guid id)
        {
            ManagedObjectReference vmReference = null;
            string state = null;

            vmReference = await GetVm(id);

            if (vmReference == null)
            {
                _logger.LogDebug($"Could not get vm reference");
                return "error";
            }

            state = await GetPowerState(id);

            _logger.LogDebug($"Shutdown OS for vm {id} requested");

            if (state == "off")
            {
                state = "already off";
                _logger.LogDebug($"Returning state: {state}");
                return state;
            }

            try
            {
                await _client.ShutdownGuestAsync(vmReference);
                state = "shutdown submitted";
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Failed to send shutdown " + id);
                return "error";
            }

            return state;
        }

        public string GetPowerState(RetrievePropertiesResponse propertiesResponse)
        {
            VmPowerState State = VmPowerState.off;
            string state = null;

            NetVimClient.ObjectContent[] oc = propertiesResponse.returnval;
            NetVimClient.ObjectContent obj = oc[0];

            foreach (DynamicProperty dp in obj.propSet)
            {
                if (dp.val.GetType() == typeof(VirtualMachineSummary))
                {
                    try
                    {
                        VirtualMachineSummary summary = (VirtualMachineSummary)dp.val;
                        //vm.Name = summary.config.name;
                        //vm.Path = summary.config.vmPathName;
                        //vm.Id = summary.config.uuid;
                        //vm.IpAddress = summary.guest.ipAddress;
                        //vm.Os = summary.guest.guestId;
                        State = (summary.runtime.powerState == VirtualMachinePowerState.poweredOn)
                            ? VmPowerState.running
                            : VmPowerState.off;

                        //vm.IsPoweredOn = (summary.runtime.powerState == VirtualMachinePowerState.poweredOn);
                        //vm.Reference = summary.vm.AsString(); //summary.vm.type + "|" + summary.vm.Value;
                        //vm.Stats = String.Format("{0} | mem-{1}% cpu-{2}%", summary.overallStatus,
                        //    Math.Round(((float)summary.quickStats.guestMemoryUsage / (float)summary.runtime.maxMemoryUsage) * 100, 0),
                        //    Math.Round(((float)summary.quickStats.overallCpuUsage / (float)summary.runtime.maxCpuUsage) * 100, 0));
                        //vm.Annotations = summary.config.annotation.Lines();
                        //vm.ContextNumbers = vm.Annotations.FindOne("context").Value();
                        //vm.ContextNames = vm.Annotations.FindOne("display").Value();
                        //vm.HasGuestAgent = (vm.Annotations.FindOne("guestagent").Value() == "true");
                        //vm.Question = GetQuestion(summary.runtime.question);
                        //vm.Status = "deployed";
                        //if (_tasks.ContainsKey(vm.Id))
                        //{
                        //    var t = _tasks[vm.Id];
                        //    vm.Task = new VmTask { Name= t.Action, WhenCreated = t.WhenCreated, Progress = t.Progress };
                        //}
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex.Message);
                    }
                }
            }

            if (State == VmPowerState.running)
            {
                state = "on";
            }
            else if (State == VmPowerState.off)
            {
                state = "off";
            }
            else
            {
                state = "error";
            }
            return state;
        }

        public async Task<string> GetPowerState(Guid uuid)
        {
            _logger.LogDebug("GetPowerState called");

            ManagedObjectReference vmReference = vmReference = await GetVm(uuid);
            if (vmReference == null)
            {
                _logger.LogDebug($"could not get state, vmReference is null");
                return "error";
            }

            //retrieve the properties specificied
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(
                _props,
                VmFilter(vmReference));

            return GetPowerState(response);
        }

        public VirtualMachineToolsStatus GetVmToolsStatus(RetrievePropertiesResponse propertiesResponse)
        {
            NetVimClient.ObjectContent[] oc = propertiesResponse.returnval;
            NetVimClient.ObjectContent obj = oc[0];
            foreach (DynamicProperty dp in obj.propSet)
            {
                if (dp.val.GetType() == typeof(VirtualMachineSummary))
                {
                    VirtualMachineSummary vmSummary = (VirtualMachineSummary)dp.val;
                    //check vmware tools status
                    var toolsStatus = vmSummary.guest.toolsStatus;
                    return toolsStatus;
                }
            }
            return VirtualMachineToolsStatus.toolsNotRunning;
        }

        public async Task<VirtualMachineToolsStatus> GetVmToolsStatus(Guid id)
        {
            ManagedObjectReference vmReference = vmReference = await GetVm(id);
            //retrieve the properties specificied
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(
                _props,
                VmFilter(vmReference));

            return GetVmToolsStatus(response);
        }

        public async Task<string> UploadFileToVm(Guid id, string username, string password, string filepath, Stream fileStream)
        {
            _logger.LogDebug("UploadFileToVm called");

            ManagedObjectReference vmReference = vmReference = await GetVm(id);

            if (vmReference == null)
            {
                var errorMessage = $"could not upload file, vmReference is null";
                _logger.LogDebug(errorMessage);
                return errorMessage;
            }
            //retrieve the properties specificied
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(
                _props,
                VmFilter(vmReference));

            NetVimClient.ObjectContent[] oc = response.returnval;
            NetVimClient.ObjectContent obj = oc[0];

            foreach (DynamicProperty dp in obj.propSet)
            {
                if (dp.val.GetType() == typeof(VirtualMachineSummary))
                {
                    VirtualMachineSummary vmSummary = (VirtualMachineSummary)dp.val;
                    //check vmware tools status
                    var tools_status = vmSummary.guest.toolsStatus;
                    if (tools_status == VirtualMachineToolsStatus.toolsNotInstalled || tools_status == VirtualMachineToolsStatus.toolsNotRunning)
                    {
                        var errorMessage = $"could not upload file, VM Tools is not running";
                        _logger.LogDebug(errorMessage);
                        return errorMessage;
                    }

                    // user credentials on the VM
                    NamePasswordAuthentication credentialsAuth = new NamePasswordAuthentication()
                    {
                        interactiveSession = false,
                        username = username,
                        password = password
                    };
                    ManagedObjectReference fileManager = new ManagedObjectReference()
                    {
                        type = "GuestFileManager",
                        Value = "guestOperationsFileManager"
                    };
                    // upload the file
                    GuestFileAttributes fileAttributes = new GuestFileAttributes();
                    var fileTransferUrl = _client.InitiateFileTransferToGuestAsync(fileManager, vmReference, credentialsAuth, filepath, fileAttributes, fileStream.Length, true).Result;

                    // Replace IP address with hostname
                    RetrievePropertiesResponse hostResponse = await _client.RetrievePropertiesAsync(_props, HostFilter(vmSummary.runtime.host, "name"));
                    string hostName = hostResponse.returnval[0].propSet[0].val as string;

                    if (!fileTransferUrl.Contains(hostName))
                    {
                        fileTransferUrl = fileTransferUrl.Replace("https://", "");
                        var s = fileTransferUrl.IndexOf("/");
                        fileTransferUrl = "https://" + hostName + fileTransferUrl.Substring(s);
                    }

                    // http put to url
                    using (var httpClientHandler = new HttpClientHandler())
                    {
                        using (var httpClient = new HttpClient(httpClientHandler))
                        {
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                var timeout = _configuration.GetSection("vmOptions").GetValue("Timeout", 3);
                                httpClient.Timeout = TimeSpan.FromMinutes(timeout);
                                fileStream.CopyTo(ms);
                                var fileContent = new ByteArrayContent(ms.ToArray());
                                _logger.LogDebug("UploadFileToVm Upload URL:  " + fileTransferUrl);
                                var uploadResponse = await httpClient.PutAsync(fileTransferUrl, fileContent);
                            }
                        }
                    }
                }
            }
            return "";
        }

        private async Task<TaskInfo> WaitForVimTask(ManagedObjectReference task)
        {
            int i = 0;
            TaskInfo info = new TaskInfo();

            //iterate the search until complete or timeout occurs
            do
            {
                //check every so often
                await Task.Delay(_pollInterval);
                info = await GetVimTaskInfo(task);
                i++;
                //check for status updates until the task is complete
            } while ((info.state == TaskInfoState.running || info.state == TaskInfoState.queued));

            //return the task info
            return info;
        }

        private async Task<TaskInfo> GetVimTaskInfo(ManagedObjectReference task)
        {
            TaskInfo info = new TaskInfo();
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(
                _props,
                TaskFilter(task));
            NetVimClient.ObjectContent[] oc = response.returnval;
            info = (TaskInfo)oc[0].propSet[0].val;
            return info;
        }

        public async Task<NicOptions> GetNicOptions(Guid id, bool canManage, List<string> allowedNetworks, VsphereVirtualMachine machine)
        {
            return new NicOptions
            {
                AvailableNetworks = await GetVmNetworks(machine, canManage, allowedNetworks),
                CurrentNetworks = await GetVMConfiguration(machine, Feature.net)
            };
        }

        public async Task<List<string>> GetVmNetworks(VsphereVirtualMachine machine, bool canManage, List<string> allowedNetworks)
        {
            List<Network> hostNetworks = _connectionService.GetNetworksByHost(machine.HostReference);
            List<string> networkNames = hostNetworks.Select(n => n.Name).ToList();

            // if a user can manage this VM, then they have access to all available NICs
            if (canManage)
            {
                return networkNames.OrderBy(x => x).ToList();
            }
            else
            {
                if (allowedNetworks != null)
                {
                    return networkNames.Intersect(allowedNetworks, StringComparer.InvariantCultureIgnoreCase).OrderBy(x => x).ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public async Task<Dictionary<string, string>> GetVMConfiguration(VsphereVirtualMachine machine, Feature feature)
        {
            VirtualDevice[] devices = machine.Devices;

            VirtualMachineConfigSpec vmcs = new VirtualMachineConfigSpec();
            Dictionary<string, string> names = new Dictionary<string, string>();
            switch (feature)
            {
                case Feature.iso:
                    IEnumerable<Description> cdroms = devices.OfType<VirtualCdrom>().Select(c => c.deviceInfo);
                    foreach (Description d in cdroms)
                    {
                        if (d != null)
                        {
                            names.Add(d.label, d.summary);
                        }
                    }
                    break;

                case Feature.net:
                case Feature.eth:
                    IEnumerable<VirtualEthernetCard> cards = devices.OfType<VirtualEthernetCard>();
                    foreach (VirtualEthernetCard c in cards)
                    {
                        var backingInfo = c.backing;
                        var deviceInfo = c.deviceInfo;
                        if (backingInfo != null && deviceInfo != null)
                        {
                            if (backingInfo.GetType() == typeof(VirtualEthernetCardDistributedVirtualPortBackingInfo))
                            {
                                var card = backingInfo as VirtualEthernetCardDistributedVirtualPortBackingInfo; var portGroupKey = card?.port?.portgroupKey;

                                if (!string.IsNullOrEmpty(portGroupKey))
                                {
                                    var network = _connectionService.GetNetworkByReference(portGroupKey);
                                    string cardName = network?.Name;

                                    if (!string.IsNullOrEmpty(cardName))
                                    {
                                        names.Add(deviceInfo.label, cardName);
                                    }
                                }
                            }
                            else if (backingInfo.GetType() == typeof(VirtualEthernetCardNetworkBackingInfo))
                            {
                                var card = backingInfo as VirtualEthernetCardNetworkBackingInfo;
                                names.Add(deviceInfo.label, card.deviceName);
                            }
                            //
                        }
                    }
                    break;
                default:
                    throw new Exception("Invalid request.");
                    //break;
            }
            return names;
        }

        public async Task<IEnumerable<string>> GetIsos(string viewId, string teamId)
        {
            // isos for team only
            var list = await GetSubfolderIsos(viewId, teamId);
            // isos for entire view
            var list2 = await GetSubfolderIsos(viewId, viewId);
            list.AddRange(list2);
            return list.Distinct();
        }

        private async Task<List<string>> GetSubfolderIsos(string viewId, string subfolder)
        {
            List<string> list = new List<string>();
            var dsName = _options.DsName;
            var baseFolder = _options.BaseFolder;
            var filepath = $"[{dsName}] {baseFolder}/{viewId}/{subfolder}";
            var datastore = await GetDatastoreByName(dsName);
            if (datastore == null)
            {
                list.Add($"Datastore {dsName} cannot be found.");
                return list;
            }
            var dsBrowser = datastore.Browser;

            ManagedObjectReference task = null;
            TaskInfo info = null;
            HostDatastoreBrowserSearchSpec spec = new HostDatastoreBrowserSearchSpec { };
            List<HostDatastoreBrowserSearchResults> results = new List<HostDatastoreBrowserSearchResults>();
            task = await _client.SearchDatastore_TaskAsync(dsBrowser, filepath, spec);
            info = await WaitForVimTask(task);
            if (info.state == TaskInfoState.error)
            {
                if (info.error.fault != null &&
                    info.error.fault.ToString().Equals("FileNotFound", StringComparison.CurrentCultureIgnoreCase))
                {
                    // folder not found, return empty
                    return list;
                }
                throw new Exception(info.error.localizedMessage);
            }
            else if (info.result != null)
            {
                results.Add((HostDatastoreBrowserSearchResults)info.result);
            }

            foreach (HostDatastoreBrowserSearchResults result in results)
            {
                if (result != null && result.file != null && result.file.Length > 0)
                {
                    list.AddRange(result.file.Select(o => result.folderPath + o.path));
                }
            }

            return list;
        }

        public async Task<TaskInfo> ReconfigureVm(Guid id, Feature feature, string label, string newvalue)
        {
            VsphereVirtualMachine machine = await GetMachineById(id);
            ManagedObjectReference vmReference = machine.Reference;

            VirtualDevice[] devices = machine.Devices;
            VirtualMachineConfigSpec vmcs = new VirtualMachineConfigSpec();

            switch (feature)
            {
                case Feature.iso:
                    VirtualCdrom cdrom = (VirtualCdrom)((!string.IsNullOrEmpty(label))
                        ? devices.Where(o => o.deviceInfo.label == label).SingleOrDefault()
                        : devices.OfType<VirtualCdrom>().FirstOrDefault());

                    if (cdrom != null)
                    {
                        if (cdrom.backing.GetType() != typeof(VirtualCdromIsoBackingInfo))
                            cdrom.backing = new VirtualCdromIsoBackingInfo();

                        ((VirtualCdromIsoBackingInfo)cdrom.backing).datastore = (await GetDatastoreByName(_options.DsName)).Reference;
                        ((VirtualCdromIsoBackingInfo)cdrom.backing).fileName = newvalue;
                        cdrom.connectable = new VirtualDeviceConnectInfo
                        {
                            connected = true,
                            startConnected = true
                        };

                        vmcs.deviceChange = new VirtualDeviceConfigSpec[] {
                            new VirtualDeviceConfigSpec {
                                device = cdrom,
                                operation = VirtualDeviceConfigSpecOperation.edit,
                                operationSpecified = true
                            }
                        };
                    }
                    break;

                case Feature.net:
                case Feature.eth:
                    VirtualEthernetCard card = (VirtualEthernetCard)((!string.IsNullOrEmpty(label))
                        ? devices.Where(o => o.deviceInfo.label == label).SingleOrDefault()
                        : devices.OfType<VirtualEthernetCard>().FirstOrDefault());

                    if (card != null)
                    {
                        Network network = _connectionService.GetNetworkByName(newvalue);

                        if (network.IsDistributed)
                        {
                            card.backing = new VirtualEthernetCardDistributedVirtualPortBackingInfo
                            {
                                port = new DistributedVirtualSwitchPortConnection
                                {
                                    portgroupKey = network.Reference,
                                    switchUuid = network.SwitchId
                                }
                            };
                        }
                        else
                        {
                            card.backing = new VirtualEthernetCardNetworkBackingInfo
                            {
                                deviceName = newvalue
                            };
                        }

                        //if (card.backing is VirtualEthernetCardNetworkBackingInfo)
                        //    ((VirtualEthernetCardNetworkBackingInfo)card.backing).deviceName = newvalue;

                        //if (card.backing is VirtualEthernetCardDistributedVirtualPortBackingInfo)
                        //    ((VirtualEthernetCardDistributedVirtualPortBackingInfo)card.backing).port.portgroupKey = newvalue;

                        card.connectable = new VirtualDeviceConnectInfo()
                        {
                            connected = true,
                            startConnected = true,
                        };

                        vmcs.deviceChange = new VirtualDeviceConfigSpec[] {
                            new VirtualDeviceConfigSpec {
                                device = card,
                                operation = VirtualDeviceConfigSpecOperation.edit,
                                operationSpecified = true
                            }
                        };
                    }
                    break;

                case Feature.boot:
                    int delay = 0;
                    if (Int32.TryParse(newvalue, out delay))
                        vmcs.AddBootOption(delay);
                    break;

                //case Feature.guest:
                //    if (newvalue.HasValue() && !newvalue.EndsWith("\n"))
                //        newvalue += "\n";
                //    vmcs.annotation = config.annotation + newvalue;
                //    if (vm.State == VmPowerState.running && vmcs.annotation.HasValue())
                //        vmcs.AddGuestInfo(Regex.Split(vmcs.annotation, "\r\n|\r|\n"));
                //    break;

                default:
                    throw new Exception("Invalid change request.");
                    //break;
            }

            ManagedObjectReference task = await _client.ReconfigVM_TaskAsync(vmReference, vmcs);
            TaskInfo info = await WaitForVimTask(task);
            if (info.state == TaskInfoState.error)
                throw new Exception(info.error.localizedMessage);
            return info;
        }

        public async Task<string> SetResolution(Guid id, int width, int height)
        {

            ManagedObjectReference vmReference = await GetVm(id);
            string state = await GetPowerState(id);

            if (vmReference == null)
            {
                _logger.LogDebug($"Could not get vm reference");
                return "error";
            }

            _logger.LogDebug($"Set Resolution vm {id} requested - {width}x{height}");

            if (state == "off")
            {
                state = "vm is powered off";
                _logger.LogDebug($"Returning state: {state}");
                return state;
            }

            try
            {
                await _client.SetScreenResolutionAsync(vmReference, width, height);
                state = "set resolution submitted";
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Failed to set resolution for vm " + id);
                return "error";
            }

            return state;
        }

        #region Helpers
        private bool IsCardDistributed(string cardName)
        {
            if (_networks != null && !string.IsNullOrEmpty(cardName))
            {
                NetworkMOR card;
                if ((card = _networks.Where(n => n.Name.Equals(cardName)).SingleOrDefault()) != null)
                {
                    return card.Type.Equals("DistributedVirtualPortgroup");
                }
            }
            return false;
        }
        private string GetPortGroupId(string cardName)
        {
            if (_networks != null && !string.IsNullOrEmpty(cardName))
            {
                NetworkMOR card;
                if ((card = _networks.Where(n => n.Name.Equals(cardName)).SingleOrDefault()) != null)
                {
                    return card.Value;
                }
            }
            return string.Empty;
        }
        private string GetSwitchUuid(string cardName)
        {
            if (_networks != null && !string.IsNullOrEmpty(cardName))
            {
                NetworkMOR card;
                if ((card = _networks.Where(n => n.Name.Equals(cardName)).SingleOrDefault()) != null)
                {
                    return card.Uuid;
                }
            }
            return string.Empty;
        }

        #endregion

        #region Filters

        public static PropertyFilterSpec[] TaskFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "Task",
                pathSet = new string[] { "info" }
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor,
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public PropertyFilterSpec[] NetworkSearchFilter()
        {
            PropertySpec prop;
            List<PropertySpec> props = new List<PropertySpec>();

            TraversalSpec trav = new TraversalSpec();
            List<SelectionSpec> list = new List<SelectionSpec>();

            SelectionSpec sel = new SelectionSpec();
            List<SelectionSpec> selectset = new List<SelectionSpec>();

            ObjectSpec objectspec = new ObjectSpec();
            PropertyFilterSpec filter = new PropertyFilterSpec();

            trav.name = "DatacenterTraversalSpec";
            trav.type = "Datacenter";
            trav.path = "networkFolder";

            sel.name = "FolderTraversalSpec";
            selectset.Add(sel);
            trav.selectSet = selectset.ToArray();
            list.Add(trav);

            trav = new TraversalSpec();
            trav.name = "FolderTraversalSpec";
            trav.type = "Folder";
            trav.path = "childEntity";
            selectset.Clear();
            sel = new SelectionSpec();
            sel.name = "DatacenterTraversalSpec";
            selectset.Add(sel);
            trav.selectSet = selectset.ToArray();
            list.Add(trav);

            prop = new PropertySpec();
            prop.type = "Datacenter";
            prop.pathSet = new string[] { "networkFolder", "name" };
            props.Add(prop);

            prop = new PropertySpec();
            prop.type = "Folder";
            prop.pathSet = new string[] { "childEntity", "name" };
            props.Add(prop);

            prop = new PropertySpec();
            prop.type = "VmwareDistributedVirtualSwitch";
            prop.pathSet = new string[] { "portgroup", "name", "parent", "uuid" };
            props.Add(prop);

            prop = new PropertySpec();
            prop.type = "DistributedVirtualPortgroup";
            prop.pathSet = new string[] { "name", "key" };
            props.Add(prop);

            objectspec = new ObjectSpec();
            objectspec.obj = _sic.rootFolder;
            objectspec.selectSet = list.ToArray();

            filter = new PropertyFilterSpec();
            filter.propSet = props.ToArray();
            filter.objectSet = new ObjectSpec[] { objectspec };
            PropertyFilterSpec[] _dvNetworkSearchFilters = new PropertyFilterSpec[] { filter };
            return _dvNetworkSearchFilters;
        }

        public static PropertyFilterSpec[] NetworkFilter(ManagedObjectReference mor)
        {
            return NetworkFilter(mor, "networkInfo.dnsConfig networkInfo.ipRouteConfig networkInfo.portgroup networkInfo.vnic networkInfo.vswitch");
        }
        public static PropertyFilterSpec[] NetworkFilter(ManagedObjectReference mor, string props)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "HostNetworkSystem",
                pathSet = props.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_net
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }

        public static PropertyFilterSpec[] VmFilter(ManagedObjectReference mor)
        {
            return VmFilter(mor, "summary");
        }

        public static PropertyFilterSpec[] VmFilter(ManagedObjectReference mor, string props)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "VirtualMachine",
                pathSet = props.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_vms or vm-mor
                selectSet = new SelectionSpec[] {
                    new TraversalSpec {
                        type = "Folder",
                        path = "childEntity"
                    }
                }
            };

            PropertyFilterSpec[] ret = new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };

            return ret;
        }

        public static PropertyFilterSpec[] HostFilter(ManagedObjectReference mor, string props)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "HostSystem",
                pathSet = props.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_vms or vm-mor
            };

            PropertyFilterSpec[] ret = new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };

            return ret;
        }

        private async Task<string> GetDistributedSwitchUuid(ManagedObjectReference mor)
        {
            RetrievePropertiesResponse response = await _client.RetrievePropertiesAsync(_props, PortGroupFilter(mor));
            ManagedObjectReference switchMOR = ((DVPortgroupConfigInfo)response.returnval[0].propSet[0].val).distributedVirtualSwitch;
            response = await _client.RetrievePropertiesAsync(_props, SwitchFilter(switchMOR));

            var config = ((VMwareDVSConfigInfo)response.returnval[0].GetProperty("config"));

            // if this is an uplink switch, return null so that we don't use it as a NIC
            if ((config.uplinkPortgroup[0].Value.Equals(mor.Value)))
                return null;
            return ((string)response.returnval[0].GetProperty("uuid"));
        }

        public static PropertyFilterSpec[] SwitchFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "VmwareDistributedVirtualSwitch",
                pathSet = new string[] { "config", "uuid", }
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_vms or vm-mor
            };

            PropertyFilterSpec[] ret = new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };

            return ret;
        }

        public static PropertyFilterSpec[] PortGroupFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "DistributedVirtualPortgroup",
                pathSet = new string[] { "config" }
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_vms or vm-mor
            };

            PropertyFilterSpec[] ret = new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };

            return ret;
        }

        public static PropertyFilterSpec[] NetworkSummaryFilter(ManagedObjectReference mor, string props)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "Network",
                pathSet = props.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_vms or vm-mor
            };

            PropertyFilterSpec[] ret = new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };

            return ret;
        }

        public static PropertyFilterSpec[] DatastoreFilter(ManagedObjectReference mor)
        {
            PropertySpec prop = new PropertySpec
            {
                type = "Datastore",
                pathSet = new string[] { "browser", "summary" }
            };

            ObjectSpec objectspec = new ObjectSpec
            {
                obj = mor, //_res
                selectSet = new SelectionSpec[] {
                    new TraversalSpec {
                        type = "ComputeResource",
                        path = "datastore"
                    }
                }
            };

            return new PropertyFilterSpec[] {
                new PropertyFilterSpec {
                    propSet = new PropertySpec[] { prop },
                    objectSet = new ObjectSpec[] { objectspec }
                }
            };
        }


        #endregion

        #region Getters

        public async Task<VsphereVirtualMachine> GetMachineById(Guid id)
        {
            ManagedObjectReference machineReference = _connectionService.GetMachineById(id);

            if (machineReference == null)
            {
                // lookup reference
                machineReference = await GetVm(id);

                // return null if not found
                if (machineReference == null)
                {
                    return null;
                }
            }

            if (_client == null)
            {
                return null;
            }

            // retrieve all machine properties we need
            RetrievePropertiesResponse propertiesResponse = await _client.RetrievePropertiesAsync(
                _props,
                VmFilter(machineReference, "name summary.guest.toolsStatus summary.runtime.host summary.runtime.powerState config.hardware.device"));

            NetVimClient.ObjectContent vm = propertiesResponse.returnval.FirstOrDefault();

            var toolsStatus = vm.GetProperty("summary.guest.toolsStatus") as Nullable<VirtualMachineToolsStatus>;
            VirtualMachineToolsStatus vmToolsStatus = VirtualMachineToolsStatus.toolsNotRunning;
            if (toolsStatus != null)
            {
                vmToolsStatus = toolsStatus.Value;
            }

            VsphereVirtualMachine machine = new VsphereVirtualMachine
            {
                Devices = vm.GetProperty("config.hardware.device") as VirtualDevice[],
                HostReference = ((ManagedObjectReference)vm.GetProperty("summary.runtime.host")).Value,
                Id = id,
                Name = vm.GetProperty("name") as string,
                Reference = vm.obj,
                State = (VirtualMachinePowerState)vm.GetProperty("summary.runtime.powerState") == VirtualMachinePowerState.poweredOn ? "on" : "off",
                VmToolsStatus = vmToolsStatus,
            };

            return machine;
        }

        private async Task<Datastore> GetDatastoreByName(string dsName)
        {
            Datastore datastore = _connectionService.GetDatastoreByName(dsName);

            if (datastore == null)
            {
                // lookup reference
                datastore = await GetNewDatastore(dsName);

                // return null if not found
                if (datastore == null)
                {
                    return null;
                }
            }

            if (_client == null)
            {
                return null;
            }

            return datastore;
        }

        private async Task<Datastore> GetNewDatastore(string dsName)
        {
            var clunkyTree = await LoadReferenceTree(_client);
            if (clunkyTree.Length == 0)
            {
                throw new InvalidOperationException();
            }
            var datastores = clunkyTree.FindType("Datastore");
            foreach (NetVimClient.ObjectContent rawDatastore in datastores)
            {
                if (dsName == rawDatastore.GetProperty("name").ToString())
                {
                    return new Datastore()
                    {
                        Name = rawDatastore.GetProperty("name").ToString(),
                        Reference = rawDatastore.obj,
                        Browser = (ManagedObjectReference)rawDatastore.GetProperty("browser")
                    };
                }
            }
            return null;
        }

        private async Task<NetVimClient.ObjectContent[]> LoadReferenceTree(VimPortTypeClient client)
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
                    }
                }
            };

            var props = new PropertySpec[]
            {
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
            RetrievePropertiesResponse response = await client.RetrievePropertiesAsync(_props, filters);

            return response.returnval;
        }

        #endregion
    }
}

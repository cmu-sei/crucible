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
using System.Threading;
using STT = System.Threading.Tasks;
using Steamfitter.Api.Infrastructure.Options;
using Stackstorm.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Steamfitter.Api.Services
{
    public interface IStackStormService : IHostedService
    {
        ConcurrentDictionary<Guid, VmIdentityStrings> GetVmList();
        string GetVmMoid(Guid uuid);
        List<Guid> GetVmGuids(string mask);
        string GetVmName(Guid uuid);
        STT.Task GetStackstormVms();
        STT.Task<string> GuestCommand(string parameters);
        STT.Task<string> GuestReadFile(string parameters);
        STT.Task<string> VmPowerOn(string parameters);
        STT.Task<string> VmPowerOff(string parameters);
        STT.Task<string> CreateVmFromTemplate(string parameters);
        STT.Task<string> VmRemove(string parameters);
    }

    public class StackStormService : IStackStormService
    {
        private readonly ILogger<StackStormService> _logger;
        private VmTaskProcessingOptions _options;
        private ConcurrentDictionary<Guid, VmIdentityStrings> _vmList = new ConcurrentDictionary<Guid, VmIdentityStrings>();
        private VSphere _vsphere;

        public StackStormService(
            IOptions<VmTaskProcessingOptions> options,
            ILogger<StackStormService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }       

        public ConcurrentDictionary<Guid, VmIdentityStrings> GetVmList()
        {
            return _vmList;
        }

        public string GetVmMoid(Guid uuid)
        {
            return _vmList[uuid].Moid;
        }

        public List<Guid> GetVmGuids(string mask)
        {
            Guid maskGuid;
            var guidList = new List<Guid>();
            if (Guid.TryParse(mask, out maskGuid))
            {
                guidList.Add(maskGuid);
                return guidList;
            }

            var matchingVms = _vmList.OrderBy(vm => vm.Value.Name).Where(vm => vm.Value.Name.ToLower().Contains(mask.ToLower())).Select(vm => vm.Key);
            return matchingVms.ToList();
        }

        public string GetVmName(Guid uuid)
        {
            return _vmList[uuid].Name;
        }

        public STT.Task StartAsync(CancellationToken cancellationToken)
        {
            Connect();
            Run();
            return STT.Task.CompletedTask;
        }

        public STT.Task StopAsync(CancellationToken cancellationToken)
        {
            return STT.Task.CompletedTask;
        }

        private void Connect()
        {
            _vsphere = new VSphere(_options.ApiBaseUrl, _options.ApiUsername, _options.ApiPassword);
        }

        private async void Run()
        {
            _logger.LogInformation($"Starting StackStormService");
            while (true)
            {
                try
                {
                    await GetStackstormVms();
                    // if no vm's were found, check again in HealthCheckSeconds.  Otherwise, check again in VmListUpdateIntervalMinutes
                    if (_vmList.Any())
                    {
                        await STT.Task.Delay(new TimeSpan(0, _options.VmListUpdateIntervalMinutes, 0));
                    }
                    else
                    {
                        _logger.LogError("The StackStormService did not find any VM's.  This could mean that StackStorm is not running or the StackStorm configuration is incorrect.");
                        await STT.Task.Delay(new TimeSpan(0, 0, _options.HealthCheckSeconds));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception encountered in StackStorm loop", ex);
                    _vmList = new ConcurrentDictionary<Guid, VmIdentityStrings>();                }
            }
        }

        public async STT.Task GetStackstormVms()
        {
            // get the list of all VM's (moid, name) using vsphere.get_vms
            var vmIdentityObjs = new List<VmIdentityStrings>();
            var uuidList = new List<Guid>();
            var apiParameters = _options.ApiParameters;
            var clusters = apiParameters["clusters"].ToString().Split(",");
            try
            {
                var vmListResult = await _vsphere.GetVms(clusters);
                // build a moid list to make only one call for all of the vm details
                var moidList = new List<string>();
                foreach (var vm in vmListResult.Vms)
                {
                    moidList.Add(vm.Moid);
                    vmIdentityObjs.Add(new VmIdentityStrings(){Moid=vm.Moid, Name=vm.Name});
                }
                // get the uuid for each vm
                var vmDetailList = await _vsphere.GetVmDetails(moidList);
                foreach (var vmIdentityObj in vmIdentityObjs)
                {
                    var uuidString = "";
                    try
                    {
                        var vmDetail = vmDetailList.Vms.Single(x => x.Name == vmIdentityObj.Name);
                        uuidString = vmDetail.Uuid;
                    }
                    catch (Exception)
                    {
                        // find the correct VM, if there are duplicate names
                        var moidListSingle = new List<string>();
                        moidListSingle.Add(vmIdentityObj.Moid);
                        var vmDetailListSingle = await _vsphere.GetVmDetails(moidListSingle);
                        var vmDetail = vmDetailListSingle.Vms.Single(x => x.Name == vmIdentityObj.Name);
                        uuidString = vmDetail.Uuid;
                    }
                    Guid uuid;
                    if (Guid.TryParse(uuidString, out uuid))
                    {
                        uuidList.Add(uuid);
                        _vmList[uuid] = vmIdentityObj;
                    }
                    else
                    {
                        _logger.LogWarning($"VM {vmIdentityObj.Name} moid:{vmIdentityObj.Moid} uuid:{uuidString} is not a Guid.");
                    }
                }
                var keysToRemove = _vmList.Keys.Except(uuidList).ToList();
                foreach (var key in keysToRemove)
                {
                    VmIdentityStrings deletedStrings;
                    _vmList.Remove(key, out deletedStrings); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an error getting the VM information from stackstorm.  {ex.Message}", ex);
                _vmList = new ConcurrentDictionary<Guid, VmIdentityStrings>();
            }
        }

        public async STT.Task<string> GuestCommand(string parameters)
        {
            var command = JsonConvert.DeserializeObject<Stackstorm.Connector.Models.Vsphere.Requests.Command>(parameters);
            // the moid parameter is actually a Guid and the moid must be looked up
            command.Moid = GetVmMoid(Guid.Parse(command.Moid));
            var executionResult = await _vsphere.GuestCommand(command);

            return executionResult.Value;
        }

        public async STT.Task<string> GuestReadFile(string parameters)
        {
            var command = JsonConvert.DeserializeObject<Stackstorm.Connector.Models.Vsphere.Requests.FileRead>(parameters);
            // the moid parameter is actually a Guid and the moid must be looked up
            command.Moid = GetVmMoid(Guid.Parse(command.Moid));
            var executionResult = await _vsphere.GuestFileRead(command);

            return executionResult.Value;
        }

        public async STT.Task<string> VmPowerOn(string parameters)
        {
            var command = JObject.Parse(parameters);
            // the moid parameter is actually a Guid and the moid must be looked up
            var moid = GetVmMoid(Guid.Parse(command["Moid"].ToString()));
            var executionResult = await _vsphere.GuestPowerOn(moid);

            return executionResult.State.ToString();
        }

        public async STT.Task<string> VmPowerOff(string parameters)
        {
            var command = JObject.Parse(parameters);
            // the moid parameter is actually a Guid and the moid must be looked up
            var moid = GetVmMoid(Guid.Parse(command["Moid"].ToString()));
            var executionResult = await _vsphere.GuestPowerOff(moid);

            return executionResult.State.ToString();
        }

        public async STT.Task<string> CreateVmFromTemplate(string parameters)
        {
            var command = JsonConvert.DeserializeObject<Stackstorm.Connector.Models.Vsphere.Requests.CreateVmFromTemplate>(parameters);
            var executionResult = await _vsphere.CreateVmFromTemplate(command);

            return executionResult.Value;
        }

        public async STT.Task<string> VmRemove(string parameters)
        {
            var command = JObject.Parse(parameters);
            // the moid parameter is actually a Guid and the moid must be looked up
            var moid = GetVmMoid(Guid.Parse(command["Moid"].ToString()));
            var executionResult = await _vsphere.RemoveVm(moid);

            return executionResult.Value;
        }

    }

    public class VmIdentityStrings
    {
        public string Moid { get; set; }
        public string Name { get; set; }
    }
}


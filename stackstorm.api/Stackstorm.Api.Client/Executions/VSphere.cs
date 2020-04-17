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
using System.Threading.Tasks;
using Stackstorm.Api.Client;
using Stackstorm.Api.Client.Models;

namespace stackstorm.api.client.Executions
{
    public interface IVSphere
    {
        Task<Execution> GetMoid(Dictionary<string, string> parameters);
        Task<Execution> GetVmConsoleUrls(Dictionary<string, string> parameters);
        Task<Execution> GetVms(Dictionary<string, string> parameters);
        Task<Execution> GuestDirectoryCreate(Dictionary<string, string> parameters);
        Task<Execution> GuestDirectoryDelete(Dictionary<string, string> parameters);
        Task<Execution> GuestFileCreate(Dictionary<string, string> parameters);
        Task<Execution> GuestFileDelete(Dictionary<string, string> parameters);
        Task<Execution> GuestFileRead(Dictionary<string, string> parameters);
        Task<Execution> GuestFileUpload(Dictionary<string, string> parameters);
        Task<Execution> GuestProcessRun(Dictionary<string, string> parameters);
        Task<Execution> GuestProcessStart(Dictionary<string, string> parameters);
        Task<Execution> GuestProcessWait(Dictionary<string, string> parameters);
        Task<Execution> GuestScriptRun(Dictionary<string, string> parameters);
        Task<Execution> Hello(Dictionary<string, string> parameters);
        Task<Execution> HostGet(Dictionary<string, string> parameters);
        Task<Execution> HostGetNetworkHits(Dictionary<string, string> parameters);
        Task<Execution> SetVm(Dictionary<string, string> parameters);
        Task<Execution> CreateVmFromTemplate(Dictionary<string, string> parameters);
        Task<Execution> VmGetEnvItems(Dictionary<string, string> parameters);
        Task<Execution> VmGetGuestInfo(Dictionary<string, string> parameters);
        Task<Execution> VmCreateBarebones(Dictionary<string, string> parameters);
        Task<Execution> VmBuildBasic(Dictionary<string, string> parameters);
        Task<Execution> VmEditMemory(Dictionary<string, string> parameters);
        Task<Execution> VmGetDetail(Dictionary<string, string> parameters);
        Task<Execution> VmAddHdd(Dictionary<string, string> parameters);
        Task<Execution> VmGetMoid(Dictionary<string, string> parameters);
        Task<Execution> VmNicAdd(Dictionary<string, string> parameters);
        Task<Execution> VmNicEdit(Dictionary<string, string> parameters);
        Task<Execution> VmPowerOff(Dictionary<string, string> parameters);
        Task<Execution> VmPowerOn(Dictionary<string, string> parameters);
        Task<Execution> VmRemove(Dictionary<string, string> parameters);
        Task<Execution> VmScsiControllerAdd(Dictionary<string, string> parameters);
        Task<Execution> VmGetUuid(Dictionary<string, string> parameters);
        Task<Execution> VmGetRuntimeInfo(Dictionary<string, string> parameters);
        Task<Execution> VmShutdown(Dictionary<string, string> parameters);
        Task<Execution> Wait(Dictionary<string, string> parameters);
    }
    public class VSphere : ExecutionsBase, IVSphere
    {
        public VSphere(ISt2Client host) : base(host)
        {
        }

        /// <summary>
        /// Returns the MOID of vSphere managed entity corresponding to the specified parameters
        /// </summary>
        public async Task<Execution> GetMoid(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.get_moid", parameters);
        }

        /// <summary>
        /// Retrieves urls of the virtual machines' consoles
        /// </summary>
        public async Task<Execution> GetVmConsoleUrls(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.get_vmconsole_urls", parameters);
        }

        /// <summary>
        /// Retrieves the virtual machines on a vCenter Server system. It computes the union of Virtual Machine sets based on each parameter
        /// </summary>
        public async Task<Execution> GetVms(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.get_vms", parameters);
        }

        /// <summary>
        /// Creates a temporary directory inside the guest
        /// </summary>
        public async Task<Execution> GuestDirectoryCreate(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_dir_create", parameters);
        }

        /// <summary>
        /// Deletes a directory inside the guest
        /// </summary>
        public async Task<Execution> GuestDirectoryDelete(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_dir_delete", parameters);
        }

        /// <summary>
        /// Creates a temporary file inside the guest
        /// </summary>
        public async Task<Execution> GuestFileCreate(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_file_create", parameters);
        }

        /// <summary>
        /// Deletes a file inside the guest
        /// </summary>
        public async Task<Execution> GuestFileDelete(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_file_delete", parameters);
        }

        /// <summary>
        /// Read a file inside the guest
        /// </summary>
        public async Task<Execution> GuestFileRead(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_file_read", parameters);
        }

        /// <summary>
        /// Upload a file to the guest
        /// </summary>
        public async Task<Execution> GuestFileUpload(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_file_upload", parameters);
        }

        /// <summary>
        /// Run a process inside the guest
        /// </summary>
        public async Task<Execution> GuestProcessRun(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_process_run", parameters);
        }

        /// <summary>
        /// Start a process inside the guest
        /// </summary>
        public async Task<Execution> GuestProcessStart(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_process_start", parameters);
        }

        /// <summary>
        /// Wait for a process inside the guest to exit
        /// </summary>
        public async Task<Execution> GuestProcessWait(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_process_wait", parameters);
        }

        /// <summary>
        /// Run a script inside the guest
        /// </summary>
        public async Task<Execution> GuestScriptRun(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.guest_script_run", parameters);
        }

        /// <summary>
        /// Wait for a Task to complete and returns its result
        /// </summary>
        public async Task<Execution> Hello(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.hello_vsphere", parameters);
        }

        /// <summary>
        /// Retrieve summary information for given Hosts (ESXi)
        /// </summary>
        public async Task<Execution> HostGet(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.host_get", parameters);
        }

        /// <summary>
        /// Retrieve Network Hints for given Hosts (ESXi)
        /// </summary>
        public async Task<Execution> HostGetNetworkHits(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.host_network_hits_get", parameters);
        }

        /// <summary>
        /// Changes configuration of a Virtual Machine
        /// </summary>
        public async Task<Execution> SetVm(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.set_vm", parameters);
        }

        /// <summary>
        /// Create a new VM from existing template
        /// </summary>
        public async Task<Execution> CreateVmFromTemplate(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_create_from_template", parameters);
        }

        /// <summary>
        /// Retrieve list of Objects from VSphere
        /// </summary>
        public async Task<Execution> VmGetEnvItems(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_env_items_get", parameters);
        }

        /// <summary>
        /// Retrieve Guest details of a VM object
        /// </summary>
        public async Task<Execution> VmGetGuestInfo(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_guest_info_get", parameters);
        }

        /// <summary>
        /// Create BareBones VM (CPU, Ram, Graphics Only)
        /// </summary>
        public async Task<Execution> VmCreateBarebones(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_barebones_create", parameters);
        }

        /// <summary>
        /// WorkFlow to build a base VM hardware and optional power on (CPU, RAM, HDD, NIC)
        /// </summary>
        public async Task<Execution> VmBuildBasic(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_basic_build", parameters);
        }

        /// <summary>
        /// Adjust CPU and RAM allocation for a Virtual Machine 
        /// </summary>
        public async Task<Execution> VmEditMemory(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_cpu_mem_edit", parameters);
        }

        /// <summary>
        /// Retrieve details of a VM object
        /// </summary>
        public async Task<Execution> VmGetDetail(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_detail_get", parameters);
        }

        /// <summary>
        /// Add New Hdd to Virtual Machine. You must Provide Either VM_ID or Name 
        /// </summary>
        public async Task<Execution> VmAddHdd(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_hdd_add", parameters);
        }

        /// <summary>
        /// Retrieve moid of a VM object
        /// </summary>
        public async Task<Execution> VmGetMoid(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_moid_get", parameters);
        }

        /// <summary>
        /// Add New Hdd to Virtual Machine. You must Provide Either VM_ID or Name 
        /// </summary>
        public async Task<Execution> VmNicAdd(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_nic_add", parameters);
        }

        /// <summary>
        /// Alter Configuration of Network Adapater
        /// </summary>
        public async Task<Execution> VmNicEdit(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_nic_edit", parameters);
        }

        /// <summary>
        /// Performs a Hardware Power Off of a VM. Note: This is not an OS shutdown 
        /// </summary>
        public async Task<Execution> VmPowerOff(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_power_off", parameters);
        }

        /// <summary>
        /// Performs a Hardware Power On of a VM 
        /// </summary>
        public async Task<Execution> VmPowerOn(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_power_on", parameters);
        }

        /// <summary>
        /// Removes the Virtual Machine
        /// </summary>
        public async Task<Execution> VmRemove(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_remove", parameters);
        }

        /// <summary>
        /// Add SCSI Controller to VM. You must provide at least one of VM_ID or Name 
        /// </summary>
        public async Task<Execution> VmScsiControllerAdd(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_scsi_controller_add", parameters);
        }

        /// <summary>
        /// Retrieve uuid of a VM object 
        /// </summary>
        public async Task<Execution> VmGetUuid(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_hw_uuid_get", parameters);
        }

        /// <summary>
        /// Retrieve Runtime details of a VM object 
        /// </summary>
        public async Task<Execution> VmGetRuntimeInfo(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_runtime_info_get", parameters);
        }

        /// <summary>
        /// Initiates a clean shutdown of the guest. Returns immediately without waiting for the guest to complete shutdown
        /// </summary>
        public async Task<Execution> VmShutdown(Dictionary<string, string> parameters)
        {
            return await AddExecution("vsphere.vm_shutdown", parameters);
        }

        /// <summary>
        /// TODO: Wait for a Task to complete and returns its result
        /// </summary>
        public async Task<Execution> Wait(Dictionary<string, string> parameters)
        {
            throw new NotImplementedException("Not sure where task Id comes from");
            //TODO
            return await AddExecution("vsphere.wait_task", parameters);
        }
    }
}

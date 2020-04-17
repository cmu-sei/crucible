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
using Newtonsoft.Json.Linq;
using Stackstorm.Api.Client.Extensions;
using Xunit;

namespace Stackstorm.Api.Client.Test
{
    public class Vsphere : StackstormBase
    {
        [Fact]
        public void HelloReturnsGuid()
        {
            var executionResult = this.Client.VSphere.Hello(null);

            var j = JObject.Parse(executionResult.Result.result.ToString());
            var guid = j["result"].ToString();
            var isValid = Guid.TryParse(guid, out _);
            Assert.True(isValid);
        }

        [Fact]
        public async void GetVmsReturnsArrayOfVms()
        {
            var cluster = "[\"domain-c9\"]";
            var executionResult = await this.Client.VSphere.GetVms(new Dictionary<string, string> {{"clusters", cluster}});
            var j = JObject.Parse(executionResult.result.ToString());

            var hasVms = false;
            //get vms and spit out
            foreach (var node in j["result"])
            {
                Assert.NotNull(node["name"]);
                Assert.NotNull(node["moid"]);

                Assert.True(!string.IsNullOrEmpty(node["name"].ToString()), "vm names can't be null or empty");
                Assert.True(!string.IsNullOrEmpty(node["moid"].ToString()), "moid names can't be null or empty");
                hasVms = true;
            }

            Assert.True(hasVms, $"Cluster {cluster} did not return any VMs");
        }

        [Fact]
        public async void GetVmDetailReturnsUuid()
        {
            var vmId = "[\"vm-303\"]"; //this is a moid
            
            var executionResult = await this.Client.VSphere.VmGetDetail(new Dictionary<string, string> {{"vm_ids", vmId}});
            var tokens = executionResult.result.ToString().ToJTokens();
            foreach (var token in tokens)
            {
                var uuid = token.First["config"]["uuid"];
                var isValid = Guid.TryParse(uuid.ToString(), out _);
                Assert.True(isValid, $"{vmId} uuid is not a guid");
            }
        }

        [Fact]
        public async void GetGuestInfoReturns()
        {
            var vmId = "[\"vm-303\"]"; //this is a moid
            
            var executionResult = await this.Client.VSphere.VmGetGuestInfo(new Dictionary<string, string> {{"vm_ids", vmId}});
            var tokens = executionResult.result.ToString().ToJTokens();

            foreach (var token in tokens)
            {
                Assert.NotNull(token);
                Assert.NotNull(token.Path);
                //TODO
            }
        }

        [Fact]
        public async void GetMoidReturnsMoid()
        {
            var vmName = "[\"Win10.10\"]";
            var executionResult = await this.Client.VSphere.GetMoid(new Dictionary<string, string> {{"object_names", vmName}, {"object_type", "VirtualMachine"}});
            var j = ((JToken) executionResult.result)["result"];
            
            foreach (var prop in j.OfType<JProperty>())
            {
                Assert.True(!string.IsNullOrEmpty(prop.Value.ToString()));
                Assert.True(!string.IsNullOrEmpty(prop.Name));
            }
        }

        [Fact]
        public async void VmGetMoidReturnsMoid()
        {
            var vmName = "[\"Win10.10\"]";
            var executionResult = await this.Client.VSphere.VmGetMoid(new Dictionary<string, string> {{"vm_names", vmName}});
            var j = ((JToken) executionResult.result)["result"];
            
            foreach (var prop in j.OfType<JProperty>())
            {
                Assert.True(!string.IsNullOrEmpty(prop.Value.ToString()));
                Assert.True(!string.IsNullOrEmpty(prop.Name));
            }
        }

        [Fact]
        public async void GuestFileRead()
        {
            var executionResult = await this.Client.VSphere.GuestFileRead(new Dictionary<string, string> {{"vm_id", "vm-302"}, {"username", "Developer"}, {"password", "develop@1"}, {"guest_file", @"C:\Users\Developer\testGet.txt"}});
            var fileTextObject = ((JObject) executionResult.result)["result"];

            Assert.NotNull(fileTextObject);
            Assert.True(!string.IsNullOrEmpty(fileTextObject.ToString()));
        }

        [Fact]
        public async void VmTurnOn()
        {
            var executionResult = await this.Client.VSphere.VmPowerOn(new Dictionary<string, string> {{"vm_id", "vm-303"}});
            Assert.NotNull(executionResult);
        }
        
        [Fact]
        public async void VmTurnOff()
        {
            var executionResult = await this.Client.VSphere.VmPowerOff(new Dictionary<string, string> {{"vm_id", "vm-303"}});
            Assert.NotNull(executionResult);
        }

        [Fact]
        public async void WaitNotImplemented()
        {
            await Assert.ThrowsAsync<NotImplementedException>(() => this.Client.VSphere.Wait(null));
        }
    }
}

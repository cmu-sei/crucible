/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using Stackstorm.Connector.Models.Vsphere;
using Xunit;

namespace Stackstorm.Connector.Test
{
    public class VSphere : StackstormConnector
    {
        [Fact]
        public async void GetGuidReturnsGuid()
        {
            var o = await this.VSphere.GetGuid();
            Assert.NotNull(o);
            Assert.Null(o.Exception);
            Assert.IsType<Guid>(o.Guid);
        }
        
        [Fact]
        public async void GetVmsReturnsVms()
        {
            var s = new List<string>();
            s.Add("domain-c9");
            
            var o = await this.VSphere.GetVms(s);
            Assert.NotNull(o);
            Assert.NotNull(o.Vms);
            Assert.Null(o.Exception);

            foreach (var vm in o.Vms)
            {
                Assert.NotNull(vm.Moid);                
            }
        }
        
        [Fact]
        public async void GetVmsDetailsReturnsRealNiceDetails()
        {
            var s = new List<string>();
            s.Add("vm-303");
            s.Add("vm-302");
            
            var o = await this.VSphere.GetVmDetails(s);
            Assert.NotNull(o);
            Assert.NotNull(o.Vms);
            Assert.Null(o.Exception);
            
            foreach (var vm in o.Vms)
            {
                Assert.NotNull(vm.Uuid);                
            }
        }

        [Fact]
        public async void GetMoidsReturnsMoids()
        {
            var r = new Requests.Moids();
            r.MachineNames = new List<string> {"Win10.10"};;
            
            var o = await this.VSphere.GetMoids(r);
            
            Assert.NotNull(o);
            Assert.NotNull(o.Vms);
            Assert.Null(o.Exception);
            
            foreach (var vm in o.Vms)
            {
                Assert.NotNull(vm.Name);                
            }
        }
        
        [Fact]
        public async void GuestFileReadReturnsFile()
        {
            var r = new Requests.FileRead
            {
                Moid = "vm-302", Username = "Developer", Password = "develop@1", GuestFilePath = @"C:\Users\Developer\testGet.txt"
            };
            var o = await this.VSphere.GuestFileRead(r);
            Assert.NotNull(o);
            Assert.Null(o.Exception);
            Assert.NotNull(o.Value);
            Assert.True(!string.IsNullOrEmpty(o.Value));
        }
        
        [Fact]
        public async void GuestProcessRunReturnsSomething()
        {
            var r = new Requests.Command {Moid = "vm-303", Username = "Developer", Password = "develop@1", CommandText = @"c:\windows\system32\cmd.exe", CommandArgs = "/C dir", CommandWorkDirectory = @"c:\windows\system32\cmd.exe"};
            var o = await this.VSphere.GuestCommand(r);
            Assert.Null(o.Exception);
            Assert.NotNull(o);
            Assert.NotNull(o.Value);
        }
        
        [Fact]
        public async void GuestPowerOn()
        {
            var o = await this.VSphere.GuestPowerOn("vm-303");
            Assert.NotNull(o.Id);
            Assert.True(o.State == Responses.PowerStates.On);
        }
        
        [Fact]
        public async void GuestPowerOff()
        {
            var o = await this.VSphere.GuestPowerOff("vm-303");
            Assert.NotNull(o.Id);
            Assert.True(o.State == Responses.PowerStates.Off);
        }
    }
}

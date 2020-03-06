/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Linq;
using System.Text.Json;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Serialization;
using Xunit;
using File = System.IO.File;

namespace Caster.Api.Tests.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Category", "TerraformState")]
    public class TerraformStateUnitTest : IClassFixture<StateFixture>
    {
        private readonly StateFixture _stateFixture;

        public TerraformStateUnitTest(StateFixture stateFixture)
        {
            _stateFixture = stateFixture;
        }

        [Fact]
        public void Test_Resource_Count()
        {
            Assert.Equal(13, _stateFixture.GetResources().Length);
        }

        #region Networks

        [Fact]
        public void Test_vSphere_Host_Port_Groups()
        {
            this.VerifyHostPortGroup("course-ext-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "tf-HostPortGroup:host-87:course-ext-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "course-ext");
            this.VerifyHostPortGroup("course-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "tf-HostPortGroup:host-87:course-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "course");
            this.VerifyHostPortGroup("course-net-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "tf-HostPortGroup:host-87:course-net-4c2eb68c-a77f-45aa-990a-6b837ee59d71", "course-net");
        }

        private void VerifyHostPortGroup(string name, string id, string addressName)
        {
            var network = _stateFixture.GetResources().Where(r => r.Id == id).FirstOrDefault();
            Assert.NotNull(network);
            Assert.Equal(name, network.Name);
            Assert.Equal("vsphere_host_port_group", network.Type);
            Assert.Equal($"vsphere_host_port_group.{addressName}", network.Address);
            Assert.Equal($"vsphere_host_port_group.{addressName}", network.BaseAddress);
        }

        #endregion

        [Fact]
        public void Test_vSphere_Virtual_Switches()
        {
            var vSwitch = _stateFixture.GetResources().Where(r => r.Id == "tf-HostVirtualSwitch:host-87:vSwitch-4c2eb68c-a77f-45aa-990a").FirstOrDefault();
            Assert.NotNull(vSwitch);
            Assert.Equal("vSwitch-4c2eb68c-a77f-45aa-990a", vSwitch.Name);
            Assert.Equal("vsphere_host_virtual_switch", vSwitch.Type);
            Assert.Equal("vsphere_host_virtual_switch.switch", vSwitch.Address);
            Assert.Equal("vsphere_host_virtual_switch.switch", vSwitch.BaseAddress);
        }

        #region vSphere_Virtual_Machines
        [Fact]
        public void Test_vSphere_Virtual_Machines()
        {
            this.VerifyVirtualMachine(
                "course.centos6.student.1.4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423ccd67-a76a-ea9f-7089-caa891a621f2",
                "course-centos6-student",
                new Guid("925e2634-52b5-4492-ba5e-c800fe3401f1"),
                0);

            this.VerifyVirtualMachine(
                "course.centos6.student.2.4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423c087c-4715-bfca-2475-9cadb6954f2e",
                "course-centos6-student",
                new Guid("925e2634-52b5-4492-ba5e-c800fe3401f1"),
                1);

            this.VerifyVirtualMachine(
                "course.centos7.server-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423c3932-665f-5764-ce9a-aa4a8d8a2023",
                "course-centos7-server",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.freebsd10-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423c0a62-8566-950c-f5e9-cc4cdf5660be",
                "course-freebsd10-ws01",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.freebsd9-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423c0fb1-220d-7da5-9310-4fdefe4024a4",
                "course-freebsd9-ws01",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.sol10-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423cb77b-6789-cef5-69e2-9b29b2b2b252",
                "course-sol10-ws01",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.sol11-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423cbe4b-761d-f479-13ec-ca4adc5e7b36",
                "course-sol11-ws01",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.ubuntu14.server-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423c09a2-5bd4-1568-3dd7-7fe81512c101",
                "course-ubuntu14-server",
                null,
                null);

            this.VerifyVirtualMachine(
                "course.ubuntu16.server-4c2eb68c-a77f-45aa-990a-6b837ee59d71",
                "423cf12c-013b-f590-3be8-f53a3115ab92",
                "course-ubuntu16-server",
                null,
                null);
        }

        private void VerifyVirtualMachine(string name, string id, string addressName, Guid? teamId, int? count)
        {
            var machine = _stateFixture.GetResources().Where(r => r.Id == id).FirstOrDefault();
            Assert.NotNull(machine);
            Assert.Equal(name, machine.Name);
            Assert.Equal("vsphere_virtual_machine", machine.Type);
            Assert.Equal($"vsphere_virtual_machine.{addressName}{(count.HasValue ? string.Format("[{0}]", count) : "")}", machine.Address);
            Assert.Equal($"vsphere_virtual_machine.{addressName}", machine.BaseAddress);
            Assert.Equal(teamId, machine.GetTeamId());
        }

        [Fact]
        public void Test_Resource_Searchable()
        {
            var resources = _stateFixture.GetResources();

            var machine = resources.Where(r => r.Id == "423c087c-4715-bfca-2475-9cadb6954f2e").FirstOrDefault();
            var searchable = machine.SearchableAttributes;
            var json = JsonSerializer.Serialize(machine, DefaultJsonSettings.Settings);

            var portgroup = resources.Where(r => r.Type == "vsphere_host_port_group").FirstOrDefault();
            json = JsonSerializer.Serialize(portgroup, DefaultJsonSettings.Settings);
        }

        #endregion
    }

    public class StateFixture
    {
        private readonly string _rawState;
        private readonly Workspace _workspace;
        private readonly State _state;
        private readonly Resource[] _resources;

        public StateFixture()
        {
            _rawState = File.ReadAllText($"{Environment.CurrentDirectory}\\Data\\terraform.tfstate");
            _workspace = new Workspace { State = _rawState };
            _state = _workspace.GetState();
            _resources = _state.GetResources();
        }

        public Resource[] GetResources()
        {
            return _resources;
        }
    }
}

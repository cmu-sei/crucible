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
using System.Linq;
using System.Text.Json;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Serialization;
using Xunit;
using File = System.IO.File;

namespace Caster.Api.Tests.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Category", "TerraformPlanOutput")]
    public class TerraformPlanOutputUnitTest : IClassFixture<PlanFixture>
    {
        private readonly PlanFixture _planFixture;
        private readonly PlanOutput _planOutput;

        public TerraformPlanOutputUnitTest(PlanFixture planFixture)
        {
            _planFixture = planFixture;
            _planOutput = planFixture.GetPlanOutput();
        }

        [Fact]
        public void Test_Resource_Count()
        {
            Assert.Equal(10, _planOutput.ResourceChanges.Count());
        }

        [Fact]
        public void Test_New_Vm_Count()
        {
            Assert.Equal(7, _planFixture.GetPlanOutput().GetAddedMachines().Count());
        }

    }

    public class PlanFixture
    {
        private readonly string _rawPlanOutput;
        public readonly PlanOutput _planOutput;

        public PlanFixture()
        {
            _rawPlanOutput = File.ReadAllText($"{Environment.CurrentDirectory}\\Data\\plan.json");
            _planOutput = JsonSerializer.Deserialize<PlanOutput>(_rawPlanOutput, DefaultJsonSettings.Settings);
        }

        public PlanOutput GetPlanOutput()
        {
            return _planOutput;
        }
    }
}

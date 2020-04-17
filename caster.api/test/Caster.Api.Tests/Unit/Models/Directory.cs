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
using Caster.Api.Domain.Models;
using Xunit;

namespace Caster.Api.Tests.Unit
{
    [Trait("Category", "Unit")]
    [Trait("Category", "Directory")]
    public class DirectoryUnitTest
    {
        public DirectoryUnitTest()
        {
        }

        [Fact]
        public void Test_Directory_Path_To_List()
        {
            var greatGrandparentId = Guid.NewGuid();
            var grandparentId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var id = Guid.NewGuid();

            var directory = new Directory()
            {
                Id = id,
                ExerciseId = Guid.NewGuid(),
                ParentId = parentId
            };

            directory.SetPath($"{greatGrandparentId}/{grandparentId}/{parentId}/");

            var expectedPathIds = new Guid[] {greatGrandparentId,grandparentId,parentId,id};
            var pathIds = directory.PathIds();

            Assert.Equal(expectedPathIds, pathIds);
        }

        [Fact]
        public void Test_Directory_Set_Import_Name()
        {
            string nameWithGuid = "DirectoryWithId__b7ef25e6-555e-41c9-88d7-22078d3a13c1";
            string nameWithoutGuid = "Directory";
            string nameWithInvalidGuid = "DirectoryWithInvalidId__b7ef25e6-555e-41c9-88d7822078d3a13c1";

            Directory dir1 = new Directory();
            Directory dir2 = new Directory();
            Directory dir3 = new Directory();

            dir1.SetImportName(nameWithGuid);
            dir2.SetImportName(nameWithoutGuid);
            dir3.SetImportName(nameWithInvalidGuid);

            Assert.Equal("DirectoryWithId", dir1.Name);
            Assert.Equal(new Guid("b7ef25e6-555e-41c9-88d7-22078d3a13c1"), dir1.Id);

            Assert.Equal("Directory", dir2.Name);
            Assert.Equal(Guid.Empty, dir2.Id);

            Assert.Equal("DirectoryWithInvalidId__b7ef25e6-555e-41c9-88d7822078d3a13c1", dir3.Name);
            Assert.Equal(Guid.Empty, dir3.Id);

        }
    }
}

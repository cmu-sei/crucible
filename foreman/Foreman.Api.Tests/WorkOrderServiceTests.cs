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
using System.Linq;
using System.Threading;
using Foreman.Data;
using Foreman.Data.Models;
using Foreman.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Foreman.Api.Tests
{
    public class WorkOrderServiceTests
    {
        private readonly IWorkOrderService _service;
        
        public WorkOrderServiceTests()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this._service = new WorkOrderService(new ApplicationDbContext(dbOptions));

            Console.WriteLine("hi");
        }
        
        [Fact]
        public async void ServiceCreates()
        {
            var o = new WorkOrder();
            o.Id = Guid.NewGuid();
            o.Start = DateTime.UtcNow;

            var trigger = new WorkOrder.WorkOrderTrigger();
            trigger.Interval = 500;
            
            o.Triggers.Add(trigger);

            await this._service.Create(o, new CancellationToken());
            
            Console.WriteLine(o.Id);

            var list = await this._service.List(new CancellationToken());
            Assert.True(list.Any());
        }
    }
}

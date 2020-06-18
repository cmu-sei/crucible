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
using Foreman.Core;
using Foreman.Core.Models;

namespace Foreman.Service
{
    public class Seed
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async void Run(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.WebHooks.Any())
                return;
            
            var hookId = Guid.Parse("b1dc89fe-96d1-43e0-b887-e070dc0d8335"); 
            var hook = new WebHook();
            hook.Id = hookId;
            hook.PostbackUrl = " http://localhost:4400/Tasks/[task.id]/execute";
            hook.PostbackMethod = WebHook.WebhookMethod.POST;
            hook.MustAuthenticate = true;
            hook.CreatedUtc = DateTime.UtcNow;
            hook.Status = StatusType.Active;
            hook.Description = "This is a sample auth hook";
            hook.Payload = "{\"id\":\"bbbbbbbb-cccc-dddd-eeee-000000000000\", \"created\":\"[datetime.now]\", \"description\":\"sample payload\"}";
            context.WebHooks.Add(hook);
            
            hookId = Guid.Parse("fca37e42-b7d8-4847-a07b-fbf3ccc447ed"); 
            hook = new WebHook();
            hook.Id = hookId;
            hook.PostbackUrl = " http://localhost:4400/hi";
            hook.PostbackMethod = WebHook.WebhookMethod.POST;
            hook.MustAuthenticate = false;
            hook.CreatedUtc = DateTime.UtcNow;
            hook.Status = StatusType.Active;
            hook.Description = "This is a sample no auth hook";
            hook.Payload = "{\"id\":\"bbbbbbbb-cccc-dddd-eeee-000000000000\", \"created\":\"[datetime.now]\", \"description\":\"sample payload\"}";
            context.WebHooks.Add(hook);
            
            var workOrder = new WorkOrder();
            workOrder.Start = DateTime.UtcNow.AddDays(-2);
            workOrder.End = DateTime.UtcNow.AddDays(2);
            workOrder.Job = JobType.WebHook;
            workOrder.Status = StatusType.Submitted;
            var trigger = new WorkOrder.WorkOrderTrigger();
            trigger.Interval = 10;
            workOrder.Triggers.Add(trigger);
            workOrder.GroupName = "TEST1";
            workOrder.WebhookId = hookId;
            context.WorkOrders.Add(workOrder);
            
            workOrder = new WorkOrder();
            workOrder.Start = DateTime.UtcNow.AddDays(-2);
            workOrder.End = DateTime.UtcNow.AddDays(2);
            workOrder.Job = JobType.Test;
            workOrder.Status = StatusType.Submitted;
            trigger = new WorkOrder.WorkOrderTrigger();
            trigger.Interval = 5;
            workOrder.Triggers.Add(trigger);
            workOrder.GroupName = "TEST2";
            context.WorkOrders.Add(workOrder);
            
            await context.SaveChangesAsync();
            
            log.Debug("Seed completed");
        }
    }
}

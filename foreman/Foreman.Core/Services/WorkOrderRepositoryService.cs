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
using System.Threading;
using System.Threading.Tasks;
using Foreman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Foreman.Core.Services
{
    public interface IWorkOrderRepositoryService
    {
        Task<IEnumerable<WorkOrder>> List(CancellationToken ct);
        Task<IEnumerable<WorkOrder>> ListByStatus(StatusType status, CancellationToken ct);
        Task<WorkOrder> GetById(Guid id, CancellationToken ct);
        Task<WorkOrder> Create(WorkOrder workOrder, CancellationToken ct);
        Task<WorkOrder> Update(WorkOrder workOrder, CancellationToken ct);
        int Count(CancellationToken ct);
        void Delete(Guid id, CancellationToken ct);
    }

    public class WorkOrderRepositoryService : IWorkOrderRepositoryService
    {
        private readonly ApplicationDbContext _context;

        public WorkOrderRepositoryService(ApplicationDbContext context)
        {
            this._context = context;
        }

        public int Count(CancellationToken ct)
        {
            return this._context.WorkOrders.Count();
        }

        public async Task<IEnumerable<WorkOrder>> List(CancellationToken ct)
        {
            return await this._context.WorkOrders
                .Include(o => o.Params)
                .Include(o => o.Results)
                .Include(o => o.Triggers)
                .ToArrayAsync(ct);
        }

        public async Task<WorkOrder> GetById(Guid id, CancellationToken ct)
        {
            return await this._context.WorkOrders.FirstOrDefaultAsync(o => o.Id == id, ct);
        }

        public async Task<IEnumerable<WorkOrder>> ListByStatus(StatusType status, CancellationToken ct)
        {
            return await this._context.WorkOrders
                .Where(o => o.Status == status)
                .Include(o => o.Params)
                .Include(o => o.Results)
                .Include(o => o.Triggers)
                .ToArrayAsync(ct);
        }

        public async Task<WorkOrder> Update(WorkOrder workOrder, CancellationToken ct)
        {
            var item = await this._context.WorkOrders
                .Include(o => o.Params)
                .Include(o => o.Results)
                .Include(o => o.Triggers)
                .FirstOrDefaultAsync(o => o.Id == workOrder.Id, ct);

            if (item == null)
                throw new ArgumentException("WorkOrder not found");

            item.Start = workOrder.Start;
            item.Status = workOrder.Status;
            item.Triggers = workOrder.Triggers;
            item.WebhookId = workOrder.WebhookId;
            item.End = workOrder.End;
            item.Job = workOrder.Job;
            item.Params = workOrder.Params;
            item.GroupName = workOrder.GroupName;
            
            // need at least one trigger to schedule a job
            if (!item.Triggers.Any())
            {
                item.Triggers.Add(new WorkOrder.WorkOrderTrigger { Interval = 0});
            }

            _context.WorkOrders.Update(item);
            await _context.SaveChangesAsync(ct);
            return item;
        }

        public async Task<WorkOrder> Create(WorkOrder workOrder, CancellationToken ct)
        {
            if (workOrder.Id == Guid.Empty)
                workOrder.Id = Guid.NewGuid();
            
            // need at least one trigger to schedule a job
            if (!workOrder.Triggers.Any())
            {
                workOrder.Triggers.Add(new WorkOrder.WorkOrderTrigger { Interval = 0});
            }

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync(ct);
            return workOrder;
        }

        public async void Delete(Guid id, CancellationToken ct)
        {
            var workOrder = await _context.WorkOrders.SingleOrDefaultAsync(m => m.Id == id);
            if (workOrder == null)
            {
                throw new ArgumentException("WorkOrder not found");
            }

            _context.WorkOrders.Remove(workOrder);
            await _context.SaveChangesAsync(ct);
        }
    }
}

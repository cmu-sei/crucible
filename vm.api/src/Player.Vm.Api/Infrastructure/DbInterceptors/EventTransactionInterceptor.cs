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
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Player.Vm.Api.Domain.Events;

namespace Player.Vm.Api.Infrastructure.DbInterceptors
{
    public class EventTransactionInterceptor : DbTransactionInterceptor
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public EventTransactionInterceptor(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        public override async Task TransactionCommittedAsync(
            DbTransaction transaction,
            TransactionEndEventData eventData,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await PublishEvents(eventData);
            await base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
        }

        public override async void TransactionCommitted(
            DbTransaction transaction,
            TransactionEndEventData eventData)
        {
            await PublishEvents(eventData);
            base.TransactionCommitted(transaction, eventData);
        }

        private async Task PublishEvents(TransactionEndEventData eventData)
        {
            var entries = eventData.Context.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added ||
                            x.State == EntityState.Modified ||
                            x.State == EntityState.Deleted)
                .ToArray();

            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                foreach (var entry in entries)
                {
                    var entityType = entry.Entity.GetType();
                    Type eventType = null;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            eventType = typeof(EntityCreated<>).MakeGenericType(entityType);
                            break;
                        case EntityState.Modified:
                            eventType = typeof(EntityUpdated<>).MakeGenericType(entityType);
                            break;
                        case EntityState.Deleted:
                            eventType = typeof(EntityDeleted<>).MakeGenericType(entityType);
                            break;
                    }

                    INotification evt = Activator.CreateInstance(eventType, new[] { entry.Entity }) as INotification;

                    if (evt != null)
                    {
                        await mediator.Publish(evt);
                    }
                }
            }
        }
    }
}
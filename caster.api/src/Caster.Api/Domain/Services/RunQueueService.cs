/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Caster.Api.Domain.Services
{
    public interface IRunQueueService : IHostedService
    {
        void Add(INotification notification);
    }

    public class RunQueueService : IRunQueueService
    {
        private readonly Container _container;
        private readonly ILogger<RunQueueService> _logger;

        private readonly BlockingCollection<INotification> _queue = new BlockingCollection<INotification>();

        public RunQueueService(Container container, ILogger<RunQueueService> logger)
        {
            _container = container;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = ExecuteAsync();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void Add(INotification notification)
        {
            _queue.Add(notification);
        }

        private async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                {
                    _ = Task.Run(async () =>
                    {
                        await this.Handle(item);
                    });
                }
            });
        }

        private async Task Handle(INotification notification)
        {
            using (var scope = AsyncScopedLifestyle.BeginScope(_container))
            {
                var mediator = scope.GetRequiredService<IMediator>();
                await mediator.Publish(notification);
            }
        }
    }
}

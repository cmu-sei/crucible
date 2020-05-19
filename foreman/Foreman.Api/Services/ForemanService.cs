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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foreman.Api.Infrastructure.Options;
using Foreman.Core;
using Foreman.Core.Models;
using Foreman.Core.Services;
using Foreman.Service;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz.Xml.JobSchedulingData20;

namespace Foreman.Api.Services
{
    internal class ForemanService : IHostedService
    {
        internal static IServiceProvider ServiceProvider { get; set; }
        internal static IServiceCollection ServiceCollection { get; set; }
        internal static IConfigurationRoot Configuration { get; set; }

        internal static ApplicationDbContext Database { get; private set; }
        internal static IOptions<ApplicationOptions> _options { get; set; }

        public ForemanService(IOptions<ApplicationOptions> options)
        {
            _options = options;
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string launch = Environment.GetEnvironmentVariable("LAUNCH_PROFILE");

            if (string.IsNullOrWhiteSpace(env))
            {
                env = "Development";
            }

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true);

            Configuration = configBuilder.Build();

            ServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IWorkOrderRepositoryService, WorkOrderRepositoryService>()
                .AddSingleton<IWebHookRepositoryService, WebHookRepositoryService>()
                .AddSingleton<ISchedulerService, SchedulerService>()
                //.AddDbContext<ApplicationDbContext>(opt => opt.UseInMemoryDatabase("foreman"))
                .AddDbContext<ApplicationDbContext>(o =>
                {
                    o.UseNpgsql(Configuration["ConnectionStrings:PostgreSQL"],
                        b => b.MigrationsAssembly("todo"));
                })
                .BuildServiceProvider();

            ServiceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Warning);

            Console.WriteLine("Starting application...");

            var s = ServiceProvider.GetService<ApplicationDbContext>();
            Seed.Run(s);

            Console.WriteLine("Database ready...");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Run();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private static async void Run()
        {
            var workorderRepositoryService = ServiceProvider.GetService<IWorkOrderRepositoryService>();
            var webHookRepositoryService = ServiceProvider.GetService<IWebHookRepositoryService>();
            var scheduleService = ServiceProvider.GetService<ISchedulerService>();

            //Console.WriteLine("Sync loop services loaded...");

            while (true)
            {
                //Console.WriteLine($"Sync loop start {DateTime.UtcNow} every {_options.Value.SyncDelayInSeconds} seconds...");

                try
                {
                    // periodically poll database
                    var ct = new CancellationToken();
                    var items = workorderRepositoryService.ListByStatus(StatusType.Submitted, ct).Result;
                    //Console.WriteLine($"Loaded {items.Count()} Submitted WorkOrders...");
                    foreach (var submittedWorkOrder in items)
                    {
                        // submit new schedule
                        WebHook webHook = null;
                        if (submittedWorkOrder.WebhookId != Guid.Empty)
                            webHook = webHookRepositoryService.GetById(submittedWorkOrder.WebhookId, ct).Result;
                        var jobKey = scheduleService.Create(submittedWorkOrder, webHook, ct);

                        // update original submittedWorkOrder
                        submittedWorkOrder.JobKey = jobKey.Id;
                        submittedWorkOrder.Status = StatusType.Active;
                        workorderRepositoryService.Update(submittedWorkOrder, ct).Wait(ct);

                        Console.WriteLine($"WorkOrder {submittedWorkOrder.Id} updated to Active {jobKey.Result}");
                    }

                    // are these jobs still running?
                    items = workorderRepositoryService.ListByStatus(StatusType.Active, ct).Result;
                    //Console.WriteLine($"Loaded {items.Count()} Active WorkOrders...");
                    foreach (var activeWorkOrder in items)
                    {
                        var o = await scheduleService.GetByName(activeWorkOrder.Id, activeWorkOrder.GroupName, ct);
                        var isDone = false;

                        if (!o.Triggers.Any())
                        {
                            if (o.IsCompleted || o.NextFireTimeUtc < DateTime.UtcNow)
                            {
                                isDone = true;
                            }
                        }
                        else
                        {
                            foreach (var t in o.Triggers)
                            {
                                if (t.FinalFireTimeUtc < DateTime.UtcNow)
                                {
                                    isDone = true;
                                }
                            }    
                        }
                        
                        if (isDone)
                        {
                            activeWorkOrder.Status = StatusType.CompletedWithSuccess;
                            await workorderRepositoryService.Update(activeWorkOrder, ct);

                            if (!string.IsNullOrEmpty(o.JobKey))
                            {
                                try
                                {
                                    scheduleService.Delete(o.JobKey, o.GroupName, ct);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Schedule delete exception {e}");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //Console.WriteLine("Sync loop end");

                await Task.Delay(new TimeSpan(0, 0, _options.Value.SyncDelayInSeconds));
            }
        }
    }
}

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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Caster.Api.Data;
using Caster.Api.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Caster.Api.Domain.Services
{
    interface IFileVersionScrubService : IHostedService
    {

    }
    public class FileVersionScrubService : IFileVersionScrubService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptionsMonitor<FileVersionScrubOptions> _fileVersionScrubOptions;
        private readonly ILogger<FileVersionScrubService> _logger;
        public FileVersionScrubService(IServiceScopeFactory serviceScopeFactory, IOptionsMonitor<FileVersionScrubOptions> fileVersionScrubOptions, ILogger<FileVersionScrubService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;           
            _fileVersionScrubOptions = fileVersionScrubOptions; 
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

        private async Task ExecuteAsync()
        {
            while (true)
            {
                try
                {
                    _logger.LogInformation($"Start scrubbing untagged file versions at {DateTime.UtcNow}");
                    await ScrubFileVersions();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception encountered while scrubbing untagged file versions.", ex);
                }
                DateTime nowDateTime = DateTime.UtcNow;
                DateTime nextCheckDate = nowDateTime.Date.AddDays(1).AddMinutes(1);
                var waitUntilTimeSpan = nextCheckDate.Subtract(nowDateTime);
                await Task.Delay(waitUntilTimeSpan);
            }
        }

        private async Task ScrubFileVersions()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var casterContext = scope.ServiceProvider.GetRequiredService<CasterContext>();
                var removeAllUntaggedOlderThanThisDate = DateTime.UtcNow.Date.AddDays(-_fileVersionScrubOptions.CurrentValue.DaysToSaveDailyUntaggedVersions);
                var saveAllUntaggedNewerThanThisDate = DateTime.UtcNow.Date.AddDays(-_fileVersionScrubOptions.CurrentValue.DaysToSaveAllUntaggedVersions);
                // remove all untagged versions older than the "days to save daily untagged versions"
                var versionsToRemove = await casterContext.FileVersions
                    .Where(v => v.TaggedById == null && v.DateSaved.Value < removeAllUntaggedOlderThanThisDate)
                    .ToArrayAsync();
                casterContext.FileVersions.RemoveRange(versionsToRemove);
                // remove all untagged versions that are not the last version of the day, but are older than the "days to save all versions"
                var versionsToKeepOnePerDay = await casterContext.FileVersions
                    .Where(v => v.DateSaved < saveAllUntaggedNewerThanThisDate && v.DateSaved >= removeAllUntaggedOlderThanThisDate)
                    .ToArrayAsync();
                versionsToRemove = versionsToKeepOnePerDay
                    .GroupBy(v => new {v.FileId, v.DateSaved.Value.Date})
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderByDescending(v => v.DateSaved).Skip(1))
                    .Where(v => v.TaggedById == null)
                    .ToArray();
                casterContext.FileVersions.RemoveRange(versionsToRemove);
                await casterContext.SaveChangesAsync();
            }
        }

    }
}

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
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Caster.Api.Data;
using Caster.Api.Domain.Events;
using Caster.Api.Domain.Models;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Caster.Api.Features.Applies.EventHandlers
{
    public class ApplyAddedHandler : INotificationHandler<ApplyAdded>
    {
        private readonly CasterContext _db;
        private readonly DbContextOptions<CasterContext> _dbOptions;
        private readonly ILogger<ApplyAddedHandler> _logger;
        private readonly ITerraformService _terraformService;
        private readonly TerraformOptions _options;
        private readonly IMediator _mediator;
        private readonly IOutputService _outputService;

        private System.Timers.Timer _timer;
        private Domain.Models.Apply _apply;
        private StringBuilder _outputBuilder = new StringBuilder();
        private bool _timerComplete = false;
        private Output _output = null;

        public ApplyAddedHandler(
            CasterContext db,
            DbContextOptions<CasterContext> dbOptions,
            ILogger<ApplyAddedHandler> logger,
            ITerraformService terraformService,
            TerraformOptions options,
            IMediator mediator,
            IOutputService outputService)
        {
            _db = db;
            _dbOptions = dbOptions;
            _logger = logger;
            _terraformService = terraformService;
            _options = options;
            _mediator = mediator;
            _outputService = outputService;
        }

        public async Task Handle(ApplyAdded notification, CancellationToken cancellationToken)
        {
            _apply = await _db.Applies
                .Include(a => a.Run)
                    .ThenInclude(r => r.Workspace)
                .SingleOrDefaultAsync(x => x.Id == notification.ApplyId);

            string workingDir = string.Empty;
            var stateRetrieved = false;

            try
            {
                _output = _outputService.GetOrAddOutput(_apply.Id);

                // Update status
                _apply.Status = Domain.Models.ApplyStatus.Applying;
                _apply.Run.Status = Domain.Models.RunStatus.Applying;
                await this.UpdateApply();

                workingDir = _apply.Run.Workspace.GetPath(_options.RootWorkingDirectory);

                _timer = new System.Timers.Timer(_options.OutputSaveInterval);
                _timer.Elapsed += OnTimedEvent;
                _timer.Start();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(ApplyAddedHandler)}.Handle");
                _apply.Status = Domain.Models.ApplyStatus.Failed;
                _apply.Run.Status = Domain.Models.RunStatus.Failed;
                await this.UpdateApply();
                return;
            }

            try
            {
                var result = _terraformService.Apply(_apply.Run.Workspace, OutputHandler);
                bool isError = result.IsError;

                lock (_apply)
                {
                    _timerComplete = true;
                    _timer.Stop();
                }

                _apply.Output = _outputBuilder.ToString();
                _apply.Status = !isError ? ApplyStatus.Applied : ApplyStatus.Failed;
                _apply.Run.Status = !isError ? RunStatus.Applied : RunStatus.Failed;

                stateRetrieved = await this.RetrieveState(workingDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in {nameof(ApplyAddedHandler)}.Handle");
            }
            finally
            {
                if (!stateRetrieved)
                {
                    _apply.Status = _apply.Status == ApplyStatus.Applied ? ApplyStatus.Applied_StateError : ApplyStatus.Failed_StateError;
                    _apply.Run.Status = _apply.Run.Status == RunStatus.Applied ? RunStatus.Applied_StateError : RunStatus.Failed_StateError;
                }

                await this.UpdateApply();
            }

            try
            {
                _output.SetCompleted();
                _outputService.RemoveOutput(_apply.Id);

                if (stateRetrieved)
                {
                    await _mediator.Publish(new ApplyCompleted(_apply.Run.Workspace));
                    _apply.Run.Workspace.CleanupFileSystem(_options.RootWorkingDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Cleaning up Apply {_apply.Id}");
            }
        }

        private async Task<bool> RetrieveState(string workingDir)
        {
            var count = 1;
            bool stateRetrieved = false;

            while (count <= _options.StateRetryCount)
            {
                try
                {
                    stateRetrieved = await _apply.Run.Workspace.RetrieveState(workingDir);

                    if (stateRetrieved)
                    {
                        await this.UpdateApply();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error Retrieving State for Workspace {_apply.Run.WorkspaceId}");
                }

                count++;
                await Task.Delay(_options.StateRetryIntervalSeconds * 1000);
            }

            return stateRetrieved;
        }

        private async Task UpdateApply()
        {
            await _db.SaveChangesAsync();
            await _mediator.Publish(new RunUpdated(_apply.RunId));
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _outputBuilder.AppendLine(e.Data);

                if (_output != null)
                {
                    _output.AddLine(e.Data);
                }
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            lock (_apply)
            {
                _timer.Stop();

                if (_timerComplete)
                {
                    return;
                }

                try
                {
                    using (var dbContext = new CasterContext(_dbOptions))
                    {
                        // Only update the Output field
                        dbContext.Applies.Attach(_apply);
                        _apply.Output = _outputBuilder.ToString();

                        dbContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Timer");
                }
                finally
                {
                    _timer.Start();
                }
            }
        }
    }
}

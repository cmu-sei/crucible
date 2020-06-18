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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Caster.Api.Data;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Hubs
{
    [Authorize(Policy = nameof(CasterClaimTypes.ContentDeveloper))]
    public class ProjectHub : Hub
    {
        private readonly IOutputService _outputService;
        private readonly CasterContext _db;

        public ProjectHub(IOutputService outputService, CasterContext db)
        {
            _outputService = outputService;
            _db = db;
        }

        public async Task JoinProject(Guid projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId.ToString());
        }

        public async Task LeaveProject(Guid projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId.ToString());
        }

        public async Task JoinWorkspace(Guid workspaceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workspaceId.ToString());
        }

        public async Task LeaveWorkspace(Guid workspaceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workspaceId.ToString());
        }

        [Authorize(Policy = nameof(CasterClaimTypes.SystemAdmin))]
        public async Task JoinWorkspacesAdmin()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, nameof(HubGroups.WorkspacesAdmin));
        }

        [Authorize(Policy = nameof(CasterClaimTypes.SystemAdmin))]
        public async Task LeaveWorkspacesAdmin()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nameof(HubGroups.WorkspacesAdmin));
        }

        #region RunOutput

        enum OutputType
        {
            Plan,
            Apply
        }

        private async IAsyncEnumerable<string> GetOutput(Guid id, OutputType type, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var output = _outputService.GetOutput(id);

            if (output == null)
            {
                string dbOutput = await this.GetDbOutput(id, type, cancellationToken);

                yield return dbOutput;
                yield break;
            }

            var resetEvent = output.Subscribe();
            var sent = string.Empty;
            bool done = false;

            do
            {
                if (output.Complete)
                {
                    done = true;
                }

                var newContent = output.Content.Substring(sent.Length);

                yield return newContent;
                sent += newContent;

                if (!done)
                {
                    try
                    {
                        await resetEvent.WaitAsync(cancellationToken);
                    }
                    catch(TaskCanceledException)
                    {
                        done = true;
                    }
                }
            }
            while (!done);

            output.Unsubscribe(resetEvent);
        }

        private async Task<string> GetDbOutput(Guid id, OutputType type, CancellationToken cancellationToken)
        {
            string dbOutput = string.Empty;

            if (type == OutputType.Plan)
            {
                dbOutput = await _db.Plans
                    .Where(p => p.Id == id)
                    .Select(p => p.Output)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            else if (type == OutputType.Apply)
            {
                dbOutput = await _db.Applies
                    .Where(p => p.Id == id)
                    .Select(p => p.Output)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return dbOutput;
        }

        public async IAsyncEnumerable<string> GetPlanOutput(Guid id, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var output in this.GetOutput(id, OutputType.Plan, cancellationToken))
            {
                yield return output;
            }
        }

        public async IAsyncEnumerable<string> GetApplyOutput(Guid id, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var output in this.GetOutput(id, OutputType.Apply, cancellationToken))
            {
                yield return output;
            }
        }

        #endregion
    }
}

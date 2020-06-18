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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Caster.Api;
using Caster.Api.Models;
using S3.Player.Api;
using S3.Player.Api.Models;
using Alloy.Api.Data.Models;

namespace Alloy.Api.Infrastructure.Extensions
{
    public static class CasterApiExtensions
    {
        public static CasterApiClient GetCasterApiClient(IHttpClientFactory httpClientFactory, string apiUrl, TokenResponse tokenResponse)
        {
            var client = ApiClientsExtensions.GetHttpClient(httpClientFactory, apiUrl, tokenResponse);
            var apiClient = new CasterApiClient(client, true);
            apiClient.BaseUri = client.BaseAddress;
            return apiClient;
        }

        public static async Task<Guid?> CreateCasterWorkspaceAsync(CasterApiClient casterApiClient, EventEntity eventEntity, Guid directoryId, string varsFileContent, bool useDynamicHost, CancellationToken ct)
        {
            try
            {
                // remove special characters from the user name, use lower case and replace spaces with underscores
                var userName = Regex.Replace(eventEntity.Username.ToLower().Replace(" ", "_"), "[@&'(\\s)<>#]", "", RegexOptions.None);
                // create the new workspace
                var workspaceCommand = new CreateWorkspaceCommand()
                {
                    Name = $"{userName}-{eventEntity.UserId.ToString()}",
                    DirectoryId = directoryId,
                    DynamicHost = useDynamicHost
                };
                var workspaceId = (await casterApiClient.CreateWorkspaceAsync(workspaceCommand, ct)).Id;
                // create the workspace variable file
                var createFileCommand = new CreateFileCommand()
                {
                    Name = $"{workspaceCommand.Name}.auto.tfvars",
                    DirectoryId = directoryId,
                    WorkspaceId = workspaceId,
                    Content = varsFileContent
                };
                await casterApiClient.CreateFileAsync(createFileCommand, ct);
                return workspaceId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<string> GetCasterVarsFileContentAsync(EventEntity eventEntity, S3PlayerApiClient playerApiClient, CancellationToken ct)
        {
            try
            {
                var varsFileContent = "";
                var view = (await playerApiClient.GetViewAsync((Guid)eventEntity.ViewId, ct)) as S3.Player.Api.Models.View;

                // TODO: exercise_id is deprecated. Remove when no longer in use
                varsFileContent = $"exercise_id = \"{view.Id}\"\r\nview_id = \"{view.Id}\"\r\nuser_id = \"{eventEntity.UserId}\"\r\nusername = \"{eventEntity.Username}\"\r\n";
                var teams = (await playerApiClient.GetViewTeamsAsync((Guid)view.Id, ct)) as IEnumerable<Team>;

                foreach (var team in teams)
                {
                    var cleanTeamName = Regex.Replace(team.Name.ToLower().Replace(" ", "_"), "[@&'(\\s)<>#]", "", RegexOptions.None);
                    varsFileContent += $"{cleanTeamName} = \"{team.Id}\"\r\n";
                }

                return varsFileContent;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static async Task<Guid?> CreateRunAsync(
            EventEntity eventEntity,
            CasterApiClient casterApiClient,
            bool isDestroy,
            CancellationToken ct)
        {
            var runCommand = new CreateRunCommand()
            {
                WorkspaceId = eventEntity.WorkspaceId,
                IsDestroy = isDestroy
            };
            try
            {
                var casterRun = await casterApiClient.CreateRunAsync(runCommand, ct);
                return casterRun.Id;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<bool> WaitForRunToBePlannedAsync(
            EventEntity eventEntity,
            CasterApiClient casterApiClient,
            int loopIntervalSeconds,
            int maxWaitMinutes,
            CancellationToken ct)
        {
            if (eventEntity.RunId == null)
            {
                return false;
            }
            var endTime = DateTime.UtcNow.AddMinutes(maxWaitMinutes);
            var status = RunStatus.Planning;
            while (status == RunStatus.Planning && DateTime.UtcNow < endTime)
            {
                var casterRun = await casterApiClient.GetRunAsync((Guid)eventEntity.RunId);
                status = casterRun.Status;
                // if not there yet, pause before the next check
                if (status == RunStatus.Planning)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(loopIntervalSeconds));
                }
            }
            if (status == RunStatus.Planned)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> ApplyRunAsync(
            EventEntity eventEntity,
            CasterApiClient casterApiClient,
            CancellationToken ct)
        {
            var initialInternalStatus = eventEntity.InternalStatus;
            // if status is Planned or Applying
            try
            {
                await casterApiClient.ApplyRunAsync((Guid)eventEntity.RunId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> DeleteCasterWorkspaceAsync(EventEntity eventEntity,
            CasterApiClient casterApiClient, TokenResponse tokenResponse, CancellationToken ct)
        {
            try
            {
                await casterApiClient.DeleteWorkspaceAsync((Guid)eventEntity.WorkspaceId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> WaitForRunToBeAppliedAsync(
            EventEntity eventEntity,
            CasterApiClient casterApiClient,
            int loopIntervalSeconds,
            int maxWaitMinutes,
            CancellationToken ct)
        {
            if (eventEntity.RunId == null)
            {
                return false;
            }
            var endTime = DateTime.UtcNow.AddMinutes(maxWaitMinutes);
            var status = ApplyStatus.Applying;
            while ((status == ApplyStatus.Applying ||
                    status == ApplyStatus.AppliedStateError ||
                    status == ApplyStatus.FailedStateError)
                    && DateTime.UtcNow < endTime)
            {
                var casterRun = await casterApiClient.GetRunAsync((Guid)eventEntity.RunId);
                status = casterRun.Status;
                // if not there yet, pause before the next check
                if (status == ApplyStatus.Applying ||
                    status == ApplyStatus.AppliedStateError ||
                    status == ApplyStatus.FailedStateError)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(loopIntervalSeconds));

                    if (status == ApplyStatus.AppliedStateError ||
                        status == ApplyStatus.FailedStateError)
                    {
                        await casterApiClient.SaveStateAsync(eventEntity.RunId.Value);
                    }
                }
            }
            if (status == ApplyStatus.Applied)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}

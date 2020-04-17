/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon� and CERT� are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<Guid?> CreateCasterWorkspaceAsync(CasterApiClient casterApiClient, ImplementationEntity implementationEntity, Guid directoryId, string varsFileContent, bool useDynamicHost, CancellationToken ct)
        {
            try
            {
                // remove special characters from the user name, use lower case and replace spaces with underscores
                var userName = Regex.Replace(implementationEntity.Username.ToLower().Replace(" ", "_"), "[@&'(\\s)<>#]", "", RegexOptions.None);
                // create the new workspace
                var workspaceCommand = new CreateWorkspaceCommand()
                {
                    Name = $"{userName}-{implementationEntity.UserId.ToString()}",
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

        public static async Task<string> GetCasterVarsFileContentAsync(ImplementationEntity implementationEntity, S3PlayerApiClient playerApiClient, CancellationToken ct)
        {
            try
            {
                var varsFileContent = "";
                var exercise = (await playerApiClient.GetExerciseAsync((Guid)implementationEntity.ExerciseId, ct)) as S3.Player.Api.Models.Exercise;

                // TODO: exercise_id is deprecated. Remove when no longer in use
                varsFileContent = $"view_id = \"{exercise.Id}\"\r\nexercise_id = \"{exercise.Id}\"\r\nuser_id = \"{implementationEntity.UserId}\"\r\nusername = \"{implementationEntity.Username}\"\r\n";
                var teams = (await playerApiClient.GetExerciseTeamsAsync((Guid)exercise.Id, ct)) as IEnumerable<Team>;

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
            ImplementationEntity implementationEntity,
            CasterApiClient casterApiClient,
            bool isDestroy,
            CancellationToken ct)
        {
            var runCommand = new CreateRunCommand()
            {
                WorkspaceId = implementationEntity.WorkspaceId,
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
            ImplementationEntity implementationEntity,
            CasterApiClient casterApiClient,
            int loopIntervalSeconds,
            int maxWaitMinutes,
            CancellationToken ct)
        {
            if (implementationEntity.RunId == null)
            {
                return false;
            }
            var endTime = DateTime.UtcNow.AddMinutes(maxWaitMinutes);
            var status = "Planning";
            while (status == "Planning" && DateTime.UtcNow < endTime)
            {
                var casterRun = await casterApiClient.GetRunAsync((Guid)implementationEntity.RunId);
                status = casterRun.Status;
                // if not there yet, pause before the next check
                if (status == "Planning")
                {
                    Thread.Sleep(TimeSpan.FromSeconds(loopIntervalSeconds));
                }
            }
            if (status == "Planned")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<bool> ApplyRunAsync(
            ImplementationEntity implementationEntity,
            CasterApiClient casterApiClient,
            CancellationToken ct)
        {
            var initialInternalStatus = implementationEntity.InternalStatus;
            // if status is Planned or Applying
            try
            {
                await casterApiClient.ApplyRunAsync((Guid)implementationEntity.RunId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> DeleteCasterWorkspaceAsync(ImplementationEntity implementationEntity,
            CasterApiClient casterApiClient, TokenResponse tokenResponse, CancellationToken ct)
        {
            try
            {
                await casterApiClient.DeleteWorkspaceAsync((Guid)implementationEntity.WorkspaceId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> WaitForRunToBeAppliedAsync(
            ImplementationEntity implementationEntity,
            CasterApiClient casterApiClient,
            int loopIntervalSeconds,
            int maxWaitMinutes,
            CancellationToken ct)
        {
            if (implementationEntity.RunId == null)
            {
                return false;
            }
            var endTime = DateTime.UtcNow.AddMinutes(maxWaitMinutes);
            var status = "Applying";
            while (status == "Applying" && DateTime.UtcNow < endTime)
            {
                var casterRun = await casterApiClient.GetRunAsync((Guid)implementationEntity.RunId);
                status = casterRun.Status;
                // if not there yet, pause before the next check
                if (status == "Applying")
                {
                    Thread.Sleep(TimeSpan.FromSeconds(loopIntervalSeconds));
                }
            }
            if (status == "Applied")
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

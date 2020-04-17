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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using S3.Player.Api;
using S3.Player.Api.Models;
using Alloy.Api.Data.Models;

namespace Alloy.Api.Infrastructure.Extensions
{
    public static class PlayerApiExtensions
    {
        public static S3PlayerApiClient GetPlayerApiClient(IHttpClientFactory httpClientFactory, string apiUrl, TokenResponse tokenResponse)
        {
            var client = ApiClientsExtensions.GetHttpClient(httpClientFactory, apiUrl, tokenResponse);
            var apiClient = new S3PlayerApiClient(client, true);
            apiClient.BaseUri = client.BaseAddress;
            return apiClient;
        }

        public static async Task<Guid?> CreatePlayerExerciseAsync(S3PlayerApiClient playerApiClient, ImplementationEntity implementationEntity, Guid parentExerciseId, CancellationToken ct)
        {
            try
            {
                var exercise = (await playerApiClient.CloneExerciseAsync(parentExerciseId, ct)) as S3.Player.Api.Models.Exercise;
                exercise.Name = $"{exercise.Name.Replace("Clone of ", "")} - {implementationEntity.Username}";
                await playerApiClient.UpdateExerciseAsync((Guid)exercise.Id, exercise, ct);
                // add user to first non-admin team
                var roles = await playerApiClient.GetRolesAsync(ct) as IEnumerable<Role>;
                var teams = (await playerApiClient.GetExerciseTeamsAsync((Guid)exercise.Id, ct)) as IEnumerable<Team>;
                foreach (var team in teams)
                {
                    if (team.Permissions.Where(p => p.Key == "ExerciseAdmin").Any())
                        continue;

                    if (team.RoleId.HasValue)
                    {
                        var role = roles.Where(r => r.Id == team.RoleId).FirstOrDefault();

                        if (role != null && role.Permissions.Where(p => p.Key == "ExerciseAdmin").Any())
                            continue;
                    }

                    await playerApiClient.AddUserToTeamAsync(team.Id.Value, implementationEntity.UserId, ct);
                }
                return exercise.Id;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<bool> DeletePlayerExerciseAsync(string playerApiUrl, Guid? exerciseId, S3PlayerApiClient playerApiClient, CancellationToken ct)
        {
            // no exercise to delete
            if (exerciseId == null)
            {
                return true;
            }
            // try to delete the exercise
            try
            {
                await playerApiClient.DeleteExerciseAsync((Guid)exerciseId, ct);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}



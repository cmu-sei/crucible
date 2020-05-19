/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using S3.Player.Api;
using S3.Player.Api.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steamfitter.Api.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<Exercise>> GetExercisesAsync(CancellationToken ct);
    }

    public class PlayerService : IPlayerService
    {
        private readonly S3PlayerApiClient _s3PlayerApiClient;
        private readonly Guid _userId;

        public PlayerService(IHttpContextAccessor httpContextAccessor, ClientOptions clientSettings)
        {
            _userId = httpContextAccessor.HttpContext.User.GetId();

            // Workaround for token bug introduced in .NET Core 2.1.  Remove in 2.2
            string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            string token = null;
            if (!string.IsNullOrEmpty(authHeader))
            {
                token = authHeader.Replace("bearer ", string.Empty).Replace("Bearer ", string.Empty);
            }

            // Go back to this when bug is fixed in .NET Core 2.2
            //var token = httpContextAccessor.HttpContext.GetTokenAsync("access_token").Result;

            _s3PlayerApiClient = new S3PlayerApiClient(new Uri(clientSettings.urls.playerApi), new TokenCredentials(token, "Bearer"));        
        }       

        public async Task<IEnumerable<Exercise>> GetExercisesAsync(CancellationToken ct)
        {
            var exercises = await _s3PlayerApiClient.GetMyExercisesAsync(ct);
            return (IEnumerable<Exercise>)exercises;
        }

    }
}


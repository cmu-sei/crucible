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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using S3.Player.Api;
using S3.Player.Api.Models;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Alloy.Api.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct);
        Task<View> CloneViewAsync(Guid viewId, CancellationToken ct);
        Task DeleteViewAsync(Guid viewId, CancellationToken ct);
        Task<IEnumerable<string>> GetUserClaimValuesAsync(CancellationToken ct);
        Task<User> GetUserAsync(CancellationToken ct);
    }

    public class PlayerService : IPlayerService 
    {
        private readonly IS3PlayerApiClient _s3PlayerApiClient;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserClaimsService _claimsService;
        private readonly ClaimsPrincipal _user;

        public PlayerService(
            IHttpContextAccessor httpContextAccessor,
            IS3PlayerApiClient s3PlayerApiClient,
            IAuthorizationService authorizationService,
            IPrincipal user,
            IUserClaimsService claimsService)
        {
            _s3PlayerApiClient = s3PlayerApiClient;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _claimsService = claimsService;
        }       

        public async Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct)
        {
            var views = await _s3PlayerApiClient.GetUserViewsAsync(_user.GetId(), ct);
            return (IEnumerable<View>)views;
        }

        public async Task<View> CloneViewAsync(Guid viewId, CancellationToken ct)
        {
            return (View) await _s3PlayerApiClient.CloneViewAsync(viewId);
        }

        public async Task DeleteViewAsync(Guid viewId, CancellationToken ct)
        {
            await _s3PlayerApiClient.DeleteViewAsync(viewId);
        }

        public async Task<IEnumerable<string>> GetUserClaimValuesAsync(CancellationToken ct)
        {
            var claimValues = new List<string>();
            var user = await _claimsService.GetClaimsPrincipal(_user.GetId(), true);
            if ((await _authorizationService.AuthorizeAsync(user, null, new SystemAdminRightsRequirement())).Succeeded) claimValues.Add("SystemAdmin");
            if ((await _authorizationService.AuthorizeAsync(user, null, new ContentDeveloperRightsRequirement())).Succeeded) claimValues.Add("ContentDeveloper");
            
            return claimValues;
        }

        public async Task<User> GetUserAsync(CancellationToken ct)
        {
            var user = (await _s3PlayerApiClient.GetUserAsync(_user.GetId())) as User;
            
            return user;
        }

    }
}


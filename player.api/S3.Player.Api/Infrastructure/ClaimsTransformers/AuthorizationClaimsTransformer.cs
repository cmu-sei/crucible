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
using S3.Player.Api.Data.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using S3.Player.Api.Extensions;
using Z.EntityFramework.Plus;
using S3.Player.Api.Data.Data.Models;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using S3.Player.Api.Options;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Services;

namespace S3.Player.Api.Infrastructure.ClaimsTransformers
{
    class AuthorizationClaimsTransformer : IClaimsTransformation
    {        
        private IUserClaimsService _claimsService;

        public AuthorizationClaimsTransformer(IUserClaimsService claimsService)
        {
            _claimsService = claimsService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var user = await _claimsService.AddUserClaims(principal, true);
            _claimsService.SetCurrentClaimsPrincipal(user);
            return user;
        }       
    }
}


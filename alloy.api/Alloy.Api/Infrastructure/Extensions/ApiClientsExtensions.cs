/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.DependencyInjection;
using Alloy.Api.Infrastructure.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Caster.Api;
using IdentityModel.Client;
using Steamfitter.Api;
using S3.Player.Api;

namespace Alloy.Api.Infrastructure.Extensions
{
    public static class ApiClientsExtensions
    {
        public static HttpClient GetHttpClient(IHttpClientFactory httpClientFactory, string apiUrl, TokenResponse tokenResponse)
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("authorization", $"{tokenResponse.TokenType} {tokenResponse.AccessToken}");
            return client;
        }

        public static async Task<TokenResponse> GetToken(IServiceScope scope)
        {
            var resourceOwnerAuthorizationOptions = scope.ServiceProvider.GetRequiredService<ResourceOwnerAuthorizationOptions>();
            var tokenResponse = await RequestTokenAsync(resourceOwnerAuthorizationOptions);
            return tokenResponse;
        }

        public static async Task<TokenResponse> RequestTokenAsync(ResourceOwnerAuthorizationOptions authorizationOptions)
        {
            var disco = await DiscoveryClient.GetAsync(authorizationOptions.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            TokenClient client = null;

            if (string.IsNullOrEmpty(authorizationOptions.ClientSecret))
            {
                client = new TokenClient(disco.TokenEndpoint, authorizationOptions.ClientId);
            }
            else
            {
                client = new TokenClient(disco.TokenEndpoint, authorizationOptions.ClientId, authorizationOptions.ClientSecret);
            }

            return await client.RequestResourceOwnerPasswordAsync(authorizationOptions.UserName, authorizationOptions.Password, authorizationOptions.Scope);
        }

    }
}



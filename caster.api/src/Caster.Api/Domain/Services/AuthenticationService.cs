/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Caster.Api.Infrastructure.Options;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Caster.Api.Domain.Services
{
    public interface IAuthenticationService
    {
        TokenResponse GetToken(CancellationToken ct = new CancellationToken());
        void InvalidateToken();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly Object _lock = new Object();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<ClientOptions> _clientOptions;
        private readonly ILogger<AuthenticationService> _logger;

        private TokenResponse _tokenResponse;

        public AuthenticationService(IHttpClientFactory httpClientFactory, IOptionsMonitor<ClientOptions> clientOptions, ILogger<AuthenticationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _clientOptions = clientOptions;
            _logger = logger;
        }

        public TokenResponse GetToken(CancellationToken ct = new CancellationToken())
        {
            if (!ValidateToken())
            {
                lock (_lock)
                {
                    // Check again so we don't renew again if 
                    // another thread already did while we were waiting on the lock
                    if (!ValidateToken())
                    {
                        _tokenResponse = RenewToken(ct);
                    }                                        
                }
            }

            return _tokenResponse;
        }

        public void InvalidateToken()
        {
            _tokenResponse = null;
        }

        private bool ValidateToken()
        {
            if (_tokenResponse == null || _tokenResponse.ExpiresIn <= _clientOptions.CurrentValue.TokenRefreshSeconds)
            {
                return false;
            }
            else
            {
                return true;
            }
        }        

        private TokenResponse RenewToken(CancellationToken ct)
        {
            try
            {
                var clientOptions = _clientOptions.CurrentValue;
                var httpClient = _httpClientFactory.CreateClient("identity");
                var response = httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = clientOptions.TokenUrl,
                    ClientId = clientOptions.ClientId,
                    Scope = clientOptions.Scope,
                    UserName = clientOptions.UserName,
                    Password = clientOptions.Password
                }, ct).Result;

                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception renewing auth token.", ex);
            }            

            return null;
        }
    }
}

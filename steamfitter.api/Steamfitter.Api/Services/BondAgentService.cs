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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using STT = System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Infrastructure.Options;

namespace Steamfitter.Api.Services
{
    public interface IBondAgentService
    {
        STT.Task<IEnumerable<BondAgent>> GetAsync(CancellationToken ct);
        STT.Task<BondAgent> GetAsync(Guid Id, CancellationToken ct);
        STT.Task<BondAgent> CreateAsync(BondAgent BondAgent, CancellationToken ct);
        STT.Task<BondAgent> UpdateAsync(Guid Id, BondAgent BondAgent, CancellationToken ct);
        STT.Task<BondAgent> DeleteAsync(Guid Id, CancellationToken ct);
    }

    public class BondAgentService : IBondAgentService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ILogger<BondAgentService> _logger;
        private BondAgentStore _bondAgentStore;

        public BondAgentService(
            IAuthorizationService authorizationService, 
            IPrincipal user, 
            IMapper mapper,
            IStackStormService stackStormService,
            BondAgentStore bondAgentStore,
            IOptions<VmTaskProcessingOptions> options,
            ILogger<BondAgentService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _bondAgentStore = bondAgentStore;
            _logger = logger;
        }

        public async STT.Task<IEnumerable<BondAgent>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            return _bondAgentStore.BondAgents.Values;
        }

        public async STT.Task<BondAgent> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            return _bondAgentStore.BondAgents[id];
        }

        public async STT.Task<BondAgent> CreateAsync(BondAgent bondAgent, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            //TODO: add permissions
            // var BondAgentAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.BondAgentAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (BondAgentAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.BondAgentAdmin.ToString()} Permission not found.");
            // end of TODO:

            if (!bondAgent.CheckinTime.HasValue)
            {
                bondAgent.CheckinTime = DateTime.UtcNow;
            }
            _bondAgentStore.BondAgents[bondAgent.VmWareUuid] = bondAgent;

            return bondAgent;
        }

        public async STT.Task<BondAgent> UpdateAsync(Guid id, BondAgent bondAgent, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var bondAgentToUpdate = _bondAgentStore.BondAgents[id];
            if (bondAgentToUpdate == null)
                throw new EntityNotFoundException<BondAgent>();
            if (!bondAgent.CheckinTime.HasValue)
            {
                bondAgent.CheckinTime = DateTime.UtcNow;
            }
            _bondAgentStore.BondAgents[id] = bondAgent;

            return bondAgent;
        }

        public async STT.Task<BondAgent> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var deletedBondAgent = new BondAgent();
            _bondAgentStore.BondAgents.Remove(id, out deletedBondAgent);

            return deletedBondAgent;
        }

        public STT.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting BondAgentService at {DateTime.UtcNow}");

            _bondAgentStore.BondAgents = new ConcurrentDictionary<Guid, BondAgent>();
            return STT.Task.CompletedTask;
        }

        public STT.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping BondAgentService at {DateTime.UtcNow}");
            return STT.Task.CompletedTask;
        }

    }


    public class BondAgentStore
    {
        public ConcurrentDictionary<Guid, BondAgent> BondAgents;

        public BondAgentStore()
        {
            BondAgents = new ConcurrentDictionary<Guid, BondAgent>();
        }

    }

}


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
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Infrastructure.Options;
using Steamfitter.Api.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Steamfitter.Api.Services
{
    public interface IExerciseAgentService
    {
        Task<IEnumerable<ExerciseAgent>> GetAsync(CancellationToken ct);
        Task<ExerciseAgent> GetAsync(Guid Id, CancellationToken ct);
        Task<ExerciseAgent> CreateAsync(ExerciseAgent ExerciseAgent, CancellationToken ct);
        Task<ExerciseAgent> UpdateAsync(Guid Id, ExerciseAgent ExerciseAgent, CancellationToken ct);
        Task<ExerciseAgent> DeleteAsync(Guid Id, CancellationToken ct);
    }

    public class ExerciseAgentService : IExerciseAgentService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ILogger<ExerciseAgentService> _logger;
        private ExerciseAgentStore _exerciseAgentStore;

        public ExerciseAgentService(
            IAuthorizationService authorizationService, 
            IPrincipal user, 
            IMapper mapper,
            IStackStormService stackStormService,
            ExerciseAgentStore exerciseAgentStore,
            IOptions<VmTaskProcessingOptions> options,
            ILogger<ExerciseAgentService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _exerciseAgentStore = exerciseAgentStore;
            _logger = logger;
        }

        public async Task<IEnumerable<ExerciseAgent>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            return _exerciseAgentStore.ExerciseAgents.Values;
        }

        public async Task<ExerciseAgent> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            return _exerciseAgentStore.ExerciseAgents[id];
        }

        public async Task<ExerciseAgent> CreateAsync(ExerciseAgent exerciseAgent, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            //TODO: add permissions
            // var ExerciseAgentAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.ExerciseAgentAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (ExerciseAgentAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ExerciseAgentAdmin.ToString()} Permission not found.");
            // end of TODO:

            if (!exerciseAgent.CheckinTime.HasValue)
            {
                exerciseAgent.CheckinTime = DateTime.UtcNow;
            }
            _exerciseAgentStore.ExerciseAgents[exerciseAgent.VmWareUuid] = exerciseAgent;

            return exerciseAgent;
        }

        public async Task<ExerciseAgent> UpdateAsync(Guid id, ExerciseAgent exerciseAgent, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var exerciseAgentToUpdate = _exerciseAgentStore.ExerciseAgents[id];
            if (exerciseAgentToUpdate == null)
                throw new EntityNotFoundException<ExerciseAgent>();
            if (!exerciseAgent.CheckinTime.HasValue)
            {
                exerciseAgent.CheckinTime = DateTime.UtcNow;
            }
            _exerciseAgentStore.ExerciseAgents[id] = exerciseAgent;

            return exerciseAgent;
        }

        public async Task<ExerciseAgent> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var deletedExerciseAgent = new ExerciseAgent();
            _exerciseAgentStore.ExerciseAgents.Remove(id, out deletedExerciseAgent);

            return deletedExerciseAgent;
        }

        public System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting ExerciseAgentService at {DateTime.UtcNow}");

            _exerciseAgentStore.ExerciseAgents = new ConcurrentDictionary<Guid, ExerciseAgent>();
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stopping ExerciseAgentService at {DateTime.UtcNow}");
            return System.Threading.Tasks.Task.CompletedTask;
        }

    }


    public class ExerciseAgentStore
    {
        public ConcurrentDictionary<Guid, ExerciseAgent> ExerciseAgents;

        public ExerciseAgentStore()
        {
            ExerciseAgents = new ConcurrentDictionary<Guid, ExerciseAgent>();
        }

    }

}


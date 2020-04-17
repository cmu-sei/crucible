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
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Services;
using Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface ISessionService
    {
        Task<IEnumerable<ViewModels.Session>> GetAsync(CancellationToken ct);
        Task<ViewModels.Session> GetAsync(Guid Id, CancellationToken ct);
        Task<ViewModels.Session> CreateAsync(ViewModels.Session Session, CancellationToken ct);
        Task<ViewModels.Session> CreateFromScenarioAsync(Guid scenarioId, CancellationToken ct);
        Task<ViewModels.Session> CreateFromSessionAsync(Guid sessionId, CancellationToken ct);
        Task<ViewModels.Session> UpdateAsync(Guid Id, ViewModels.Session Session, CancellationToken ct);
        Task<bool> DeleteAsync(Guid Id, CancellationToken ct);
        Task<ViewModels.Session> StartAsync(Guid Id, CancellationToken ct);
        Task<ViewModels.Session> PauseAsync(Guid Id, CancellationToken ct);
        Task<ViewModels.Session> ContinueAsync(Guid Id, CancellationToken ct);
        Task<ViewModels.Session> EndAsync(Guid Id, CancellationToken ct);
    }

    public class SessionService : ISessionService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly IDispatchTaskService _dispatchTaskService;
        private readonly IStackStormService _stackstormService;

        public SessionService(SteamfitterContext context,
                                IAuthorizationService authorizationService,
                                IPrincipal user,
                                IMapper mapper,
                                IDispatchTaskService dispatchTaskService,
                                IStackStormService stackstormService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _dispatchTaskService = dispatchTaskService;
            _stackstormService = stackstormService;
        }

        public async Task<IEnumerable<ViewModels.Session>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Sessions
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<Session>>(items);
        }

        public async Task<ViewModels.Session> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Sessions
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<Session>(item);
        }

        // public async Task<IEnumerable<ViewModels.Session>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        // {
        //     if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
        //         throw new ForbiddenException();

        //     var user = await _context.Users
        //         .Include(u => u.SessionMemberships)
        //             .ThenInclude(em => em.Session)                
        //         .Where(u => u.Id == userId)
        //         .SingleOrDefaultAsync(ct);

        //     if (user == null)
        //         throw new EntityNotFoundException<User>();

        //     var Sessions = user.SessionMemberships.Select(x => x.Session);

        //     return _mapper.Map<IEnumerable<ViewModels.Session>>(Sessions);
        // }
        
        public async Task<ViewModels.Session> CreateAsync(ViewModels.Session session, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            session.DateCreated = DateTime.UtcNow;
            session.CreatedBy = _user.GetId();
            var sessionEntity = Mapper.Map<SessionEntity>(session);

            //TODO: add permissions
            // var SessionAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.SessionAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (SessionAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.SessionAdmin.ToString()} Permission not found.");

            _context.Sessions.Add(sessionEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(sessionEntity.Id, ct);
        }

        public async Task<ViewModels.Session> CreateFromScenarioAsync(Guid scenarioId, CancellationToken ct)
        {
            var scenarioEntity = _context.Scenarios.Find(scenarioId);
            if (scenarioEntity == null)
                throw new EntityNotFoundException<Scenario>($"Scenario {scenarioId} was not found.");

            var sessionEntity = new SessionEntity() {
                CreatedBy = _user.GetId(),
                Name = $"From Scenario {scenarioEntity.Name}",
                Description = scenarioEntity.Description,
                OnDemand = true,
                ScenarioId = scenarioId
            };
            var durationHours = scenarioEntity.DurationHours != null ? (int)scenarioEntity.DurationHours : 720;
            sessionEntity.EndDate = sessionEntity.StartDate.AddHours(durationHours);
            _context.Sessions.Add(sessionEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the DispatchTasks
            var oldDispatchTaskEntities = _context.DispatchTasks.Where(dt => dt.ScenarioId == scenarioId && dt.TriggerTaskId == null);
            // copy the PARENT DispatchTasks
            foreach (var oldDispatchTaskEntity in oldDispatchTaskEntities)
            {
                await _dispatchTaskService.CopyAsync(oldDispatchTaskEntity.Id, sessionEntity.Id, "session", ct);
            }

            return _mapper.Map<Session>(sessionEntity);
        }

        public async Task<ViewModels.Session> CreateFromSessionAsync(Guid oldSessionId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var oldSessionEntity = _context.Sessions.Find(oldSessionId);
            if (oldSessionEntity == null)
                throw new EntityNotFoundException<Session>($"Session {oldSessionId} was not found.");

            var newSessionEntity = new SessionEntity() {
                CreatedBy = _user.GetId(),
                Name = $"Copy of {oldSessionEntity.Name}",
                Description = oldSessionEntity.Description,
                OnDemand = true,
                ScenarioId = oldSessionEntity.ScenarioId
            };

            _context.Sessions.Add(newSessionEntity);
            await _context.SaveChangesAsync(ct);

            // copy all of the DispatchTasks
            var oldDispatchTaskEntities = _context.DispatchTasks.Where(dt => dt.SessionId == oldSessionId && dt.TriggerTaskId == null);
            // copy the PARENT DispatchTasks
            foreach (var oldDispatchTaskEntity in oldDispatchTaskEntities)
            {
                await _dispatchTaskService.CopyAsync(oldDispatchTaskEntity.Id, newSessionEntity.Id, "session", ct);
            }

            return await GetAsync(newSessionEntity.Id, ct);
        }

        public async Task<ViewModels.Session> UpdateAsync(Guid id, ViewModels.Session session, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var sessionToUpdate = await _context.Sessions.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (sessionToUpdate == null)
                throw new EntityNotFoundException<Session>();

            session.DateCreated = sessionToUpdate.DateCreated;
            session.CreatedBy = sessionToUpdate.CreatedBy;
            session.DateModified = DateTime.UtcNow;
            session.ModifiedBy = _user.GetId();
            Mapper.Map(session, sessionToUpdate);

            _context.Sessions.Update(sessionToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(sessionToUpdate, session);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var SessionToDelete = await _context.Sessions.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (SessionToDelete == null)
                throw new EntityNotFoundException<Session>();

            _context.Sessions.Remove(SessionToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<ViewModels.Session> StartAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var session = await _context.Sessions
                .SingleAsync(o => o.Id == id, ct);
            var dateTimeStart = DateTime.UtcNow;
            session.DateModified = dateTimeStart;
            session.ModifiedBy = _user.GetId();
            session.StartDate = dateTimeStart;
            session.Status = SessionStatus.active;

            _context.Sessions.Update(session);
            await _context.SaveChangesAsync(ct);
            // TODO:  create a better way to do this that doesn't require getting ALL of the VM's
            // We just need to grab all of the VM's from the session exercise
            await _stackstormService.GetStackstormVms();
            var dispatchTasks = await _dispatchTaskService.GetBySessionIdAsync(session.Id, ct);
            foreach (var dispatchTask in dispatchTasks)
            {
                if (dispatchTask.TriggerTaskId is null && dispatchTask.TriggerCondition != TaskTrigger.Manual)
                {
                    await _dispatchTaskService.ExecuteAsync(dispatchTask.Id, ct);
                }
            }

            return _mapper.Map<Session>(session);
        }

        public async Task<ViewModels.Session> PauseAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<ViewModels.Session> ContinueAsync(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<ViewModels.Session> EndAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var session = await _context.Sessions
                .SingleAsync(o => o.Id == id, ct);
            var endDateTime = DateTime.UtcNow;
            session.DateModified = endDateTime;
            session.ModifiedBy = _user.GetId();
            session.Status = SessionStatus.ended;
            session.EndDate = endDateTime;

            _context.Sessions.Update(session);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map<Session>(session);
        }

    }
}


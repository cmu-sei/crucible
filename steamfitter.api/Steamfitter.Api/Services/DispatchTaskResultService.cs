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
using Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IDispatchTaskResultService
    {
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetAsync(CancellationToken ct);
        Task<ViewModels.DispatchTaskResult> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByVmIdAsync(Guid vmId, CancellationToken ct);
        Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByDispatchTaskIdAsync(Guid dispatchTaskId, CancellationToken ct);
        Task<ViewModels.DispatchTaskResult> CreateAsync(ViewModels.DispatchTaskResult dispatchTaskResult, CancellationToken ct);
        Task<ViewModels.DispatchTaskResult> UpdateAsync(Guid id, ViewModels.DispatchTaskResult dispatchTaskResult, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class DispatchTaskResultService : IDispatchTaskResultService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;

        public DispatchTaskResultService(SteamfitterContext context,
                                            IAuthorizationService authorizationService,
                                            IPrincipal user,
                                            IMapper mapper)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.DispatchTaskResults
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<DispatchTaskResult>>(items);
        }

        public async Task<ViewModels.DispatchTaskResult> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.DispatchTaskResults
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<DispatchTaskResult>(item);
        }

        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskIdList = _context.DispatchTasks.Where(dt => dt.SessionId == sessionId).Select(dt => dt.Id.ToString()).ToList();
            var dispatchTaskResults = _context.DispatchTaskResults.Where(dt => dispatchTaskIdList.Contains(dt.DispatchTaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.DispatchTaskResult>>(dispatchTaskResults);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var sessionIdList = _context.Sessions.Where(s => s.ExerciseId == exerciseId).Select(s => s.Id.ToString()).ToList();
            var dispatchTaskIdList = _context.DispatchTasks.Where(dt => sessionIdList.Contains(dt.SessionId.ToString())).Select(dt => dt.Id.ToString()).ToList();
            var dispatchTaskResults = _context.DispatchTaskResults.Where(r => dispatchTaskIdList.Contains(r.DispatchTaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.DispatchTaskResult>>(dispatchTaskResults);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskIdList = _context.DispatchTasks.Where(dt => dt.UserId == userId && dt.ScenarioId == null && dt.SessionId == null).Select(dt => dt.Id.ToString()).ToList();
            var dispatchTaskResults = _context.DispatchTaskResults.Where(r => dispatchTaskIdList.Contains(r.DispatchTaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.DispatchTaskResult>>(dispatchTaskResults);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByVmIdAsync(Guid vmId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskResults = _context.DispatchTaskResults.Where(dt => dt.VmId == vmId);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTaskResult>>(dispatchTaskResults);
        }
        
        public async Task<IEnumerable<ViewModels.DispatchTaskResult>> GetByDispatchTaskIdAsync(Guid dispatchTaskId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskResults = _context.DispatchTaskResults.Where(dt => dt.DispatchTaskId == dispatchTaskId);

            return _mapper.Map<IEnumerable<ViewModels.DispatchTaskResult>>(dispatchTaskResults);
        }
        
        public async Task<ViewModels.DispatchTaskResult> CreateAsync(ViewModels.DispatchTaskResult dispatchTaskResult, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            dispatchTaskResult.DateCreated = DateTime.UtcNow;
            dispatchTaskResult.CreatedBy = _user.GetId();
            var dispatchTaskResultEntity = Mapper.Map<DispatchTaskResultEntity>(dispatchTaskResult);

            //TODO: add permissions
            // var DispatchTaskResultAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.DispatchTaskResultAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (DispatchTaskResultAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.DispatchTaskResultAdmin.ToString()} Permission not found.");

            _context.DispatchTaskResults.Add(dispatchTaskResultEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(dispatchTaskResultEntity.Id, ct);
        }

        public async Task<ViewModels.DispatchTaskResult> UpdateAsync(Guid id, ViewModels.DispatchTaskResult dispatchTaskResult, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var dispatchTaskResultToUpdate = await _context.DispatchTaskResults.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (dispatchTaskResultToUpdate == null)
                throw new EntityNotFoundException<DispatchTaskResult>();

            dispatchTaskResult.DateCreated = dispatchTaskResultToUpdate.DateCreated;
            dispatchTaskResult.CreatedBy = dispatchTaskResultToUpdate.CreatedBy;
            dispatchTaskResult.DateModified = DateTime.UtcNow;
            dispatchTaskResult.ModifiedBy = _user.GetId();
            Mapper.Map(dispatchTaskResult, dispatchTaskResultToUpdate);

            _context.DispatchTaskResults.Update(dispatchTaskResultToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(dispatchTaskResultToUpdate, dispatchTaskResult);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var DispatchTaskResultToDelete = await _context.DispatchTaskResults.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (DispatchTaskResultToDelete == null)
                throw new EntityNotFoundException<DispatchTaskResult>();

            _context.DispatchTaskResults.Remove(DispatchTaskResultToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

    }
}


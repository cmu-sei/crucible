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
using STT = System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using SAVM = Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IResultService
    {
        STT.Task<IEnumerable<ViewModels.Result>> GetAsync(CancellationToken ct);
        STT.Task<ViewModels.Result> GetAsync(Guid id, CancellationToken ct);
        STT.Task<IEnumerable<ViewModels.Result>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct);
        STT.Task<IEnumerable<ViewModels.Result>> GetByViewIdAsync(Guid viewId, CancellationToken ct);
        STT.Task<IEnumerable<ViewModels.Result>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        STT.Task<IEnumerable<ViewModels.Result>> GetByVmIdAsync(Guid vmId, CancellationToken ct);
        STT.Task<IEnumerable<ViewModels.Result>> GetByTaskIdAsync(Guid taskId, CancellationToken ct);
        STT.Task<ViewModels.Result> CreateAsync(ViewModels.Result result, CancellationToken ct);
        STT.Task<ViewModels.Result> UpdateAsync(Guid id, ViewModels.Result result, CancellationToken ct);
        STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class ResultService : IResultService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;

        public ResultService(SteamfitterContext context,
                                            IAuthorizationService authorizationService,
                                            IPrincipal user,
                                            IMapper mapper)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
        }

        public async STT.Task<IEnumerable<ViewModels.Result>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Results
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<SAVM.Result>>(items);
        }

        public async STT.Task<ViewModels.Result> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Results
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<SAVM.Result>(item);
        }

        public async STT.Task<IEnumerable<ViewModels.Result>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskIdList = _context.Tasks.Where(dt => dt.ScenarioId == scenarioId).Select(dt => dt.Id.ToString()).ToList();
            var results = _context.Results.Where(dt => taskIdList.Contains(dt.TaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.Result>>(results);
        }
        
        public async STT.Task<IEnumerable<ViewModels.Result>> GetByViewIdAsync(Guid viewId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var scenarioIdList = _context.Scenarios.Where(s => s.ViewId == viewId).Select(s => s.Id.ToString()).ToList();
            var taskIdList = _context.Tasks.Where(dt => scenarioIdList.Contains(dt.ScenarioId.ToString())).Select(dt => dt.Id.ToString()).ToList();
            var results = _context.Results.Where(r => taskIdList.Contains(r.TaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.Result>>(results);
        }
        
        public async STT.Task<IEnumerable<ViewModels.Result>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var taskIdList = _context.Tasks.Where(dt => dt.UserId == userId).Select(dt => dt.Id.ToString()).ToList();
            var results = _context.Results.Where(r => taskIdList.Contains(r.TaskId.ToString()));

            return _mapper.Map<IEnumerable<ViewModels.Result>>(results.OrderByDescending(r => r.StatusDate));
        }
        
        public async STT.Task<IEnumerable<ViewModels.Result>> GetByVmIdAsync(Guid vmId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var results = _context.Results.Where(dt => dt.VmId == vmId);

            return _mapper.Map<IEnumerable<ViewModels.Result>>(results);
        }
        
        public async STT.Task<IEnumerable<ViewModels.Result>> GetByTaskIdAsync(Guid taskId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var results = _context.Results.Where(dt => dt.TaskId == taskId);

            return _mapper.Map<IEnumerable<ViewModels.Result>>(results);
        }
        
        public async STT.Task<ViewModels.Result> CreateAsync(ViewModels.Result result, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            result.DateCreated = DateTime.UtcNow;
            result.CreatedBy = _user.GetId();
            var resultEntity = _mapper.Map<ResultEntity>(result);

            //TODO: add permissions
            // var ResultAdminPermission = await _context.Permissions
            //     .Where(p => p.Key == PlayerClaimTypes.ResultAdmin.ToString())
            //     .FirstOrDefaultAsync();

            // if (ResultAdminPermission == null)
            //     throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ResultAdmin.ToString()} Permission not found.");

            _context.Results.Add(resultEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(resultEntity.Id, ct);
        }

        public async STT.Task<ViewModels.Result> UpdateAsync(Guid id, ViewModels.Result result, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var resultToUpdate = await _context.Results.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (resultToUpdate == null)
                throw new EntityNotFoundException<SAVM.Result>();

            result.DateCreated = resultToUpdate.DateCreated;
            result.CreatedBy = resultToUpdate.CreatedBy;
            result.DateModified = DateTime.UtcNow;
            result.ModifiedBy = _user.GetId();
            _mapper.Map(result, resultToUpdate);

            _context.Results.Update(resultToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(resultToUpdate, result);
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var ResultToDelete = await _context.Results.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (ResultToDelete == null)
                throw new EntityNotFoundException<SAVM.Result>();

            _context.Results.Remove(ResultToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

    }
}


/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Infrastructure.Extensions;
using S3.Vm.Api.Infrastructure.Exceptions;
using S3.VM.Api.Data.Contexts;
using S3.VM.Api.Data.Models;
using S3.VM.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace S3.VM.Api.Services
{
    public interface IVmService
    {
        Task<VmSummary[]> GetAllAsync(CancellationToken ct);
        Task<ViewModels.Vm> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ViewModels.Vm>> GetByTeamIdAsync(Guid teamId, string name, bool includePersonal, bool onlyMine, CancellationToken ct);
        Task<IEnumerable<ViewModels.Vm>> GetByExerciseIdAsync(Guid exerciseId, string name, bool includePersonal, bool onlyMine, CancellationToken ct);
        Task<ViewModels.Vm> CreateAsync(VmCreateForm form, CancellationToken ct);
        Task<ViewModels.Vm> UpdateAsync(Guid id, VmUpdateForm form, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
        Task<bool> AddToTeamAsync(Guid vmId, Guid teamId, CancellationToken ct);
        Task<bool> RemoveFromTeamAsync(Guid vmId, Guid teamId, CancellationToken ct);
    }

    public class VmService : IVmService
    {
        private readonly Context _context;
        private readonly IPlayerService _playerService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;

        public VmService(Context context, IPlayerService playerService, IPrincipal user, IMapper mapper)
        {
            _context = context;
            _playerService = playerService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
        }

        public async Task<VmSummary[]> GetAllAsync(CancellationToken ct)
        {
            if (!(await _playerService.IsSystemAdmin(ct)))
                throw new ForbiddenException();

            var vms = await _context.Vms
                .ProjectTo<VmSummary>()
                .ToArrayAsync(ct);

            return vms;
        }

        public async Task<ViewModels.Vm> GetAsync(Guid id, CancellationToken ct)
        {
            var vmEntity = await _context.Vms
                .Include(v => v.VmTeams)
                .Where(v => v.Id == id)
                .SingleOrDefaultAsync(ct);

            if (vmEntity == null)
                return null;

            var teamIds = vmEntity.VmTeams.Select(x => x.TeamId);

            if (!(await _playerService.CanAccessTeamsAsync(teamIds, ct)))
                throw new ForbiddenException();

            if(vmEntity.UserId.HasValue && vmEntity.UserId != _user.GetId() && !(await _playerService.CanManageTeamsAsync(teamIds, false, ct)))
                throw new ForbiddenException("This machine belongs to another user");

            var model = _mapper.Map<ViewModels.Vm>(vmEntity);
            model.CanAccessNicConfiguration = await _playerService.CanManageTeamsAsync(teamIds, false, ct);

            var teamId = await _playerService.GetPrimaryTeamByExerciseIdAsync(model.ExerciseId, ct);
            model.TeamId = teamId;
            return model;
        }

        public async Task<IEnumerable<ViewModels.Vm>> GetByTeamIdAsync(Guid teamId, string name, bool includePersonal, bool onlyMine, CancellationToken ct)
        {
            if (!(await _playerService.CanAccessTeamAsync(teamId, ct)))
                throw new ForbiddenException();

            var vmQuery = _context.VmTeams
                .Where(v => v.TeamId == teamId)
                .Select(v => v.Vm)
                .Distinct();

            if (onlyMine)
            {
                vmQuery = vmQuery.Where(v => v.UserId.HasValue && v.UserId == _user.GetId());
            }
            else if (!includePersonal)
            {
                vmQuery = vmQuery.Where(v => !v.UserId.HasValue);
            }                            

            if (!string.IsNullOrEmpty(name))
                vmQuery = vmQuery.Where(v => v.Name == name); 
            // order the vms by name honoring trailing number as a number (i.e. abc1, abc2, abc10, abc11)
            vmQuery = sortVmsByNumber(vmQuery);
            var vmList = await vmQuery.ToListAsync(ct);

            if(includePersonal && !onlyMine)
            {
                var personalVms = vmList.Where(v => v.UserId.HasValue).ToList();

                if (personalVms.Any())
                {
                    if (!(await _playerService.CanManageTeamAsync(teamId, ct)))
                    {
                        foreach (var userVm in personalVms)
                        {
                            if (userVm.UserId.Value != _user.GetId())
                            {
                                vmList.Remove(userVm);
                            }
                        }
                    }
                }
            }            

            return _mapper.Map<IEnumerable<ViewModels.Vm>>(vmList);
        }

        public async Task<IEnumerable<ViewModels.Vm>> GetByExerciseIdAsync(Guid exerciseId, string name, bool includePersonal, bool onlyMine, CancellationToken ct)
        {
            List<VmEntity> vmList = new List<VmEntity>();
            var teams = await _playerService.GetTeamsByExerciseIdAsync(exerciseId, ct);
            var teamIds = teams.Select(t => t.Id.Value);

            if (onlyMine)
            {
                var vmQuery = _context.VmTeams
                .Include(v => v.Vm)
                .Where(v => teamIds.Contains(v.TeamId))
                .Where(v => v.Vm.UserId.HasValue && v.Vm.UserId == _user.GetId());

                var vmTeams = await vmQuery.ToListAsync();
                vmList = vmTeams.Select(v => v.Vm).Distinct().ToList();

                if (vmList.Count > 1)
                {
                    // Order by vm on user's primary team, since workstation app only looks at first result currently
                    var primaryTeam = teams.FirstOrDefault(t => t.IsPrimary.Value);

                    if (primaryTeam != null)
                    {
                        vmList = vmList.OrderByDescending(v => v.VmTeams.Select(x => x.TeamId).Contains(primaryTeam.Id.Value)).ToList();
                    }
                }
            }     
            else
            {
                var vmQuery = _context.VmTeams
                .Where(v => teamIds.Contains(v.TeamId))
                .Select(v => v.Vm)
                .Distinct();

                if (!includePersonal)
                {
                    vmQuery = vmQuery.Where(v => !v.UserId.HasValue);
                }

                if (!string.IsNullOrEmpty(name))
                    vmQuery = vmQuery.Where(v => v.Name == name);

                // order the vms by name honoring trailing number as a number (i.e. abc1, abc2, abc10, abc11)
                vmQuery = sortVmsByNumber(vmQuery);
                vmList = await vmQuery.ToListAsync(ct);

                if (includePersonal && !onlyMine)
                {
                    var personalVms = vmList.Where(v => v.UserId.HasValue).ToList();

                    if (personalVms.Any())
                    {
                        if (!(await _playerService.CanManageTeamsAsync(teamIds, false, ct)))
                        {
                            foreach (var userVm in personalVms)
                            {
                                if (userVm.UserId.Value != _user.GetId())
                                {
                                    vmList.Remove(userVm);
                                }
                            }
                        }
                    }
                }
            }            

            return _mapper.Map<IEnumerable<ViewModels.Vm>>(vmList);
        }

        public async Task<ViewModels.Vm> CreateAsync(VmCreateForm form, CancellationToken ct)
        {
            if (_context.Vms.Where(v => v.Id == form.Id).Any())
            {
                throw new ForbiddenException("Vm already exists");
            }

            var vmEntity = _mapper.Map<VmEntity>(form);
            var formTeams = vmEntity.VmTeams.Select(v => v.TeamId).Distinct();

            if (!formTeams.Any())
                throw new ForbiddenException("Must include at least 1 team");

            if (!(await _playerService.CanManageTeamsAsync(formTeams, true, ct)))
                throw new ForbiddenException();

            var teamList = await _context.Teams
                .Where(t => formTeams.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(ct);

            foreach(var vmTeam in vmEntity.VmTeams)
            {                
                if(!teamList.Contains(vmTeam.TeamId))
                {
                    _context.Teams.Add(new TeamEntity() { Id = vmTeam.TeamId });
                }
            }

            _context.Vms.Add(vmEntity);
            await _context.SaveChangesAsync(ct);
            
            return _mapper.Map<ViewModels.Vm>(vmEntity);
        }

        public async Task<ViewModels.Vm> UpdateAsync(Guid id, VmUpdateForm form, CancellationToken ct)
        {
            var vmEntity = await _context.Vms.Where(v => v.Id == id).SingleOrDefaultAsync(ct);

            if (vmEntity == null)
                throw new EntityNotFoundException<ViewModels.Vm>();

            var teams = vmEntity.VmTeams.Select(v => v.TeamId).Distinct();

            if (!(await _playerService.CanManageTeamsAsync(teams, false, ct)))
                throw new ForbiddenException();

            vmEntity = _mapper.Map(form, vmEntity);

            _context.Vms.Update(vmEntity);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map<ViewModels.Vm>(vmEntity);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var vmEntity = await _context.Vms.Where(v => v.Id == id).SingleOrDefaultAsync(ct);

            if (vmEntity == null)
                throw new EntityNotFoundException<ViewModels.Vm>();

            var teams = vmEntity.VmTeams.Select(v => v.TeamId).Distinct();

            if (!(await _playerService.CanManageTeamsAsync(teams, false, ct)))
                throw new ForbiddenException();

            _context.Vms.Remove(vmEntity);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> AddToTeamAsync(Guid vmId, Guid teamId, CancellationToken ct)
        {
            var vm = await _context.Vms.SingleOrDefaultAsync(v => v.Id == vmId, ct);

            if (vm == null)
                throw new EntityNotFoundException<ViewModels.Vm>();

            if (!(await _playerService.CanManageTeamAsync(teamId, ct)))
                throw new ForbiddenException();

            var team = await _context.Teams.SingleOrDefaultAsync(t => t.Id == teamId, ct);

            if (team == null)
            {
                TeamEntity te = new TeamEntity { Id = teamId };
                _context.Teams.Add(te);                
            }

            var vmteam = await _context.VmTeams.SingleOrDefaultAsync(vt => vt.VmId == vmId && vt.TeamId == teamId);

            if (vmteam != null)
                return true;

            VmTeamEntity entity = new VmTeamEntity { VmId = vmId, TeamId = teamId };
            _context.VmTeams.Add(entity);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> RemoveFromTeamAsync(Guid vmId, Guid teamId, CancellationToken ct)
        {
            if (!(await _playerService.CanManageTeamAsync(teamId, ct)))
                throw new ForbiddenException();

            var vmteam = await _context.VmTeams.SingleOrDefaultAsync(vt => vt.VmId == vmId && vt.TeamId == teamId);
            var numTeams = await _context.VmTeams.Where(vt => vt.VmId == vmId).CountAsync();

            if (vmteam == null)
                return true;

            if (numTeams == 1)
                throw new ForbiddenException("Vm must be on at least one team");

            _context.VmTeams.Remove(vmteam);
            await _context.SaveChangesAsync(ct);            

            return true;
        }

        // order the vms by name honoring trailing number as a number (i.e. abc1, abc2, abc10, abc11)
        private IQueryable<VmEntity> sortVmsByNumber(IQueryable<VmEntity> vmQuery)
        {
            var numchars = new char[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
            return vmQuery
            .OrderBy(v => v.Name.TrimEnd(numchars))
            .ThenBy(v => v.Name.TrimEnd(numchars).Length < v.Name.Length ? 
                            int.Parse(v.Name.Substring(v.Name.TrimEnd(numchars).Length)) : 0);
        }
    }
}


/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Extensions;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace S3.Player.Api.Services
{
    public interface IExerciseService
    {
        Task<IEnumerable<ViewModels.Exercise>> GetAsync(CancellationToken ct);
        Task<ViewModels.Exercise> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<ViewModels.Exercise>> GetByUserIdAsync(Guid userId, CancellationToken ct);
        Task<ViewModels.Exercise> CreateAsync(ViewModels.Exercise exercise, CancellationToken ct);
        Task<Exercise> CloneAsync(Guid id, CancellationToken ct);
        Task<ViewModels.Exercise> UpdateAsync(Guid id, ViewModels.Exercise exercise, CancellationToken ct);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    }

    public class ExerciseService : IExerciseService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private IUserClaimsService _claimsService;

        public ExerciseService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user, IMapper mapper, IUserClaimsService claimsService)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _claimsService = claimsService;
        }

        public async Task<IEnumerable<ViewModels.Exercise>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.Exercises
                .ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<Exercise>>(items);
        }

        public async Task<ViewModels.Exercise> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded &&
                !(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseMemberRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Exercises
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return _mapper.Map<Exercise>(item);
        }

        public async Task<IEnumerable<ViewModels.Exercise>> GetByUserIdAsync(Guid userId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new SameUserRequirement(userId))).Succeeded)
                throw new ForbiddenException();

            var user = await _context.Users
                .Include(u => u.ExerciseMemberships)
                    .ThenInclude(em => em.Exercise)                
                .Where(u => u.Id == userId)
                .SingleOrDefaultAsync(ct);

            if (user == null)
                throw new EntityNotFoundException<User>();

            var exercises = user.ExerciseMemberships.Select(x => x.Exercise);

            return _mapper.Map<IEnumerable<ViewModels.Exercise>>(exercises);
        }
        
        public async Task<ViewModels.Exercise> CreateAsync(ViewModels.Exercise exercise, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseCreationRequirement())).Succeeded)
                throw new ForbiddenException();

            var exerciseEntity = Mapper.Map<ExerciseEntity>(exercise);

            var exerciseAdminPermission = await _context.Permissions
                .Where(p => p.Key == PlayerClaimTypes.ExerciseAdmin.ToString())
                .FirstOrDefaultAsync(ct);

            if (exerciseAdminPermission == null)
                throw new EntityNotFoundException<Permission>($"{PlayerClaimTypes.ExerciseAdmin.ToString()} Permission not found.");

            var userId = _user.GetId();

            // Create an Admin team with the caller as a member
            var teamEntity = new TeamEntity() { Name = "Admin" };
            teamEntity.Permissions.Add(new TeamPermissionEntity() { Permission = exerciseAdminPermission });

            var exerciseMembershipEntity = new ExerciseMembershipEntity { Exercise = exerciseEntity, UserId = userId };            
            exerciseEntity.Teams.Add(teamEntity);
            exerciseEntity.Memberships.Add(exerciseMembershipEntity);

            _context.Exercises.Add(exerciseEntity);
            await _context.SaveChangesAsync(ct);

            var teamMembershipEntity = new TeamMembershipEntity { Team = teamEntity, UserId = userId, ExerciseMembership = exerciseMembershipEntity };
            exerciseMembershipEntity.PrimaryTeamMembership = teamMembershipEntity;
            _context.TeamMemberships.Add(teamMembershipEntity);
            _context.ExerciseMemberships.Update(exerciseMembershipEntity);
            await _context.SaveChangesAsync(ct);

            return await GetAsync(exerciseEntity.Id, ct);
        }

        public async Task<Exercise> CloneAsync(Guid idToBeCloned, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseCreationRequirement())).Succeeded)
                throw new ForbiddenException();

            var exercise = await _context.Exercises
                .Include(o => o.Teams)
                    .ThenInclude(o => o.Applications)
                .Include(o => o.Teams)
                    .ThenInclude(o => o.Permissions)
                .Include(o => o.Applications)
                .SingleOrDefaultAsync(o => o.Id == idToBeCloned, ct);

            var newExercise = exercise.Clone();
            newExercise.Name = $"Clone of {newExercise.Name}";

            //copy exercise applications
            foreach (var application in exercise.Applications)
            {
                var newApplication = application.Clone();
                newExercise.Applications.Add(newApplication);
            }

            //copy teams
            foreach (var team in exercise.Teams)
            {
                var newTeam = team.Clone();

                //copy team applications
                foreach (var applicationInstance in team.Applications)
                {
                    var newApplicationInstance = applicationInstance.Clone();
               
                    var application = exercise.Applications.FirstOrDefault(o => o.Id == applicationInstance.ApplicationId);
                    var newApplication = newExercise.Applications.FirstOrDefault(o => application != null && o.Name == application.Name);

                    newApplicationInstance.Application = newApplication;

                    newTeam.Applications.Add(newApplicationInstance);
                }

                //copy team permissions
                foreach (var permission in team.Permissions)
                {
                    var newPermission = new TeamPermissionEntity(newTeam.Id, permission.PermissionId);
                    newTeam.Permissions.Add(newPermission);
                }

                newExercise.Teams.Add(newTeam);
            }

            _context.Add(newExercise);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map<Exercise>(newExercise);
        }

        public async Task<ViewModels.Exercise> UpdateAsync(Guid id, ViewModels.Exercise exercise, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var exerciseToUpdate = await _context.Exercises.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (exerciseToUpdate == null)
                throw new EntityNotFoundException<Exercise>();

            Mapper.Map(exercise, exerciseToUpdate);

            _context.Exercises.Update(exerciseToUpdate);
            await _context.SaveChangesAsync(ct);

            return _mapper.Map(exerciseToUpdate, exercise);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ExerciseAdminRequirement(id))).Succeeded)
                throw new ForbiddenException();

            var exerciseToDelete = await _context.Exercises.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (exerciseToDelete == null)
                throw new EntityNotFoundException<Exercise>();

            _context.Exercises.Remove(exerciseToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }
    }
}


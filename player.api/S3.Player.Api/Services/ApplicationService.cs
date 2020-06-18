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
using S3.Player.Api.Data.Data;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Infrastructure.Exceptions;
using S3.Player.Api.ViewModels;
using Z.EntityFramework.Plus;

namespace S3.Player.Api.Services
{
    public interface IApplicationService
    {
        // Application Templates
        Task<IEnumerable<ViewModels.ApplicationTemplate>> GetTemplatesAsync(CancellationToken ct);
        Task<ViewModels.ApplicationTemplate> GetTemplateAsync(Guid id, CancellationToken ct);
        Task<ViewModels.ApplicationTemplate> CreateTemplateAsync(ViewModels.ApplicationTemplateForm form, CancellationToken ct);
        Task<ViewModels.ApplicationTemplate> UpdateTemplateAsync(Guid id, ViewModels.ApplicationTemplateForm form, CancellationToken ct);
        Task<bool> DeleteTemplateAsync(Guid id, CancellationToken ct);

        // Applications
        Task<IEnumerable<ViewModels.Application>> GetApplicationsByViewAsync(Guid viewId, CancellationToken ct);
        Task<ViewModels.Application> GetApplicationAsync(Guid id, CancellationToken ct);
        Task<ViewModels.Application> CreateApplicationAsync(Guid viewId, ViewModels.Application application, CancellationToken ct);
        Task<ViewModels.Application> UpdateApplicationAsync(Guid id, ViewModels.Application application, CancellationToken ct);
        Task<bool> DeleteApplicationAsync(Guid id, CancellationToken ct);

        // Application Instances
        Task<IEnumerable<ViewModels.ApplicationInstance>> GetInstancesByTeamAsync(Guid teamId, CancellationToken ct);
        Task<ViewModels.ApplicationInstance> GetInstanceAsync(Guid id, CancellationToken ct);
        Task<ViewModels.ApplicationInstance> CreateInstanceAsync(Guid teamId, ViewModels.ApplicationInstanceForm instance, CancellationToken ct);
        Task<ViewModels.ApplicationInstance> UpdateInstanceAsync(Guid id, ViewModels.ApplicationInstanceForm instance, CancellationToken ct);
        Task<bool> DeleteInstanceAsync(Guid id, CancellationToken ct);
    }

    public class ApplicationService : IApplicationService
    {
        private readonly PlayerContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;

        public ApplicationService(PlayerContext context, IAuthorizationService authorizationService, IPrincipal user)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
        }

        #region Application Templates

        public async Task<IEnumerable<ViewModels.ApplicationTemplate>> GetTemplatesAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.ApplicationTemplates
                .ProjectTo<ViewModels.ApplicationTemplate>()
                .ToListAsync(ct);

            return items;
        }

        public async Task<ViewModels.ApplicationTemplate> GetTemplateAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.ApplicationTemplates
                .ProjectTo<ViewModels.ApplicationTemplate>()
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            return item;
        }

        public async Task<ViewModels.ApplicationTemplate> CreateTemplateAsync(ViewModels.ApplicationTemplateForm form, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var templateEntity = Mapper.Map<ApplicationTemplateEntity>(form);

            _context.ApplicationTemplates.Add(templateEntity);
            await _context.SaveChangesAsync(ct);

            return Mapper.Map<ViewModels.ApplicationTemplate>(templateEntity);
        }

        public async Task<ViewModels.ApplicationTemplate> UpdateTemplateAsync(Guid id, ViewModels.ApplicationTemplateForm form, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var templateToUpdate = await _context.ApplicationTemplates.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (templateToUpdate == null)
                throw new EntityNotFoundException<ApplicationTemplate>();

            Mapper.Map(form, templateToUpdate);

            _context.ApplicationTemplates.Update(templateToUpdate);
            await _context.SaveChangesAsync(ct);

            return await GetTemplateAsync(templateToUpdate.Id, ct);
        }

        public async Task<bool> DeleteTemplateAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new FullRightsRequirement())).Succeeded)
                throw new ForbiddenException();

            var templateToDelete = await _context.ApplicationTemplates.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (templateToDelete == null)
                throw new EntityNotFoundException<ApplicationTemplate>();

            _context.ApplicationTemplates.Remove(templateToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        #endregion

        #region Applications

        public async Task<IEnumerable<ViewModels.Application>> GetApplicationsByViewAsync(Guid viewId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(viewId))).Succeeded)
                throw new ForbiddenException();

            var view = await _context.Views
                .Include(e => e.Applications)
                .Where(e => e.Id == viewId)
                .SingleOrDefaultAsync(ct);

            if (view == null)
                throw new EntityNotFoundException<View>();

            return Mapper.Map<IEnumerable<ViewModels.Application>>(view.Applications);
        }

        public async Task<ViewModels.Application> GetApplicationAsync(Guid id, CancellationToken ct)
        {
            var item = await _context.Applications
                .ProjectTo<ViewModels.Application>()
                .SingleOrDefaultAsync(o => o.Id == id, ct);

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(item.ViewId))).Succeeded)
                throw new ForbiddenException();

            return item;
        }

        public async Task<ViewModels.Application> CreateApplicationAsync(Guid viewId, ViewModels.Application application, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(viewId))).Succeeded)
                throw new ForbiddenException();

            var viewExists = await _context.Views.Where(e => e.Id == viewId).AnyAsync(ct);

            if(!viewExists)
                throw new EntityNotFoundException<View>();

            var applicationEntity = Mapper.Map<ApplicationEntity>(application);

            _context.Applications.Add(applicationEntity);
            await _context.SaveChangesAsync(ct);

            application = Mapper.Map<ViewModels.Application>(applicationEntity);

            return application;
        }

        public async Task<ViewModels.Application> UpdateApplicationAsync(Guid id, ViewModels.Application application, CancellationToken ct)
        {
            var applicationToUpdate = await _context.Applications.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (applicationToUpdate == null)
                throw new EntityNotFoundException<Application>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(application.ViewId))).Succeeded)
                throw new ForbiddenException();

            Mapper.Map(application, applicationToUpdate);

            _context.Applications.Update(applicationToUpdate);
            await _context.SaveChangesAsync(ct);

            return Mapper.Map(applicationToUpdate, application);
        }

        public async Task<bool> DeleteApplicationAsync(Guid id, CancellationToken ct)
        {
            var applicationToDelete = await _context.Applications.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (applicationToDelete == null)
                throw new EntityNotFoundException<Application>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(applicationToDelete.ViewId))).Succeeded)
                throw new ForbiddenException();

            _context.Applications.Remove(applicationToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        #endregion

        #region Application Instances

        public async Task<IEnumerable<ViewModels.ApplicationInstance>> GetInstancesByTeamAsync(Guid teamId, CancellationToken ct)
        {
            var teamQuery = _context.Teams
                .Where(e => e.Id == teamId)
                .Future();

            var instanceQuery = _context.ApplicationInstances
                .Where(i => i.TeamId == teamId)
                .OrderBy(a => a.DisplayOrder)
                .ProjectTo<ViewModels.ApplicationInstance>()
                .Future();

            var team = (await teamQuery.ToListAsync()).SingleOrDefault();

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new TeamAccessRequirement(team.ViewId, teamId))).Succeeded)
                throw new ForbiddenException();

            return await instanceQuery.ToListAsync();
        }

        public async Task<ViewModels.ApplicationInstance> GetInstanceAsync(Guid id, CancellationToken ct)
        {
            var instance = await _context.ApplicationInstances
                .ProjectTo<ViewModels.ApplicationInstance>()
                .SingleOrDefaultAsync(a => a.Id == id, ct);

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(instance.ViewId))).Succeeded)
                throw new ForbiddenException();

            return instance;
        }

        public async Task<ViewModels.ApplicationInstance> CreateInstanceAsync(Guid teamId, ViewModels.ApplicationInstanceForm form, CancellationToken ct)
        {
            var team = await _context.Teams.Where(e => e.Id == teamId).SingleOrDefaultAsync(ct);

            if (team == null)
                throw new EntityNotFoundException<Team>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(team.ViewId))).Succeeded)
                throw new ForbiddenException();

            var instanceEntity = Mapper.Map<ApplicationInstanceEntity>(form);

            _context.ApplicationInstances.Add(instanceEntity);
            await _context.SaveChangesAsync(ct);

            var instance = await _context.ApplicationInstances
                .ProjectTo<ViewModels.ApplicationInstance>()
                .SingleOrDefaultAsync(i => i.Id == instanceEntity.Id, ct);

            return instance;
        }

        public async Task<ViewModels.ApplicationInstance> UpdateInstanceAsync(Guid id, ViewModels.ApplicationInstanceForm form, CancellationToken ct)
        {
            var instanceToUpdate = await _context.ApplicationInstances
                .Include(ai => ai.Team)
                .SingleOrDefaultAsync(v => v.Id == id, ct);

            if (instanceToUpdate == null)
                throw new EntityNotFoundException<ApplicationInstance>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(instanceToUpdate.Team.ViewId))).Succeeded)
                throw new ForbiddenException();

            Mapper.Map(form, instanceToUpdate);

            _context.ApplicationInstances.Update(instanceToUpdate);
            await _context.SaveChangesAsync(ct);

            var instance = await _context.ApplicationInstances
                .ProjectTo<ViewModels.ApplicationInstance>()
                .SingleOrDefaultAsync(i => i.Id == instanceToUpdate.Id, ct);

            return instance;
        }

        public async Task<bool> DeleteInstanceAsync(Guid id, CancellationToken ct)
        {
            var instanceToDelete = await _context.ApplicationInstances
                .Include(ai => ai.Team)
                .SingleOrDefaultAsync(v => v.Id == id, ct);

            if (instanceToDelete == null)
                throw new EntityNotFoundException<ApplicationInstance>();

            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ViewAdminRequirement(instanceToDelete.Team.ViewId))).Succeeded)
                throw new ForbiddenException();

            _context.ApplicationInstances.Remove(instanceToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

        #endregion
    }
}

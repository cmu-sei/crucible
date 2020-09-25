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
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Extensions;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using SAVM = Steamfitter.Api.ViewModels;

namespace Steamfitter.Api.Services
{
    public interface IVmCredentialService
    {
        STT.Task<IEnumerable<SAVM.VmCredential>> GetAsync(CancellationToken ct);
        STT.Task<SAVM.VmCredential> GetAsync(Guid Id, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.VmCredential>> GetByScenarioTemplateIdAsync(Guid scenarioTemplateId, CancellationToken ct);
        STT.Task<IEnumerable<SAVM.VmCredential>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct);
        STT.Task<SAVM.VmCredential> CreateAsync(SAVM.VmCredential Task, CancellationToken ct);
        STT.Task<SAVM.VmCredential> UpdateAsync(Guid Id, SAVM.VmCredential Task, CancellationToken ct);
        STT.Task<bool> DeleteAsync(Guid Id, CancellationToken ct);
    }

    public class VmCredentialService : IVmCredentialService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskService> _logger;

        public VmCredentialService(
            SteamfitterContext context,
            IAuthorizationService authorizationService,
            IPrincipal user,
            IMapper mapper,
            ILogger<TaskService> logger)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _logger = logger;
        }

        public async STT.Task<IEnumerable<SAVM.VmCredential>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var items = await _context.VmCredentials.ToListAsync(ct);         
            
            return _mapper.Map<IEnumerable<SAVM.VmCredential>>(items);
        }

        public async STT.Task<SAVM.VmCredential> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.VmCredentials
                .SingleOrDefaultAsync(o => o.Id == id, ct);
            if (item == null)
                throw new EntityNotFoundException<SAVM.VmCredential>();

            return _mapper.Map<SAVM.VmCredential>(item);
        }

        public async STT.Task<IEnumerable<SAVM.VmCredential>> GetByScenarioTemplateIdAsync(Guid scenarioTemplateId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmCredentials = _context.VmCredentials.Where(x => x.ScenarioTemplateId == scenarioTemplateId);

            return _mapper.Map<IEnumerable<SAVM.VmCredential>>(vmCredentials);
        }

        public async STT.Task<IEnumerable<SAVM.VmCredential>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmCredentials = _context.VmCredentials.Where(x => x.ScenarioId == scenarioId);

            return _mapper.Map<IEnumerable<SAVM.VmCredential>>(vmCredentials);
        }

        public async STT.Task<SAVM.VmCredential> CreateAsync(SAVM.VmCredential vmCredential, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            if (vmCredential.ScenarioTemplateId == null && vmCredential.ScenarioId == null)
                throw new ArgumentException("A VmCredential MUST be associated to either a Scenario Template or a Scenario.");

            vmCredential.DateCreated = DateTime.UtcNow;
            vmCredential.CreatedBy = _user.GetId();
            var vmCredentialEntity = _mapper.Map<VmCredentialEntity>(vmCredential);

            _context.VmCredentials.Add(vmCredentialEntity);
            await _context.SaveChangesAsync(ct);
            vmCredential = await GetAsync(vmCredentialEntity.Id, ct);

            return vmCredential;
        }

        public async STT.Task<SAVM.VmCredential> UpdateAsync(Guid id, SAVM.VmCredential vmCredential, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmCredentialToUpdate = await _context.VmCredentials.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (vmCredentialToUpdate == null)
                throw new EntityNotFoundException<SAVM.VmCredential>();

            vmCredential.CreatedBy = vmCredentialToUpdate.CreatedBy;
            vmCredential.DateCreated = vmCredentialToUpdate.DateCreated;
            vmCredential.DateModified = DateTime.UtcNow;
            vmCredential.ModifiedBy = _user.GetId();
            _mapper.Map(vmCredential, vmCredentialToUpdate);

            _context.VmCredentials.Update(vmCredentialToUpdate);
            await _context.SaveChangesAsync(ct);
            var updatedVmCredential = _mapper.Map(vmCredentialToUpdate, vmCredential);

            return updatedVmCredential;
        }

        public async STT.Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var vmCredentialToDelete = await _context.VmCredentials.SingleOrDefaultAsync(v => v.Id == id, ct);
            if (vmCredentialToDelete == null)
                throw new EntityNotFoundException<SAVM.VmCredential>();

            _context.VmCredentials.Remove(vmCredentialToDelete);
            await _context.SaveChangesAsync(ct);

            return true;
        }

    }

}


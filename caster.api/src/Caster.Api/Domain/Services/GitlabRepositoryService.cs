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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Caster.Api.Data;
using AutoMapper;
using Caster.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Caster.Api.Domain.Models;
using System.Text.Json;
using Caster.Api.Infrastructure.Serialization;

namespace Caster.Api.Domain.Services
{
    public interface IGitlabRepositoryService
    {
        Task<bool> GetModulesAsync(bool forceUpdate, CancellationToken cancellationToken);
        Task<bool> GetModuleAsync(string id, CancellationToken cancellationToken);
    }
    public class GitlabRepositoryService : IGitlabRepositoryService
    {
        private readonly CasterContext _db;
        private readonly IMapper _mapper;
        private readonly IOptionsMonitor<TerraformOptions> _terraformOptions;
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private string _token;

        public GitlabRepositoryService(
            CasterContext db,
            IMapper mapper,
            IOptionsMonitor<TerraformOptions> terraformOptions,
            IHttpClientFactory httpClientFactory
        )
        {
            _db = db;
            _mapper = mapper;
            _terraformOptions = terraformOptions;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> GetModulesAsync(
            bool forceUpdate,
            CancellationToken cancellationToken)
        {
            var requestTime = DateTime.UtcNow;
            DateTime updateCutoffDate;
            if (forceUpdate)
            {
                // force update of all modules
                updateCutoffDate = DateTime.MinValue;
            }
            else
            {
                // set the cutoff date to the most recxent DateModifed
                var dbDateModified = await _db.Modules.Select(m => m.DateModified).MaxAsync<DateTime?>(cancellationToken);
                updateCutoffDate = dbDateModified == null ? DateTime.MinValue : (DateTime)dbDateModified;
            }
            _httpClient = _httpClientFactory.CreateClient("gitlab");
            _token =  _terraformOptions.CurrentValue.GitlabToken;
            var groupId =  _terraformOptions.CurrentValue.GitlabGroupId;

            if (!groupId.HasValue) {
                var groupIdName = nameof(_terraformOptions.CurrentValue.GitlabGroupId);
                throw new ArgumentNullException(groupIdName, $"{groupIdName} must be set in order to retrieve Modules");
            }

            var response = await _httpClient.GetAsync($"groups/{groupId}/projects?private_token={_token}&include_subgroups=true");
            ValidateResponse(response);
            var json = await response.Content.ReadAsByteArrayAsync();

            var gitlabModules = JsonSerializer
                .Deserialize<GitlabModule[]>(
                    json,
                    DefaultJsonSettings.Settings);

            foreach (var gitlabModule in gitlabModules)
            {
                if (DateTime.Compare(gitlabModule.LastActivityAt, updateCutoffDate) > 0)
                {
                    var module = gitlabModule.ToModule(requestTime);

                    var existingModule = await _db.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.Path == module.Path);
                    if (existingModule == null)
                    {
                        // add the module
                        _db.Modules.Add(module);
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        // attach and update the module
                        module.Id = existingModule.Id;
                        _db.Modules.Update(module);
                        await _db.SaveChangesAsync(cancellationToken);
                    }

                    // get versions from Gitlab and update the database
                    await GetVersionsAsync(gitlabModule.Id, module.Id, gitlabModule.RepoUrl, cancellationToken);
                }
            }

            return true;

        }

        public async Task<bool> GetModuleAsync(string id, CancellationToken cancellationToken)
        {
            var requestTime = DateTime.UtcNow;
            _httpClient = _httpClientFactory.CreateClient("gitlab");
            _token =  _terraformOptions.CurrentValue.GitlabToken;
            // get module info from Gitlab and insert/update the database
            var response = await _httpClient.GetAsync($"projects/{id}?private_token={_token}");
            var json = await response.Content.ReadAsByteArrayAsync();

            var gitlabModule = JsonSerializer.Deserialize<GitlabModule>(
                json,
                DefaultJsonSettings.Settings);

            var module = gitlabModule.ToModule(requestTime);

            var existingModule = await _db.Modules.AsNoTracking().FirstOrDefaultAsync(m => m.Path == module.Path);
            if (existingModule == null)
            {
                // add the module
                _db.Modules.Add(module);
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // attach and update the module
                module.Id = existingModule.Id;
                _db.Modules.Update(module);
                await _db.SaveChangesAsync(cancellationToken);
            }

            // get versions from Gitlab and update the database
            await GetVersionsAsync(gitlabModule.Id, module.Id, gitlabModule.RepoUrl, cancellationToken);

            return true;
        }

        private async Task<bool> GetVersionsAsync(int id, Guid moduleId, string _baseUrl, CancellationToken cancellationToken)
        {
            // get the releases/versions
            var versions = new List<Domain.Models.ModuleVersion>();
            var response = await _httpClient.GetAsync($"projects/{id}/releases?private_token={_token}");
            var json = await response.Content.ReadAsByteArrayAsync();
            var releases =JsonSerializer.Deserialize<GitlabRelease[]>(
                json,
                DefaultJsonSettings.Settings);

            // delete current versions
            var versionsToRemove = _db.ModuleVersions.Where(mv => mv.ModuleId == moduleId);
            _db.ModuleVersions.RemoveRange(versionsToRemove);

            // insert all of the new versions
            foreach (var release in releases)
            {
                // get the module outputs
                var outputs = await GetOutputsAsync(id, release.Name, cancellationToken);
                // get the module variables
                var variables = await GetVariablesAsync(id, release.Name, cancellationToken);

                var version = new Caster.Api.Domain.Models.ModuleVersion(){
                    ModuleId = moduleId,
                    Name = release.Name,
                    UrlLink = _baseUrl.Replace("ref=master", $"ref={release.Name}"),
                    Variables = variables,
                    Outputs = outputs
                };
                _db.ModuleVersions.Add(version);
            }
            await _db.SaveChangesAsync();

            return true;
        }

        private async Task<List<string>> GetOutputsAsync(int id, string versionName, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"projects/{id}/repository/files/outputs.tf.json/raw?ref={versionName}&private_token={_token}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<string>();
            }

            var json = await response.Content.ReadAsByteArrayAsync();
            return GitlabModuleOutputResponse.GetModuleOutputs(json);
        }

        private async Task<List<Domain.Models.ModuleVariable>> GetVariablesAsync(int id, string versionName, CancellationToken cancellationToken)
        {
            // get the module variables
            var response = await _httpClient.GetAsync($"projects/{id}/repository/files/variables.tf.json/raw?ref={versionName}&private_token={_token}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Domain.Models.ModuleVariable>();
            }

            var responseJson = await response.Content.ReadAsByteArrayAsync();
            return GitlabModuleVariableResponse.GetModuleVariables(responseJson);
        }

        private void ValidateResponse(HttpResponseMessage responseMessage)
        {
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK) return;

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Module Repository Authorization Failed.  Ask the system administrator to verify the repository authorization token.");
            }
        }
    }
}


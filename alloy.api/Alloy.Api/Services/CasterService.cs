/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Caster.Api;
using Caster.Api.Models;
using Alloy.Api.Extensions;
using Alloy.Api.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Alloy.Api.Services
{
    public interface ICasterService
    {
        // Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct);
        Task<IEnumerable<Directory>> GetDirectoriesAsync(CancellationToken ct);
        // Task<IEnumerable<Workspace>> GetWorkspacesAsync(CancellationToken ct);
        // Task<Workspace> CreateWorkspaceInDirectoryAsync(Guid directoryId, string varsFileContent, CancellationToken ct);
    }

    public class CasterService : ICasterService
    {
        private readonly ICasterApiClient _casterApiClient;
        private readonly Guid _userId;
        private readonly string _userName;

        public CasterService(IHttpContextAccessor httpContextAccessor, ClientOptions clientSettings, ICasterApiClient casterApiClient)
        {
            _userId = httpContextAccessor.HttpContext.User.GetId();
            _userName = httpContextAccessor.HttpContext.User.Claims.First(c => c.Type.ToLower() == "name").Value;
            _casterApiClient = casterApiClient;
        }       

        // public async Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct)
        // {
        //     var views = await _casterApiClient.GetAllViewsAsync(ct);

        //     return views;
        // }

        public async Task<IEnumerable<Directory>> GetDirectoriesAsync(CancellationToken ct)
        {
            var directories = await _casterApiClient.GetAllDirectoriesAsync(false, false, ct);

            return directories;
        }

        // public async Task<IEnumerable<Workspace>> GetWorkspacesAsync(CancellationToken ct)
        // {
        //     var directories = await _casterApiClient.GetWorkspacesAsync(ct);

        //     return directories;
        // }

        // public async Task<Workspace> CreateWorkspaceInDirectoryAsync(Guid directoryId, string varsFileContent, CancellationToken ct)
        // {
        //     // remove special characters from the user name
        //     var userName = Regex.Replace(_userName, @"[^\w\.-]", "", RegexOptions.None);
        //     // create the new workspace
        //     var workspaceCommand = new CreateWorkspaceCommand() 
        //     {
        //         Name = $"x-{userName}-{_userId.ToString()}",
        //         DirectoryId = directoryId
        //     };
        //     var newWorkspace = await _casterApiClient.CreateWorkspaceAsync(workspaceCommand, ct);
        //     // create the workspace variable file
        //     var createFileCommand = new CreateFileCommand()
        //     {
        //         Name = $"{workspaceCommand.Name}.auto.tfvars",
        //         DirectoryId = directoryId,
        //         WorkspaceId = newWorkspace.Id,
        //         Content = varsFileContent
        //     };
        //     await _casterApiClient.CreateFileAsync(createFileCommand, ct);

        //     return newWorkspace;
        // }

    }
}


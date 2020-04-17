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
using System.Threading.Tasks;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Data
{
    public partial class CasterContext : DbContext
    {
        public async Task<List<File>> GetWorkspaceFiles(Workspace workspace, Directory directory)
        {
            if (workspace.DirectoryId != directory.Id)
                throw new ArgumentException("workspace must be in directory");

            var pathDirectoryIds = directory.PathIds();

            var files = await this.Files
                .Where(f => (f.WorkspaceId == workspace.Id) ||
                            (pathDirectoryIds.Contains(f.DirectoryId) && f.Workspace == null))
                .ToListAsync();

            return files;
        }

        public async Task<bool> AnyIncompleteRuns(Guid workspaceId)
        {
             return await this.Runs
                .Where(r =>
                    r.WorkspaceId == workspaceId &&
                    r.Status != RunStatus.Applied && r.Status != RunStatus.Failed && r.Status != RunStatus.Rejected)
                .AnyAsync();
        }
    }
}


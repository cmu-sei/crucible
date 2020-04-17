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
using Caster.Api.Data;
using Caster.Api.Domain.Models;
using Caster.Api.Infrastructure.Extensions;
using Caster.Api.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using static Caster.Api.Utilities.Synchronization.AsyncLock;

namespace Caster.Api.Domain.Services
{
    public interface IImportService
    {
        Task<ImportResult> ImportExercise(Exercise existingExercise, Exercise importedExercise, bool preserveIds);
        Task<ImportResult> ImportDirectory(Directory existingDirectory, Directory importedDirectory, bool preserveIds);
    }

    public class ImportService : IImportService
    {
        private readonly ILockService _lockService;
        private readonly CasterContext _db;
        private readonly bool _isAdmin;
        private readonly Guid _userId;

        public ImportService(
            ILockService lockService,
            CasterContext db,
            IIdentityResolver identityResolver)
        {
            _lockService = lockService;
            _db = db;
            _isAdmin = identityResolver.IsAdminAsync().Result;
            _userId = identityResolver.GetClaimsPrincipal().GetId();
        }

        public async Task<ImportResult> ImportExercise(Exercise existingExercise, Exercise importedExercise, bool preserveIds)
        {
            List<File> lockedFiles = new List<File>();
            List<AsyncLockResult> fileLocks = new List<AsyncLockResult>();

            try
            {
                foreach (var directory in importedExercise.Directories.Where(x => !x.ParentId.HasValue))
                {
                    var existingDir = existingExercise.Directories.FirstOrDefault(x => x.Name.Equals(directory.Name));

                    if (existingDir == null)
                    {
                        if (preserveIds && directory.Id != Guid.Empty)
                        {
                            existingDir = new Directory(directory.Name, id: directory.Id);
                        }
                        else
                        {
                            existingDir = new Directory(directory.Name);
                        }

                        _db.Entry(existingDir).State = EntityState.Added;
                        existingDir.ExerciseId = existingExercise.Id;
                        existingExercise.Directories.Add(existingDir);
                    }

                    lockedFiles.AddRange((await this.ImportDirectoryInternal(existingDir, directory, preserveIds, fileLocks)).LockedFiles);
                }
            }
            finally
            {
                foreach (var lockResult in fileLocks)
                {
                    lockResult.Dispose();
                }
            }

            return new ImportResult
            {
                LockedFiles = lockedFiles
            };
        }

        public async Task<ImportResult> ImportDirectory(Directory existingDir, Domain.Models.Directory dirToImport, bool preserveIds)
        {
            ImportResult result = new ImportResult();
            List<AsyncLockResult> fileLocks = new List<AsyncLockResult>();

            try
            {
                result = await this.ImportDirectoryInternal(existingDir, dirToImport, preserveIds, fileLocks);
            }
            finally
            {
                foreach (var lockResult in fileLocks)
                {
                    lockResult.Dispose();
                }
            }

            return result;
        }

        private async Task<ImportResult> ImportDirectoryInternal(
            Directory existingDir,
            Directory dirToImport,
            bool preserveIds,
            List<AsyncLockResult> fileLocks)
        {
            var lockedFiles = new List<File>();

            foreach (var workspace in dirToImport.Workspaces)
            {
                var dbWorkspace = existingDir.Workspaces.FirstOrDefault(x => x.Name.Equals(workspace.Name));
                var workspaceToUse = dbWorkspace;

                if (dbWorkspace == null)
                {
                    var newWorkspace = new Workspace(workspace.Name, existingDir);
                    existingDir.Workspaces.Add(newWorkspace);
                    workspaceToUse = newWorkspace;
                }

                foreach(var file in workspace.Files)
                {
                    var dbFile = workspaceToUse.Files.FirstOrDefault(x => x.Name.Equals(file.Name));

                    if (dbFile == null)
                    {
                        workspaceToUse.Files.Add(file);
                        existingDir.Files.Add(file);
                        file.Save(
                            _userId,
                            _isAdmin,
                            bypassLock: true);
                    }
                    else
                    {
                        var fileUpdateResult = await this.UpdateFile(dbFile, file);
                        fileLocks.Add(fileUpdateResult.LockResult);

                        if (fileUpdateResult.UnableToLock)
                        {
                            lockedFiles.Add(dbFile);
                        }
                    }
                }
            }

            foreach (var file in dirToImport.Files)
            {
                var dbFile = existingDir.Files.FirstOrDefault(x => x.Name.Equals(file.Name));

                if (dbFile == null)
                {
                    existingDir.Files.Add(file);
                    file.Save(
                        _userId,
                        _isAdmin,
                        bypassLock: true);
                }
                else
                {
                    var fileUpdateResult = await this.UpdateFile(dbFile, file);
                    fileLocks.Add(fileUpdateResult.LockResult);

                    if (fileUpdateResult.UnableToLock)
                    {
                        lockedFiles.Add(dbFile);
                    }
                }
            }

            foreach(var directory in dirToImport.Children)
            {
                var dbChildDir = existingDir.Children.FirstOrDefault(x => x.Name.Equals(directory.Name));
                var childDirToUse = dbChildDir;

                if (dbChildDir == null)
                {
                    Guid? id = null;

                    if (preserveIds && directory.Id != Guid.Empty)
                    {
                        id = directory.Id;
                    }

                    var newDir = new Directory(directory.Name, existingDir, id);
                    existingDir.Children.Add(newDir);
                    childDirToUse = newDir;
                    _db.Entry(newDir).State = EntityState.Added;
                }

                var l = (await this.ImportDirectoryInternal(childDirToUse, directory, preserveIds, fileLocks)).LockedFiles;

                lockedFiles.AddRange(l);
            }

            return new ImportResult
            {
                LockedFiles = lockedFiles
            };
        }

        private class FileUpdateResult
        {
            public bool FileUpdated { get; set; }
            public bool UnableToLock { get; set; }
            public AsyncLockResult LockResult { get; set;}
        }

        private async Task<FileUpdateResult> UpdateFile(Domain.Models.File dbFile, Domain.Models.File file)
        {
            var result = new FileUpdateResult();

            result.LockResult = await _lockService.GetFileLock(dbFile.Id).LockAsync(0);

            // Don't need to update or throw error if contents haven't changed
            if (!dbFile.Content.Equals(file.Content))
            {
                if (!result.LockResult.AcquiredLock)
                {
                    result.UnableToLock = true;
                    result.FileUpdated = false;
                }
                else if (dbFile.CanLock(_userId, _isAdmin))
                {
                    dbFile.Content = file.Content;
                    dbFile.Save(
                        _userId,
                        _isAdmin,
                        bypassLock: true);

                    result.FileUpdated = true;
                    result.UnableToLock = false;
                }
                else
                {
                    result.FileUpdated = false;
                    result.UnableToLock = true;
                }
            }

            return result;
        }
    }
}

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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Caster.Api.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Caster.Api.Domain.Models
{
    public class Workspace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid DirectoryId { get; set; }
        public virtual Directory Directory { get; set; }

        public string State { get; set;}

        public string StateBackup { get; set; }

        public DateTime? LastSynced { get; set; }

        public string[] SyncErrors { get; set; }

        public bool DynamicHost { get; set; }

        public Guid? HostId { get; set; }
        public virtual Host Host { get; set; }

        public virtual ICollection<Run> Runs { get; set; } = new List<Run>();
        public virtual ICollection<File> Files { get; set; } = new List<File>();

        public bool IsDefault
        {
            get
            {
                return Name == "Default";
            }
        }

        public Workspace() {}

        public Workspace(string name, Directory directory)
        {
            this.Name = name;

            if (directory != null)
            {
                this.Directory = directory;
                this.DirectoryId = directory.Id;
            }
        }

        private const string StateFileName = "terraform.tfstate";
        private const string StateFileBackupName = "terraform.tfstate.backup";
        private const string StateFileDirectory = "terraform.tfstate.d";

        public State GetState()
        {
            if (this.State == null)
                return new State();

            return JsonSerializer.Deserialize<State>(this.State, DefaultJsonSettings.Settings);
        }

        public State GetStateBackup()
        {
            if (this.StateBackup == null)
                return new State();

            return JsonSerializer.Deserialize<State>(this.StateBackup, DefaultJsonSettings.Settings);
        }

        public Resource[] GetRemovedResources()
        {
            var state = this.GetState();
            var previousState = this.GetStateBackup();

            var resources = state.GetResources();
            var previousResources = previousState.GetResources();

            var removedResources = new List<Resource>();

            foreach (var resource in previousResources)
            {
                if (!resources.Any(r => r.Id == resource.Id))
                {
                    removedResources.Add(resource);
                }
            }

            return removedResources.ToArray();
        }

        public void CleanupFileSystem(string basePath)
        {
            var workingDir = GetPath(basePath);

            if (System.IO.Directory.Exists(workingDir))
            {
                this.DeleteDirectory(workingDir);
            }
        }

        public string GetPath(string basePath)
        {
            return Path.Combine(basePath, Id.ToString());
        }

        public string GetStatePath(string basePath, bool backupState)
        {
            var stateFileName = backupState ? Workspace.StateFileBackupName : Workspace.StateFileName;

            if (this.IsDefault)
            {
                return Path.Combine(basePath, stateFileName);
            }
            else
            {
                return Path.Combine(basePath, Workspace.StateFileDirectory, this.Name, stateFileName);
            }
        }

        public async Task PrepareFileSystem(string workingDir, IEnumerable<File> files)
        {
            // Remove any previous runs
            if (System.IO.Directory.Exists(workingDir))
            {
                this.DeleteDirectory(workingDir);
            }

            System.IO.Directory.CreateDirectory(workingDir);

            // Write Files
            foreach (File file in files)
            {
                var filePath = Path.Combine(workingDir, file.Name);
                using (var writer = System.IO.File.CreateText(filePath))
                {
                    await writer.WriteLineAsync(file.Content);
                }
            }

            // Write State files
            if (this.IsDefault)
            {
                if (!string.IsNullOrEmpty(this.State))
                {
                    var statePath = this.GetStatePath(workingDir, backupState: false);
                    using (var writer = System.IO.File.CreateText(statePath))
                    {
                        await writer.WriteLineAsync(this.State);
                    }
                }

                if (!string.IsNullOrEmpty(this.StateBackup))
                {
                    var stateBackupPath = this.GetStatePath(workingDir, backupState: true);
                    using (var writer = System.IO.File.CreateText(stateBackupPath))
                    {
                        await writer.WriteLineAsync(this.StateBackup);
                    }
                }
            }
            else
            {
                var workspaceStateDir = Path.Combine(workingDir, Workspace.StateFileDirectory, this.Name);
                System.IO.Directory.CreateDirectory(workspaceStateDir);

                if (!string.IsNullOrEmpty(this.State))
                {
                    var statePath = this.GetStatePath(workingDir, backupState: false);
                    using (var writer = System.IO.File.CreateText(statePath))
                    {
                        await writer.WriteLineAsync(this.State);
                    }

                    if (!string.IsNullOrEmpty(this.StateBackup))
                    {
                        var stateBackupPath = this.GetStatePath(workingDir, backupState: true);
                        using (var writer = System.IO.File.CreateText(stateBackupPath))
                        {
                            await writer.WriteLineAsync(this.StateBackup);
                        }
                    }
                }
            }
        }

        private void DeleteDirectory(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            // To avoid errors if read-only files are in the directory.
            // This seems to happen sometimes when modules are downloaded from git
            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                if (info.Attributes != FileAttributes.Normal)
                {
                    info.Attributes = FileAttributes.Normal;
                }
            }

            directory.Delete(true);
        }

        public async Task<bool> RetrieveState(string workingDir)
        {
            bool success = false;

            bool includeBackup = !string.IsNullOrEmpty(this.State);

            string state = null;
            string stateBackup = null;
            string statePath = this.GetStatePath(workingDir, backupState: false);
            string stateBackupPath = this.GetStatePath(workingDir, backupState: true);

            if (System.IO.File.Exists(statePath))
            {
                state = await System.IO.File.ReadAllTextAsync(statePath);
            }

            if (System.IO.File.Exists(stateBackupPath))
            {
                stateBackup = await System.IO.File.ReadAllTextAsync(stateBackupPath);
            }

            if (!string.IsNullOrEmpty(state))
            {
                if (includeBackup)
                {
                    if (!string.IsNullOrEmpty(stateBackup))
                    {
                        this.StateBackup = stateBackup;
                    }
                    else
                    {
                        return false;
                    }
                }

                this.State = state;
                success = true;
            }

            return success;
        }
    }

    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder
                .Property<string[]>(w => w.SyncErrors)
                .HasConversion(
                    list => String.Join('\n', list),
                    str => str.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                );

            builder.HasIndex(r => r.DirectoryId);

            builder
                .HasOne(w => w.Directory)
                .WithMany(d => d.Workspaces)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

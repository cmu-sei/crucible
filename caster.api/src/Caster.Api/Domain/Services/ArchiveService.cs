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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caster.Api.Domain.Models;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace Caster.Api.Domain.Services
{
    public interface IArchiveService
    {
        Task<ArchiveResult> ArchiveExercise(Exercise exercise, ArchiveType type, bool includeIds);
        Task<ArchiveResult> ArchiveDirectory(Directory directory,  ArchiveType type, bool includeIds);
        Directory ExtractDirectory(System.IO.Stream stream, string filename);
        Exercise ExtractExercise(System.IO.Stream stream, string filename);
    }

    public class ArchiveService : IArchiveService
    {
        #region Archive

        /// <summary>
        /// Archive an Exercise.
        /// Assumes all Directories are fully populated with Files, Workspaces, and child Directories
        /// </summary>
        public async Task<ArchiveResult> ArchiveExercise(Exercise exercise, ArchiveType type, bool includeIds)
        {
            var stream = new System.IO.MemoryStream();

            using (ArchiveOutputStream archiveStream = ArchiveOutputStream.Create(stream, type))
            {
                foreach(var directory in exercise.Directories.Where(d => d.ParentId == null))
                {
                    await ArchiveDirectory(directory, "", archiveStream, includeIds, includeDirName: true);
                }
            }

            stream.Position = 0;

            return new ArchiveResult
            {
                Data = stream,
                Name = $"{exercise.Name}.{type.GetExtension()}",
                Type = type.GetContentType()
            };
        }

        /// <summary>
        /// Archive a Directory.
        /// Assumes the Directory is fully populated with Files, Workspaces, and child Directories
        /// </summary>
        public async Task<ArchiveResult> ArchiveDirectory(Directory directory, ArchiveType type, bool includeIds)
        {
            var stream = new System.IO.MemoryStream();

            using (ArchiveOutputStream archiveStream = ArchiveOutputStream.Create(stream, type))
            {
                await ArchiveDirectory(directory, "", archiveStream, includeIds, includeDirName: false);
            }

            stream.Position = 0;

            return new ArchiveResult
            {
                Data = stream,
                Name = $"{directory.GetExportName(includeIds)}.{type.GetExtension()}",
                Type = type.GetContentType()
            };
        }

        private async Task ArchiveDirectory(Directory directory, string ancestors, ArchiveOutputStream archiveStream, bool includeIds, bool includeDirName)
        {
            var rootPath = $"{ancestors}{(includeDirName ? directory.GetExportName(includeIds) : string.Empty)}/";
            var fullPath = $"{ancestors}{directory.GetExportName(includeIds)}";

            foreach(var file in directory.Files.Where(f => f.WorkspaceId == null))
            {
                archiveStream.PutNextEntry($"{rootPath}{file.Name}", file.Content.Length);

                using (var sw = new System.IO.StreamWriter(archiveStream.Stream, leaveOpen: true))
                {
                    await sw.WriteAsync(file.Content);
                }

                archiveStream.CloseEntry();
            }

            foreach(var workspace in directory.Workspaces)
            {
                var path = $"{rootPath}__Workspaces__/{workspace.Name}/";

                archiveStream.PutNextEntry(path, 0);
                archiveStream.CloseEntry();

                foreach(var file in workspace.Files)
                {
                    archiveStream.PutNextEntry($"{path}{file.Name}", file.Content.Length);

                    using (var sw = new System.IO.StreamWriter(archiveStream.Stream, leaveOpen: true))
                    {
                        await sw.WriteAsync(file.Content);
                    }

                    archiveStream.CloseEntry();
                }
            }

            foreach(var dir in directory.Children) {
                await ArchiveDirectory(dir, $"{rootPath}", archiveStream, includeIds, true);
            }
        }

        #endregion

        #region Extract

        public Exercise ExtractExercise(System.IO.Stream stream, string filename)
        {
            Exercise exercise = new Exercise(System.IO.Path.GetFileNameWithoutExtension(filename));
            this.Extract(stream, filename, exercise: exercise);

            return exercise;
        }

        public Directory ExtractDirectory(System.IO.Stream stream, string filename)
        {
            Directory directory = new Directory(System.IO.Path.GetFileNameWithoutExtension(filename), null);
            this.Extract(stream, filename, directory: directory);

            return directory;
        }

        private void Extract(System.IO.Stream stream, string filename, Exercise exercise = null, Directory directory = null)
        {
            using (var archiveInputStream = ArchiveInputStream.Create(stream, ArchiveTypeHelpers.GetType(filename)))
            {
                while (archiveInputStream.GetNextEntry() is ArchiveEntry archiveEntry)
                {
                    var file = this.EnsureCreated(exercise, directory, archiveEntry.Name, archiveEntry.IsFile);

                    if (file != null && archiveEntry.IsFile)
                    {
                        var buffer = new byte[4096];

                        using (var contentStream = new System.IO.MemoryStream())
                        {
                            StreamUtils.Copy(archiveInputStream.Stream, contentStream, buffer);
                            file.Content = Encoding.ASCII.GetString(contentStream.ToArray());
                        }
                    }
                }
            }
        }

        private File EnsureCreated(Exercise exercise, Directory root, string path, bool isFile)
        {
            var isWorkspace = false;
            Directory parent = root;
            Workspace workspace = null;
            File file = null;

            if (String.IsNullOrEmpty(path))
            {
                return file;
            }

            string[] pathParts = this.SplitPath(path);
            for (int i = 0; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i];
                if (pathPart.Equals("__Workspaces__"))
                {
                    isWorkspace = true;
                    continue;
                }

                if (isWorkspace)
                {
                    if (isFile &&
                        (i == pathParts.Length - 1) &&
                        workspace != null &&
                        !workspace.Files.Any(x => x.Name.Equals(pathPart)))
                    {
                        file = new File();
                        file.Name = pathPart;
                        workspace.Files.Add(file);
                    }
                    else
                    {
                        var newWorkspace = new Workspace(pathPart, parent);
                        var existingWorkspace = parent.Workspaces.FirstOrDefault(x => x.Name.Equals(pathPart));

                        if (existingWorkspace == null)
                        {
                            parent.Workspaces.Add(newWorkspace);
                            workspace = newWorkspace;
                        }
                        else
                        {
                            workspace = existingWorkspace;
                        }
                    }
                }
                else
                {
                    if (isFile &&
                        (i == pathParts.Length - 1) &&
                        !parent.Files.Any(x => x.Name.Equals(pathPart)))
                    {
                        file = new File();
                        file.Name = pathPart;
                        parent.Files.Add(file);
                    }
                    else
                    {
                        var newDir = new Directory(pathPart, parent);
                        Directory existingDir = null;

                        if (parent == null && exercise != null)
                        {
                            existingDir = exercise.Directories.FirstOrDefault(x => x.Name == newDir.Name);
                        }
                        else if (parent != null)
                        {
                            existingDir = parent.Children.FirstOrDefault(x => x.Name == newDir.Name);
                        }

                        if (existingDir == null)
                        {
                            if (parent == null)
                            {
                                exercise.Directories.Add(newDir);
                            }
                            else
                            {
                                parent.Children.Add(newDir);
                            }

                            parent = newDir;
                        }
                        else
                        {
                            parent = existingDir;
                        }
                    }
                }
            }

            return file;
        }

        private string[] SplitPath(string path)
        {
            var paths = System.IO.Path.TrimEndingDirectorySeparator(path).Split(new[]
            {
                System.IO.Path.DirectorySeparatorChar,
                System.IO.Path.AltDirectorySeparatorChar
            });

            return paths;
        }

        #endregion

        private class ArchiveOutputStream : IDisposable
        {
            public System.IO.Stream Stream { get; set; }

            private ArchiveOutputStream(System.IO.Stream stream)
            {
                this.Stream = stream;
            }

            public static ArchiveOutputStream Create(System.IO.Stream stream, ArchiveType type)
            {
                switch (type)
                {
                    case ArchiveType.zip:
                        return ArchiveOutputStream.CreateZipStream(stream);
                    case ArchiveType.tgz:
                        return ArchiveOutputStream.CreateTgzStream(stream);
                    default:
                        throw new ArgumentException();
                }
            }

            private static ArchiveOutputStream CreateZipStream(System.IO.Stream stream)
            {
                ZipOutputStream zipStream = new ZipOutputStream(stream);
                zipStream.IsStreamOwner = false;
                return new ArchiveOutputStream(zipStream);
            }

            private static ArchiveOutputStream CreateTgzStream(System.IO.Stream stream)
            {
                GZipOutputStream gzipStream = new GZipOutputStream(stream);
                TarOutputStream tarStream = new TarOutputStream(gzipStream);
                gzipStream.IsStreamOwner = false;
                return new ArchiveOutputStream(tarStream);
            }

            public void PutNextEntry(string name, long size)
            {
                if (Stream is ZipOutputStream)
                {
                    ((ZipOutputStream)Stream).PutNextEntry(new ZipEntry(name));
                }
                else if (Stream is TarOutputStream)
                {
                    var tarEntry = TarEntry.CreateTarEntry(name);
                    tarEntry.Size = size;
                    ((TarOutputStream)Stream).PutNextEntry(tarEntry);
                }
            }

            public void CloseEntry()
            {
                if (Stream is TarOutputStream)
                {
                    ((TarOutputStream)Stream).CloseEntry();
                }
            }

            public void Dispose()
            {
                this.Stream.Dispose();
            }
        }

        private class ArchiveInputStream : IDisposable
        {
            public System.IO.Stream Stream { get; set; }

            private ArchiveInputStream(System.IO.Stream stream)
            {
                this.Stream = stream;
            }

            public static ArchiveInputStream Create(System.IO.Stream stream, ArchiveType type)
            {
                switch (type)
                {
                    case ArchiveType.zip:
                        return ArchiveInputStream.CreateZipStream(stream);
                    case ArchiveType.tgz:
                        return ArchiveInputStream.CreateTgzStream(stream);
                    default:
                        throw new ArgumentException();
                }
            }

            private static ArchiveInputStream CreateZipStream(System.IO.Stream stream)
            {
                ZipInputStream zipStream = new ZipInputStream(stream);
                zipStream.IsStreamOwner = false;
                return new ArchiveInputStream(zipStream);
            }

            private static ArchiveInputStream CreateTgzStream(System.IO.Stream stream)
            {
                GZipInputStream gzipStream = new GZipInputStream(stream);
                TarInputStream tarStream = new TarInputStream(gzipStream);
                gzipStream.IsStreamOwner = false;
                return new ArchiveInputStream(tarStream);
            }

            public ArchiveEntry GetNextEntry()
            {
                ArchiveEntry archiveEntry = null;

                if (Stream is ZipInputStream)
                {
                    var zipEntry = ((ZipInputStream)Stream).GetNextEntry();

                    if (zipEntry != null)
                    {
                        archiveEntry = new ArchiveEntry(zipEntry);
                    }
                }
                else if (Stream is TarInputStream)
                {
                    var tarEntry = ((TarInputStream)Stream).GetNextEntry();

                    if (tarEntry != null)
                    {
                        archiveEntry = new ArchiveEntry(tarEntry);
                    }
                }

                return archiveEntry;
            }

            public void Dispose()
            {
                this.Stream.Dispose();
            }
        }

        private class ArchiveEntry
        {
            private ZipEntry ZipEntry{ get; set; }
            private TarEntry TarEntry { get; set; }

            public ArchiveEntry(ZipEntry zipEntry)
            {
                this.ZipEntry = zipEntry;
            }

            public ArchiveEntry(TarEntry tarEntry)
            {
                this.TarEntry = tarEntry;
            }

            public bool IsFile
            {
                get
                {
                    if (this.ZipEntry != null)
                    {
                        return !this.ZipEntry.IsDirectory;
                    }

                    if (this.TarEntry != null)
                    {
                        return !this.TarEntry.IsDirectory;
                    }

                    return false;
                }
            }

            public string Name
            {
                get
                {
                    if (this.ZipEntry != null)
                    {
                        return this.ZipEntry.Name;
                    }

                    if (this.TarEntry != null)
                    {
                        return this.TarEntry.Name;
                    }

                    return null;
                }
            }
        }
    }
}

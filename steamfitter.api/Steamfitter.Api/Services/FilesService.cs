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
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Steamfitter.Api.Data;
using Steamfitter.Api.Data.Models;
using Steamfitter.Api.Infrastructure.Authorization;
using Steamfitter.Api.Infrastructure.Exceptions;
using Steamfitter.Api.Infrastructure.Options;
using Steamfitter.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Steamfitter.Api.Services
{
    public interface IFilesService
    {
        Task<IEnumerable<FileEntity>> GetAsync(CancellationToken ct);
        Task<FileEntity> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<FileEntity>> SaveAsync(IEnumerable<IFormFile> files, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }

    public class FilesService : IFilesService
    {
        private readonly SteamfitterContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly ClaimsPrincipal _user;
        private readonly IMapper _mapper;
        private readonly FilesOptions _options;

        public FilesService(SteamfitterContext context, IAuthorizationService authorizationService, IPrincipal user, IMapper mapper,
            FilesOptions fileSettings)
        {
            _context = context;
            _authorizationService = authorizationService;
            _user = user as ClaimsPrincipal;
            _mapper = mapper;
            _options = fileSettings;
        }

        public async Task<IEnumerable<FileEntity>> GetAsync(CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var list = await _context.Files.ToArrayAsync(ct);

            if (list == null)
                throw new EntityNotFoundException<IEnumerable<FileEntity>>();

            return list;
        }

        //get file by id
        public async Task<FileEntity> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Files.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (item == null)
                throw new EntityNotFoundException<FileEntity>();

            return item;
        }

        public async Task<IEnumerable<FileEntity>> SaveAsync(IEnumerable<IFormFile> files, CancellationToken ct)
        {
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var list = new List<FileEntity>();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = $"{_options.LocalDirectory}{formFile.FileName}";
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        // write file
                        await formFile.CopyToAsync(stream);

                        // not a zip? zip it!
                        if (!Path.GetExtension(formFile.FileName).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
                            filePath = this.Zip(filePath, formFile.FileName);

                        //build return obj
                        var item = new FileEntity();
                        item.Name = formFile.Name;
                        item.Length = formFile.Length;
                        item.DateCreated = DateTime.UtcNow;
                        item.StoragePath = filePath;
                        list.Add(item);
                    }
                }
            }

            //save to db
            if (list.Count > 0)
            {
                _context.Files.AddRange(list);
                await _context.SaveChangesAsync(ct);
            }

            return list;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            //delete db record
            if (!(await _authorizationService.AuthorizeAsync(_user, null, new ContentDeveloperRequirement())).Succeeded)
                throw new ForbiddenException();

            var item = await _context.Files.SingleOrDefaultAsync(v => v.Id == id, ct);

            if (item == null)
                throw new EntityNotFoundException<FileEntity>();

            //delete or move actual file
            File.Delete(item.StoragePath);

            //delete from db
            _context.Files.Remove(item);
            await _context.SaveChangesAsync(ct);
        }

        private string Zip(string filePath, string fileName)
        {
            // Create a temp folder
            var tempPath = Path.GetTempPath();
            var tempId = Guid.NewGuid();
            var tempFolder = $"{tempPath}{tempId}";
            var zipFile = $"{fileName.Replace(Path.GetExtension(fileName), "")}.zip";
            var finalFile = $"{Directory.GetParent(filePath)}{Path.DirectorySeparatorChar}{fileName.Replace(Path.GetExtension(fileName), "")}.zip";
            
            Directory.CreateDirectory(tempFolder);

            // move file
            File.Copy(filePath, $"{tempFolder}{Path.DirectorySeparatorChar}{fileName}");
            
            // Zip folder
            ZipFile.CreateFromDirectory(tempFolder, zipFile);

            if(File.Exists(finalFile))
                File.Delete(finalFile);
            
            //move zip
            File.Move(zipFile, finalFile);

            // Delete folder
            Directory.Delete(tempFolder, true);
            
            return finalFile;
        }
    }
}

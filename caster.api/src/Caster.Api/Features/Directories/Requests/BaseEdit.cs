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
using System.Threading.Tasks;
using Caster.Api.Data;
using AutoMapper;
using Caster.Api.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Caster.Api.Data.Extensions;

namespace Caster.Api.Features.Directories
{
    public abstract class BaseEdit
    {
        public abstract class Handler
        {
            protected readonly CasterContext _db;
            protected readonly IMapper _mapper;

            public Handler(CasterContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            protected async Task UpdatePaths(Domain.Models.Directory directory, Guid? parentId)
            {
                string parentPath = null;
                string oldPath = directory.Path;

                if (parentId.HasValue)
                {
                    var parentDirectory = await _db.Directories.FindAsync(parentId);

                    if (parentDirectory == null)
                        throw new EntityNotFoundException<Directory>("Parent Directory Not Found");

                    parentPath = parentDirectory.Path;
                }

                var descendants = await _db.Directories.GetChildren(directory, false).ToListAsync();

                directory.SetPath(parentPath);

                foreach(var desc in descendants)
                {
                    desc.Path = desc.Path.Replace(oldPath, directory.Path);
                }
            }
        }
    }
}

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Caster.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Features.Files
{
    public interface IGetFileQuery
    {
        Task<File> ExecuteAsync(Guid fileId, bool includeContent = true);
    }

    public class GetFileQuery : IGetFileQuery
    {
        private readonly CasterContext _db;
        private readonly IMapper _mapper;

        private ConcurrentDictionary<Guid, File> _cache = new ConcurrentDictionary<Guid, File>();

        public GetFileQuery(
            CasterContext db,
            IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a single Domain.Models.File by Id from the database and projects it into a File
        /// Will retrieve result from cache if available.
        /// This is intended to be Scoped to avoid multiple db round-trips in the same API call, not a persistent global cache
        /// </summary>
        public async Task<File> ExecuteAsync(Guid fileId, bool includeContent = true)
        {
            File file;

            if (!_cache.TryGetValue(fileId, out file))
            {
                var includeList = new List<Expression<System.Func<File, object>>>();
                if (includeContent) includeList.Add(dest => dest.Content);

                file = await _db.Files
                    .Where(f => f.Id == fileId)
                    .IgnoreQueryFilters()
                    .ProjectTo<File>(_mapper.ConfigurationProvider, includeList.ToArray())
                    .FirstOrDefaultAsync();

                _cache.TryAdd(fileId, file);
            }

            if (!includeContent)
            {
                file.Content = null;
            }

            return file;
        }
    }
}


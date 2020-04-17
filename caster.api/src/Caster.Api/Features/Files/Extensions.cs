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
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Features.Files
{
    public static class FileExtensions
    {
        public static IQueryable<File> GetAll(
            this IQueryable<Domain.Models.File> query,
            IConfigurationProvider configurationProvider,
            bool includeDeleted,
            bool includeContent,
            Guid? directoryId = null)
        {
            IQueryable<Domain.Models.File> initialQuery = query;

            if (directoryId.HasValue)
            {
                initialQuery = initialQuery.Where(f => f.DirectoryId == directoryId);
            }

            if(includeDeleted)
            {
                initialQuery = query.IgnoreQueryFilters();
            }

            IQueryable<File> returnQuery;

            if(includeContent)
            {
                returnQuery = initialQuery.ProjectTo<File>(configurationProvider, dest => dest.Content);
            }
            else
            {
                returnQuery = initialQuery.ProjectTo<File>(configurationProvider);
            }

            return returnQuery;
        }
    }
}

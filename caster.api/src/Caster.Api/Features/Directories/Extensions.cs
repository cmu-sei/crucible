/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Caster.Api.Features.Directories
{
    public static class DirectoryExtensions
    {
        public static IQueryable<Directory> Expand(this IQueryable<Domain.Models.Directory> query, IConfigurationProvider configuration, bool includeRelated, bool includeFileContent)
        {
            IQueryable<Directory> newQuery;

            if (includeRelated)
            {
                if (includeFileContent) 
                {
                    newQuery = query.ProjectTo<Directory>(configuration, dest => dest.Files, dest => dest.Files.Select(f => f.Content), dest => dest.Workspaces);                        
                }
                else
                {
                    newQuery = query.ProjectTo<Directory>(configuration, dest => dest.Files, dest => dest.Workspaces);
                }
            }                
            else
            {
                newQuery = query.ProjectTo<Directory>(configuration);
            }

            return newQuery;
        }

        public static IQueryable<Domain.Models.Directory> GetChildren(this IQueryable<Domain.Models.Directory> query, Domain.Models.Directory directory, bool includeSelf)
        {
            var pattern = $"{directory.Path}%";
            query = query.Where(d => EF.Functions.Like(d.Path, pattern));

            if (!includeSelf)
            {
                query = query.Where(d => d.Id != directory.Id);
            }

            return query;
        }
    }
}

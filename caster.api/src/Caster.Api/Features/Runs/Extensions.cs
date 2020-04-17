/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Caster.Api.Features.Runs
{
    public static class RunsExtensions
    {
        public static IQueryable<Run> Expand(this IQueryable<Domain.Models.Run> query, IConfigurationProvider configuration, bool includePlan, bool includeApply)
        {
            var includeList = new List<Expression<System.Func<Run, object>>>();

            if (includePlan) includeList.Add(dest => dest.Plan);
            if (includeApply) includeList.Add(dest => dest.Apply);

            return query.ProjectTo<Run>(configuration, includeList.ToArray());
        }

        public static IQueryable<Domain.Models.Run> Limit(this IQueryable<Domain.Models.Run> query, int? limit)
        {
            if (limit.HasValue)
            {
                return query.Take(limit.Value);
            }
            else
            {
                return query;
            }
        }
    }
}

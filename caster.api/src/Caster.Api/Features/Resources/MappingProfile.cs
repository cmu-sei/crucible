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
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using Caster.Api.Domain.Models;

namespace Caster.Api.Features.Resources
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Models.Resource, Resource>();

            // Allows passing in of a property name to exclude it from mapping at runtime
            ForAllMaps((typeMap, mappingExpression) =>
            {
                mappingExpression.ForAllMembers(memberOptions =>
                {
                    memberOptions.Condition((o1, o2, o3, o4, resolutionContext) =>
                    {
                        var name = memberOptions.DestinationMember.Name;
                        if (resolutionContext.Items.TryGetValue(MemberExclusionKey, out object exclusions))
                        {
                            if (((IEnumerable<string>)exclusions).Contains(name))
                            {
                                return false;
                            }
                        }
                        return true;
                    });
                });
            });
        }

        public static string MemberExclusionKey { get; } = "exclude";
    }

    public static class IMappingOperationOptionsExtensions
    {
        public static void ExcludeMembers(this AutoMapper.IMappingOperationOptions options, params string[] members)
        {
            options.Items[MappingProfile.MemberExclusionKey] = members;
        }
    }
}

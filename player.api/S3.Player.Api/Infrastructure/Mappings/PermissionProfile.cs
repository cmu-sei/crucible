/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace S3.Player.Api.Infrastructure.Mappings
{
    public class PermissionProfile : AutoMapper.Profile
    {
        public PermissionProfile()
        {
            CreateMap<PermissionEntity, Permission>();
            CreateMap<PermissionForm, PermissionEntity>();

            CreateMap<UserEntity, UserPermissions>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    src.Role.Permissions.Select(x => x.Permission).Concat(
                    src.Permissions.Select(x => x.Permission))))
                .ForMember(dest => dest.TeamPermissions, opt => opt.MapFrom(src => src.TeamMemberships));

            CreateMap<TeamMembershipEntity, TeamPermissions>()
                .ForMember(dest => dest.ViewId, opt => opt.MapFrom(src => src.ViewMembership.ViewId))
                .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.ViewMembership.PrimaryTeamMembershipId == src.Id))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    src.Role.Permissions.Select(x => x.Permission).Concat(
                    src.Team.Role.Permissions.Select(x => x.Permission)).Concat(
                    src.Team.Permissions.Select(x => x.Permission))));
        }
    }
}

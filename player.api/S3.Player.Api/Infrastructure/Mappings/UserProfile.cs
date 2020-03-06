/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.ViewModels;
using System.Linq;

namespace S3.Player.Api.Infrastructure.Mappings
{
    public class UserProfile : AutoMapper.Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserEntity>()
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());

            CreateMap<UserEntity, User>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleId.HasValue ? src.Role.Name : null))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(x => x.Permission)))
                .ForMember(dest => dest.IsSystemAdmin, opt => opt.MapFrom(src => (src.Permissions.Where(p => p.Permission.Key == PlayerClaimTypes.SystemAdmin.ToString()).Any()) ||
                    src.Role.Permissions.Where(p => p.Permission.Key == PlayerClaimTypes.SystemAdmin.ToString()).Any()));            
        }
    }
}


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
using Microsoft.AspNetCore.Authorization;
using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.Infrastructure.Authorization;
using S3.Player.Api.Services;
using S3.Player.Api.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace S3.Player.Api.Infrastructure.Mappings
{
    public class TeamProfile : AutoMapper.Profile
    {
        public TeamProfile()
        {
            CreateMap<Team, TeamEntity>();
            CreateMap<TeamForm, TeamEntity>();

            CreateMap<TeamEntity, TeamDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.RoleId.HasValue ? src.Role.Name : null))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(x => x.Permission)));

            CreateMap<TeamDTO, Team>()
                .ForMember(dest => dest.CanManage, opt => opt.ResolveUsing<ManageTeamResolver>())
                .ForMember(dest => dest.IsMember, opt => opt.ResolveUsing<TeamMemberResolver>())
                .ForMember(dest => dest.IsPrimary, opt => opt.ResolveUsing<PrimaryTeamResolver>());
        }
    }

    public class ManageTeamResolver : IValueResolver<TeamDTO, Team, bool>
    {
        private IAuthorizationService _authorizationService;
        private ClaimsPrincipal _user;

        public ManageTeamResolver(IAuthorizationService authorizationService, IUserClaimsService userClaimsService)
        {
            _authorizationService = authorizationService;
            _user = userClaimsService.GetCurrentClaimsPrincipal();
        }

        public bool Resolve(TeamDTO source, Team destination, bool member, ResolutionContext context)
        {
            return _authorizationService.AuthorizeAsync(_user, null, new ManageExerciseRequirement(source.ExerciseId)).Result.Succeeded;
        }
    }

    public class TeamMemberResolver : IValueResolver<TeamDTO, Team, bool>
    {
        private IAuthorizationService _authorizationService;
        private ClaimsPrincipal _user;

        public TeamMemberResolver(IAuthorizationService authorizationService, IUserClaimsService userClaimsService)
        {
            _authorizationService = authorizationService;
            _user = userClaimsService.GetCurrentClaimsPrincipal();
        }

        public bool Resolve(TeamDTO source, Team destination, bool member, ResolutionContext context)
        {
            return _authorizationService.AuthorizeAsync(_user, null, new TeamMemberRequirement(source.Id)).Result.Succeeded;
        }
    }

    public class PrimaryTeamResolver : IValueResolver<TeamDTO, Team, bool>
    {
        private IAuthorizationService _authorizationService;
        private ClaimsPrincipal _user;

        public PrimaryTeamResolver(IAuthorizationService authorizationService, IUserClaimsService userClaimsService)
        {
            _authorizationService = authorizationService;
            _user = userClaimsService.GetCurrentClaimsPrincipal();
        }

        public bool Resolve(TeamDTO source, Team destination, bool member, ResolutionContext context)
        {
            return _authorizationService.AuthorizeAsync(_user, null, new PrimaryTeamRequirement(source.Id)).Result.Succeeded;
        }
    }
}


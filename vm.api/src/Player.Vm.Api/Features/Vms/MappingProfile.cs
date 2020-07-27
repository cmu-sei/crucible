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
using Player.Vm.Api.Domain.Services;
using Player.Vm.Api.Infrastructure.Extensions;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Player.Vm.Api.Features.Vms
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Models.Vm, Vm>()
                .ForMember(dest => dest.TeamIds, opt => opt.MapFrom(src => src.VmTeams.Select(x => x.TeamId)));

            CreateMap<VmUpdateForm, Domain.Models.Vm>();

            CreateMap<VmCreateForm, Domain.Models.Vm>()
                .ForMember(dest => dest.VmTeams, opt => opt.MapFrom(src => src.TeamIds.Select(x => new Domain.Models.VmTeam(x, src.Id.Value))));
        }
    }

    public class VmOwnerResolver : IValueResolver<Domain.Models.Vm, Vm, bool>
    {
        private ClaimsPrincipal _user;

        public VmOwnerResolver(IPrincipal user)
        {
            _user = user as ClaimsPrincipal;
        }

        public bool Resolve(Domain.Models.Vm source, Vm destination, bool member, ResolutionContext context)
        {
            if (source.UserId.HasValue && source.UserId.Value == _user.GetId())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

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
using S3.Player.Api.Infrastructure.Extensions;
using S3.VM.Api.Data.Models;
using S3.VM.Api.Services;
using S3.VM.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace S3.VM.Api.Infrastructure.Mappings
{
    public class VmProfile : AutoMapper.Profile
    {
        public VmProfile()
        {
            CreateMap<VmEntity, ViewModels.Vm>()
                .ForMember(dest => dest.IsOwner, opt => opt.ResolveUsing<VmOwnerResolver>())           
                .ForMember(dest => dest.AllowedNetworks, opt => opt.MapFrom(src => src.AllowedNetworks.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()))     
                .ForMember(dest => dest.ExerciseId, opt => opt.ResolveUsing<ExerciseIdResolver>());

            CreateMap<VmEntity, ViewModels.VmSummary>()
                .ForMember(dest => dest.TeamIds, opt => opt.MapFrom(src => src.VmTeams.Select(x => x.TeamId)));

            CreateMap<ViewModels.VmUpdateForm, VmEntity>()
                .ForMember(dest => dest.AllowedNetworks, opt => opt.MapFrom(src => String.Join(" ", src.AllowedNetworks))); 

            CreateMap<ViewModels.VmCreateForm, VmEntity>()
                .ForMember(dest => dest.VmTeams,         opt => opt.MapFrom(src => src.TeamIds.Select(x => new VmTeamEntity(x, src.Id.Value))))
                .ForMember(dest => dest.AllowedNetworks, opt => opt.MapFrom(src => String.Join(" ", src.AllowedNetworks)));
        }
    }

    public class VmOwnerResolver : IValueResolver<VmEntity, ViewModels.Vm, bool>
    {
        private ClaimsPrincipal _user;

        public VmOwnerResolver(IPrincipal user)
        {
            _user = user as ClaimsPrincipal;
        }

        public bool Resolve(VmEntity source, ViewModels.Vm destination, bool member, ResolutionContext context)
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

    public class ExerciseIdResolver : IValueResolver<VmEntity, ViewModels.Vm, Guid>
    {
        private IPlayerService _playerService;

        public ExerciseIdResolver(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Guid Resolve(VmEntity source, ViewModels.Vm destination, Guid destMember, ResolutionContext context)
        {
            return _playerService.GetCurrentExerciseId();
        }
    }
}


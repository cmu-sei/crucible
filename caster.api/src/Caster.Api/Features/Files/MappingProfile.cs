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
using AutoMapper;

namespace Caster.Api.Features.Files
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Domain.Models.File, File>()
                .ForMember(m => m.Content, opt => opt.ExplicitExpansion())
                .ForMember(m => m.ModifiedByName, opt => opt.MapFrom((src, dest) => src.ModifiedBy.Name))
                .ForMember(m => m.LockedByName, opt => opt.MapFrom((src) => src.LockedBy.Name));
            CreateMap<Create.Command, Domain.Models.File>();
            CreateMap<Edit.Command, Domain.Models.File>();
            CreateMap<PartialEdit.Command, Domain.Models.File>()
                .ForMember(dest => dest.WorkspaceId, opt => opt.MapFrom((src, dest) => src.WorkspaceId.HasValue ? src.WorkspaceId.Value : dest.WorkspaceId))
                .ForAllOtherMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Domain.Models.FileVersion, FileVersion>()
                .ForMember(m => m.Content, opt => opt.ExplicitExpansion())
                .ForMember(m => m.ModifiedByName, opt => opt.MapFrom((src, dest) => src.ModifiedBy.Name))
                .ForMember(m => m.TaggedByName, opt => opt.MapFrom((src) => src.TaggedBy.Name));
            CreateMap<Domain.Models.File, Domain.Models.FileVersion>()
                .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}

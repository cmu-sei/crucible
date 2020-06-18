/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.Player.Api.Data.Data.Models;
using S3.Player.Api.ViewModels;
using System.Net;

namespace S3.Player.Api.Infrastructure.Mappings
{
    public class ApplicationProfile : AutoMapper.Profile
    {
        public ApplicationProfile()
        {
            CreateMap<ApplicationTemplateEntity, ApplicationTemplate>().ReverseMap();

            CreateMap<ApplicationTemplateForm, ApplicationTemplateEntity>();

            CreateMap<ApplicationEntity, Application>().ReverseMap();

            CreateMap<ApplicationInstanceEntity, ApplicationInstanceForm>().ReverseMap();

            CreateMap<ApplicationInstanceEntity, ApplicationInstance>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    (src.Application.Name ?? src.Application.Template.Name ?? string.Empty)
                        .Replace("{viewId}", src.Application.ViewId.ToString())
                        .Replace("{viewName}", src.Application.View.Name ?? string.Empty)
                        .Replace("{teamId}", src.TeamId.ToString())
                        .Replace("{teamName}", src.Team.Name ?? string.Empty)))

                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src =>
                    src.Application.Icon ?? (src.Application.Template != null ? src.Application.Template.Icon: string.Empty)))

                .ForMember(dest => dest.Embeddable, opt => opt.MapFrom(src =>
                    src.Application.Embeddable ?? (src.Application.Template != null ? src.Application.Template.Embeddable : false)))

                .ForMember(dest => dest.LoadInBackground, opt => opt.MapFrom(src =>
                    src.Application.LoadInBackground ?? (src.Application.Template != null ? src.Application.Template.LoadInBackground : false)))

                .ForMember(dest => dest.Url, opt => opt.MapFrom(src =>
                    (src.Application.Url ?? src.Application.Template.Url ?? string.Empty)
                        .Replace("{viewId}", src.Application.ViewId.ToString())
                        .Replace("{viewName}", src.Application.View.Name != null ? WebUtility.UrlEncode(src.Application.View.Name) : string.Empty)
                        .Replace("{teamId}", src.TeamId.ToString())
                        .Replace("{teamName}", src.Team.Name != null ? WebUtility.UrlEncode(src.Team.Name) : string.Empty)))

                .ForMember(dest => dest.ViewId, opt => opt.MapFrom(src =>
                    src.Application.ViewId));
        }
    }
}

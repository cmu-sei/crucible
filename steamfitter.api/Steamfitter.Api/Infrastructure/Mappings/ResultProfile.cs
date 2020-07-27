/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Steamfitter.Api.Data.Models;
using Steamfitter.Api.ViewModels;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace Steamfitter.Api.Infrastructure.Mappings
{
    public class ResultProfile : AutoMapper.Profile
    {
        public ResultProfile()
        {
            CreateMap<ResultEntity, Result>()
                .ForMember(dest => dest.ActionParameters, m => m.MapFrom(src => ConvertToActionParameters(src)));

            CreateMap<Result, ResultEntity>()
                .ForMember(dest => dest.InputString, m => m.MapFrom(src => ConvertToInputString(src.ActionParameters)));;
        }

        private Dictionary<string, string> ConvertToActionParameters(ResultEntity src)
        {
            var parameters = new Dictionary<string, string>();
            try
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(src.InputString);
            }
            catch (Exception ex)
            {
                parameters["BadInputString"] = src.InputString;
                Console.WriteLine($"Error mapping InputString for Result {src.Id} of Task {src.TaskId}");
            }

            return parameters;
        }

        private string ConvertToInputString(Dictionary<string, string> actionParameters)
        {
            var inputString = JsonSerializer.Serialize(actionParameters);

            return inputString;
        }

    }
}



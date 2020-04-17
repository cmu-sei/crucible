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
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Caster.Api.Infrastructure.Serialization;

namespace Caster.Api.Domain.Models
{
    public class GitlabModule
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonPropertyName("path_with_namespace")]
        public string Path { get; set; }
        public string Description { get; set; }

        [JsonPropertyName("last_activity_at")]
        public DateTime LastActivityAt { get; set; }

        [JsonPropertyName("http_url_to_repo")]
        public string RepoUrl { get; set; }

        public Module ToModule(DateTime requestTime)
        {
            return new Module() {
                Name = this.Name,
                Path = this.Path,
                Description = this.Description,
                DateModified = requestTime
            };
        }
    }

    public class GitlabRelease
    {
        [JsonPropertyName("tag_name")]
        public string Name { get; set; }
    }

    public static class GitlabModuleVariableResponse
    {
        public static List<ModuleVariable> GetModuleVariables(byte[] jsonResponse)
        {
            List<ModuleVariable> moduleVariables = new List<ModuleVariable>();

            var variables = JsonSerializer
                .Deserialize<Dictionary<string, Dictionary<string, GitlabModuleVariable>>>(
                    jsonResponse,
                    DefaultJsonSettings.Settings);

            foreach (var outerPair in variables)
            {
                if (outerPair.Key == "variable") {
                    foreach (var innerPair in outerPair.Value)
                    {
                        innerPair.Value.Name = innerPair.Key;
                        moduleVariables.Add(innerPair.Value.ToModuleVariable());
                    }
                }
            }

            return moduleVariables;
        }
    }

    public class GitlabModuleVariable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        [JsonConverter(typeof(NumberToStringConverter))]
        public string Default { get; set; }

        public ModuleVariable ToModuleVariable()
        {
            return new ModuleVariable() {
                Name = this.Name,
                Description = this.Description,
                VariableType = this.Type,
                DefaultValue = this.Default
            };
        }
    }

    public static class GitlabModuleOutputResponse
    {
        public static List<string> GetModuleOutputs(byte[] jsonResponse)
        {
            List<string> moduleOutputs = new List<string>();

            var outputs = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, Dictionary<string, GitlabModuleOutput>>>(
                jsonResponse, DefaultJsonSettings.Settings);

            foreach (var outerPair in outputs)
            {
                if (outerPair.Key == "output") {
                    foreach (var innerPair in outerPair.Value)
                    {
                        innerPair.Value.Name = innerPair.Key;
                        moduleOutputs.Add(innerPair.Value.ToString());
                    }
                }
            }

            return moduleOutputs;
        }
    }

    public class GitlabModuleOutput
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Description}";
        }
    }
}


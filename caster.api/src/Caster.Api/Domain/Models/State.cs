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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Caster.Api.Infrastructure.Serialization;

namespace Caster.Api.Domain.Models
{
    public class State
    {
        public TFResource[] Resources { get; set; } = new TFResource[0];

        public Resource[] GetResources()
        {
            var resources = new List<Resource>();

            foreach (var res in this.Resources.Where(r => r.Mode == "managed"))
            {
                foreach (var instance in res.Instances)
                {
                    var index = "";

                    if (instance.Index_Key != null)
                    {
                        if (double.TryParse(instance.Index_Key, out double result))
                        {
                            index = $"[{instance.Index_Key}]";
                        }
                        else
                        {
                            index = $"[\"{instance.Index_Key}\"]";
                        }
                    }

                    var resource = new Resource
                    {
                        Attributes = instance.Attributes,
                        Address = $"{res.Address}{index}",
                        BaseAddress = $"{res.Address}",
                        Id = instance.Attributes.GetProperty("id").GetString(),
                        Name = instance.Attributes.TryGetProperty("name", out JsonElement element) ? element.GetString() : res.Name,
                        Tainted = instance.Status == "tainted",
                        Type = res.Type
                    };

                    resources.Add(resource);
                }
            }

            return resources.ToArray();
        }
    }

    public class TFResource
    {
        public string Mode { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
        public string Module { get; set; } = string.Empty;

        public string Address
        {
            get
            {
                var module = string.IsNullOrEmpty(this.Module) ?
                    string.Empty : $"{this.Module}.";

                return $"{module}{this.Type}.{this.Name}";
            }
        }

        public Instance[] Instances { get; set; }
    }

    public class Instance
    {
        [JsonConverter(typeof(NumberToStringConverter))]
        public string Index_Key { get; set; }
        public string Status { get; set; }
        public JsonElement Attributes { get; set; }
    }
}


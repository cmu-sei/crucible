/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.CodeAnalysis;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caster.Api.Infrastructure.Serialization
{
    public class OptionalConverter : JsonConverter<Optional<Guid?>>
    {
        public override Optional<Guid?> Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new Optional<Guid?>(null);
            }

            if (reader.TokenType == JsonTokenType.None)
            {
                return new Optional<Guid?>();
            }

            Guid innerValue;

            if (reader.TryGetGuid(out innerValue))
            {
                return new Optional<Guid?>(innerValue);
            }
            else
            {
                return new Optional<Guid?>(null);
            }
        }

        public override void Write(Utf8JsonWriter writer, Optional<Guid?> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

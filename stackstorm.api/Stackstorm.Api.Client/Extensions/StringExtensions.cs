/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Stackstorm.Api.Client.Extensions
{
    public static class StringExtensions
    {
        public class BetterToken
        {
            public string TokenId { get; set; }
            public JToken JToken { get; set; }
        }
        
        public static IList<JToken> ToJTokens(this string o)
        {
            var tokens = new List<JToken>();
            var j = JObject.Parse(o);
            foreach (var node in j["result"].Children())
                tokens.Add(node);
            return tokens;
        }

        public static JObject ToJObject(this string o)
        {
            return JObject.Parse(o);
        }

        public static string ToSt2Array(this IEnumerable<string> strings)
        {
            return $"[\"{string.Join("\",\"", strings).Trim()}\"]";
        }
    }
}

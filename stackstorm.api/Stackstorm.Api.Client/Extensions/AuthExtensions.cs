/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Net.Http;
using Stackstorm.Api.Client.Exceptions;
using Stackstorm.Api.Client.Models;

namespace Stackstorm.Api.Client.Extensions
{
    /// <summary> An authentication extensions. </summary>
    public static class AuthExtensions
    {
        /// <summary>
        ///  A HttpClient extension method that adds an x-auth-token to the client headers
        /// </summary>
        /// <param name="client"> The client to act on. </param>
        /// <param name="token">  The token. </param>
        public static void AddXAuthToken(this HttpClient client, TokenResponse token)
        {
            if (token == null)
                throw new InvalidTokenException("Please login first, or could not find a login token.");

            client.DefaultRequestHeaders.Add("x-auth-token", token.token);
        }
    }
}

/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stackstorm.Api.Client.Models;

namespace Stackstorm.Api.Client.Apis
{
    /// <summary> The packs api. </summary>
    /// <seealso cref="T:Stackstorm.Api.Client.Apis.IPacksApi"/>
    public class PacksApi : IPacksApi
    {
        private ISt2Client _host;

        /// <summary>
        ///  Initializes a new instance of the Stackstorm.Api.Client.Apis.PacksApi class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null. </exception>
        /// <param name="host"> The host. </param>
        public PacksApi(ISt2Client host)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            _host = host;
        }

        /// <summary> Get a list of packs. </summary>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IPacksApi.GetPacksAsync()"/>
        public async Task<IList<Pack>> GetPacksAsync()
        {
            return await _host.GetApiRequestAsync<List<Pack>>("/v1/packs");
        }

        /// <summary> Gets packs by name. </summary>
        /// <param name="packName"> Name of the pack. </param>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IPacksApi.GetPacksByNameAsync(string)"/>
        public async Task<IList<Pack>> GetPacksByNameAsync(string packName)
        {
            return await _host.GetApiRequestAsync<List<Pack>>("/v1/packs?name=" + packName);
        }

        /// <summary> Gets packs by identifier. </summary>
        /// <param name="packId"> Identifier for the pack. </param>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        /// <seealso cref="M:Stackstorm.Api.Client.Apis.IPacksApi.GetPacksByIdAsync(string)"/>
        public async Task<IList<Pack>> GetPacksByIdAsync(string packId)
        {
            return await _host.GetApiRequestAsync<List<Pack>>("/v1/packs?ref=" + packId);
        }
    }
}

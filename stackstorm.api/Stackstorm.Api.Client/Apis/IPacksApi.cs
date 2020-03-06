/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Stackstorm.Api.Client.Models;

namespace Stackstorm.Api.Client.Apis
{
    /// <summary> Interface for packs API. </summary>
    public interface IPacksApi
    {
        /// <summary> Get a list of packs. </summary>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        Task<IList<Pack>> GetPacksAsync();

        /// <summary> Gets packs by name. </summary>
        /// <param name="packName"> Name of the pack. </param>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        Task<IList<Pack>> GetPacksByNameAsync(string packName);

        /// <summary> Gets packs by identifier. </summary>
        /// <param name="packId"> Identifier for the pack. </param>
        /// <returns> A List of <see cref="Pack"/>. </returns>
        Task<IList<Pack>> GetPacksByIdAsync(string packId);
    }
}

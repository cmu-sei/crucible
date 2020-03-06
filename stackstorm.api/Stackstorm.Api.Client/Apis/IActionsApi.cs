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
    using Action = Action;

    /// <summary>Interface for actions API. </summary>
    public interface IActionsApi
    {
        /// <summary>Get all available Actions. </summary>
        /// <returns>A List of <see cref="Action"/>. </returns>
        Task<IList<Action>> GetActionsAsync();

        /// <summary>Gets actions for pack. </summary>
        /// <param name="pack">The pack name. </param>
        /// <returns>A List of <see cref="Action"/>. </returns>
        Task<IList<Action>> GetActionsForPackAsync(string pack);

        Task<IList<Action>> GetActionsForPackByNameAsync(string pack, string name);

        /// <summary>Gets actions by name. </summary>
        /// <param name="name">The action name. </param>
        /// <returns>A List of <see cref="Action"/>. </returns>
        Task<IList<Action>> GetActionsByNameAsync(string name);

        /// <summary>Deletes the action described by actionId. </summary>
        /// <param name="actionId">can be either the ID (e.g. 1 or the ref e.g. mypack.myaction). </param>
        Task DeleteActionAsync(string actionId);

        /// <summary>Creates a new action. </summary>
        /// <param name="action">The <see cref="Action"/> to create. </param>
        Task<Action> CreateActionAsync(CreateAction action);
    }
}

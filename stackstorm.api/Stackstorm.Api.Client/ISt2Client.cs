/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System.Threading.Tasks;
using stackstorm.api.client.Executions;
using Stackstorm.Api.Client.Apis;

namespace Stackstorm.Api.Client
{
    /// <summary> Interface for StackStorm API client. </summary>
    public interface ISt2Client
    {
        /// <summary> Refresh the auth token. </summary>
        Task RefreshTokenAsync();

        /// <summary> Make an asynchronous GET request to the API. </summary>
        /// <typeparam name="TResponseType"> Expected Type of the response. </typeparam>
        /// <param name="url"> URL of the GET request. </param>
        /// <returns> The Typed response. </returns>
        Task<TResponseType> GetApiRequestAsync<TResponseType>(string url);

        /// <summary> Make an asynchronous POST request to the API. </summary>
        /// <typeparam name="TResponseType"> Expected Type of the response. </typeparam>
        /// <typeparam name="TRequestType">  Expected Type of of the request message. </typeparam>
        /// <param name="url">    URL of the POST request. </param>
        /// <param name="request"> The request. </param>
        /// <returns> The Typed response. </returns>
        Task<TResponseType> PostApiRequestAsync<TResponseType, TRequestType>(string url, TRequestType request);

        /// <summary> Make a DELETE request to the API. </summary>
        /// <param name="url"> URL of the request. </param>
        Task DeleteApiRequestAsync(string url);

        /// <summary> Accessor for the Actions related methods </summary>
        /// <value> The actions accessor. </value>
        IActionsApi Actions { get; }

        /// <summary> Accessor for the Packs related methods. </summary>
        /// <value> The Packs accessor. </value>
        IPacksApi Packs { get; }

        /// <summary> Accessor for the Executions related methods. </summary>
        /// <value> The Executions accessor. </value>
        IExecutionsApi Executions { get; }

        /// <summary> Accessor for the Rules related methods. </summary>
        /// <value> The Rules accessor. </value>
        IRulesApi Rules { get; }
        
        IVSphere VSphere { get; }
        ICore Core { get; }

        void Dispose();

        /// <summary> Query if this object has a token. </summary>
        /// <returns> true if token, false if not. </returns>
        bool HasToken();
    }
}

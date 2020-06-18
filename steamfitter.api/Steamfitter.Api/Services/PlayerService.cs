/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.Player.Api;
using S3.Player.Api.Models;
using System.Collections.Generic;
using System.Threading;
using STT = System.Threading.Tasks;


namespace Steamfitter.Api.Services
{
    public interface IPlayerService
    {
        STT.Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct);
    }

    public class PlayerService : IPlayerService
    {
        private readonly IS3PlayerApiClient _s3PlayerApiClient;

        public PlayerService(
            IS3PlayerApiClient s3PlayerApiClient)
        {
            _s3PlayerApiClient = s3PlayerApiClient;
        }       

        public async STT.Task<IEnumerable<View>> GetViewsAsync(CancellationToken ct)
        {
            var views = await _s3PlayerApiClient.GetMyViewsAsync(ct);
            return (IEnumerable<View>)views;
        }

    }
}


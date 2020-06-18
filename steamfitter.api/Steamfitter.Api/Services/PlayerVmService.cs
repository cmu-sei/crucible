/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using S3.VM.Api;
using S3.VM.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using STT = System.Threading.Tasks;

namespace Steamfitter.Api.Services
{
    public interface IPlayerVmService
    {
        STT.Task<IEnumerable<Vm>> GetViewVmsAsync(Guid viewId, CancellationToken ct);
    }

    public class PlayerVmService : IPlayerVmService
    {
        private readonly IS3VmApiClient _s3VmApiClient;
        private readonly Guid _userId;

        public PlayerVmService(
            IS3VmApiClient s3VmApiClient)
        {
            _s3VmApiClient = s3VmApiClient;
        }       

        public async STT.Task<IEnumerable<Vm>> GetViewVmsAsync(Guid viewId, CancellationToken ct)
        {
            var vms = await _s3VmApiClient.GetViewVmsAsync(viewId, null, true, false, ct);
            return (IEnumerable<Vm>)vms;
        }

    }
}


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
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Steamfitter.Api.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using STT = System.Threading.Tasks;
using Steamfitter.Api.Services;
using Steamfitter.Api.Infrastructure.Exceptions;

namespace Steamfitter.Api.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class EngineHub : Hub
    {
        //private readonly CancellationToken _ct;
        //private readonly IResultService _resultService;

        public EngineHub()
        {
            //CancellationTokenSource source = new CancellationTokenSource();
            //_ct = source.Token;
        }

        public async STT.Task Join(string myIdentity)
        {
            // TODO: log the identity of this client.
            // perhaps associate certain VM's with this client?
            // perhaps have one client per view?
            return;
        }

        public async STT.Task ReceivedResults(IEnumerable<Result> results)
        {
            // await _resultService.MarkTasksSentAsync(results, _ct);
        }

        public async STT.Task TaskResults(IEnumerable<Result> results)
        {
            // await _resultService.UpdateActualOutputsAsync(results, _ct);
        }
    }
}


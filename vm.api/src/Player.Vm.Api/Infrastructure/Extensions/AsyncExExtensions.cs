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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;

namespace Player.Vm.Api.Infrastructure.Extensions
{
    public static class AsyncExExtensions
    {
        public static async Task<bool> WaitAsync(this AsyncAutoResetEvent mEvent, TimeSpan timeout, CancellationToken token = default)
        {
            var timeOut = new CancellationTokenSource(timeout);
            var comp = CancellationTokenSource.CreateLinkedTokenSource(timeOut.Token, token);

            try
            {
                await mEvent.WaitAsync(comp.Token).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException e)
            {
                if (token.IsCancellationRequested)
                    throw; //Forward OperationCanceledException from external Token
                return false; //Here the OperationCanceledException was raised by Timeout
            }
        }
    }
}

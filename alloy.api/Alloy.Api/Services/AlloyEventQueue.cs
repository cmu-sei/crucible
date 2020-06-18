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
using System.Collections.Concurrent;
using System.Threading;
using Alloy.Api.Data.Models;

namespace Alloy.Api.Services
{

    public interface IAlloyEventQueue
    {
        void Add(EventEntity eventEntity);

        EventEntity Take(CancellationToken cancellationToken);
    }

    public class AlloyEventQueue : IAlloyEventQueue
    {
        private BlockingCollection<EventEntity> _eventQueue = new BlockingCollection<EventEntity>();

        public void Add(EventEntity eventEntity)
        {
            if (eventEntity == null)
            {
                throw new ArgumentNullException(nameof(eventEntity));
            }
            _eventQueue.Add(eventEntity);
        }

        public EventEntity Take(CancellationToken cancellationToken)
        {
            return _eventQueue.Take(cancellationToken);
        }
    }

}

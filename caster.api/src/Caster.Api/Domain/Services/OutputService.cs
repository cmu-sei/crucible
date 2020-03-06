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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Caster.Api.Utilities.Synchronization;
using Nito.AsyncEx;

namespace Caster.Api.Domain.Services
{
    public interface IOutputService
    {
        Output GetOutput(Guid objectId);
        Output GetOrAddOutput(Guid objectId);
        void RemoveOutput(Guid objectId);
    }

    public class OutputService : IOutputService
    {
        private ConcurrentDictionary<Guid, Output> _outputs = new ConcurrentDictionary<Guid, Output>();

        public OutputService()
        {
        }

        public Output GetOutput(Guid objectId)
        {
            Output output;

            if (_outputs.TryGetValue(objectId, out output))
            {
                return output;
            }
            else
            {
                return null;
            }
        }

        public Output GetOrAddOutput(Guid objectId)
        {
            return _outputs.GetOrAdd(objectId, new Output());
        }

        public void RemoveOutput(Guid objectId)
        {
            _outputs.Remove(objectId, out _);
        }
    }

    public class Output
    {
        private Object _lock { get; } = new Object();

        private string _content = string.Empty;
        public string Content
        {
            get
            {
                lock (_lock)
                {
                    return _content;
                }
            }
        }

        private bool _complete;
        public bool Complete
        {
            get
            {
                lock (_lock)
                {
                    return _complete;
                }
            }
        }

        private List<AsyncAutoResetEvent> ResetEvents { get; set; } = new List<AsyncAutoResetEvent>();

        public void AddLine(string output)
        {
            lock (_lock)
            {
                _content += output + Environment.NewLine;

                foreach (var resetEvent in this.ResetEvents)
                {
                    resetEvent.Set();
                }
            }
        }

        public void SetCompleted()
        {
            lock (_lock)
            {
                _complete = true;

                foreach (var resetEvent in this.ResetEvents)
                {
                    resetEvent.Set();
                }
            }
        }

        public AsyncAutoResetEvent Subscribe()
        {
            lock (_lock)
            {
                var resetEvent = new AsyncAutoResetEvent(false);
                this.ResetEvents.Add(resetEvent);
                return resetEvent;
            }
        }

        public void Unsubscribe(AsyncAutoResetEvent resetEvent)
        {
            lock (_lock)
            {
                this.ResetEvents.Remove(resetEvent);
            }
        }
    }
}

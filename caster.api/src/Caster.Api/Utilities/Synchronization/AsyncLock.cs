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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Caster.Api.Utilities.Synchronization
{
    public class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore;

        public AsyncLock()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public TaskWrapper<AsyncLockResult> LockAsync(int timeout) =>
            new TaskWrapper<AsyncLockResult>(LockInternalAsync(timeout));

        private async Task<AsyncLockResult> LockInternalAsync(int timeout)
        {
            bool acquiredLock = await _semaphore.WaitAsync(timeout);
            return new AsyncLockResult(_semaphore, acquiredLock);
        }

        public class AsyncLockResult : IDisposable
        {
            public bool AcquiredLock { get; }

            private readonly SemaphoreSlim _semaphore;

            public AsyncLockResult(SemaphoreSlim semaphore, bool acquiredLock)
            {
                _semaphore = semaphore;
                this.AcquiredLock = acquiredLock;
            }

            public void Dispose()
            {
                if (this.AcquiredLock)
                    _semaphore.Release();
            }
        }
    }

    public struct TaskWrapper<T>
    {
        private readonly Task<T> _task;

        public TaskWrapper(Task<T> task)
        {
            _task = task;
        }

        public TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();
    }
}

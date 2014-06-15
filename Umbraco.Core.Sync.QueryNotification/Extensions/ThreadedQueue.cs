using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Core.Sync.QueryNotification.Extensions
{
    public class ThreadedQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _waitTime;

        public ThreadedQueue(int waitTime = 20)
        {
            _waitTime = waitTime;
            Start();
        }

        private void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (_queue.Count == 0)
                    {
                        Thread.Sleep(_waitTime);
                        continue;
                    }

                    var item = _queue.Dequeue();
                    try
                    {
                        if (OnEnqueue != null)
                            OnEnqueue(this, item);
                    }
                    catch (Exception ex)
                    {
                        if (OnError != null)
                            OnError(item, ex);
                    }
                }
            });
        }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
        }

        public event EventHandler<T> OnEnqueue;
        public event EventHandler<Exception> OnError;
    }
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collection is queue.")]
    public sealed class HubMessageQueue : IReadOnlyCollection<HubMessage>, IDisposable
    {
        private readonly ConcurrentQueue<HubMessage> _queue = new ConcurrentQueue<HubMessage>();
        private readonly IObservable<HubMessage> _messages;
        private IDisposable _subscription;
        private volatile TaskCompletionSource<bool> _receiveMessageTask = new TaskCompletionSource<bool>();

        public HubMessageQueue(IObservable<HubMessage> messages)
        {
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _subscription = messages.Subscribe(new HubMessageHandler(this));
        }

        public int Count => _queue.Count;

        public async Task<HubMessage> GetNextAsync()
        {
            while (true)
            {
                var receiveMessageTask = _receiveMessageTask.Task;
                if (_queue.TryDequeue(out var message))
                {
                    return message;
                }

                var moreMessages = await receiveMessageTask.ConfigureAwait(false);

                if (!moreMessages)
                {
                    // Observable is completed and there will be no mor messages.
                    return null;
                }
            }
        }

        public bool TryDequeue(out HubMessage message)
        {
            return _queue.TryDequeue(out message);
        }

        public IEnumerator<HubMessage> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        public void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        private void NotifyMessageReceived(bool moreMessages)
        {
            var receiveMessageTask = _receiveMessageTask;
            _receiveMessageTask = new TaskCompletionSource<bool>();
            receiveMessageTask.SetResult(moreMessages);
        }

        private class HubMessageHandler : IObserver<HubMessage>
        {
            private readonly HubMessageQueue _parent;

            public HubMessageHandler(HubMessageQueue parent)
            {
                _parent = parent;
            }

            public void OnNext(HubMessage value)
            {
                if (!(value is PingMessage))
                {
                    _parent._queue.Enqueue(value);
                    _parent.NotifyMessageReceived(true);
                }
            }

            public void OnCompleted()
            {
                _parent.NotifyMessageReceived(false);
            }

            public void OnError(Exception error)
            {
            }
        }
    }
}

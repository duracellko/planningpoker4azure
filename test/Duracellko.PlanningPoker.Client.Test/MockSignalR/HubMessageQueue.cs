using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Collection is queue.")]
public sealed class HubMessageQueue : IReadOnlyCollection<HubMessage>, IDisposable
{
    private readonly ConcurrentQueue<HubMessage> _queue = new ConcurrentQueue<HubMessage>();
    private IDisposable? _subscription;
    private volatile TaskCompletionSource<(bool, Exception?)> _receiveMessageTask = new TaskCompletionSource<(bool, Exception?)>();

    public HubMessageQueue(IObservable<HubMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        _subscription = messages.Subscribe(new HubMessageHandler(this));
    }

    public int Count => _queue.Count;

    public async Task<HubMessage?> GetNextAsync()
    {
        var moreMessages = true;
        Exception? error = null;
        while (true)
        {
            var receiveMessageTask = _receiveMessageTask.Task;
            if (_queue.TryDequeue(out var message))
            {
                return message;
            }

            if (moreMessages)
            {
                (moreMessages, error) = await receiveMessageTask.ConfigureAwait(false);
            }
            else
            {
                // Observable is completed and there will be no more messages.
                if (error != null)
                {
                    throw error;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public bool TryDequeue([MaybeNullWhen(false)] out HubMessage message)
    {
        return _queue.TryDequeue(out message);
    }

    public IEnumerator<HubMessage> GetEnumerator() => _queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();

    public void Dispose()
    {
        if (_subscription != null)
        {
            _subscription.Dispose();
            _subscription = null;
        }
    }

    private void NotifyMessageReceived(bool moreMessages, Exception? error)
    {
        _receiveMessageTask.SetResult((moreMessages, error));

        if (moreMessages)
        {
            // If there are more messages then create new TaskCompletionSource for next message.
            _receiveMessageTask = new TaskCompletionSource<(bool, Exception?)>();
        }
    }

    private sealed class HubMessageHandler : IObserver<HubMessage>
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
                _parent.NotifyMessageReceived(true, null);
            }
        }

        public void OnCompleted()
        {
            _parent.NotifyMessageReceived(false, null);
        }

        public void OnError(Exception error)
        {
            _parent.NotifyMessageReceived(false, error);
        }
    }
}

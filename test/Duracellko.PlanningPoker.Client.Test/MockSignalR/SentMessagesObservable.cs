using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal sealed class SentMessagesObservable : IObservable<HubMessage>
    {
        private readonly PipeReader _reader;
        private readonly HubMessageStore _messageStore;
        private readonly List<IObserver<HubMessage>> _observers = new List<IObserver<HubMessage>>();

        internal SentMessagesObservable(PipeReader reader, HubMessageStore messageStore)
        {
            _reader = reader;
            _messageStore = messageStore;
        }

        public IDisposable Subscribe(IObserver<HubMessage> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            _observers.Add(observer);
            return new Subscriber(this, observer);
        }

        public async Task ReadMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var buffer = result.Buffer;
                ReadMessages(ref buffer);
                _reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            await _reader.CompleteAsync().ConfigureAwait(false);
            Complete();
        }

        private void ReadMessages(ref ReadOnlySequence<byte> buffer)
        {
            while (buffer.Length >= 8)
            {
                var messageIdBuffer = buffer.Slice(0, 8);
                Span<byte> messageIdBytes = stackalloc byte[8];
                messageIdBuffer.CopyTo(messageIdBytes);

                long messageId = BitConverter.ToInt64(messageIdBytes);
                ReadMessage(messageId);

                buffer = buffer.Slice(8);
            }
        }

        private void ReadMessage(long messageId)
        {
            if (_messageStore.TryGetMessage(messageId, out var message))
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(message);
                }

                _messageStore.TryRemove(messageId);
            }
        }

        private void Complete()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        private sealed class Subscriber : IDisposable
        {
            private readonly SentMessagesObservable _parent;
            private readonly IObserver<HubMessage> _observer;

            public Subscriber(SentMessagesObservable parent, IObserver<HubMessage> observer)
            {
                _parent = parent;
                _observer = observer;
            }

            public void Dispose()
            {
                _parent._observers.Remove(_observer);
            }
        }
    }
}

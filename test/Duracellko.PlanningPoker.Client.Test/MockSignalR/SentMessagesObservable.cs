using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal sealed class SentMessagesObservable : IObservable<HubMessage>
    {
        private readonly PipeReader _reader;
        private readonly HubMessageStore _messageStore;
        private readonly ConcurrentDictionary<long, IObserver<HubMessage>> _observers = new ConcurrentDictionary<long, IObserver<HubMessage>>();
        private long _nextId;

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

            long observerId = Interlocked.Increment(ref _nextId);
            if (!_observers.TryAdd(observerId, observer))
            {
                throw new InvalidOperationException("Observer ID should be unique.");
            }

            return new Subscriber(this, observerId);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All exceptions are sent to observers.")]
        internal async Task ReadMessages(CancellationToken cancellationToken)
        {
            bool completed = false;
            try
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
            }
            catch (Exception ex)
            {
                SetError(ex);
                completed = true;
            }
            finally
            {
                await _reader.CompleteAsync().ConfigureAwait(false);
                if (!completed)
                {
                    Complete();
                }
            }
        }

        private void ReadMessages(ref ReadOnlySequence<byte> buffer)
        {
            Span<byte> messageIdBytes = stackalloc byte[8];
            while (buffer.Length >= 8)
            {
                var messageIdBuffer = buffer.Slice(0, 8);
                messageIdBuffer.CopyTo(messageIdBytes);
                long messageId = BitConverter.ToInt64(messageIdBytes);
                ReadMessage(messageId);

                buffer = buffer.Slice(8);
            }
        }

        private void ReadMessage(long messageId)
        {
            var message = _messageStore[messageId];
            foreach (var observer in GetObservers())
            {
                observer.OnNext(message);
            }

            _messageStore.TryRemove(messageId);
        }

        private void Complete()
        {
            foreach (var observer in GetObservers())
            {
                observer.OnCompleted();
            }
        }

        private void SetError(Exception error)
        {
            foreach (var observer in GetObservers())
            {
                observer.OnError(error);
            }
        }

        private IEnumerable<IObserver<HubMessage>> GetObservers()
        {
            return _observers.ToArray().Select(p => p.Value);
        }

        private sealed class Subscriber : IDisposable
        {
            private readonly SentMessagesObservable _parent;
            private readonly long _observerId;

            public Subscriber(SentMessagesObservable parent, long observerId)
            {
                _parent = parent;
                _observerId = observerId;
            }

            public void Dispose()
            {
                _parent._observers.TryRemove(_observerId, out _);
            }
        }
    }
}

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR;

internal sealed class InMemoryTransport : IDuplexPipe, IDisposable
{
    private readonly Pipe _receivePipe = new Pipe();
    private readonly Pipe _sendPipe = new Pipe();
    private readonly HubMessageStore _messageStore;
    private readonly SentMessagesObservable _sentMessagesObservable;
    private readonly CancellationTokenSource _closeCancellationToken = new CancellationTokenSource();
    private readonly IDisposable _sentMessagesSubscription;
    private bool _messageReadingStarted;

    public InMemoryTransport(HubMessageStore messageStore)
    {
        _messageStore = messageStore;
        _sentMessagesObservable = new SentMessagesObservable(_sendPipe.Reader, messageStore);

        _sentMessagesSubscription = _sentMessagesObservable.Subscribe(new HubMessageHandler(this));
    }

    public PipeReader Input => _receivePipe.Reader;

    public PipeWriter Output => _sendPipe.Writer;

    public IObservable<HubMessage> SentMessages => _sentMessagesObservable;

    public void Dispose()
    {
        _closeCancellationToken.Cancel();

        // When reading of messages is started then wait until it is completed (cancelled by the token).
        // Otherwise subscription to sent messages can be closed immediately.
        if (!_messageReadingStarted)
        {
            DisposeSubscription();
        }
    }

    internal async Task ReceiveMessage(HubMessage message, CancellationToken cancellationToken)
    {
        var messageId = _messageStore.Add(message);

        static void WriteLong(IBufferWriter<byte> buffer, long value)
        {
            var span = buffer.GetSpan(8);
            BitConverter.TryWriteBytes(span, value);
            buffer.Advance(8);
        }

        var writer = _receivePipe.Writer;
        WriteLong(writer, messageId);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"", Justification = "Receiving messages is not controlled by a consumer.")]
    internal async void OpenChannel()
    {
        try
        {
            var cancellationToken = _closeCancellationToken.Token;
            if (!cancellationToken.IsCancellationRequested)
            {
                await ProvideServerHandshake(_sendPipe.Reader, _receivePipe.Writer, cancellationToken).ConfigureAwait(false);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _messageReadingStarted = true;
                await _sentMessagesObservable.ReadMessages(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Server channel is stopped by OperationCanceledException.
        }
    }

    private static async Task ProvideServerHandshake(PipeReader reader, PipeWriter writer, CancellationToken cancellationToken)
    {
        bool isHandshakeCompleted = false;
        while (!isHandshakeCompleted)
        {
            var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var buffer = result.Buffer;
            if (HandshakeProtocol.TryParseRequestMessage(ref buffer, out var requestMessage))
            {
                var responseMessage = HandshakeResponseMessage.Empty;
                if (requestMessage.Protocol != nameof(MessageStoreHubProtocol))
                {
                    responseMessage = new HandshakeResponseMessage($"Protocol '{requestMessage.Protocol}' is not supported.");
                }

                reader.AdvanceTo(buffer.Start, buffer.Start);

                HandshakeProtocol.WriteResponseMessage(responseMessage, writer);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                isHandshakeCompleted = true;
            }
            else
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
            }

            if (result.IsCompleted)
            {
                break;
            }
        }
    }

    private void DisposeSubscription()
    {
        _closeCancellationToken.Dispose();
        _sentMessagesSubscription.Dispose();
    }

    private sealed class HubMessageHandler : IObserver<HubMessage>
    {
        private readonly InMemoryTransport _parent;

        public HubMessageHandler(InMemoryTransport parent)
        {
            _parent = parent;
        }

        public void OnNext(HubMessage value)
        {
            // This observer just disposes InMemoryTransport after receiving last message.
        }

        public void OnCompleted()
        {
            _parent.DisposeSubscription();
        }

        public void OnError(Exception error)
        {
            _parent.DisposeSubscription();
        }
    }
}

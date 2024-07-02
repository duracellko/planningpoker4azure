using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR;

internal sealed class MessageStoreHubProtocol : IHubProtocol
{
    private readonly HubMessageStore _messageStore;

    public MessageStoreHubProtocol(HubMessageStore messageStore)
    {
        _messageStore = messageStore;
    }

    public string Name => nameof(MessageStoreHubProtocol);

    public int Version => 0;

    public TransferFormat TransferFormat => TransferFormat.Binary;

    public ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        long id = _messageStore.Add(message);
        return BitConverter.GetBytes(id);
    }

    public void WriteMessage(HubMessage message, IBufferWriter<byte> output)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(output);

        long messageId = _messageStore.Add(message);
        var messageIdBytes = output.GetSpan(8);
        BitConverter.TryWriteBytes(messageIdBytes, messageId);
        output.Advance(8);
    }

    public bool TryParseMessage(ref ReadOnlySequence<byte> input, IInvocationBinder binder, [MaybeNullWhen(false)] out HubMessage message)
    {
        if (input.Length < 8)
        {
            message = null;
            return false;
        }

        var messageIdBuffer = input.Slice(0, 8);
        Span<byte> messageIdBytes = stackalloc byte[8];
        messageIdBuffer.CopyTo(messageIdBytes);

        long messageId = BitConverter.ToInt64(messageIdBytes);
        message = _messageStore[messageId];
        _messageStore.TryRemove(messageId);

        input = input.Slice(8);
        return true;
    }

    public bool IsVersionSupported(int version) => true;
}

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.MockSignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Implements IAsyncDisposable.")]
    internal sealed class PlanningPokerSignalRClientFixture : IAsyncDisposable
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);

        private readonly CancellationTokenSource _timeoutCancellationToken;
        private bool _disposed;

        public PlanningPokerSignalRClientFixture()
        {
            Mock = new MockHubConnection();
            SentMessages = new HubMessageQueue(Mock.SentMessages);
            Target = new PlanningPokerSignalRClient(Mock.HubConnectionBuilder);
            _timeoutCancellationToken = System.Diagnostics.Debugger.IsAttached ? new CancellationTokenSource() : new CancellationTokenSource(_timeout);
        }

        public MockHubConnection Mock { get; }

        public PlanningPokerSignalRClient Target { get; }

        public HubMessageQueue SentMessages { get; }

        public CancellationToken CancellationToken => _timeoutCancellationToken.Token;

        public Task ReceiveMessage(HubMessage message)
        {
            return Mock.ReceiveMessage(message, CancellationToken);
        }

        public Task<HubMessage> GetSentMessage()
        {
            return SentMessages.GetNextAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                Target.Dispose();
                SentMessages.Dispose();
                await Mock.DisposeAsync();
                _disposed = true;
            }
        }
    }
}

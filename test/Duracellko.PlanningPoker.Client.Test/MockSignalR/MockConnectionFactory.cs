using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal class MockConnectionFactory : IConnectionFactory
    {
        private readonly QueueTransport _transport;

        public MockConnectionFactory(QueueTransport transport)
        {
            _transport = transport;
        }

        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            _transport.OpenChannel();
            return new ValueTask<ConnectionContext>(new MockConnectionContext(_transport));
        }
    }
}

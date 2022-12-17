using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal sealed class MockConnectionFactory : IConnectionFactory
    {
        private readonly InMemoryTransport _transport;

        public MockConnectionFactory(InMemoryTransport transport)
        {
            _transport = transport;
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is returned and disposed outside.")]
        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            _transport.OpenChannel();
            return new ValueTask<ConnectionContext>(new MockConnectionContext(_transport));
        }
    }
}

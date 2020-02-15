using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    public sealed class MockHubConnection : IDisposable
    {
        private readonly HubMessageStore _messageStore = new HubMessageStore();
        private readonly QueueTransport _transport;
        private readonly Lazy<HubConnection> _hubConnection;

        private IServiceProvider _serviceProvider;
        private ILoggerFactory _loggerFactory;

        public MockHubConnection()
        {
            _transport = new QueueTransport(_messageStore);
            _hubConnection = new Lazy<HubConnection>(CreateHubConnection, LazyThreadSafetyMode.None);
            HubConnectionBuilder = new MockHubConnectionBuilder(this);
        }

        public HubConnection HubConnection => _hubConnection.Value;

        public IHubConnectionBuilder HubConnectionBuilder { get; }

        public IObservable<HubMessage> SentMessages => _transport.SentMessages;

        public Task ReceiveMessage(HubMessage message, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return _transport.ReceiveMessage(message, cancellationToken);
        }

        public void Dispose()
        {
            if (_hubConnection.IsValueCreated)
            {
                _hubConnection.Value.DisposeAsync().Wait();
            }

            _transport.Dispose();
            _loggerFactory?.Dispose();
            (_serviceProvider as IDisposable)?.Dispose();
        }

        private HubConnection CreateHubConnection()
        {
            var connectionFactory = new MockConnectionFactory(_transport);
            var protocol = new MessageStoreHubProtocol(_messageStore);
            _serviceProvider = HubConnectionBuilder.Services.BuildServiceProvider();
            _loggerFactory = _serviceProvider.GetService<ILoggerFactory>() ?? new LoggerFactory();
            return new HubConnection(connectionFactory, protocol, new MockEndPoint(), _serviceProvider, _loggerFactory);
        }
    }
}

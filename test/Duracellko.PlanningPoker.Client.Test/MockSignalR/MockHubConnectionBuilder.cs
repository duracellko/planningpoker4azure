using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR;

internal sealed class MockHubConnectionBuilder : IHubConnectionBuilder
{
    private readonly MockHubConnection _mockHubConnection;

    public MockHubConnectionBuilder(MockHubConnection mockHubConnection)
    {
        _mockHubConnection = mockHubConnection;
    }

    public IServiceCollection Services { get; } = new ServiceCollection();

    public HubConnection Build() => _mockHubConnection.HubConnection;
}

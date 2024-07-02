using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.Web;

public sealed class AzurePlanningPokerNodeService : IHostedService, IDisposable
{
    private readonly PlanningPokerAzureNode _node;

    public AzurePlanningPokerNodeService(PlanningPokerAzureNode node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _node.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _node.Stop();
    }

    public void Dispose()
    {
        _node.Dispose();
    }
}

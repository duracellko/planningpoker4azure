using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Web;

public sealed class AzurePlanningPokerNodeService : IHostedService, IDisposable
{
    private static readonly Action<ILogger, Exception?> _logErrorStart = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(0, "ErrorStartAzurePlanningPokerNode"),
        "Starting Azure PlanningPoker Node failed.");

    private readonly PlanningPokerAzureNode _node;
    private readonly ILogger _logger;

    public AzurePlanningPokerNodeService(PlanningPokerAzureNode node, ILogger<AzurePlanningPokerNodeService> logger)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Log error and continue without Azure node service.")]
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _node.Start();
        }
        catch (Exception ex)
        {
            _logErrorStart(_logger, ex);
        }
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

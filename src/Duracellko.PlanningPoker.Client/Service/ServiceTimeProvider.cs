using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Objects provides difference between client and server time.
/// </summary>
public class ServiceTimeProvider : IServiceTimeProvider
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(5);

    private readonly IPlanningPokerClient _planningPokerClient;
    private readonly DateTimeProvider _dateTimeProvider;
    private DateTime _lastUpdateTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTimeProvider"/> class.
    /// </summary>
    /// <param name="planningPokerClient">Planning poker client to obtain time from server.</param>
    /// <param name="dateTimeProvider">The provider of current time.</param>
    public ServiceTimeProvider(IPlanningPokerClient planningPokerClient, DateTimeProvider dateTimeProvider)
    {
        _planningPokerClient = planningPokerClient ?? throw new ArgumentNullException(nameof(planningPokerClient));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    /// <summary>
    /// Gets the difference between client and server time.
    /// </summary>
    /// <remarks>
    /// This value is used to convert time in service responses to client time.
    /// This solves problems, when client time is not correctly set.
    /// </remarks>
    public TimeSpan ServiceTimeOffset { get; private set; }

    /// <summary>
    /// Obtains server time and updates <see cref="ServiceTimeOffset"/> value.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Asynchronous operation.</returns>
    /// <remarks>
    /// The value is updated only, when it is older than 5 minutes.
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "On fail ServiceOffsetTime is updated next time.")]
    public async Task UpdateServiceTimeOffset(CancellationToken cancellationToken)
    {
        if (_dateTimeProvider.UtcNow <= _lastUpdateTime.Add(UpdateInterval))
        {
            return;
        }

        try
        {
            var timeResult = await _planningPokerClient.GetCurrentTime(cancellationToken);
            var utcNow = _dateTimeProvider.UtcNow;
            ServiceTimeOffset = timeResult.CurrentUtcTime - utcNow;
            _lastUpdateTime = utcNow;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

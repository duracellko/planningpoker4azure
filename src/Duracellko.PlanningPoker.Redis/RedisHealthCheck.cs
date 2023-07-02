using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Azure.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Duracellko.PlanningPoker.Redis
{
    /// <summary>
    /// The object provides health status of Redis database.
    /// </summary>
    public sealed class RedisHealthCheck : IHealthCheck, IDisposable
    {
        private readonly IAzurePlanningPokerConfiguration _configuration;
        private readonly object _redisLock = new object();
        private ConnectionMultiplexer? _redis;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisHealthCheck"/> class.
        /// </summary>
        /// <param name="configuration">The configuration with connection string to Redis database.</param>
        public RedisHealthCheck(IAzurePlanningPokerConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private string ConnectionString
        {
            get
            {
                var connectionString = _configuration.ServiceBusConnectionString!;
                if (connectionString.StartsWith("REDIS:", StringComparison.Ordinal))
                {
                    connectionString = connectionString.Substring(6);
                }

                return connectionString;
            }
        }

        /// <summary>
        /// Runs the health check, returning the status of the Redis database.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>The health status of the Redis database.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All errors are reported as unhealthy status.")]
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RedisHealthCheck));
            }

            try
            {
                var redis = await Connect();
                await redis.GetDatabase().PingAsync();
                return HealthCheckResult.Healthy(Resources.Health_RedisHealthy);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(Resources.Health_RedisUnhealthy, ex);
            }
        }

        /// <summary>
        /// Closes Redis connection and releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_redisLock)
            {
                if (_redis != null)
                {
                    _redis.Dispose();
                    _redis = null;
                }
            }

            _disposed = true;
        }

        private async Task<ConnectionMultiplexer> Connect()
        {
            if (_redis != null)
            {
                return _redis;
            }

            var redis = await ConnectionMultiplexer.ConnectAsync(ConnectionString);

            var keepConnection = false;
            lock (_redisLock)
            {
                if (_redis == null)
                {
                    _redis = redis;
                    keepConnection = true;
                }
            }

            if (!keepConnection)
            {
                await redis.CloseAsync();
                await redis.DisposeAsync();
            }

            return _redis;
        }
    }
}

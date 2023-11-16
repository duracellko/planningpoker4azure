using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duracellko.PlanningPoker.Health
{
    /// <summary>
    /// The object provides health status of <see cref="Data.IScrumTeamRepository"/> object.
    /// </summary>
    public class ScrumTeamRepositoryHealthCheck : IHealthCheck
    {
        private static readonly CompositeFormat _healthRepositoryHealthy = CompositeFormat.Parse(Resources.Health_RepositoryHealthy);

        private readonly IScrumTeamRepository _scrumTeamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamRepositoryHealthCheck"/> class.
        /// </summary>
        /// <param name="scrumTeamRepository">The Scrum Team repository to provide health check for.</param>
        public ScrumTeamRepositoryHealthCheck(IScrumTeamRepository scrumTeamRepository)
        {
            _scrumTeamRepository = scrumTeamRepository ?? throw new ArgumentNullException(nameof(scrumTeamRepository));
        }

        /// <summary>
        /// Runs the health check, returning the status of the Scrum Team repository.
        /// </summary>
        /// <param name="context">A context object associated with the current execution.</param>
        /// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> that can be used to cancel the health check.</param>
        /// <returns>The health status of the Scrum Team repository.</returns>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "All errors are reported as unhealthy status.")]
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = _scrumTeamRepository.ScrumTeamNames.Count();
                return Task.FromResult(HealthCheckResult.Healthy(string.Format(CultureInfo.InvariantCulture, _healthRepositoryHealthy, count)));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(Resources.Health_RepositoryUnhealthy, ex));
            }
        }
    }
}

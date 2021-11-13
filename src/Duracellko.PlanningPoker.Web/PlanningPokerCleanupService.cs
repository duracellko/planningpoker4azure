using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.Web
{
    public sealed class PlanningPokerCleanupService : IHostedService, IDisposable
    {
        private readonly IPlanningPoker _planningPoker;
        private readonly IScrumTeamRepository _teamRepository;
        private readonly IPlanningPokerConfiguration _configuration;

        private System.Timers.Timer? _cleanupTimer;

        public PlanningPokerCleanupService(IPlanningPoker planningPoker, IScrumTeamRepository teamRepository, IPlanningPokerConfiguration configuration)
        {
            _planningPoker = planningPoker ?? throw new ArgumentNullException(nameof(planningPoker));
            _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var timerInterval = _configuration.ClientInactivityCheckInterval;

            _cleanupTimer = new System.Timers.Timer(timerInterval.TotalMilliseconds);
            _cleanupTimer.Elapsed += new ElapsedEventHandler(CleanupTimerOnElapsed);
            _cleanupTimer.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_cleanupTimer != null)
            {
                _cleanupTimer.Dispose();
                _cleanupTimer = null;
            }
        }

        private void CleanupTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            DisconnectInactiveMembers();
            DeleteExpiredScrumTeams();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore error and try next time.")]
        private void DisconnectInactiveMembers()
        {
            try
            {
                _planningPoker.DisconnectInactiveObservers();
            }
            catch (Exception)
            {
                // ignore and try next time
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore error and try next time.")]
        private void DeleteExpiredScrumTeams()
        {
            try
            {
                _teamRepository.DeleteExpiredScrumTeams();
            }
            catch (Exception)
            {
                // ignore and try next time
            }
        }
    }
}

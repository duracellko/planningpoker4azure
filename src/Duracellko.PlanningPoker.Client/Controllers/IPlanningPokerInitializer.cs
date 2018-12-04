using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Initializes a new planning poker game (<see cref="PlanningPokerController"/>).
    /// </summary>
    public interface IPlanningPokerInitializer
    {
        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="scrumTeam">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        void InitializeTeam(ScrumTeam scrumTeam, string username);

        /// <summary>
        /// Initialize <see cref="PlanningPokerController"/> object with Scrum Team data received from server.
        /// </summary>
        /// <param name="teamInfo">Scrum Team data received from server.</param>
        /// <param name="username">Name of user joining the Scrum Team.</param>
        /// <remarks>This method overloads setup additional information after reconnecting to existing team.</remarks>
        void InitializeTeam(ReconnectTeamResult teamInfo, string username);
    }
}

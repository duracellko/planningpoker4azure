using System;
using System.Collections.Generic;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Provides services of planning poker scrum teams manager required by Azure node.
    /// </summary>
    public interface IAzurePlanningPoker : IPlanningPoker
    {
        /// <summary>
        /// Gets an observable object sending messages from all Scrum teams.
        /// </summary>
        IObservable<ScrumTeamMessage> ObservableMessages { get; }

        /// <summary>
        /// Gets the date time provider to provide current date-time.
        /// </summary>
        DateTimeProvider DateTimeProvider { get; }

        /// <summary>
        /// Gets the GUID provider to provide new GUID objects.
        /// </summary>
        GuidProvider GuidProvider { get; }

        /// <summary>
        /// Sets collection of Scrum team names, which exists in the Azure and need to be initialized in this node.
        /// </summary>
        /// <param name="teamNames">The list of team names.</param>
        void SetTeamsInitializingList(IEnumerable<string> teamNames);

        /// <summary>
        /// Inserts existing Scrum team into collection and marks the team as initialized in this node.
        /// </summary>
        /// <param name="team">The Scrum team to insert.</param>
        void InitializeScrumTeam(ScrumTeam team);

        /// <summary>
        /// Specifies that all teams are initialized and ready to use by this node.
        /// </summary>
        void EndInitialization();
    }
}

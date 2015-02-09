// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Defines methods to manage Scrum teams.
    /// </summary>
    public interface IPlanningPoker
    {
        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        IEnumerable<string> ScrumTeamNames { get; }

        /// <summary>
        /// Creates new Scrum team with specified Scrum master.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>The new Scrum team.</returns>
        IScrumTeamLock CreateScrumTeam(string teamName, string scrumMasterName);

        /// <summary>
        /// Adds existing Scrum team to collection of teams.
        /// </summary>
        /// <param name="team">The Scrum team to add.</param>
        /// <returns>The joined Scrum team.</returns>
        IScrumTeamLock AttachScrumTeam(ScrumTeam team);

        /// <summary>
        /// Gets existing Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>The Scrum team.</returns>
        IScrumTeamLock GetScrumTeam(string teamName);

        /// <summary>
        /// Calls specified callback when an observer receives new message or after configured timeout.
        /// </summary>
        /// <param name="observer">The observer to wait for message to receive.</param>
        /// <param name="callback">
        /// The callback delegate to call when a message is received or after timeout. First parameter specifies if message was received or not
        /// (the timeout occurs). Second parameter specifies observer, who received a message.
        /// </param>
        void GetMessagesAsync(Observer observer, Action<bool, Observer> callback);

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for configured period of time.
        /// </summary>
        void DisconnectInactiveObservers();

        /// <summary>
        /// Disconnects all observers, who did not checked for messages for specified period of time.
        /// </summary>
        /// <param name="inactivityTime">The inactivity time.</param>
        void DisconnectInactiveObservers(TimeSpan inactivityTime);
    }
}

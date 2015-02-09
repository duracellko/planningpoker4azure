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
    /// When implemented, the object can manage locking of a Scrum team. Locking is used, so that only one thread access or
    /// modifies Scrum team at time.
    /// </summary>
    public interface IScrumTeamLock : IDisposable
    {
        /// <summary>
        /// Gets the Scrum team associated to the lock.
        /// </summary>
        /// <value>The Scrum team.</value>
        ScrumTeam Team { get; }

        /// <summary>
        /// Locks the Scrum team, so that other threads are not able to access the team.
        /// </summary>
        void Lock();
    }
}

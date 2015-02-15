// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Data
{
    /// <summary>
    /// Repository to access storage of ScrumTeam objects.
    /// </summary>
    public interface IScrumTeamRepository
    {
        /// <summary>
        /// Loads the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>The Scrum team with specified name.</returns>
        ScrumTeam LoadScrumTeam(string teamName);

        /// <summary>
        /// Saves the Scrum team to repository.
        /// </summary>
        /// <param name="team">The Scrum team.</param>
        void SaveScrumTeam(ScrumTeam team);

        /// <summary>
        /// Deletes the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        void DeleteScrumTeam(string teamName);

        /// <summary>
        /// Deletes the expired Scrum teams, which were not used for period of expiration time.
        /// </summary>
        void DeleteExpiredScrumTeams();
    }
}

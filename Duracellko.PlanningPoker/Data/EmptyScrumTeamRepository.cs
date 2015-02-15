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
    /// Scrum team repository that does not actually store nor loads teams. Provides legacy functionality.
    /// </summary>
    public class EmptyScrumTeamRepository : IScrumTeamRepository
    {
        /// <summary>
        /// Gets a collection of Scrum team names.
        /// </summary>
        public IEnumerable<string> ScrumTeamNames
        {
            get { return Enumerable.Empty<string>(); }
        }

        /// <summary>
        /// Loads the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>
        /// The Scrum team with specified name.
        /// </returns>
        public ScrumTeam LoadScrumTeam(string teamName)
        {
            return null;
        }

        /// <summary>
        /// Saves the Scrum team to repository.
        /// </summary>
        /// <param name="team">The Scrum team.</param>
        public void SaveScrumTeam(ScrumTeam team)
        {
            // do nothing
        }

        /// <summary>
        /// Deletes the Scrum team from repository.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        public void DeleteScrumTeam(string teamName)
        {
            // do nothing
        }

        /// <summary>
        /// Deletes the expired Scrum teams, which were not used for period of expiration time.
        /// </summary>
        public void DeleteExpiredScrumTeams()
        {
            // do nothing
        }

                /// <summary>
        /// Deletes all Scrum teams.
        /// </summary>
        public void DeleteAll()
        {
            // do nothing
        }
    }
}

// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Configuration
{
    /// <summary>
    /// Configuration section for planning poker.
    /// </summary>
    public class PlanningPokerConfigurationElement : ConfigurationSection, IPlanningPokerConfiguration
    {
        /// <summary>
        /// Gets or sets the time in seconds, after which is client disconnected when he/she does not check for new messages.
        /// </summary>
        /// <value>The client inactivity timeout.</value>
        [ConfigurationProperty("clientInactivityTimeout", DefaultValue = 900)]
        public int ClientInactivityTimeout
        {
            get { return (int)this["clientInactivityTimeout"]; }
            set { this["clientInactivityTimeout"] = value; }
        }

        /// <summary>
        /// Gets or sets the interval in seconds, how often is executed job to search for and disconnect inactive clients.
        /// </summary>
        /// <value>The client inactivity check interval.</value>
        [ConfigurationProperty("clientInactivityCheckInterval", DefaultValue = 60)]
        public int ClientInactivityCheckInterval
        {
            get { return (int)this["clientInactivityCheckInterval"]; }
            set { this["clientInactivityCheckInterval"] = value; }
        }

        /// <summary>
        /// Gets or sets the time, how long can client wait for new message. Empty message collection is sent to the client after the specified time.
        /// </summary>
        /// <value>The wait for message timeout.</value>
        [ConfigurationProperty("waitForMessageTimeout", DefaultValue = 60)]
        public int WaitForMessageTimeout
        {
            get { return (int)this["waitForMessageTimeout"]; }
            set { this["waitForMessageTimeout"] = value; }
        }

        /// <summary>
        /// Gets or sets the repository folder to store scrum teams.
        /// </summary>
        /// <value>
        /// The repository folder.
        /// </value>
        [ConfigurationProperty("repositoryFolder", DefaultValue = @"App_Data\Teams")]
        public string RepositoryFolder
        {
            get { return (string)this["repositoryFolder"]; }
            set { this["repositoryFolder"] = value; }
        }

        /// <summary>
        /// Gets or sets the time in seconds, after which team (file) is deleted from repository when it is not used.
        /// </summary>
        /// <value>The repository file expiration time.</value>
        [ConfigurationProperty("repositoryTeamExpiration", DefaultValue = 1200)]
        public int RepositoryTeamExpiration
        {
            get { return (int)this["repositoryTeamExpiration"]; }
            set { this["repositoryTeamExpiration"] = value; }
        }

        #region IPlanningPokerConfiguration

        TimeSpan IPlanningPokerConfiguration.ClientInactivityTimeout
        {
            get { return TimeSpan.FromSeconds(this.ClientInactivityTimeout); }
        }

        TimeSpan IPlanningPokerConfiguration.ClientInactivityCheckInterval
        {
            get { return TimeSpan.FromSeconds(this.ClientInactivityCheckInterval); }
        }

        TimeSpan IPlanningPokerConfiguration.WaitForMessageTimeout
        {
            get { return TimeSpan.FromSeconds(this.WaitForMessageTimeout); }
        }

        TimeSpan IPlanningPokerConfiguration.RepositoryTeamExpiration
        {
            get { return TimeSpan.FromSeconds(this.RepositoryTeamExpiration); }
        }

        #endregion
    }
}

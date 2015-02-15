// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Configuration;

namespace Duracellko.PlanningPoker.Data
{
    /// <summary>
    /// Settings for scrum team repository using file system.
    /// </summary>
    public class FileScrumTeamRepositorySettings : IFileScrumTeamRepositorySettings
    {
        private IPlanningPokerConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScrumTeamRepositorySettings"/> class.
        /// </summary>
        /// <param name="configuration">The planning poker configuration.</param>
        public FileScrumTeamRepositorySettings(IPlanningPokerConfiguration configuration)
        {
            this.configuration = configuration ?? new PlanningPokerConfigurationElement();
        }

        /// <summary>
        /// Gets the folder storing scrum team files.
        /// </summary>
        /// <value>
        /// The storage folder.
        /// </value>
        public string Folder
        {
            get
            {
                var result = this.configuration.RepositoryFolder;
                if (string.IsNullOrEmpty(result))
                {
                    result = @"App_Data\Teams";
                }

                result = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, result);
                result = Path.GetFullPath(result);
                return result;
            }
        }
    }
}

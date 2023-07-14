using System;
using System.IO;
using Duracellko.PlanningPoker.Configuration;

namespace Duracellko.PlanningPoker.Data
{
    /// <summary>
    /// Settings for scrum team repository using file system.
    /// </summary>
    public class FileScrumTeamRepositorySettings : IFileScrumTeamRepositorySettings
    {
        private readonly IPlanningPokerConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScrumTeamRepositorySettings"/> class.
        /// </summary>
        /// <param name="configuration">The planning poker configuration.</param>
        public FileScrumTeamRepositorySettings(IPlanningPokerConfiguration configuration)
        {
            _configuration = configuration ?? new PlanningPokerConfiguration();
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
                var result = _configuration.RepositoryFolder;
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

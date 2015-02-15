// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Data;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Duracellko.PlanningPoker.Azure.Data
{
    /// <summary>
    /// Settings for scrum team repository using file system in Microsoft Azure.
    /// </summary>
    public class AzureScrumTeamRepositorySettings : IFileScrumTeamRepositorySettings
    {
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
                return RoleEnvironment.GetLocalResource("ScrumTeamRepository").RootPath;
            }
        }
    }
}

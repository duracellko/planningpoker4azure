using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Provides querying and mananging URI navigation.
    /// </summary>
    public class AppNavigationManager : INavigationManager
    {
        private readonly NavigationManager _navigationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppNavigationManager"/> class.
        /// </summary>
        /// <param name="navigationManager">ASP.NET Core Components NavigationManager.</param>
        public AppNavigationManager(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
        [SuppressMessage("Design", "CA1054:Os parâmetros Uri não devem ser strings", Justification = "De acordo com o NavigationManager do Blazor.")]
        public void NavigateTo(string uri)
        {
            _navigationManager.NavigateTo(uri);
        }
    }
}

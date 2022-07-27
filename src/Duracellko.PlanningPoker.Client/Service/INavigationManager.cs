using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Provides an abstraction for querying and mananging URI navigation.
    /// </summary>
    public interface INavigationManager
    {
        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
        [SuppressMessage("Design", "CA1054:Os parâmetros Uri não devem ser strings", Justification = "De acordo com o NavigationManager do Blazor.")]
        void NavigateTo(string uri);
    }
}

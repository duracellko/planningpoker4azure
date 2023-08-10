using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Provides an abstraction for querying and mananging URI navigation.
    /// </summary>
    public interface INavigationManager
    {
        /// <summary>
        /// Gets the current URI. The <see cref="Uri" /> is always represented as an absolute URI in string form.
        /// </summary>
        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Follows NavigationManager from Blazor.")]
        string Uri { get; }

        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "Follows NavigationManager from Blazor.")]
        void NavigateTo(string uri);
    }
}

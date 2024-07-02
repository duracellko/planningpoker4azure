using System;
using Microsoft.AspNetCore.Components;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Provides URI of Planning Poker services.
/// </summary>
public class PlanningPokerUriProvider : IPlanningPokerUriProvider
{
    private readonly NavigationManager _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerUriProvider"/> class.
    /// </summary>
    /// <param name="navigationManager">ASP.NET Core Components NavigationManager.</param>
    public PlanningPokerUriProvider(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    /// <summary>
    /// Gets base URI of Planning Poker Services.
    /// </summary>
    public Uri? BaseUri
    {
        get
        {
            var baseUri = _navigationManager.BaseUri;
            return string.IsNullOrEmpty(baseUri) ? null : new Uri(baseUri, UriKind.Absolute);
        }
    }
}

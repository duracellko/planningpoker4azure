using System;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Provides URI of Planning Poker services.
    /// </summary>
    public interface IPlanningPokerUriProvider
    {
        /// <summary>
        /// Gets base URI of Planning Poker Services.
        /// </summary>
        Uri BaseUri { get; }
    }
}

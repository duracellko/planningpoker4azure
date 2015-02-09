// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Configuration;

namespace Duracellko.PlanningPoker.Azure.Configuration
{
    /// <summary>
    /// Configuration of Planning Poker for Azure platform.
    /// </summary>
    public interface IAzurePlanningPokerConfiguration : IPlanningPokerConfiguration
    {
        /// <summary>
        /// Gets a time to wait for end of initialization phase.
        /// </summary>
        TimeSpan InitializationTimeout { get; }

        /// <summary>
        /// Gets a time to wait for any message in initialization phase.
        /// </summary>
        TimeSpan InitializationMessageTimeout { get; }

        /// <summary>
        /// Gets a time interval, when a planning poker node notifies other nodes about its activity and checks for inactive subscriptions.
        /// </summary>
        TimeSpan SubscriptionMaintenanceInterval { get; }

        /// <summary>
        /// Gets a time that an inactive subscription is deleted after.
        /// </summary>
        TimeSpan SubscriptionInactivityTimeout { get; }
    }
}

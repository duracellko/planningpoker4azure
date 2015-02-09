// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Configuration;

namespace Duracellko.PlanningPoker.Azure.Configuration
{
    /// <summary>
    /// Configuration section of planning poker for Azure platform.
    /// </summary>
    public class AzurePlanningPokerConfigurationElement : PlanningPokerConfigurationElement, IAzurePlanningPokerConfiguration
    {
        /// <summary>
        /// Gets or sets a time in seconds to wait for end of initialization phase.
        /// </summary>
        /// <value>The initialization wait time.</value>
        [ConfigurationProperty("initializationTimeout", DefaultValue = 60)]
        public int InitializationTimeout
        {
            get { return (int)this["initializationTimeout"]; }
            set { this["initializationTimeout"] = value; }
        }

        /// <summary>
        /// Gets or sets a time in seconds to wait for any message in initialization phase.
        /// </summary>
        /// <value>The initialization message wait time.</value>
        [ConfigurationProperty("initializationMessageTimeout", DefaultValue = 5)]
        public int InitializationMessageTimeout
        {
            get { return (int)this["initializationMessageTimeout"]; }
            set { this["initializationMessageTimeout"] = value; }
        }

        /// <summary>
        /// Gets or sets a time interval in seconds, when a planning poker node notifies other nodes about its activity and checks for inactive subscriptions.
        /// </summary>
        /// <value>The subscription maintenance time interval.</value>
        [ConfigurationProperty("subscriptionMaintenanceInterval", DefaultValue = 300)]
        public int SubscriptionMaintenanceInterval
        {
            get { return (int)this["subscriptionMaintenanceInterval"]; }
            set { this["subscriptionMaintenanceInterval"] = value; }
        }

        /// <summary>
        /// Gets or sets a time in seconds that an inactive subscription is deleted after.
        /// </summary>
        /// <value>The subscription inactivity time.</value>
        [ConfigurationProperty("subscriptionInactivityTimeout", DefaultValue = 900)]
        public int SubscriptionInactivityTimeout
        {
            get { return (int)this["subscriptionInactivityTimeout"]; }
            set { this["subscriptionInactivityTimeout"] = value; }
        }

        #region IAzurePlanningPokerConfiguration

        TimeSpan IAzurePlanningPokerConfiguration.InitializationTimeout
        {
            get { return TimeSpan.FromSeconds(this.InitializationTimeout); }
        }

        TimeSpan IAzurePlanningPokerConfiguration.InitializationMessageTimeout
        {
            get { return TimeSpan.FromSeconds(this.InitializationMessageTimeout); }
        }

        TimeSpan IAzurePlanningPokerConfiguration.SubscriptionMaintenanceInterval
        {
            get { return TimeSpan.FromSeconds(this.SubscriptionMaintenanceInterval); }
        }

        TimeSpan IAzurePlanningPokerConfiguration.SubscriptionInactivityTimeout
        {
            get { return TimeSpan.FromSeconds(this.SubscriptionInactivityTimeout); }
        }

        #endregion
    }
}

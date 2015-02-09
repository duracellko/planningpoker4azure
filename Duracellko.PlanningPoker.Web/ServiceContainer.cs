// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.Web;
#if AZURE
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
#endif
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Service;
using Duracellko.PlanningPoker.Web.Service;
using Microsoft.Practices.Unity;

namespace Duracellko.PlanningPoker.Web
{
    /// <summary>
    /// Registers and configures Dependency Injection container.
    /// </summary>
    public static class ServiceContainer
    {
        #region Properties

        /// <summary>
        /// Gets the Dependency Injection container.
        /// </summary>
        /// <value>The Unity container.</value>
        public static IUnityContainer Container { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the Dependency Injection container.
        /// </summary>
        public static void InitializeContainer()
        {
            if (Container == null)
            {
                var container = new UnityContainer();

#if AZURE
                container.RegisterType<IPlanningPoker, AzurePlanningPokerController>(new ContainerControlledLifetimeManager());
                container.RegisterType<IAzurePlanningPoker, AzurePlanningPokerController>(new ContainerControlledLifetimeManager());
                container.RegisterType<PlanningPokerAzureNode>(new ContainerControlledLifetimeManager());
                container.RegisterType<IPlanningPokerService, PlanningPokerService>();
                container.RegisterType<IServiceBehavior, NoCacheServiceBehavior>("NoCacheServiceBehavior");
                container.RegisterType<IServiceBus, AzureServiceBus>();
                container.RegisterType<IMessageConverter, MessageConverter>();

                var configuration = (AzurePlanningPokerConfigurationElement)ConfigurationManager.GetSection("planningPoker");
                if (configuration == null)
                {
                    configuration = new AzurePlanningPokerConfigurationElement();
                }

                container.RegisterInstance<IAzurePlanningPokerConfiguration>(configuration);
#else
                container.RegisterType<IPlanningPoker, PlanningPokerController>(new ContainerControlledLifetimeManager());
                container.RegisterType<IPlanningPokerService, PlanningPokerService>();
                container.RegisterType<IServiceBehavior, NoCacheServiceBehavior>("NoCacheServiceBehavior");

                var configuration = (PlanningPokerConfigurationElement)ConfigurationManager.GetSection("planningPoker");
                if (configuration == null)
                {
                    configuration = new PlanningPokerConfigurationElement();
                }

                container.RegisterInstance<IPlanningPokerConfiguration>(configuration);
#endif

                Container = container;
            }
        }

        #endregion
    }
}
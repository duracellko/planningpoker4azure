// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
#if AZURE
using Duracellko.PlanningPoker.Azure;
#endif
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Microsoft.Practices.Unity;

namespace Duracellko.PlanningPoker.Web
{
    /// <summary>
    /// Planning Poker HTTP application.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc", Justification = "MVC is Model-View-Controller.")]
    public class PlanningPokerMvcApplication : System.Web.HttpApplication
    {
        #region Fields

        private Timer cleanupTimer;

        #endregion

        #region Protected methods

        /// <summary>
        /// Provides application startup actions.
        /// </summary>
        protected void Application_Start()
        {
            ServiceContainer.InitializeContainer();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

#if AZURE
            ServiceContainer.Container.Resolve<PlanningPokerAzureNode>().Start();
#endif
            this.StartTimer();
        }

        /// <summary>
        /// Provides Application end actions.
        /// </summary>
        protected void Application_End()
        {
            if (this.cleanupTimer != null)
            {
                this.cleanupTimer.Dispose();
                this.cleanupTimer = null;
            }

            ServiceContainer.Container.Dispose();
        }

        #endregion

        #region Private methods

        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }); // Parameter defaults
        }

        private static void CleanupTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            DisconnectInactiveMembers();
            DeleteExpiredScrumTeams();
        }

        private static void DisconnectInactiveMembers()
        {
            try
            {
                ServiceContainer.Container.Resolve<IPlanningPoker>().DisconnectInactiveObservers();
            }
            catch (Exception)
            {
                // ignore and try next time
            }
        }

        private static void DeleteExpiredScrumTeams()
        {
            try
            {
                ServiceContainer.Container.Resolve<IScrumTeamRepository>().DeleteExpiredScrumTeams();
            }
            catch (Exception)
            {
                // ignore and try next time
            }
        }

        private void StartTimer()
        {
            var configuration = (PlanningPokerConfigurationElement)ConfigurationManager.GetSection("planningPoker");
            var timerInterval = configuration != null ? configuration.ClientInactivityCheckInterval : 60;

            this.cleanupTimer = new Timer(timerInterval * 1000);
            this.cleanupTimer.Elapsed += new ElapsedEventHandler(CleanupTimerOnElapsed);
            this.cleanupTimer.Start();
        }

        #endregion
    }
}
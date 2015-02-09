// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Duracellko.PlanningPoker.Web.Controllers
{
    /// <summary>
    /// Planning poker home page controller.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Default action of the controller.
        /// </summary>
        /// <returns>Returns page with planning poker JavaScript control.</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Planning poker main action.
        /// </summary>
        /// <returns>Partial view of planning poker JavaScript control template.</returns>
        public ActionResult PlanningPoker()
        {
            return this.PartialView();
        }
    }
}

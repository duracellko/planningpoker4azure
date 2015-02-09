// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Provides current date and time. Can be overridden to use mock date and time when testing.
    /// </summary>
    public class DateTimeProvider
    {
        #region Fields

        private static readonly DateTimeProvider DefaultProvider = new DateTimeProvider();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default date-time provider that provides system time.
        /// </summary>
        /// <value>The default date-time provider.</value>
        public static DateTimeProvider Default
        {
            get
            {
                return DefaultProvider;
            }
        }

        /// <summary>
        /// Gets the current time expressed as local time.
        /// </summary>
        /// <value>Current date-time.</value>
        public virtual DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// Gets the current time expressed as UTC time.
        /// </summary>
        /// <value>Current date-time.</value>
        public virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        #endregion
    }
}

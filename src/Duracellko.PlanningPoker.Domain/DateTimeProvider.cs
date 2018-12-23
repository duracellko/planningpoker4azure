using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Provides current date and time. Can be overridden to use mock date and time when testing.
    /// </summary>
    public class DateTimeProvider
    {
        private static readonly DateTimeProvider DefaultProvider = new DateTimeProvider();

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
    }
}

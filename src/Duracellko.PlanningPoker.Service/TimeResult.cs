using System;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Response information about service current time.
    /// </summary>
    public class TimeResult
    {
        /// <summary>
        /// Gets or sets current time of service in UTC time zone.
        /// </summary>
        public DateTime CurrentUtcTime { get; set; }
    }
}

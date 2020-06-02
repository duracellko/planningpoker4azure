using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Generates new GUID. Can be overridden to use mock GUID when testing.
    /// </summary>
    public class GuidProvider
    {
        /// <summary>
        /// Gets the default GUID provider that provides unique random GUID.
        /// </summary>
        /// <value>The default GUID provider.</value>
        public static GuidProvider Default { get; } = new GuidProvider();

        /// <summary>
        /// Generates a new GUID.
        /// </summary>
        /// <returns>A new GUID object.</returns>
        public virtual Guid NewGuid() => Guid.NewGuid();
    }
}

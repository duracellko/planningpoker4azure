using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Scrum Team data for serialization.
    /// </summary>
    public class ScrumTeamData
    {
        /// <summary>
        /// Gets or sets the Scrum team name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current Scrum team state.
        /// </summary>
        public TeamState State { get; set; }

        /// <summary>
        /// Gets or sets collection of Scrum Team members.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
        public IList<MemberData> Members { get; set; }

        /// <summary>
        /// Gets or sets the estimation result.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
        public IDictionary<string, Estimation> EstimationResult { get; set; }
    }
}

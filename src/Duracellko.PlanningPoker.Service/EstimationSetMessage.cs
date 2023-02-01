using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Message sent to all members and observers, when the collection of available estimations is changed.
    /// </summary>
    public class EstimationSetMessage : Message
    {
        /// <summary>
        /// Gets or sets the estimations associated to the message.
        /// </summary>
        /// <value>
        /// The estimations collection.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
        public IList<Estimation> Estimations { get; set; } = new List<Estimation>();
    }
}

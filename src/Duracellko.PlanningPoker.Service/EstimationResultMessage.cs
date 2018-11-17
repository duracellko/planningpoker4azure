using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Message sent to all members and observers after all members picked an estimation.
    /// The message contains collection <see cref="EstimationResultItem"/> objects.
    /// </summary>
    [Serializable]
    public class EstimationResultMessage : Message
    {
        /// <summary>
        /// Gets or sets the estimation result items associated to the message.
        /// </summary>
        /// <value>
        /// The estimation result items collection.
        /// </value>
        [JsonProperty("estimationResult")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
        public IList<EstimationResultItem> EstimationResult { get; set; }
    }
}

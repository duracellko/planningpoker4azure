using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Message sent to all members and observers after all members picked an estimation.
/// The message contains collection <see cref="EstimationResultItem"/> objects.
/// </summary>
public class EstimationResultMessage : Message
{
    /// <summary>
    /// Gets or sets the estimation result items associated to the message.
    /// </summary>
    /// <value>
    /// The estimation result items collection.
    /// </value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
    public IList<EstimationResultItem> EstimationResult { get; set; } = new List<EstimationResultItem>();
}

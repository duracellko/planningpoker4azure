// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Message sent to all members and observers after all members picked an estimation.
    /// The message contains collection <see cref="EstimationResultItem"/> objects.
    /// </summary>
    [Serializable]
    [DataContract(Name = "estimationResultMessage", Namespace = Namespaces.PlanningPokerData)]
    public class EstimationResultMessage : Message
    {
        /// <summary>
        /// Gets or sets the estimation result items associated to the message.
        /// </summary>
        /// <value>
        /// The estimation result items collection.
        /// </value>
        [DataMember(Name = "estimationResult")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
        public IList<EstimationResultItem> EstimationResult { get; set; }
    }
}

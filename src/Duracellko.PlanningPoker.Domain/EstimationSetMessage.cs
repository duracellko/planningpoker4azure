using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Message sent to all members and observers, when the collection of available estimations is changed.
/// </summary>
public class EstimationSetMessage : Message
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EstimationSetMessage"/> class.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="estimations">The estimation set associated to the message.</param>
    public EstimationSetMessage(MessageType type, IEnumerable<Estimation> estimations)
        : base(type)
    {
        Estimations = estimations ?? throw new ArgumentNullException(nameof(estimations));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EstimationSetMessage"/> class.
    /// </summary>
    /// <param name="messageData">Message serialization data.</param>
    internal EstimationSetMessage(Serialization.MessageData messageData)
        : base(messageData)
    {
        if (messageData.Estimations == null)
        {
            throw new ArgumentNullException(nameof(messageData));
        }

        Estimations = messageData.Estimations.ToList();
    }

    /// <summary>
    /// Gets the collection of estimations associated to the message.
    /// </summary>
    /// <value>
    /// The estimations.
    /// </value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Message is sent to client and forgotten. Client can modify it as it wants.")]
    public IEnumerable<Estimation> Estimations { get; }

    /// <summary>
    /// Gets serialization data of the object.
    /// </summary>
    /// <returns>The serialization data.</returns>
    protected internal override Serialization.MessageData GetData()
    {
        var result = base.GetData();
        result.Estimations = Estimations.ToList();
        return result;
    }
}

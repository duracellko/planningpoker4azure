using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure;

/// <summary>
/// Message of event in specific Scrum team, which notifies that the collection of available estimations has changed.
/// </summary>
public class ScrumTeamEstimationSetMessage : ScrumTeamMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamEstimationSetMessage"/> class.
    /// </summary>
    public ScrumTeamEstimationSetMessage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScrumTeamEstimationSetMessage"/> class.
    /// </summary>
    /// <param name="teamName">The name of team, this message is related to.</param>
    /// <param name="messageType">The type of message.</param>
    public ScrumTeamEstimationSetMessage(string teamName, MessageType messageType)
        : base(teamName, messageType)
    {
    }

    /// <summary>
    /// Gets or sets the estimations associated to the message.
    /// </summary>
    /// <value>
    /// The estimations collection.
    /// </value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
    public IList<double?> Estimations { get; set; } = [];
}

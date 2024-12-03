using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Service;

/// <summary>
/// Scrum team is a group of members, who play planning poker, and observers, who watch the game.
/// </summary>
public class ScrumTeam
{
    /// <summary>
    /// Gets or sets the Scrum team name.
    /// </summary>
    /// <value>The Scrum team name.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scrum master of the team.
    /// </summary>
    /// <value>The Scrum master.</value>
    public TeamMember? ScrumMaster { get; set; }

    /// <summary>
    /// Gets or sets the collection members joined to the Scrum team.
    /// </summary>
    /// <value>The members collection.</value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
    public IList<TeamMember> Members { get; set; } = [];

    /// <summary>
    /// Gets or sets the observers watching planning poker game of the Scrum team.
    /// </summary>
    /// <value>The observers collection.</value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
    public IList<TeamMember> Observers { get; set; } = [];

    /// <summary>
    /// Gets or sets the current Scrum team state.
    /// </summary>
    /// <value>The team state.</value>
    public TeamState State { get; set; }

    /// <summary>
    /// Gets or sets the available estimations the members can pick from.
    /// </summary>
    /// <value>The collection of available estimations.</value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
    public IList<Estimation> AvailableEstimations { get; set; } = [];

    /// <summary>
    /// Gets or sets the estimation result of last team estimation.
    /// </summary>
    /// <value>
    /// The estimation result items collection.
    /// </value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Data contract has all properties read-write.")]
    public IList<EstimationResultItem>? EstimationResult { get; set; }

    /// <summary>
    /// Gets or sets the collection of participants in current estimation.
    /// </summary>
    /// <value>The collection of estimation participants.</value>
    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "All properties of data contract are read-write.")]
    public IList<EstimationParticipantStatus>? EstimationParticipants { get; set; }

    /// <summary>
    /// Gets or sets the end time of countdown timer, when the timer is started; otherwise <c>null</c>.
    /// </summary>
    public DateTime? TimerEndTime { get; set; }
}

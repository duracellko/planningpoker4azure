namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Type of Planning Poker Azure node message.
    /// </summary>
    public enum NodeMessageType
    {
        /// <summary>
        /// Message of a change in Scrum team.
        /// </summary>
        ScrumTeamMessage,

        /// <summary>
        /// Message specifies that a new Scrum team has been created.
        /// </summary>
        TeamCreated,

        /// <summary>
        /// Message sent by a new node to request for a list of existing Scrum team names.
        /// </summary>
        RequestTeamList,

        /// <summary>
        /// Message sent as response to RequestTeamList message.
        /// </summary>
        TeamList,

        /// <summary>
        /// Message sent by a new node to request for serialized Scrum teams.
        /// </summary>
        RequestTeams,

        /// <summary>
        /// Message sent as response to RequestTeams. One message is sent per one team.
        /// </summary>
        InitializeTeam
    }
}

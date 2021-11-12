namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Item of estimation result. It specifies what member picked what estimation.
    /// </summary>
    public class EstimationResultItem
    {
        /// <summary>
        /// Gets or sets the member, who picked an estimation.
        /// </summary>
        /// <value>
        /// The Scrum team member.
        /// </value>
        public TeamMember? Member { get; set; }

        /// <summary>
        /// Gets or sets the estimation picked by the member.
        /// </summary>
        /// <value>
        /// The picked estimation.
        /// </value>
        public Estimation? Estimation { get; set; }
    }
}

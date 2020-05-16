namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Available estimation card decks that can be used be a team.
    /// </summary>
    public enum Deck
    {
        /// <summary>
        /// Rounded Fibonacci numbers:
        /// 0, 0.5, 1, 2, 3, 5, 8, 13, 20, 40, 100
        /// </summary>
        Standard,

        /// <summary>
        /// Fibonacci numbers:
        /// 0, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89
        /// </summary>
        Fibonacci,

        /// <summary>
        /// Rock, Paper, Scissors, Lizard, Spock
        /// </summary>
        RockPaperScissorsLizardSpock
    }
}

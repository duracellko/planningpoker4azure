namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Available estimation card decks that can be used be a team.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "List is not a sentence.")]
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
    /// Rating 1 - 10:
    /// 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    /// </summary>
    Rating,

    /// <summary>
    /// T-Shirt sizes:
    /// XS, S, M, L, XL
    /// </summary>
    Tshirt,

    /// <summary>
    /// Rock, Paper, Scissors, Lizard, Spock
    /// </summary>
    RockPaperScissorsLizardSpock
}

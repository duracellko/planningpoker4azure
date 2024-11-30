using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Factory object to create selected estimation deck.
/// </summary>
public class DeckProvider
{
    private static readonly CompositeFormat _errorDeckNotSupported = CompositeFormat.Parse(Resources.Error_DeckNotSupported);

    private readonly IEnumerable<Estimation> _standardDeck =
    [
        new(0.0),
        new(0.5),
        new(1.0),
        new(2.0),
        new(3.0),
        new(5.0),
        new(8.0),
        new(13.0),
        new(20.0),
        new(40.0),
        new(100.0),
        new(double.PositiveInfinity),
        new()
    ];

    private readonly IEnumerable<Estimation> _fibonacciDeck =
    [
        new(0.0),
        new(1.0),
        new(2.0),
        new(3.0),
        new(5.0),
        new(8.0),
        new(13.0),
        new(21.0),
        new(34.0),
        new(55.0),
        new(89.0),
        new(double.PositiveInfinity),
        new()
    ];

    private readonly IEnumerable<Estimation> _ratingDeck =
    [
        new(1.0),
        new(2.0),
        new(3.0),
        new(4.0),
        new(5.0),
        new(6.0),
        new(7.0),
        new(8.0),
        new(9.0),
        new(10.0)
    ];

    private readonly IEnumerable<Estimation> _tshirtSizes =
    [
        new(-999509.0), // XS
        new(-999508.0), // S
        new(-999507.0), // M
        new(-999506.0), // L
        new(-999505.0), // XL
    ];

    private readonly IEnumerable<Estimation> _rockPaperScissorsLizardSpock =
    [
        new(-999909.0), // Rock
        new(-999908.0), // Paper
        new(-999907.0), // Scissors
        new(-999906.0), // Lizard
        new(-999905.0), // Spock
    ];

    /// <summary>
    /// Gets the default estimation deck provider.
    /// </summary>
    /// <value>The default estimation deck provider.</value>
    public static DeckProvider Default { get; } = new DeckProvider();

    /// <summary>
    /// Gets a collection of estimation cards for selected deck.
    /// </summary>
    /// <param name="deck">The selected deck to create estimation cards for.</param>
    /// <returns>The collection of estimation cards.</returns>
    public IEnumerable<Estimation> GetDeck(Deck deck)
    {
        return deck switch
        {
            Deck.Standard => _standardDeck,
            Deck.Fibonacci => _fibonacciDeck,
            Deck.Rating => _ratingDeck,
            Deck.Tshirt => _tshirtSizes,
            Deck.RockPaperScissorsLizardSpock => _rockPaperScissorsLizardSpock,
            _ => throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _errorDeckNotSupported, deck), nameof(deck)),
        };
    }

    /// <summary>
    /// Gets default collection of estimation cards.
    /// </summary>
    /// <returns>The collection of estimation cards.</returns>
    public IEnumerable<Estimation> GetDefaultDeck() => GetDeck(Deck.Standard);
}

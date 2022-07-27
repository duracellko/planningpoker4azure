using System;
using System.Collections.Generic;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Factory object to create selected estimation deck.
    /// </summary>
    public class DeckProvider
    {
        private readonly IEnumerable<Estimation> _standardDeck = new Estimation[]
        {
            new Estimation(8.0),
            new Estimation(16.0),
            new Estimation(24.0),
            new Estimation(32.0),
            new Estimation(40.0),
            new Estimation(48.0),
            new Estimation(56.0),
            new Estimation(64.0),
            new Estimation(72.0),
            new Estimation(80.0),
            new Estimation(120.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
        };

        private readonly IEnumerable<Estimation> _fibonacciDeck = new Estimation[]
        {
            new Estimation(3.0),
            new Estimation(5.0),
            new Estimation(8.0),
            new Estimation(13.0),
            new Estimation(21.0),
            new Estimation(34.0),
            new Estimation(55.0),
            new Estimation(89.0),
            new Estimation(144.0),
            new Estimation(233.0),
            new Estimation(377.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
        };

        private readonly IEnumerable<Estimation> _ratingDeck = new Estimation[]
        {
            new Estimation(1.0),
            new Estimation(2.0),
            new Estimation(3.0),
            new Estimation(4.0),
            new Estimation(5.0),
            new Estimation(6.0),
            new Estimation(7.0),
            new Estimation(8.0),
            new Estimation(9.0),
            new Estimation(10.0)
        };

        private readonly IEnumerable<Estimation> _tshirtSizes = new Estimation[]
        {
            new Estimation(-999509.0), // PP
            new Estimation(-999508.0), // P
            new Estimation(-999507.0), // M
            new Estimation(-999506.0), // G
            new Estimation(-999505.0), // GG
        };

        private readonly IEnumerable<Estimation> _rockPaperScissorsLizardSpock = new Estimation[]
        {
            new Estimation(-999909.0), // Pedra
            new Estimation(-999908.0), // Papel
            new Estimation(-999907.0), // Tesoura
            new Estimation(-999906.0), // Lagarto
            new Estimation(-999905.0), // Spock
        };

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
            switch (deck)
            {
                case Deck.Standard:
                    return _standardDeck;
                case Deck.Fibonacci:
                    return _fibonacciDeck;
                case Deck.Rating:
                    return _ratingDeck;
                case Deck.Tshirt:
                    return _tshirtSizes;
                case Deck.RockPaperScissorsLizardSpock:
                    return _rockPaperScissorsLizardSpock;
                default:
                    throw new ArgumentException($"O baralho '{deck}' não é suportado.", nameof(deck));
            }
        }

        /// <summary>
        /// Gets default collection of estimation cards.
        /// </summary>
        /// <returns>The collection of estimation cards.</returns>
        public IEnumerable<Estimation> GetDefaultDeck() => GetDeck(Deck.Standard);
    }
}

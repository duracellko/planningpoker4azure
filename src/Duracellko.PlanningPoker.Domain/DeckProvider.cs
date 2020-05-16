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
            new Estimation(0.0),
            new Estimation(0.5),
            new Estimation(1.0),
            new Estimation(2.0),
            new Estimation(3.0),
            new Estimation(5.0),
            new Estimation(8.0),
            new Estimation(13.0),
            new Estimation(20.0),
            new Estimation(40.0),
            new Estimation(100.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
        };

        private readonly IEnumerable<Estimation> _fibonacciDeck = new Estimation[]
        {
            new Estimation(0.0),
            new Estimation(1.0),
            new Estimation(2.0),
            new Estimation(3.0),
            new Estimation(5.0),
            new Estimation(8.0),
            new Estimation(13.0),
            new Estimation(21.0),
            new Estimation(34.0),
            new Estimation(55.0),
            new Estimation(89.0),
            new Estimation(double.PositiveInfinity),
            new Estimation()
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
                default:
                    throw new ArgumentException($"Deck '{deck}' is not supported.", nameof(deck));
            }
        }

        /// <summary>
        /// Gets default collection of estimation cards.
        /// </summary>
        /// <returns>The collection of estimation cards.</returns>
        public IEnumerable<Estimation> GetDefaultDeck() => GetDeck(Deck.Standard);
    }
}

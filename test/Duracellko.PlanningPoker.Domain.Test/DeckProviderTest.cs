using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class DeckProviderTest
    {
        [TestMethod]
        public void GetDeck_Standard_ReturnsStandardEstimations()
        {
            var result = DeckProvider.Default.GetDeck(Deck.Standard);

            var expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
        }

        [TestMethod]
        public void GetDeck_Fibonacci_ReturnsFibonacciEstimations()
        {
            var result = DeckProvider.Default.GetDeck(Deck.Fibonacci);

            var expectedCollection = new double?[]
            {
                0.0, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 21.0, 34.0, 55.0, 89.0, double.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
        }

        [TestMethod]
        public void GetDefaultDeck_ReturnsStandardEstimations()
        {
            var result = DeckProvider.Default.GetDefaultDeck();

            var expectedCollection = new double?[]
            {
                0.0, 0.5, 1.0, 2.0, 3.0, 5.0, 8.0, 13.0, 20.0, 40.0, 100.0, double.PositiveInfinity, null
            };
            CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
        }
    }
}

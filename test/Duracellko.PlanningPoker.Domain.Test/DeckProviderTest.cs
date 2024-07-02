using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test;

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
    public void GetDeck_Rating_Returns1To10()
    {
        var result = DeckProvider.Default.GetDeck(Deck.Rating);

        var expectedCollection = new double?[]
        {
            1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void GetDeck_Tshirt_ReturnsTshirtSizes()
    {
        var result = DeckProvider.Default.GetDeck(Deck.Tshirt);

        var expectedCollection = new double?[]
        {
            -999509.0, -999508.0, -999507.0, -999506.0, -999505.0
        };
        CollectionAssert.AreEquivalent(expectedCollection, result.Select(e => e.Value).ToList());
    }

    [TestMethod]
    public void GetDeck_RockPaperScissorsLizardSpock_ReturnsEstimations()
    {
        var result = DeckProvider.Default.GetDeck(Deck.RockPaperScissorsLizardSpock);

        var expectedCollection = new double?[]
        {
            -999909.0, -999908.0, -999907.0, -999906.0, -999905.0
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

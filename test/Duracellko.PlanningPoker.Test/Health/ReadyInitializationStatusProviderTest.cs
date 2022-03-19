using Duracellko.PlanningPoker.Health;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Test.Health
{
    [TestClass]
    public class ReadyInitializationStatusProviderTest
    {
        [TestMethod]
        public void IsInitialized_ReturnsTrue()
        {
            var target = new ReadyInitializationStatusProvider();
            var result = target.IsInitialized;
            Assert.IsTrue(result);
        }
    }
}

using System.Threading.Tasks;
using Duracellko.PlanningPoker.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Health
{
    [TestClass]
    public class PlanningPokerControllerHealthCheckTest
    {
        [TestMethod]
        public async Task CheckHealthAsync_Initialized_Healthy()
        {
            // Arrange
            var target = CreateHealthCheck(isInitialized: true);

            // Act
            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

            // Verify
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        [TestMethod]
        public async Task CheckHealthAsync_NotInitialized_Unhealthy()
        {
            // Arrange
            var target = CreateHealthCheck(isInitialized: false);

            // Act
            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

            // Verify
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }

        [TestMethod]
        public async Task CheckHealthAsync_NotInitializedAndThenInitialized_UnhealthyAndThenHealthy()
        {
            var initializationStatusProvider = new Mock<IInitializationStatusProvider>();
            initializationStatusProvider.SetupGet(o => o.IsInitialized).Returns(false);
            var target = CreateHealthCheck(initializationStatusProvider.Object);

            var result = await target.CheckHealthAsync(new HealthCheckContext(), default);
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);

            initializationStatusProvider.SetupGet(o => o.IsInitialized).Returns(true);
            result = await target.CheckHealthAsync(new HealthCheckContext(), default);
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }

        private static PlanningPokerControllerHealthCheck CreateHealthCheck(
            IInitializationStatusProvider? initializationStatusProvider = null,
            bool isInitialized = false)
        {
            if (initializationStatusProvider == null)
            {
                var initializationStatusProviderMock = new Mock<IInitializationStatusProvider>();
                initializationStatusProviderMock.SetupGet(o => o.IsInitialized).Returns(isInitialized);
                initializationStatusProvider = initializationStatusProviderMock.Object;
            }

            return new PlanningPokerControllerHealthCheck(initializationStatusProvider);
        }
    }
}

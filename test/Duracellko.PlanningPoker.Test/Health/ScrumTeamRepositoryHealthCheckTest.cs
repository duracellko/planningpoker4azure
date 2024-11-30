using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Health;

[TestClass]
public class ScrumTeamRepositoryHealthCheckTest
{
    [TestMethod]
    public async Task CheckHealthAsync_RepositoryReturnsEmptyTeamsList_Healthy()
    {
        // Arrange
        var scrumTeamRepository = new Mock<IScrumTeamRepository>();
        scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Returns([]);
        var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

        // Act
        var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

        // Verify
        Assert.AreEqual(HealthStatus.Healthy, result.Status);
    }

    [TestMethod]
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Single use of arrays in tests.")]
    public async Task CheckHealthAsync_RepositoryReturns1Team_Healthy()
    {
        // Arrange
        var scrumTeamRepository = new Mock<IScrumTeamRepository>();
        scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Returns(["MyTeam"]);
        var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

        // Act
        var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

        // Verify
        Assert.AreEqual(HealthStatus.Healthy, result.Status);
    }

    [TestMethod]
    public async Task CheckHealthAsync_RepositoryThrowsException_Unhealthy()
    {
        // Arrange
        var scrumTeamRepository = new Mock<IScrumTeamRepository>();
        scrumTeamRepository.SetupGet(o => o.ScrumTeamNames).Throws(new InvalidOperationException());
        var target = new ScrumTeamRepositoryHealthCheck(scrumTeamRepository.Object);

        // Act
        var result = await target.CheckHealthAsync(new HealthCheckContext(), default);

        // Verify
        Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
    }
}

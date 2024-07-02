using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Service;

[TestClass]
public class ServiceTimeProviderTest
{
    public static IEnumerable<object[]> TestData { get; } = new[]
    {
        new object[] { new DateTime(2021, 11, 18, 10, 21, 2, DateTimeKind.Utc), TimeSpan.Zero },
        new object[] { new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc), TimeSpan.FromSeconds(1) },
        new object[] { new DateTime(2021, 11, 18, 10, 21, 1, DateTimeKind.Utc), TimeSpan.FromSeconds(-1) },
        new object[] { new DateTime(2021, 11, 18, 22, 10, 22, DateTimeKind.Utc), TimeSpan.FromSeconds(42560) },
        new object[] { new DateTime(2021, 11, 18, 9, 59, 58, DateTimeKind.Utc), TimeSpan.FromSeconds(-1264) },
    };

    [TestMethod]
    public void ServiceTimeOffset_AfterConstruction_ZeroOffset()
    {
        var target = CreateServiceTimeProvider();
        var result = target.ServiceTimeOffset;
        Assert.AreEqual(TimeSpan.Zero, result);
    }

    [DataTestMethod]
    [DynamicData(nameof(TestData))]
    public async Task UpdateServiceTimeOffset_ServiceCurrentTime_UpdatesServiceTimeOffset(DateTime serviceTime, TimeSpan expectedOffset)
    {
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 15, 41, DateTimeKind.Utc));
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var timeResult = new TimeResult
        {
            CurrentUtcTime = serviceTime
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(c => dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 2, DateTimeKind.Utc)))
            .ReturnsAsync(timeResult);
        var target = CreateServiceTimeProvider(planningPokerClient.Object, dateTimeProvider);

        await target.UpdateServiceTimeOffset(default);

        planningPokerClient.Verify(o => o.GetCurrentTime(It.IsAny<CancellationToken>()), Times.Once());
        Assert.AreEqual(expectedOffset, target.ServiceTimeOffset);
    }

    [DataTestMethod]
    public async Task UpdateServiceTimeOffset_CallTwiceInTheSameTime_CallsClientOnce()
    {
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 15, 41, DateTimeKind.Utc));
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc)
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>())).ReturnsAsync(timeResult);
        var target = CreateServiceTimeProvider(planningPokerClient.Object, dateTimeProvider);

        await target.UpdateServiceTimeOffset(default);
        await target.UpdateServiceTimeOffset(default);

        planningPokerClient.Verify(o => o.GetCurrentTime(It.IsAny<CancellationToken>()), Times.Once());
    }

    [DataTestMethod]
    public async Task UpdateServiceTimeOffset_CallTwiceAfter5Minutes_CallsClientOnce()
    {
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 15, 41, DateTimeKind.Utc));
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc)
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>())).ReturnsAsync(timeResult);
        var target = CreateServiceTimeProvider(planningPokerClient.Object, dateTimeProvider);

        await target.UpdateServiceTimeOffset(default);
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 20, 41, DateTimeKind.Utc));
        await target.UpdateServiceTimeOffset(default);

        planningPokerClient.Verify(o => o.GetCurrentTime(It.IsAny<CancellationToken>()), Times.Once());
    }

    [DataTestMethod]
    public async Task UpdateServiceTimeOffset_CallTwiceAfter6Minutes_UpdatesServiceTimeOffset()
    {
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 15, 41, DateTimeKind.Utc));
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc)
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(c => dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 2, DateTimeKind.Utc)))
            .ReturnsAsync(timeResult);
        var target = CreateServiceTimeProvider(planningPokerClient.Object, dateTimeProvider);

        await target.UpdateServiceTimeOffset(default);

        Assert.AreEqual(TimeSpan.FromSeconds(1), target.ServiceTimeOffset);

        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 26, 55, DateTimeKind.Utc));
        timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2021, 11, 18, 10, 25, 59, DateTimeKind.Utc)
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(c => dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 26, 56, DateTimeKind.Utc)))
            .ReturnsAsync(timeResult);

        await target.UpdateServiceTimeOffset(default);

        planningPokerClient.Verify(o => o.GetCurrentTime(It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.AreEqual(TimeSpan.FromSeconds(-57), target.ServiceTimeOffset);
    }

    [DataTestMethod]
    public async Task UpdateServiceTimeOffset_ThrowsInvalidOperationExceptionSecondTime_KeepServiceTimeOffsetUnchanged()
    {
        var dateTimeProvider = new DateTimeProviderMock();
        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 8, 15, 41, DateTimeKind.Utc));
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var timeResult = new TimeResult
        {
            CurrentUtcTime = new DateTime(2021, 11, 18, 10, 21, 3, DateTimeKind.Utc)
        };
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(c => dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 21, 2, DateTimeKind.Utc)))
            .ReturnsAsync(timeResult);
        var target = CreateServiceTimeProvider(planningPokerClient.Object, dateTimeProvider);

        await target.UpdateServiceTimeOffset(default);

        Assert.AreEqual(TimeSpan.FromSeconds(1), target.ServiceTimeOffset);

        dateTimeProvider.SetUtcNow(new DateTime(2021, 11, 18, 10, 26, 55, DateTimeKind.Utc));
        planningPokerClient.Setup(o => o.GetCurrentTime(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        await target.UpdateServiceTimeOffset(default);

        planningPokerClient.Verify(o => o.GetCurrentTime(It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.AreEqual(TimeSpan.FromSeconds(1), target.ServiceTimeOffset);
    }

    private static ServiceTimeProvider CreateServiceTimeProvider(IPlanningPokerClient? planningPokerClient = null, DateTimeProvider? dateTimeProvider = null)
    {
        if (planningPokerClient == null)
        {
            var planningPokerClientMock = new Mock<IPlanningPokerClient>();
            planningPokerClient = planningPokerClientMock.Object;
        }

        if (dateTimeProvider == null)
        {
            dateTimeProvider = new DateTimeProviderMock();
        }

        return new ServiceTimeProvider(planningPokerClient, dateTimeProvider);
    }
}

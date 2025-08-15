using System;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Service;

[TestClass]
public class TimerSettingsRepositoryTest
{
    [TestMethod]
    [DataRow("00:00:00", 0)]
    [DataRow("00:00:01", 1)]
    [DataRow("00:01:00", 60)]
    [DataRow("01:00:00", 3600)]
    [DataRow("00:12:34", 754)]
    [DataRow("11:59:59", 43199)]
    [DataRow("05:00", 18000)]
    [DataRow("13:01:00", 46860)]
    [DataRow("1:2:03", 3723)]
    [DataRow("2", 172800)]
    public async Task GetTimerDurationAsync_LoadsStringValue_ReturnsTimeSpan(
        string loadedStringValue,
        int expectedTimeInSeconds)
    {
        var jsRuntime = new Mock<IJSRuntime>();
        var target = new TimerSettingsRepository(jsRuntime.Object);
        jsRuntime.Setup(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", It.IsAny<object?[]>()))
            .ReturnsAsync(loadedStringValue);

        var result = await target.GetTimerDurationAsync();

        jsRuntime.Verify(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", Array.Empty<object?>()));
        Assert.AreEqual(TimeSpan.FromSeconds(expectedTimeInSeconds), result);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task GetTimerDurationAsync_EmptyString_ReturnsNull(string? loadedStringValue)
    {
        var jsRuntime = new Mock<IJSRuntime>();
        var target = new TimerSettingsRepository(jsRuntime.Object);
        jsRuntime.Setup(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", It.IsAny<object?[]>()))
            .ReturnsAsync(loadedStringValue);

        var result = await target.GetTimerDurationAsync();

        jsRuntime.Verify(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", Array.Empty<object?>()));
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow("00h05m00s")]
    [DataRow("hh:mm:ss")]
    public async Task GetTimerDurationAsync_InvalidTimeSpanValue_ReturnsNull(string loadedStringValue)
    {
        var jsRuntime = new Mock<IJSRuntime>();
        var target = new TimerSettingsRepository(jsRuntime.Object);
        jsRuntime.Setup(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", It.IsAny<object?[]>()))
            .ReturnsAsync(loadedStringValue);

        var result = await target.GetTimerDurationAsync();

        jsRuntime.Verify(o => o.InvokeAsync<string?>("Duracellko.PlanningPoker.getTimerDuration", Array.Empty<object?>()));
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(0, "00:00:00")]
    [DataRow(1, "00:00:01")]
    [DataRow(60, "00:01:00")]
    [DataRow(3600, "01:00:00")]
    [DataRow(754, "00:12:34")]
    [DataRow(43199, "11:59:59")]
    public async Task SetTimerDurationAsync_TimeDurationValue_SavesStringValue(
        int timeDurationInSeconds,
        string expectedStringValue)
    {
        var jsRuntime = new Mock<IJSRuntime>();
        var target = new TimerSettingsRepository(jsRuntime.Object);
        string? storedStringValue = null;
        var jsVoidResult = new Mock<IJSVoidResult>();
        jsRuntime.Setup(o => o.InvokeAsync<IJSVoidResult>("Duracellko.PlanningPoker.setTimerDuration", It.IsAny<object?[]>()))
            .Callback<string, object?[]>((_, args) => storedStringValue = (string?)args[0])
            .Returns(new ValueTask<IJSVoidResult>(jsVoidResult.Object));

        await target.SetTimerDurationAsync(TimeSpan.FromSeconds(timeDurationInSeconds));

        jsRuntime.Setup(o => o.InvokeAsync<IJSVoidResult>("Duracellko.PlanningPoker.setTimerDuration", It.Is<object?[]>(a => a.Length == 1)));
        Assert.AreEqual(expectedStringValue, storedStringValue);
    }
}

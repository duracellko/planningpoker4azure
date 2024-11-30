using System;
using System.Threading.Tasks;
using Bunit;
using Duracellko.PlanningPoker.Client.Components;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components;

[TestClass]
public sealed class BusyIndicatorPanelTest : IDisposable
{
    private readonly Bunit.TestContext _context = new Bunit.TestContext();

    public void Dispose()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task ShowBusyIndicator_RunShowBusyIndicatorFunction()
    {
        var busyIndicatorService = new BusyIndicatorService();
        var jsRuntime = new Mock<IJSRuntime>();
        InitializeContext(busyIndicatorService, jsRuntime: jsRuntime.Object);

        using var target = _context.RenderComponent<BusyIndicatorPanel>();

        await target.InvokeAsync(busyIndicatorService.Show);

        jsRuntime.Verify(o => o.InvokeAsync<object>("Duracellko.PlanningPoker.showBusyIndicator", It.Is<object?[]>(args => args.Length == 1 && args[0] is ElementReference)));
    }

    [TestMethod]
    public async Task DisposeBusyIndicator_RunHideFunction()
    {
        var busyIndicatorService = new BusyIndicatorService();
        var jsRuntime = new Mock<IJSRuntime>();
        InitializeContext(busyIndicatorService, jsRuntime: jsRuntime.Object);

        using var target = _context.RenderComponent<BusyIndicatorPanel>();

        IDisposable? disposable = null;
        await target.InvokeAsync(() => disposable = busyIndicatorService.Show());

        Assert.IsNotNull(disposable);
        await target.InvokeAsync(() => disposable.Dispose());

        jsRuntime.Verify(o => o.InvokeAsync<object>("Duracellko.PlanningPoker.hide", It.Is<object?[]>(args => args.Length == 1 && args[0] is ElementReference)));
    }

    private void InitializeContext(BusyIndicatorService busyIndicatorService, IJSRuntime? jsRuntime = null)
    {
        if (jsRuntime == null)
        {
            var jsRuntimeMock = new Mock<IJSRuntime>();
            jsRuntime = jsRuntimeMock.Object;
        }

        _context.Services.AddSingleton(busyIndicatorService);
        _context.Services.AddSingleton(jsRuntime);
    }
}

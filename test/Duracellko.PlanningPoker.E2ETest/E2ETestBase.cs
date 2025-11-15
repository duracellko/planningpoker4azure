using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.E2ETest;

[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposable objects are disposed in TestCleanup.")]
public abstract class E2ETestBase : PageTest
{
    private readonly List<IBrowserContext> _contexts = [];

    [SuppressMessage("Style", "IDE0032:Use auto property", Justification = "Do not use auto-property with initialization.")]
    private int _setupClientsCount = 1;

    protected BrowserTestConfiguration? Configuration { get; private set; }

    protected ServerFixture? Server { get; private set; }

    protected IList<ClientTest> ClientTests { get; } = [];

    protected ClientTest ClientTest => ClientTests[0];

    protected ScreenshotCapture? ScreenshotCapture { get; private set; }

    protected int SetupClientsCount
    {
        get => _setupClientsCount;
        set
        {
            if (value < 1)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            }
            else if (value > 10)
            {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 10);
            }

            _setupClientsCount = value;
        }
    }

    [TestInitialize]
    public void TestInitialize()
    {
        ClientTests.Clear();
        Server = new ServerFixture();
        ScreenshotCapture = new ScreenshotCapture();
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        ScreenshotCapture = null;
        ClientTests.Clear();

        foreach (var context in _contexts)
        {
            await context.DisposeAsync();
        }

        _contexts.Clear();

        if (Server != null)
        {
            await Server.DisposeAsync();
            Server = null;
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions();
        options.ViewportSize = new ViewportSize
        {
            Width = 1920,
            Height = 1080
        };

        return options;
    }

    protected void Configure(bool serverSide, bool useHttpClient, [CallerMemberName] string testName = "")
    {
        Configuration = new BrowserTestConfiguration(GetType().Name, testName, serverSide, useHttpClient);
    }

    protected async Task StartServer()
    {
        Assert.IsNotNull(Context);
        Assert.IsNotNull(Server);
        Server.UseServerSide = GetConfiguration().ServerSide;
        Server.UseHttpClient = GetConfiguration().UseHttpClient;
        await Server.Start();
        await AssertServerSide();
        await AssertClientConnectionType();
        await AssertServerIsHealthy();
    }

    protected async Task StartClients()
    {
        Assert.IsNotNull(Server);
        for (var i = 0; i < SetupClientsCount; i++)
        {
            IPage page;
            if (i == 0)
            {
                page = Page;
            }
            else
            {
                var context = await NewContextAsync(ContextOptions());
                _contexts.Add(context);
                page = await context.NewPageAsync();
            }

            ClientTests.Add(new ClientTest(page, Server.Uri!));
        }
    }

    protected Task<string> TakeScreenshot(string name)
    {
        Assert.IsNotNull(ScreenshotCapture);
        return ScreenshotCapture.TakeScreenshot(Page, GetConfiguration(), name);
    }

    protected Task<string> TakeScreenshot(int index, string name)
    {
        Assert.IsNotNull(ScreenshotCapture);
        var page = ClientTests[index].Page;
        return ScreenshotCapture.TakeScreenshot(page, GetConfiguration(), name);
    }

    private BrowserTestConfiguration GetConfiguration()
    {
        return Configuration ?? throw new InvalidOperationException("Test has not been configured.");
    }

    private async Task AssertServerIsHealthy()
    {
        Assert.IsNotNull(Server);
        var client = new HttpClient
        {
            BaseAddress = Server.Uri
        };
        var response = await client.GetStringAsync(new Uri("health", UriKind.Relative));
        Assert.AreEqual("Healthy", response);
    }

    private async Task AssertServerSide()
    {
        Assert.IsNotNull(Server);
        var client = new HttpClient();
        var response = await client.GetStringAsync(Server.Uri);

        var expected = GetConfiguration().ServerSide ? "server" : "webassembly";
        expected = @"<!--Blazor:{""type"":""" + expected + @"""";
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Contains(expected, StringComparison.Ordinal));
    }

    private async Task AssertClientConnectionType()
    {
        Assert.IsNotNull(Server);
        var client = new HttpClient
        {
            BaseAddress = Server.Uri
        };
        var response = await client.GetStringAsync(new Uri("configuration", UriKind.Relative));
        Assert.IsNotNull(response);

        var configuration = System.Text.Json.JsonDocument.Parse(response);
        var property = configuration.RootElement.GetProperty("useHttpClient");

        Assert.AreEqual(GetConfiguration().UseHttpClient, property.GetBoolean());
    }
}

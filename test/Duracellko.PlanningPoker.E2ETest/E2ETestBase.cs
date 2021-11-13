using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposable objects are disposed in TestCleanup.")]
    public abstract class E2ETestBase
    {
        protected ServerFixture? Server { get; private set; }

        protected IList<BrowserFixture> BrowserFixtures { get; } = new List<BrowserFixture>();

        protected IList<BrowserTestContext> Contexts { get; } = new List<BrowserTestContext>();

        protected IList<ClientTest> ClientTests { get; } = new List<ClientTest>();

        protected BrowserFixture? BrowserFixture => BrowserFixtures.FirstOrDefault();

        protected BrowserTestContext? Context => Contexts.FirstOrDefault();

        protected ClientTest ClientTest => ClientTests.First();

        protected ScreenshotCapture? ScreenshotCapture { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Contexts.Clear();
            ClientTests.Clear();
            Server = new ServerFixture();
            BrowserFixtures.Clear();
            BrowserFixtures.Add(new BrowserFixture());
            ScreenshotCapture = new ScreenshotCapture();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ScreenshotCapture = null;
            ClientTests.Clear();
            Contexts.Clear();

            foreach (var browserFixture in BrowserFixtures)
            {
                browserFixture.Dispose();
            }

            BrowserFixtures.Clear();

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        protected IWebDriver? GetBrowser() => BrowserFixture?.Browser;

        protected IWebDriver? GetBrowser(int index) => BrowserFixtures[index].Browser;

        protected async Task StartServer()
        {
            Assert.IsNotNull(Context);
            Assert.IsNotNull(Server);
            Server.UseServerSide = Context.ServerSide;
            Server.UseHttpClient = Context.UseHttpClient;
            await Server.Start();
            await AssertServerSide(Context.ServerSide);
            await AssertClientConnectionType(Context.UseHttpClient);
        }

        protected void StartClients()
        {
            Assert.IsNotNull(Server);
            bool first = true;
            foreach (var context in Contexts)
            {
                BrowserFixture browserFixture;
                if (first)
                {
                    browserFixture = BrowserFixtures[0];
                    first = false;
                }
                else
                {
                    browserFixture = new BrowserFixture();
                    BrowserFixtures.Add(browserFixture);
                }

                browserFixture.Initialize(context.BrowserType);
                Assert.IsNotNull(browserFixture.Browser);
                ClientTests.Add(new ClientTest(browserFixture.Browser, Server));
            }
        }

        protected async Task AssertServerSide(bool serverSide)
        {
            Assert.IsNotNull(Server);
            var client = new HttpClient();
            var response = await client.GetStringAsync(Server.Uri);

            var expected = serverSide ? "server" : "webassembly";
            expected = @"<script src=""_framework/blazor." + expected + @".js""></script>";
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Contains(expected, StringComparison.Ordinal));
        }

        protected async Task AssertClientConnectionType(bool useHttpClient)
        {
            Assert.IsNotNull(Server);
            var client = new HttpClient();
            client.BaseAddress = Server.Uri;
            var response = await client.GetStringAsync(new Uri("configuration", UriKind.Relative));
            Assert.IsNotNull(response);

            var configuration = System.Text.Json.JsonDocument.Parse(response);
            var property = configuration.RootElement.GetProperty("useHttpClient");
            Assert.IsNotNull(property);

            Assert.AreEqual(useHttpClient, property.GetBoolean());
        }

        protected string TakeScreenshot(string name)
        {
            Assert.IsNotNull(ScreenshotCapture);
            var browser = GetBrowser();
            Assert.IsNotNull(browser);
            Assert.IsNotNull(Context);
            return ScreenshotCapture.TakeScreenshot((ITakesScreenshot)browser, Context, name);
        }

        protected string TakeScreenshot(int index, string name)
        {
            Assert.IsNotNull(ScreenshotCapture);
            var browser = GetBrowser(index);
            Assert.IsNotNull(browser);
            Assert.IsNotNull(Context);
            return ScreenshotCapture.TakeScreenshot((ITakesScreenshot)browser, Context, name);
        }
    }
}

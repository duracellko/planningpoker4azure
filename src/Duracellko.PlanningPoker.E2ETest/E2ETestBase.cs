using System;
using System.Net.Http;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.E2ETest.Browser;
using Duracellko.PlanningPoker.E2ETest.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace Duracellko.PlanningPoker.E2ETest
{
    public abstract class E2ETestBase
    {
        protected ServerFixture Server { get; private set; }

        protected BrowserFixture BrowserFixture { get; private set; }

        protected IWebDriver Browser => BrowserFixture.Browser;

        protected BrowserTestContext Context { get; set; }

        protected ClientTest ClientTest { get; set; }

        protected ScreenshotCapture ScreenshotCapture { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            Server = new ServerFixture();
            BrowserFixture = new BrowserFixture();
            ScreenshotCapture = new ScreenshotCapture();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ScreenshotCapture = null;
            ClientTest = null;
            Context = null;

            if (BrowserFixture != null)
            {
                BrowserFixture.Dispose();
                BrowserFixture = null;
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        protected async Task AssertServerSide(bool serverSide)
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(Server.Uri);

            var expected = serverSide ? "server" : "webassembly";
            expected = @"<script src=""_framework/blazor." + expected + @".js""></script>";
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Contains(expected, StringComparison.Ordinal));
        }

        protected string TakeScreenshot(string name)
        {
            return ScreenshotCapture.TakeScreenshot((ITakesScreenshot)Browser, Context, name);
        }
    }
}

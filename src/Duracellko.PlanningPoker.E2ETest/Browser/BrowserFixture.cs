using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Duracellko.PlanningPoker.E2ETest.Browser
{
    public class BrowserFixture : IDisposable
    {
        ~BrowserFixture()
        {
            Dispose(false);
        }

        public IWebDriver Browser { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize(BrowserType browserType)
        {
            if (Browser != null)
            {
                throw new InvalidOperationException("Selenium driver was already started.");
            }

            var options = CreateDriverOptions(browserType);
            var driver = new RemoteWebDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Browser = driver;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Browser != null)
                {
                    Browser.Dispose();
                    Browser = null;
                }
            }
        }

        private static DriverOptions CreateDriverOptions(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                    var options = new ChromeOptions();
                    options.AddArgument("--headless");
                    options.AddArgument("--window-size=1920,1080");
                    options.SetLoggingPreference(LogType.Browser, LogLevel.All);
                    return options;
                default:
                    throw new NotSupportedException($"Browser type '{browserType}' is not supported.");
            }
        }
    }
}

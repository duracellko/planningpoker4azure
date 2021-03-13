using System;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

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

            var driver = CreateDriver(browserType);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
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

        private static IWebDriver CreateDriver(BrowserType browserType)
        {
            switch (browserType)
            {
                case BrowserType.Chrome:
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--headless");
                    chromeOptions.AddArgument("--window-size=1920,1080");
                    chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
                    return new ChromeDriver(GetDriverLocation(browserType), chromeOptions);
                case BrowserType.Firefox:
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument("-headless");
                    firefoxOptions.AddArgument("--window-size=1920,1080");
                    firefoxOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
                    return new FirefoxDriver(GetDriverLocation(browserType), firefoxOptions);
                default:
                    throw new NotSupportedException($"Browser type '{browserType}' is not supported.");
            }
        }

        private static string GetDriverLocation(BrowserType browserType)
        {
            string driverName = null;
            string environmentVariable = null;
            switch (browserType)
            {
                case BrowserType.Chrome:
                    driverName = "chromedriver";
                    environmentVariable = "CHROMEWEBDRIVER";
                    break;
                case BrowserType.Firefox:
                    driverName = "geckodriver";
                    environmentVariable = "GECKOWEBDRIVER";
                    break;
                default:
                    throw new NotSupportedException($"Browser type '{browserType}' is not supported.");
            }

            var driverLocation = Environment.GetEnvironmentVariable(environmentVariable);
            if (string.IsNullOrEmpty(driverLocation) || !Directory.Exists(driverLocation))
            {
                var assemblyLocation = Path.GetDirectoryName(typeof(BrowserFixture).Assembly.Location);
                var seleniumFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation))));
                seleniumFolder = Path.Combine(seleniumFolder, "node_modules", "selenium-standalone", ".selenium");
                driverLocation = Path.Combine(seleniumFolder, driverName);

                var driverFile = Directory.GetFiles(driverLocation)
                    .Select(p => Path.GetFileName(p))
                    .Where(f => !f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => f, StringComparer.OrdinalIgnoreCase)
                    .First();
                driverFile = Path.Combine(driverLocation, driverFile);

                var windir = Environment.GetEnvironmentVariable("windir");
                var isWindows = !string.IsNullOrEmpty(windir) && Directory.Exists(windir);
                var targetFile = isWindows ? (driverName + ".exe") : driverName;
                targetFile = Path.Combine(driverLocation, targetFile);
                if (!File.Exists(targetFile) || File.GetLastWriteTimeUtc(targetFile) != File.GetLastWriteTimeUtc(driverFile))
                {
                    File.Copy(driverFile, targetFile, true);
                }
            }

            Console.WriteLine($"{browserType} location: {driverLocation}");
            return driverLocation;
        }
    }
}

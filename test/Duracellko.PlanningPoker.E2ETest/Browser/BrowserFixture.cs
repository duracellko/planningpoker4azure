using System;
using System.Diagnostics.CodeAnalysis;
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

        public IWebDriver? Browser { get; private set; }

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

        [SuppressMessage("Major Code Smell", "S1066:Collapsible \"if\" statements should be merged", Justification = "Follows IDisposable pattern.")]
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

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Driver Service is disposed by driver.")]
        private static IWebDriver CreateDriver(BrowserType browserType)
        {
            var driverPath = GetDriverPath(browserType);
            switch (browserType)
            {
                case BrowserType.Chrome:
                    var chromeDriverService = ChromeDriverService.CreateDefaultService(
                        Path.GetDirectoryName(driverPath),
                        Path.GetFileName(driverPath));

                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--headless");
                    chromeOptions.AddArgument("--window-size=1920,1080");
                    chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);

                    return new ChromeDriver(chromeDriverService, chromeOptions);
                case BrowserType.Firefox:
                    var firefoxDriverService = FirefoxDriverService.CreateDefaultService(
                        Path.GetDirectoryName(driverPath),
                        Path.GetFileName(driverPath));

                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument("-headless");
                    firefoxOptions.AddArgument("--window-size=1920,1080");
                    firefoxOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);

                    return new FirefoxDriver(firefoxDriverService, firefoxOptions);
                default:
                    throw new NotSupportedException($"Browser type '{browserType}' is not supported.");
            }
        }

        private static string GetDriverPath(BrowserType browserType)
        {
            string driverName;
            string environmentVariable;
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

            var driverPath = GetDriverPathFromEnvironmentVariable(driverName, environmentVariable);
            if (string.IsNullOrEmpty(driverPath))
            {
                driverPath = GetDriverPathFromNodeModules(driverName);
            }

            Console.WriteLine($"{browserType} location: {driverPath}");
            return driverPath;
        }

        private static string? GetDriverPathFromEnvironmentVariable(string driverName, string environmentVariable)
        {
            var driverLocation = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrEmpty(driverLocation) && Directory.Exists(driverLocation))
            {
                return Path.Combine(driverLocation, GetDriverFileName(driverName));
            }

            return null;
        }

        private static string GetDriverPathFromNodeModules(string driverName)
        {
            var assemblyLocation = Path.GetDirectoryName(typeof(BrowserFixture).Assembly.Location);
            var seleniumFolder = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(assemblyLocation))));
            seleniumFolder = Path.Combine(seleniumFolder!, "node_modules", "selenium-standalone", ".selenium");

            var driverLocation = Path.Combine(seleniumFolder, driverName, "latest-x64");
            if (!Directory.Exists(driverLocation))
            {
                driverLocation = Path.Combine(seleniumFolder, driverName);
            }

            var driverFile = Directory.GetFiles(driverLocation)
                .Select(p => Path.GetFileName(p))
                .Where(f => !f.Contains(".zip", StringComparison.OrdinalIgnoreCase)) // Ignore also *.zip.etag
                .Where(f => !f.Contains("LICENSE.", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f, StringComparer.OrdinalIgnoreCase)
                .First();
            driverFile = Path.Combine(driverLocation, driverFile);

            var targetFile = Path.Combine(driverLocation, GetDriverFileName(driverName));
            if (targetFile != driverFile &&
                (!File.Exists(targetFile) || File.GetLastWriteTimeUtc(targetFile) != File.GetLastWriteTimeUtc(driverFile)))
            {
                File.Copy(driverFile, targetFile, true);
            }

            return targetFile;
        }

        private static string GetDriverFileName(string driverName)
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            var isWindows = !string.IsNullOrEmpty(windir) && Directory.Exists(windir);
            return isWindows ? (driverName + ".exe") : driverName;
        }
    }
}

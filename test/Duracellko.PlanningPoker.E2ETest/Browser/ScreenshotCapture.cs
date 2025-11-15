using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Duracellko.PlanningPoker.E2ETest.Browser;

public class ScreenshotCapture
{
    private string BasePath
    {
        get
        {
            if (field == null)
            {
                var assemblyLocation = typeof(ScreenshotCapture).Assembly.Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                field = Path.Combine(assemblyDirectory!, "Screenshots");
            }

            return field;
        }
    }

    public async Task<string> TakeScreenshot(IPage page, BrowserTestConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var screenshotFolder = GetScreenshotFolder(configuration);
        var screenshotPath = Path.Combine(screenshotFolder, name + ".png");
        var options = new PageScreenshotOptions
        {
            Path = screenshotPath
        };
        await page.ScreenshotAsync(options);
        return screenshotPath;
    }

    private string GetScreenshotFolder(BrowserTestConfiguration configuration)
    {
        var serverSide = configuration.ServerSide ? "Server" : "Client";
        var connectionType = configuration.UseHttpClient ? "HttpClient" : "SignalR";
        var path = Path.Combine(BasePath, serverSide, connectionType, configuration.ClassName, configuration.TestName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}

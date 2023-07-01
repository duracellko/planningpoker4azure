using System;
using System.Collections.Generic;
using System.IO;

namespace Duracellko.PlanningPoker.Web
{
    internal static class LocalizationService
    {
        public static IEnumerable<string> GetSupportedCultures()
        {
            yield return "en-US";
            yield return "en";

            var mainAssembly = typeof(LocalizationService).Assembly;
            var mainDirectoryPath = Path.GetDirectoryName(mainAssembly.Location);
            var resourceAssemblyFileName = string.Concat(mainAssembly.GetName().Name, ".resources.dll");

            foreach (var subdirectory in Directory.EnumerateDirectories(mainDirectoryPath!))
            {
                var subdirectoryName = Path.GetFileName(subdirectory);
                if (!string.Equals(subdirectoryName, "en-US", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(subdirectoryName, "en", StringComparison.OrdinalIgnoreCase))
                {
                    var satelliteAssemblyPath = Path.Combine(subdirectory, resourceAssemblyFileName);
                    if (File.Exists(satelliteAssemblyPath))
                    {
                        yield return subdirectoryName;
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Web.Model
{
    public class ClientScriptsLibrary
    {
        private const string LibManFileName = "libman.json";

        private readonly List<string> _cascadingStyleSheets = new List<string>();
        private readonly List<string> _javaScripts = new List<string>();
        private readonly object _initializationLock = new object();

        private bool _initialized;

        public IEnumerable<string> CascadingStyleSheets => _cascadingStyleSheets;

        public IEnumerable<string> JavaScripts => _javaScripts;

        private static string LibManPath
        {
            get
            {
                var assembly = typeof(ClientScriptsLibrary).Assembly;
                var assemblyFolder = Path.GetDirectoryName(assembly.Location);
                return Path.Combine(assemblyFolder!, LibManFileName);
            }
        }

        public async Task Load()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializationLock)
            {
                if (_initialized)
                {
                    return;
                }
            }

            var clientScripts = await GetClientScripts();

            lock (_initializationLock)
            {
                if (!_initialized)
                {
                    _cascadingStyleSheets.AddRange(clientScripts.Where(IsCascadingStyleSheet));
                    _javaScripts.AddRange(clientScripts.Where(IsJavaScript));
                    _initialized = true;
                }
            }
        }

        private static async Task<List<string>> GetClientScripts()
        {
            JsonDocument libManDocument;
            using (var stream = File.OpenRead(LibManPath))
            {
                var jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
                libManDocument = jsonDocument ?? throw new InvalidOperationException("Error reading client scripts. libman.json file is empty.");
            }

            var libraries = libManDocument.RootElement.GetProperty("libraries");
            var clientScripts = new List<string>();
            foreach (var library in libraries.EnumerateArray())
            {
                clientScripts.AddRange(GetLibraryClientScripts(library));
            }

            return clientScripts;
        }

        [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection", Justification = "Do not create full list to find first item only.")]
        private static IEnumerable<string> GetLibraryClientScripts(JsonElement libraryElement)
        {
            var libraryFullName = libraryElement.GetProperty("library").GetString();
            if (string.IsNullOrEmpty(libraryFullName))
            {
                throw new InvalidOperationException("Library name is empty.");
            }

            var separatorPosition = libraryFullName.IndexOf('@', StringComparison.Ordinal);
            var libraryName = libraryFullName.Substring(0, separatorPosition);
            var libraryVersion = libraryFullName.Substring(separatorPosition + 1);

            var libraryFiles = libraryElement.GetProperty("files").EnumerateArray()
                .Select(f => f.GetString());

            var cssFile = libraryFiles.FirstOrDefault(f => IsCascadingStyleSheet(f));
            if (cssFile != null)
            {
                yield return libraryName + '/' + libraryVersion + '/' + cssFile;
            }

            var jsFile = libraryFiles.FirstOrDefault(f => IsJavaScript(f));
            if (jsFile != null)
            {
                yield return libraryName + '/' + libraryVersion + '/' + jsFile;
            }
        }

        private static bool IsCascadingStyleSheet(string? scriptPath)
        {
            return !string.IsNullOrEmpty(scriptPath) &&
                scriptPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsJavaScript(string? scriptPath)
        {
            return !string.IsNullOrEmpty(scriptPath) &&
                scriptPath.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Web.Model;

public class ClientScriptsLibrary
{
    private const string LibManFileName = "libman.json";

    private readonly List<ClientScriptReference> _cascadingStyleSheets = [];
    private readonly List<ClientScriptReference> _javaScripts = [];
    private readonly Lock _initializationLock = new();

    private bool _initialized;

    public IEnumerable<ClientScriptReference> CascadingStyleSheets => _cascadingStyleSheets;

    public IEnumerable<ClientScriptReference> JavaScripts => _javaScripts;

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
                _cascadingStyleSheets.AddRange(clientScripts.Where(r => IsCascadingStyleSheet(r.ToString())));
                _javaScripts.AddRange(clientScripts.Where(r => IsJavaScript(r.ToString())));
                _initialized = true;
            }
        }
    }

    private static async Task<List<ClientScriptReference>> GetClientScripts()
    {
        JsonDocument libManDocument;
        using (var stream = File.OpenRead(LibManPath))
        {
            var jsonDocument = await JsonSerializer.DeserializeAsync<JsonDocument>(stream);
            libManDocument = jsonDocument ?? throw new InvalidOperationException(Resources.Error_EmptyLibmanFile);
        }

        var libraries = libManDocument.RootElement.GetProperty("libraries");
        var clientScripts = new List<ClientScriptReference>();
        foreach (var library in libraries.EnumerateArray())
        {
            clientScripts.AddRange(GetLibraryClientScripts(library));
        }

        return clientScripts;
    }

    [SuppressMessage("Performance", "CA1851:Possible multiple enumerations of 'IEnumerable' collection", Justification = "Do not create full list to find first item only.")]
    private static IEnumerable<ClientScriptReference> GetLibraryClientScripts(JsonElement libraryElement)
    {
        var libraryFullName = libraryElement.GetProperty("library").GetString();
        if (string.IsNullOrEmpty(libraryFullName))
        {
            throw new InvalidOperationException(Resources.Error_EmptyLibraryName);
        }

        var separatorPosition = libraryFullName.IndexOf('@', StringComparison.Ordinal);
        var libraryName = libraryFullName[..separatorPosition];
        var libraryVersion = libraryFullName[(separatorPosition + 1)..];

        var libraryFiles = libraryElement.GetProperty("files").EnumerateArray()
            .Select(f => f.GetString());

        var cssFile = libraryFiles.FirstOrDefault(IsCascadingStyleSheet);
        if (cssFile != null)
        {
            yield return new ClientScriptReference(libraryName, libraryVersion, cssFile);
        }

        var jsFile = libraryFiles.FirstOrDefault(IsJavaScript);
        if (jsFile != null)
        {
            yield return new ClientScriptReference(libraryName, libraryVersion, jsFile);
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

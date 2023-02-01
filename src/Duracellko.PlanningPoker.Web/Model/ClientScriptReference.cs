using System;

namespace Duracellko.PlanningPoker.Web.Model
{
    public class ClientScriptReference
    {
        public ClientScriptReference(string libraryName, string version, string file)
        {
            LibraryName = libraryName ?? throw new ArgumentNullException(nameof(libraryName));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            File = file ?? throw new ArgumentNullException(nameof(file));
        }

        public string LibraryName { get; }

        public string Version { get; }

        public string File { get; }

        public override string ToString() => LibraryName + '/' + Version + '/' + File;
    }
}

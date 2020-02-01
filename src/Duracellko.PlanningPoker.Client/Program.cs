using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;

namespace Duracellko.PlanningPoker.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            Startup.ConfigureServices(builder.Services);
            builder.RootComponents.Add<App>("app");

            await builder.Build().RunAsync();
        }
    }
}

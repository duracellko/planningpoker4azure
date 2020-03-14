using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddBaseAddressHttpClient();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var useHttpClient = await GetClientConfiguration(serviceProvider);
            Startup.ConfigureServices(builder.Services, false, useHttpClient);
            builder.RootComponents.Add<App>("app");

            await builder.Build().RunAsync();
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Use default configuration, when loading fails.")]
        private static async Task<bool> GetClientConfiguration(IServiceProvider serviceProvider)
        {
            try
            {
                using (var httpClient = serviceProvider.GetRequiredService<HttpClient>())
                {
                    var clientConfiguration = await httpClient.GetStringAsync("configuration");
                    return string.Equals(clientConfiguration.Trim(), "HttpClient", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

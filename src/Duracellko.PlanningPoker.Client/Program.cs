using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var environment = builder.HostEnvironment;
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(environment.BaseAddress) });

            var configuration = builder.Configuration.Build();
            var useHttpClient = configuration.GetValue<bool>("UseHttpClient");
            Startup.ConfigureServices(builder.Services, false, useHttpClient);

            await builder.Build().RunAsync();
        }
    }
}

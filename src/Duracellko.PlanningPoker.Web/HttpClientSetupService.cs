using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.Web
{
    public class HttpClientSetupService : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IServer _server;

        public HttpClientSetupService(HttpClient httpClient, IServer server)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serverAddresses = _server.Features.Get<IServerAddressesFeature>();
            var address = serverAddresses.Addresses.FirstOrDefault();
            if (address == null)
            {
                // Default ASP.NET Core Kestrel endpoint
                address = "http://localhost:5000";
            }
            else
            {
                address = address.Replace("*", "localhost", StringComparison.Ordinal);
                address = address.Replace("+", "localhost", StringComparison.Ordinal);
                address = address.Replace("[::]", "localhost", StringComparison.Ordinal);
            }

            _httpClient.BaseAddress = new Uri(address);

            return Task.CompletedTask;
        }
    }
}

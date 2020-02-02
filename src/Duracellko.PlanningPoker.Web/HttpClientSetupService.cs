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
        private readonly PlanningPokerServerUriProvider _uriProvider;
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public HttpClientSetupService(
            HttpClient httpClient,
            PlanningPokerServerUriProvider uriProvider,
            IServer server,
            IHostApplicationLifetime applicationLifetime)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _uriProvider = uriProvider ?? throw new ArgumentNullException(nameof(uriProvider));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var applicationStartedToken = _applicationLifetime.ApplicationStarted;
            if (applicationStartedToken.IsCancellationRequested)
            {
                ConfigureHttpClient();
            }
            else
            {
                applicationStartedToken.Register(ConfigureHttpClient);
            }

            return Task.CompletedTask;
        }

        private void ConfigureHttpClient()
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

            var baseUri = new Uri(address);
            _httpClient.BaseAddress = baseUri;
            _uriProvider.InitializeBaseUri(baseUri);
        }
    }
}

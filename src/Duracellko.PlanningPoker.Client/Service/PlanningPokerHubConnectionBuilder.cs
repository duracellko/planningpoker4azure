using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Creates new instance of HubConnection.
    /// </summary>
    public class PlanningPokerHubConnectionBuilder : IHubConnectionBuilder
    {
        private const string ServiceUri = "signalr/PlanningPoker";

        private static TimeSpan[] _reconnectDelays = new[]
        {
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(0.5),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5)
        };

        private readonly IPlanningPokerUriProvider _uriProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerHubConnectionBuilder"/> class.
        /// </summary>
        /// <param name="uriProvider">URI provider that provides URI of Planning Poker service.</param>
        public PlanningPokerHubConnectionBuilder(IPlanningPokerUriProvider uriProvider)
        {
            _uriProvider = uriProvider ?? throw new ArgumentNullException(nameof(uriProvider));
        }

        /// <summary>
        /// Gets the builder service collection.
        /// </summary>
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// Creates a HubConnection.
        /// </summary>
        /// <returns>A HubConnection built using the configured options.</returns>
        public HubConnection Build()
        {
            // Postpone obtaining URL, because NavigationManager can be initialized later.
            var uri = _uriProvider.BaseUri != null ? new Uri(_uriProvider.BaseUri, ServiceUri) : new Uri(ServiceUri);

            var builder = new HubConnectionBuilder()
                .WithUrl(uri)
                .AddNewtonsoftJsonProtocol()
                .WithAutomaticReconnect(_reconnectDelays);

            foreach (var service in Services)
            {
                builder.Services.Add(service);
            }

            return builder.Build();
        }
    }
}

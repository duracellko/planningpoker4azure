using System;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, bool serverSide = false, bool useHttpClient = false)
        {
            // Services are scoped, because on server-side scope is created for each client session.
            if (serverSide)
            {
                services.AddScoped<IServiceTimeProvider, ServerSideServiceTimeProvider>();
            }
            else
            {
                services.AddScoped<IPlanningPokerUriProvider, PlanningPokerUriProvider>();
                services.AddScoped<IServiceTimeProvider, ServiceTimeProvider>();
            }

            if (useHttpClient)
            {
                services.AddScoped<IPlanningPokerClient, PlanningPokerClient>();
            }
            else
            {
                services.AddScoped<IPlanningPokerClient, PlanningPokerSignalRClient>();
            }

            var timerInterval = TimeSpan.FromMilliseconds(serverSide ? 1000 : 500);
            services.AddScoped(p => new TimerFactory(timerInterval));
            services.AddScoped<ITimerFactory>(p => p.GetRequiredService<TimerFactory>());

            services.AddSingleton<DateTimeProvider>();

            services.AddScoped<IMemberCredentialsStore, MemberCredentialsStore>();
            services.AddScoped<ITimerSettingsRepository, TimerSettingsRepository>();
            services.AddScoped<Microsoft.AspNetCore.SignalR.Client.IHubConnectionBuilder, PlanningPokerHubConnectionBuilder>();

            services.AddScoped<INavigationManager, AppNavigationManager>();
            services.AddScoped<MessageBoxService>();
            services.AddScoped<IMessageBoxService>(p => p.GetRequiredService<MessageBoxService>());
            services.AddScoped<BusyIndicatorService>();
            services.AddScoped<IBusyIndicatorService>(p => p.GetRequiredService<BusyIndicatorService>());
            services.AddScoped<IPlanningPokerInitializer>(p => p.GetRequiredService<PlanningPokerController>());

            services.AddScoped<PlanningPokerController>();
            services.AddScoped<CreateTeamController>();
            services.AddScoped<JoinTeamController>();
            services.AddTransient<MessageReceiver>();
        }
    }
}

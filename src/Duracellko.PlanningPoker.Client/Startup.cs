using System.Diagnostics.CodeAnalysis;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client
{
    public class Startup
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Startup interface expected by Blazor.")]
        public void ConfigureServices(IServiceCollection services)
        {
            // Services are scoped, because on server-side scope is created for each client session.
            services.AddScoped<IPlanningPokerClient, PlanningPokerClient>();
            services.AddScoped<IMemberCredentialsStore, MemberCredentialsStore>();

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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Startup interface expected by Blazor.")]
        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}

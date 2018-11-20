using System.Diagnostics.CodeAnalysis;
using Duracellko.PlanningPoker.Client.Controllers;
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
            services.AddSingleton<IPlanningPokerClient, PlanningPokerClient>();

            services.AddSingleton<MessageBoxService>();
            services.AddSingleton<IMessageBoxService>(p => p.GetRequiredService<MessageBoxService>());
            services.AddSingleton<BusyIndicatorService>();
            services.AddSingleton<IBusyIndicatorService>(p => p.GetRequiredService<BusyIndicatorService>());

            services.AddSingleton<PlanningPokerController>();
            services.AddSingleton<CreateTeamController>();
            services.AddSingleton<JoinTeamController>();
            services.AddTransient<MessageReceiver>();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Startup interface expected by Blazor.")]
        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}

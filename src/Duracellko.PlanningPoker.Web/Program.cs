using System;
using System.Net.Http;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.Health;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Domain.Serialization;
using Duracellko.PlanningPoker.Health;
using Duracellko.PlanningPoker.Redis;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Duracellko.PlanningPoker.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var app = CreateWebApplication(args);
            app.Run();
        }

        public static WebApplication CreateWebApplication(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();
            ConfigureApp(app, app, app.Environment);

            return app;
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddControllers()
                .AddApplicationPart(typeof(PlanningPokerService).Assembly)
                .AddMvcOptions(o => o.Conventions.Add(new PlanningPokerApplication()));
            services.AddRazorPages()
                .AddApplicationPart(typeof(Program).Assembly);
            services.AddSignalR();
            var healthChecks = services.AddHealthChecks()
                .AddCheck<PlanningPokerControllerHealthCheck>("PlanningPoker")
                .AddCheck<ScrumTeamRepositoryHealthCheck>("ScrumTeamRepository");

            var planningPokerConfiguration = GetPlanningPokerConfiguration(configuration);
            var isAzure = !string.IsNullOrEmpty(planningPokerConfiguration.ServiceBusConnectionString);

            services.AddSingleton<DateTimeProvider>();
            services.AddSingleton<GuidProvider>();
            services.AddSingleton<DeckProvider>();
            services.AddSingleton<ScrumTeamSerializer>();
            services.AddSingleton<IPlanningPokerConfiguration>(planningPokerConfiguration);

            if (isAzure)
            {
                services.AddSingleton<IAzurePlanningPokerConfiguration>(planningPokerConfiguration);
                services.AddSingleton(sp => new AzurePlanningPokerController(
                    sp.GetService<DateTimeProvider>(),
                    sp.GetService<GuidProvider>(),
                    sp.GetService<DeckProvider>(),
                    sp.GetService<IAzurePlanningPokerConfiguration>(),
                    sp.GetService<IScrumTeamRepository>(),
                    sp.GetService<TaskProvider>(),
                    sp.GetRequiredService<ILogger<PlanningPokerController>>()));
                services.AddSingleton<IAzurePlanningPoker>(sp => sp.GetRequiredService<AzurePlanningPokerController>());
                services.AddSingleton<IPlanningPoker>(sp => sp.GetRequiredService<AzurePlanningPokerController>());
                services.AddSingleton<IInitializationStatusProvider>(sp => sp.GetRequiredService<AzurePlanningPokerController>());
                services.AddSingleton<PlanningPokerAzureNode>();
                services.AddSingleton<IHostedService, AzurePlanningPokerNodeService>();
                services.AddSingleton<IMessageConverter, MessageConverter>();
                services.AddSingleton<IRedisMessageConverter, RedisMessageConverter>();

                if (planningPokerConfiguration.ServiceBusConnectionString!.StartsWith("REDIS:", StringComparison.Ordinal))
                {
                    services.AddSingleton<IServiceBus, RedisServiceBus>();
                }
                else
                {
                    services.AddSingleton<IServiceBus, AzureServiceBus>();
                    healthChecks.AddCheck<AzureServiceBusHealthCheck>("AzureServiceBus");
                }
            }
            else
            {
                services.AddSingleton<IPlanningPoker>(sp => new PlanningPokerController(
                    sp.GetService<DateTimeProvider>(),
                    sp.GetService<GuidProvider>(),
                    sp.GetService<DeckProvider>(),
                    sp.GetService<IPlanningPokerConfiguration>(),
                    sp.GetService<IScrumTeamRepository>(),
                    sp.GetService<TaskProvider>(),
                    sp.GetRequiredService<ILogger<PlanningPokerController>>()));
                services.AddSingleton<IInitializationStatusProvider, ReadyInitializationStatusProvider>();
            }

            if (!string.IsNullOrEmpty(planningPokerConfiguration.RepositoryFolder))
            {
                services.AddTransient<IScrumTeamRepository, FileScrumTeamRepository>();
                services.AddSingleton<IFileScrumTeamRepositorySettings, FileScrumTeamRepositorySettings>();
            }
            else
            {
                services.AddSingleton<IScrumTeamRepository, EmptyScrumTeamRepository>();
            }

            services.AddSingleton<IHostedService, PlanningPokerCleanupService>();

            var clientConfiguration = GetPlanningPokerClientConfiguration(configuration);
            services.AddSingleton<PlanningPokerClientConfiguration>(clientConfiguration);

            if (clientConfiguration.UseServerSide != Model.ServerSideConditions.Never)
            {
                services.AddServerSideBlazor();
                services.AddSingleton<HttpClient>();
                services.AddSingleton<PlanningPokerServerUriProvider>();
                services.AddSingleton<Client.Service.IPlanningPokerUriProvider>(sp => sp.GetRequiredService<PlanningPokerServerUriProvider>());
                services.AddSingleton<IHostedService, HttpClientSetupService>();

                // Register services used by client on server-side.
                Client.Startup.ConfigureServices(services, true, clientConfiguration.UseHttpClient);
            }
        }

        private static void ConfigureApp(IApplicationBuilder app, IEndpointRouteBuilder endpoints, IWebHostEnvironment env)
        {
            var clientConfiguration = app.ApplicationServices.GetRequiredService<PlanningPokerClientConfiguration>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                if (clientConfiguration.UseServerSide != Model.ServerSideConditions.Always)
                {
                    app.UseWebAssemblyDebugging();
                }
            }

            var rewriteOptions = new RewriteOptions()
                .AddRewrite(@"^appsettings\.json$", "configuration", false);
            app.UseRewriter(rewriteOptions);

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            endpoints.MapHealthChecks("/health");
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapHub<PlanningPokerHub>("/signalr/PlanningPoker");
            if (clientConfiguration.UseServerSide != Model.ServerSideConditions.Never)
            {
                endpoints.MapBlazorHub();
            }

            endpoints.MapFallbackToPage("/Home");
        }

        private static AzurePlanningPokerConfiguration GetPlanningPokerConfiguration(IConfiguration configuration)
        {
            return configuration.GetSection("PlanningPoker").Get<AzurePlanningPokerConfiguration>() ?? new AzurePlanningPokerConfiguration();
        }

        private static PlanningPokerClientConfiguration GetPlanningPokerClientConfiguration(IConfiguration configuration)
        {
            return configuration.GetSection("PlanningPokerClient").Get<PlanningPokerClientConfiguration>() ?? new PlanningPokerClientConfiguration();
        }
    }
}

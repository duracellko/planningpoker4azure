using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using Duracellko.PlanningPoker.Azure;
using Duracellko.PlanningPoker.Azure.Configuration;
using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public bool UseServerSide => Configuration.GetSection("PlanningPokerClient").GetValue<bool?>("UseServerSide") ?? false;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddApplicationPart(typeof(PlanningPokerService).Assembly)
                .AddMvcOptions(o => o.Conventions.Add(new PlanningPokerApplication()))
                .AddNewtonsoftJson();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet
                });
            });

            var planningPokerConfiguration = GetPlanningPokerConfiguration();
            var isAzure = !string.IsNullOrEmpty(planningPokerConfiguration.ServiceBusConnectionString);

            services.AddSingleton<DateTimeProvider>();
            services.AddSingleton<IPlanningPokerConfiguration>(planningPokerConfiguration);
            if (isAzure)
            {
                services.AddSingleton<IAzurePlanningPokerConfiguration>(planningPokerConfiguration);
                services.AddSingleton<IAzurePlanningPoker, AzurePlanningPokerController>();
                services.AddSingleton<IPlanningPoker>(sp => sp.GetRequiredService<IAzurePlanningPoker>());
                services.AddSingleton<PlanningPokerAzureNode>();
                services.AddSingleton<IServiceBus, AzureServiceBus>();
                services.AddSingleton<IMessageConverter, MessageConverter>();
                services.AddScoped<IHostedService, AzurePlanningPokerNodeService>();
            }
            else
            {
                services.AddSingleton<IPlanningPoker, PlanningPokerController>();
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

            services.AddScoped<IHostedService, PlanningPokerCleanupService>();

            var clientConfiguration = new PlanningPokerClientConfiguration
            {
                UseServerSideBlazor = UseServerSide
            };
            services.AddSingleton<PlanningPokerClientConfiguration>(clientConfiguration);

            if (clientConfiguration.UseServerSideBlazor)
            {
                services.AddServerSideBlazor();
                services.AddSingleton<HttpClient>();
                services.AddTransient<IHostedService, HttpClientSetupService>();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "ASP.NET Core convention for Startup class.")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();

            var clientConfiguration = app.ApplicationServices.GetRequiredService<PlanningPokerClientConfiguration>();
            if (!clientConfiguration.UseServerSideBlazor)
            {
                app.UseClientSideBlazorFiles<Duracellko.PlanningPoker.Client.Startup>();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToPage("/Index");
            });
        }

        private AzurePlanningPokerConfiguration GetPlanningPokerConfiguration()
        {
            return Configuration.GetSection("PlanningPoker").Get<AzurePlanningPokerConfiguration>() ?? new AzurePlanningPokerConfiguration();
        }
    }
}

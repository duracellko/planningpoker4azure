using System;
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
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.Web
{
    public class Startup : IStartup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public bool UseServerSide => Configuration.GetSection("PlanningPokerClient").GetValue<bool?>("UseServerSide") ?? false;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApplicationPart(typeof(PlanningPokerService).Assembly)
                .AddMvcOptions(o => o.Conventions.Add(new PlanningPokerApplication()));

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm
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
                services.AddServerSideBlazor<Duracellko.PlanningPoker.Client.Startup>();
                services.AddSingleton<HttpClient>();
                services.AddTransient<IHostedService, HttpClientSetupService>();
            }

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // PlanningPoker page should be routed on client side. Always return Index on server side.
            var rewriteOptions = new RewriteOptions()
                .AddRewrite("^Index", "Index", false)
                .AddRewrite("^PlanningPoker", "Index", false);
            app.UseRewriter(rewriteOptions);

            app.UseResponseCompression();
            app.UseMvc();

            var clientConfiguration = app.ApplicationServices.GetRequiredService<PlanningPokerClientConfiguration>();
            if (clientConfiguration.UseServerSideBlazor)
            {
                app.UseServerSideBlazor<Duracellko.PlanningPoker.Client.Startup>();
            }
            else
            {
                app.UseBlazor<Duracellko.PlanningPoker.Client.Startup>();
            }
        }

        private AzurePlanningPokerConfiguration GetPlanningPokerConfiguration()
        {
            return Configuration.GetSection("PlanningPoker").Get<AzurePlanningPokerConfiguration>() ?? new AzurePlanningPokerConfiguration();
        }
    }
}

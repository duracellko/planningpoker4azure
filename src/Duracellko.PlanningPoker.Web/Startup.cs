using System;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApplicationPart(typeof(PlanningPokerService).Assembly)
                .AddMvcOptions(o => o.Conventions.Add(new PlanningPokerApplication()));

            var planningPokerConfiguration = GetPlanningPokerConfiguration();

            services.AddSingleton<DateTimeProvider>();
            services.AddSingleton<IPlanningPokerConfiguration>(planningPokerConfiguration);
            services.AddSingleton<IPlanningPoker, PlanningPokerController>();
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

            app.UseStaticFiles();
            app.UseMvc();
        }

        private PlanningPokerConfiguration GetPlanningPokerConfiguration()
        {
            return Configuration.GetSection("PlanningPoker").Get<PlanningPokerConfiguration>() ?? new PlanningPokerConfiguration();
        }
    }
}

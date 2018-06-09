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

            services.AddSingleton<IPlanningPoker, PlanningPokerController>();
            services.AddTransient<IScrumTeamRepository, FileScrumTeamRepository>();
            services.AddSingleton<IFileScrumTeamRepositorySettings, FileScrumTeamRepositorySettings>();
            services.AddSingleton<IPlanningPokerConfiguration, PlanningPokerConfiguration>();

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
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
    }
}

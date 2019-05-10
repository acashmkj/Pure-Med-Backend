using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using PureMedBlockChain_Backend.Data;
using PureMedBlockChain_Backend.Models;

namespace PureMedBlockChain_Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Option to make PureMedBlockChain accessible throughout application
            services.AddOptions();

            //Adding Logging 
            services.AddLogging();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddDbContext<UserDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<ProductInfoDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<BlockChainsDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            //Adding Background Scheduler to start miner and verifiers every 15 minutes
            #region Background Scheduler Using Quartz DI

            services.Add(new ServiceDescriptor(typeof(IJob), typeof(ScheduledMiner), ServiceLifetime.Transient));
            services.AddSingleton<IJobFactory, ScheduledJobFactory>();
            services.AddSingleton<IJobDetail>(provider =>
            {
                return JobBuilder.Create<ScheduledMiner>()
                  .WithIdentity("Miner.job", "group1")
                  .Build();
            });

            services.AddSingleton<ITrigger>(provider =>
            {
                return TriggerBuilder.Create()
                .WithIdentity($"Miner.trigger", "group1")
                .StartNow()
                .WithSimpleSchedule
                 (s =>
                    s.WithInterval(TimeSpan.FromMinutes(40))
                    .RepeatForever()
                 )
                 .Build();
            });

            services.AddSingleton<IScheduler>(provider =>
            {
                var schedulerFactory = new StdSchedulerFactory();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetService<IJobFactory>();
                scheduler.Start();
                return scheduler;
            });

            #endregion

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Make PureMedBlockChain accessible throughout application
            services.Configure<BlockChain>(Configuration);
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IScheduler scheduler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            //Start Scheduler
            scheduler.ScheduleJob(app.ApplicationServices.GetService<IJobDetail>(), app.ApplicationServices.GetService<ITrigger>());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

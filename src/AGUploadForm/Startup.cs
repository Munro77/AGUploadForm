using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AGUploadForm.Data;
using AGUploadForm.Models;
using AGUploadForm.Models.Settings;
using AGUploadForm.Services;
using Backload.MiddleWare;
using NLog.Extensions.Logging;
using NLog.Web;


namespace AGUploadForm
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("formsettings.json")
                .AddJsonFile("VIPSettings.json",optional:true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                //builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            IEnumerable<IConfigurationSource> a = builder.Sources;
            

            
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddOptions();
            services.Configure<FormSettings>(Configuration);
            services.Configure<VIPSettings>(Configuration);
            services.Configure<AppSettings>(Configuration);
            

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            loggerFactory.AddNLog();

            env.ConfigureNLog("nlog.config");

            //app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            //Adding Backload for handling files
            // Invoke the internal handler only if path contains /Backload/FileHandler
            app.MapWhen(
               context => context.Request.Path.Value.Contains("/Backload/FileHandler"),
               appBranch =>
               {
                   appBranch.UseBackload();
               });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                //template: "{controller=Jobs}/{action=Index}/{id?}");
                //template: "{controller=Home}/{action=BasicPlusUI}/{id?}");
                routes.MapRoute(
                    "vip",                                              // Route name
                    "vip/{id}",                           // URL with parameters
                    new { controller = "Home", action = "Index" }  // Parameter defaults
                    );
            });
        }
    }
}

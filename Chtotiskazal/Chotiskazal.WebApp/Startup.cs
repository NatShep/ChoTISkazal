using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chotiskazal.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
              //  options.UseSqlite(
                //    Configuration.GetConnectionString("DefaultConnection")));
        
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//            .AddEntityFrameworkStores<ApplicationDbContext>();
         
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => 
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                });

            services.AddControllersWithViews();
            
            services.AddRazorPages();
            
            
            var dbFileName = Configuration.GetValue<string>("wordDb");
            
         //   services.AddSingleton(new NewWordsService(new RuEngDictionary(), new WordsRepository(dbFileName)));

            var yadicapiKey = Configuration.GetValue<string>("yadicapi:key");
            var yadicapiTimeout = Configuration.GetValue<TimeSpan>("yadicapi:timeout");

            services.AddSingleton(new YandexDictionaryApiClient(yadicapiKey, yadicapiTimeout));

            var yatransapiKey = Configuration.GetValue<string>("yatransapi:key");
            var yatransapiTimeout = Configuration.GetValue<TimeSpan>("yatransapi:timeout");
            services.AddSingleton(new YandexTranslateApiClient(yatransapiKey, yatransapiTimeout));

            services.AddSingleton(new DictionaryService(new DictionaryRepository(dbFileName)));
            services.AddSingleton(new UserService(new UserRepo(dbFileName)));
            services.AddSingleton(new ExamsAndMetricService(new ExamsAndMetricsRepo(dbFileName)));
            services.AddSingleton(new UsersPairsService(new UserWordsRepo(dbFileName)));
            services.AddHostedService<YapiPingHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Menu}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
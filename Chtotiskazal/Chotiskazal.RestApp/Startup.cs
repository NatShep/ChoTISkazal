using System;
using Chotiskazal.Logic.DAL;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.yapi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chotiskazal.RestApp
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
            services.AddControllers();
            
            var dbFileName = Configuration.GetValue<string>("wordDb");
            services.AddSingleton(new NewWordsService(new RuEngDictionary(), new WordsRepository(dbFileName)));

            var yadicapiKey = Configuration.GetValue<string>("yadicapi:key");
            var yadicapiTimeout = Configuration.GetValue<TimeSpan>("yadicapi:timeout");

            services.AddSingleton(new YandexDictionaryApiClient(yadicapiKey, yadicapiTimeout));

            var yatransapiKey = Configuration.GetValue<string>("yatransapi:key");
            var yatransapiTimeout = Configuration.GetValue<TimeSpan>("yatransapi:timeout");

            services.AddSingleton(new YandexTranslateApiClient(yatransapiKey, yatransapiTimeout));

            services.AddHostedService<YapiPingHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            

           app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
    
        }
    }
}

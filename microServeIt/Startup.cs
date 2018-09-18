using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace microServeIt
{
    public class WhiteBoxStartup
    {
        public static WhiteBoxStartup LatestInstance { get;private set;}

        public WhiteBoxStartup(IConfiguration configuration) { Configuration = configuration; }

        public IConfiguration Configuration { get; }
        public IServiceCollection Services { get; private set;}
        public IServiceProvider ServiceProvider { get; private set; }
        public IHostingEnvironment HostingEnvironment { get; private set;}

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvc();
            services.AddServeIt();
            services.AddScoped<ServeItDiagnosticsB>();
            Services = services;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseServeIt();
            ServiceProvider= app.ApplicationServices;
            HostingEnvironment=env;
            LatestInstance=this;
        }
    }
}

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
            services.AddMvc();
            services.AddLogging();
            services.AddScoped<ITestServeIt,TestServeIt>();
            services.AddScoped<TestServeItB>();
            services.AddSingleton(services);
            Services = services;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseServeIt();
            ServiceProvider= app.ApplicationServices;
            HostingEnvironment=env;
            LatestInstance=this;
        }
    }

    public interface ITestServeIt 
    { 
        Dictionary<string, object> GetParameters(Dictionary<string, object> args);
        (string, string) GetParameters(string a, string b);
    }

    public class TestServeIt : ITestServeIt 
    {
        public Dictionary<string, object> GetParameters(Dictionary<string, object> args) => args;
        public (string, string) GetParameters(string a, string b) => (a, b); 
    }

    public class TestServeItB
    {
        public Dictionary<string, object> ReturnParameters(Dictionary<string, object> args) => args;
    }

}

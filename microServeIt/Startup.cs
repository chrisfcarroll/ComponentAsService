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
        Dictionary<string, object> ShowRouteValues(Dictionary<string, object> allMvcRouteValues);
        (string, string) GetParameters(string a, string b);
        (string, int) GetParameters(string a, int b);
        (object, object, object) GetParameters(object a, object b, object c);
        object[] GetParameters(params object[] args);
    }

    public class TestServeIt : ITestServeIt 
    {
        public Dictionary<string, object> ShowRouteValues(Dictionary<string, object> allMvcRouteValues) => allMvcRouteValues;
        public (string, string) GetParameters(string a, string b) => (a, b);
        public (string, int) GetParameters(string a, int b) => (a, b);
        public (object, object, object) GetParameters(object a, object b, object c) => (a, b, c);
        public object[] GetParameters(params object[] args) => args;
    }

    public class TestServeItB
    {
        public Dictionary<string, object> ReturnParameters(Dictionary<string, object> args) => args;
    }

}

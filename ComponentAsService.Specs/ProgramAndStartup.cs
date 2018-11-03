using AspNetCore.Mvc.Routes.DebuggingLoggerMiddleware;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Component.As.Service.Specs
{
    public class ProgramAndStartup
    {
        public static void ExampleMain(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                                            WebHost.CreateDefaultBuilder(args)
                                                   .UseStartup<ProgramAndStartup>();
        public void ConfigureServices(IServiceCollection services)
        {
            services
               .AddMvc()
               .AddComponentAsService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app
               .UseDeveloperExceptionPage()
               .UseMvcWithDefaultRoute()
               .UseComponentAsService<ComponentAsServiceDiagnostics>()
               .UseMiddleware<RouteDebuggingLogger>();
        }
    }

    public class HomeController : Controller
    {
        public string Index()         => "HomeController/Index";
        public string Index(int a) => "HomeController/Index: you said a=" + a;
    }

    [Controller]
    public class Home : Controller
    {
        public string Index()         => "[Controller]Home/Index";
        public string Index(string b) => "[Controller]Home/Index: you said b=" + b;
    }
    
}

using AspNetCore.Mvc.Routes.DebuggingLoggerMiddleware;
using Component.As.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Example.AspNetCore.Empty
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
        public IActionResult Index()
            => Content("<h2>HomeController/Index</h2>","text/html");
        
        public IActionResult Index(string a) 
            => Content("<h2>HomeController/Index</h2><p>You said a=" + a +"</p>", "text/html");
    }    

    public class AController : Controller
    {
        public IActionResult Index()=> Content("AController/Index ");
        public IActionResult Index(string a)=> Content("AController/Index: you said a=" + a);
    }

    [Controller]
    public class A : Controller
    {
        public IActionResult Index()=> Content("[Controller]A/Index ");
        public IActionResult Index(string b)=> Content("[Controller]A/Index: you said b=" + b);
    }

}

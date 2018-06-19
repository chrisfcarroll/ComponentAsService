using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ServeIt
{
    public class Startup
    {
        public Startup(IConfiguration configuration) { Configuration = configuration; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseServeIt();            
        }
    }

    public static class ServeItStartUpExtensions
    {
        static string regexIdentifierU =
            @"^_?(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}|\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc})+$"
               .Replace("{", "{{")
               .Replace("}", "}}");

        public static string regexIdentifier = "^_?[A-Za-z_0-9]+$";

        public static void UseServeIt(this IApplicationBuilder app)
        {
            var serveItRouteHandler = 
                new RouteHandler(
                     context =>
                     {
                         var routeValues = context.GetRouteData().Values;
                         return context.Response.WriteAsync( JsonConvert.SerializeObject(routeValues)  );
                     });
            
            var routes = 
                new RouteBuilder(app, serveItRouteHandler)
                    .MapRoute(
                      "ServeIt component/method/...parameters Route",
                      $@"{{component:regex({regexIdentifier})}}/{{method:regex({regexIdentifier})}}/{{*params}}"
                     )
                   .Build();
            
            app.UseRouter(routes);
            
        }
    }
}

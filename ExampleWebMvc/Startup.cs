using ExampleComponent;
using microServeIt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ExampleWebMvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddServeIt();
            services.AddScoped<IExemplify, ExampleImp>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()){app.UseDeveloperExceptionPage();}

            app.UseMvc(routes =>
                       {
                           routes.MapRoute(
                                           name: "default",
                                           template: "{controller=Home}/{action=Index}/{id?}");
                       });
            app.UseServeIt();
        }
    }
}

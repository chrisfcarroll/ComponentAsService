using ComponentAsService;
using ExampleComponent;
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
            services.AddComponentAsService();
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
            app.UseComponentAsService();
        }
    }
}

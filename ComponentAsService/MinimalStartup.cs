using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ComponentAsService
{
    public class MinimalStartup
    {
        public void ConfigureServices(IServiceCollection services) => services.AddComponentAsService();
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) => app.UseComponentAsService();
    }
}
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Component.As.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
                .CreateDefaultBuilder(args)
                .ConfigureServices(services=> services.AddComponentAsService())
                .Configure(app=>app.UseComponentAsService<ComponentAsServiceDiagnostics>() );
    }
}

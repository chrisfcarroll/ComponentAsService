using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace ComponentAsService.Specs.WhenStartingUp
{
    public class RoutingConfigurationIsRespected
    {
        readonly ITestOutputHelper console;
        public RoutingConfigurationIsRespected(ITestOutputHelper console) => this.console = console;

        [Fact]
        public async Task GivenOverriddenServiceNameKeyAndMethodNameKey()
        {
            var server = GivenServerWithComponentAsService<StartupWithOptions>();
            var client = server.CreateClient().With(c => c.BaseAddress = new Uri("http://localhost"));

            //
            var httpResult= await client.GetAsync(
                $"{nameof(IComponentAsServiceDiagnostics)}/" +
                $"{nameof(IComponentAsServiceDiagnostics.GetRouteValues)}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);

            //
            httpResult.StatusCode.ShouldBe(HttpStatusCode.OK);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);
            dictionaryResult
                .ShouldHaveKey(StartupWithOptions.ServiceNameKey)
                .ShouldBe(nameof(IComponentAsServiceDiagnostics));
            dictionaryResult
                .ShouldHaveKey(StartupWithOptions.MethodNameKey)
                .ShouldBe(nameof(IComponentAsServiceDiagnostics.GetRouteValues));
        }

        static TestServer GivenServerWithComponentAsService<TStartup>()
        {
            return TestServerBuilder.RunningServerUsingStartup<TStartup>( typeof(ComponentAsServiceController).Assembly);
        }
        
        class StartupWithOptions
        {
            public static readonly string ServiceNameKey = "service" + Guid.NewGuid().ToString("N");
            public static readonly string MethodNameKey = "method" + Guid.NewGuid().ToString("N");

            public StartupWithOptions(IConfiguration configuration) { }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddComponentAsService(new ComponentAsServiceConfiguration(ServiceNameKey,MethodNameKey));
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseComponentAsService(routeTemplate: $"{{{ServiceNameKey}}}/{{{MethodNameKey}}}"
                );
            }
        }
    }
}
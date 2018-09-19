using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace ComponentAsService.Specs.WhenStartingUp
{
    public class UseComponentAsServiceStartsUp
    {
        readonly ITestOutputHelper console;
        readonly HttpClient client;

        public UseComponentAsServiceStartsUp(ITestOutputHelper console)
        {
            this.console = console;
            var server = GivenServerWithComponentAsService<StartupUsingAddServeItWithoutAddMvc>();
            this.client = server.CreateClient().With(c => c.BaseAddress = new Uri("http://localhost"));
        }

        [Fact]
        public async Task GivenOnlyAddComponentAsServiceHasBeenCalled()
        {
            var httpResult= await client.GetAsync(
                $"{nameof(IComponentAsServiceDiagnostics)}/" +
                $"{nameof(IComponentAsServiceDiagnostics.GetRouteValues)}");

            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            httpResult.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        static TestServer GivenServerWithComponentAsService<TStartup>()
        {
            return TestServerBuilder.RunningServerUsingStartup<TStartup>( typeof(ComponentAsServiceController).Assembly);
        }

        class StartupUsingAddServeItWithoutAddMvc
        {
            public StartupUsingAddServeItWithoutAddMvc(IConfiguration configuration) {  }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddComponentAsService();
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseComponentAsService();
            }
        }
    }
}
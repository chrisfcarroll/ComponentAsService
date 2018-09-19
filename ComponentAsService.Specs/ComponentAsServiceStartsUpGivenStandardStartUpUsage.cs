using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace ComponentAsService.Specs
{
    public class ComponentAsServiceStartsUpGivenStandardStartUpUsage : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;
        public ComponentAsServiceStartsUpGivenStandardStartUpUsage(ITestOutputHelper console) => this.console = console;

        [Fact]
        public async Task Method_AddComponentAsService_CallsAddMvc()
        {
            var client=GivenClientForRunningServer<StartupUsingAddServeItWithoutAddMvc>().ShouldNotBeNull();
            var httpResult= await client.GetAsync($"{nameof(IComponentAsServiceDiagnostics)}/{nameof(IComponentAsServiceDiagnostics.GetRouteValues)}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);            
        }
        
        public class StartupUsingAddServeItWithoutAddMvc
        {
            public StartupUsingAddServeItWithoutAddMvc(IConfiguration configuration) { }
            public void ConfigureServices(IServiceCollection services) { services.AddComponentAsService();}
            public void Configure(IApplicationBuilder app, IHostingEnvironment env) { app.UseComponentAsService(); }
        }
    }
}
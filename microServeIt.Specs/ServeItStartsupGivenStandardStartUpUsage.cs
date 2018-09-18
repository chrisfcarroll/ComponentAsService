using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace microServeIt.Specs
{
    public class ServeItStartsupGivenStandardStartUpUsage : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;
        public ServeItStartsupGivenStandardStartUpUsage(ITestOutputHelper console) => this.console = console;

        [Fact]
        public async Task Method_AddServeIt_CallsAddMvc()
        {
            var client=GivenClientForRunningServer<StartupUsingAddServeItWithoutAddMvc>().ShouldNotBeNull();
            var httpResult= await client.GetAsync($"{nameof(IServeItDiagnostics)}/{nameof(IServeItDiagnostics.ShowRouteValues)}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);            
        }
        
        public class StartupUsingAddServeItWithoutAddMvc
        {
            public StartupUsingAddServeItWithoutAddMvc(IConfiguration configuration) { }
            public void ConfigureServices(IServiceCollection services) { services.AddServeIt();}
            public void Configure(IApplicationBuilder app, IHostingEnvironment env) { app.UseServeIt(); }
        }
    }
}
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace microServeIt.Tests
{
    public class ServeItStartupFacts : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;
        public ServeItStartupFacts(ITestOutputHelper console) => this.console = console;

        [Fact]
        public void Method_UseServeIt_ThrowsExplanationIfMvcServiceIsNotFound()
        {
            Assert.ThrowsAny<Exception>( ()=> GivenClientForRunningServer<StartupMissingMvc>())
                  .With(e=>console.WriteLine(e.ToString()))
                  .Message
                  .ShouldContain(ServeItStartUpExtensions.MvcRoutingRequiredMessage );
        }
        
        public class StartupMissingMvc
        {
            public StartupMissingMvc(IConfiguration configuration) { }
            public void ConfigureServices(IServiceCollection services)              {}
            public void Configure(IApplicationBuilder app, IHostingEnvironment env) { app.UseServeIt(); }
        }
    }
}
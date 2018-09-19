using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace ComponentAsService.Specs.WhenStartingUp
{
    public class UseComponentAsServiceThrowsIfItCannotStartup : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;
        public UseComponentAsServiceThrowsIfItCannotStartup(ITestOutputHelper console) => this.console = console;

        [Fact]
        public void ThrowsExplanationIfAddComponentAsServiceWasNotCalled()
        {
            Assert.Throws<InvalidOperationException>( ()=> GivenClientForRunningServer<StartupMissingAddComponentAsService>())
                  .With(e=>console.WriteLine(e.ToString()))
                  .Message
                  .ShouldContain(ComponentAsServiceExtensions.AddComponentAsServiceRequiredMessage );
        }

        [Fact]
        public void ThrowsExplanationIfRouteTemplateOmitsSpecialNames()
        {
            Assert.Throws<ArgumentException>( ()=> GivenClientForRunningServer<StartupMissingCustomSpecialNames>())
                .With(e=>console.WriteLine(e.ToString()));
            Assert.Throws<ArgumentException>( ()=> GivenClientForRunningServer<StartupMissingCustomRouteTemplate>())
                .With(e=>console.WriteLine(e.ToString()));
        }

        public class StartupMissingCustomSpecialNames
        {
            public static string ServicePlaceholder = "service" + Guid.NewGuid().ToString("N");
            public static string MethodPlaceholder = "method" + Guid.NewGuid().ToString("N");

            public StartupMissingCustomSpecialNames(IConfiguration configuration) { }
            public void ConfigureServices(IServiceCollection services) { services.AddComponentAsService();}

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseComponentAsService(routeTemplate: $"{{{ServicePlaceholder}}}/{{{MethodPlaceholder}}}");
            }
        }
        public class StartupMissingCustomRouteTemplate
        {
            public static string ServicePlaceholder = "service" + Guid.NewGuid().ToString("N");
            public static string MethodPlaceholder = "method" + Guid.NewGuid().ToString("N");

            public StartupMissingCustomRouteTemplate(IConfiguration configuration) { }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddComponentAsService(new ComponentAsServiceConfiguration(ServicePlaceholder,MethodPlaceholder));
            }

            public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            {
                app.UseComponentAsService();
            }
        }
        
        public class StartupMissingAddComponentAsService
        {
            public StartupMissingAddComponentAsService(IConfiguration configuration) { }
            public void ConfigureServices(IServiceCollection services) {}
            public void Configure(IApplicationBuilder app, IHostingEnvironment env) { app.UseComponentAsService(); }
        }        
    }
}
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace ComponentAsService.Specs.WhenServing
{
    public class ControllerThrowsIfItCannotRouteAUrl
    {
        readonly ITestOutputHelper console;

        [Fact]
        public void Serve_ThrowsExplanationIfServiceNotFound()
        {
            var serviceCollection = new ServiceCollection();
            new WhiteBoxStartup(new ConfigurationBuilder().Build()).ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var controller= 
                new ComponentAsServiceController(serviceCollection, serviceProvider, serviceProvider.GetService<ILogger<ComponentAsServiceController>>(), ComponentAsServiceConfiguration.DefaultValues)
                    .WithControllerContext("Serve", new {service = "FakeNonExistentService", method = "method"});

            controller.ControllerContext.RouteData= new RouteData();                                                          
            controller.ControllerContext.RouteData.Values.Add("service","FakeNonExistentService");
            controller.ControllerContext.RouteData.Values.Add("method","method");

            var ex=Assert.Throws<ArgumentException>(() => controller.Serve());
            console.WriteLine(ex.ToString());
            ex.Message.ShouldContain("FakeNonExistentService");
        }
        
        [Fact]
        public void Serve_ThrowsExplanationIfMethodNotFound()
        {
            var serviceCollection = new ServiceCollection();
            new WhiteBoxStartup(new ConfigurationBuilder().Build()).ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var controller= 
                new ComponentAsServiceController(serviceCollection, serviceProvider, serviceProvider.GetService<ILogger<ComponentAsServiceController>>(), ComponentAsServiceConfiguration.DefaultValues)
                   .WithControllerContext("Serve", new {service = nameof(IComponentAsServiceDiagnostics), method = "NonExistentMethod"});
            controller.ControllerContext.RouteData= new RouteData();                                                          
            controller.ControllerContext.RouteData.Values.Add("service",nameof(IComponentAsServiceDiagnostics));
            controller.ControllerContext.RouteData.Values.Add("method","NonExistentMethod");

            var ex = Assert.Throws<ArgumentException>(() => controller.Serve());            
            console.WriteLine(ex.ToString());                          
            ex.Message.ShouldContain("NonExistentMethod");
        }

        public ControllerThrowsIfItCannotRouteAUrl(ITestOutputHelper console) => this.console = console;
    }
}

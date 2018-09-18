using System;
using microServeIt.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace microServeIt.Specs
{
    public class ServeItThrowsIfItCannotRouteAUrl
    {
        readonly ITestOutputHelper console;

        [Fact]
        public void ServeItController_ThrowsExplanationIfServiceNotFound()
        {
            var serviceCollection = new ServiceCollection();
            new WhiteBoxStartup(new ConfigurationBuilder().Build()).ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serveItController= 
                new ServeItController(serviceCollection, serviceProvider, serviceProvider.GetService<ILogger<ServeItController>>())
                    .WithControllerContext("Serve", new {service = "FakeNonExistentService", method = "method"});

            serveItController.ControllerContext.RouteData= new RouteData();                                                          
            serveItController.ControllerContext.RouteData.Values.Add("service","FakeNonExistentService");
            serveItController.ControllerContext.RouteData.Values.Add("method","method");

            var ex=Assert.Throws<ArgumentException>(() => serveItController.Serve());
            console.WriteLine(ex.ToString());
            ex.Message.ShouldContain("FakeNonExistentService");
        }
        
        [Fact]
        public void ServeItController_ThrowsExplanationIfMethodNotFound()
        {
            var serviceCollection = new ServiceCollection();
            new WhiteBoxStartup(new ConfigurationBuilder().Build()).ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serveItController= 
                new ServeItController(serviceCollection, serviceProvider, serviceProvider.GetService<ILogger<ServeItController>>())
                   .WithControllerContext("Serve", new {service = nameof(IServeItDiagnostics), method = "NonExistentMethod"});
            serveItController.ControllerContext.RouteData= new RouteData();                                                          
            serveItController.ControllerContext.RouteData.Values.Add("service",nameof(IServeItDiagnostics));
            serveItController.ControllerContext.RouteData.Values.Add("method","NonExistentMethod");

            var ex = Assert.Throws<ArgumentException>(() => serveItController.Serve());            
            console.WriteLine(ex.ToString());                          
            ex.Message.ShouldContain("NonExistentMethod");
        }

        public ServeItThrowsIfItCannotRouteAUrl(ITestOutputHelper console) => this.console = console;
    }
}

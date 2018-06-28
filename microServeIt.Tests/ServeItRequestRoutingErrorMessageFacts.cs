using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using microServeIt.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TestBase;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

namespace microServeIt.Tests
{
    public class ServeItRequestRoutingFailureMessages
    {
        readonly ITestOutputHelper console;

        [Fact]
        public void ServeItController_ExplainsIfServiceNotFound()
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
        public void ServeItController_ExplainsIfMethodNotFound()
        {
            var serviceCollection = new ServiceCollection();
            new WhiteBoxStartup(new ConfigurationBuilder().Build()).ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serveItController= 
                new ServeItController(serviceCollection, serviceProvider, serviceProvider.GetService<ILogger<ServeItController>>())
                   .WithControllerContext("Serve", new {service = nameof(ITestServeIt), method = "NonExistentMethod"});
            serveItController.ControllerContext.RouteData= new RouteData();                                                          
            serveItController.ControllerContext.RouteData.Values.Add("service",nameof(ITestServeIt));
            serveItController.ControllerContext.RouteData.Values.Add("method","NonExistentMethod");

            var ex = Assert.Throws<ArgumentException>(() => serveItController.Serve());            
            console.WriteLine(ex.ToString());                          
            ex.Message.ShouldContain("NonExistentMethod");
        }

        public ServeItRequestRoutingFailureMessages(ITestOutputHelper console) => this.console = console;
    }
}

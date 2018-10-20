using System;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using TestBase;

namespace ComponentAsService2.Specs
{
    public class Prerequisite_TestInfrastructureWorks
    {
        [Fact]
        public void CanCreateTestServerAndCreateClient()
        {
            var client
                = new TestServer(ComponentAsService2.Program.CreateWebHostBuilder(new string[0]))
                  .CreateClient()
                  .With(c => c.BaseAddress = new Uri("https://localhost"));
            
            Xunit.Assert.NotNull(client);
        }
    }
}
using System;
using Microsoft.AspNetCore.TestHost;
using TestBase;
using Xunit;

namespace Component.As.Service.Specs
{
    public class Prerequisite_TestInfrastructureWorks
    {
        [Fact]
        public void CanCreateTestServerAndCreateClient()
        {
            var client
                = new TestServer(Program.CreateWebHostBuilder(new string[0]))
                  .CreateClient()
                  .With(c => c.BaseAddress = new Uri("https://localhost"));
            
            Xunit.Assert.NotNull(client);
        }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using TestBase;

namespace ComponentAsService2.Specs
{
    public class RoutingSpecs
    {
        [Theory]
        [InlineData("/Calculator/Add?a=1&b=2", 3)]
        [InlineData("/Calculator/Add?a=1&b=2&c=3", 6)]
        public async Task RoutesToTypeAndMethod(string url, int expected)
        {
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected.ToString());
        }

        [NotYetATheory("First need access to incoming route values for parameters")]
        [InlineData("/Calculator/Add?a=1.1&b=2", 3.1)]
        [InlineData("/Calculator/Add?a=1&b=2&c=3.3", 6.3)]
        public async Task CanResolveOverloads(string url, float expected)
        {
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected.ToString());
        }

        public RoutingSpecs()
            => client
                   = new TestServer(Program.CreateWebHostBuilder(new string[0]))
                     .CreateClient()
                     .With(c => c.BaseAddress = new Uri("https://localhost"));

        HttpClient client;
    }
}
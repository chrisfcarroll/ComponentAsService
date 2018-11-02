using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using TestBase;
using Xunit;

namespace Component.As.Service.Specs
{
    public class RoutingSpecs
    {
        [Theory]
        [InlineData("/ComponentAsServiceDiagnostics/And?a=true&b=true", "true")]
        [InlineData("/ComponentAsServiceDiagnostics/And?a=1&b=1", "false")]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2&c=3", "6")]
        public async Task RoutesToTypeAndMethod(string url, string expected)
        {
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected);
        }

        [Theory] 
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1.1&b=2", "1.12" /* string concatenation*/)]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2&c=3.3", "6.3")]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2&c=3", "6")]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2", "3")]
        public async Task CanResolveOverloads(string url, string expected)
        {
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected);
        }

        public RoutingSpecs()
            => client
                   = new TestServer(Program.CreateWebHostBuilder(new string[0]))
                     .CreateClient()
                     .With(c => c.BaseAddress = new Uri("https://localhost"));

        HttpClient client;
    }
}
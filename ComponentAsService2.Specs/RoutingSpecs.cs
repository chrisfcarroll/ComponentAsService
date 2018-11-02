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

        [Fact]
        public async Task RoutesToTypeAndMethodUsingMvcComplexObjectBinding()
        {
            var diagnostic= new DiagnosticModel
            {
                A= "A", B= 1.1f, Recurse = new DiagnosticModel{A="R.A", B=1.2f}
            };
            var query = "?a=-1&diagnostic.A=A&diagnostic.B=1.1&diagnostic.Recurse.A=R.A&diagnostic.Recurse.B=1.2";
            var url = "/ComponentAsServiceDiagnostics/BindComplexObject" + query;
            var expected = new ComponentAsServiceDiagnostics().BindComplexObject(-1, diagnostic);

            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected);
        }

        [Fact]
        public async Task RoutesToTypeAndMethodUsingMvcComplexObjectBindingConfoundedByIgnoringCase()
        {
            var misdiagnostic= new DiagnosticModel
            {
                A= "-1", B= 1.1f, Recurse = new DiagnosticModel{A="R.A", B=1.2f}
            };

            var query = "?a=-1&A=A&B=1.1&Recurse.A=R.A&Recurse.B=1.2";
            var url = "/ComponentAsServiceDiagnostics/BindComplexObject" + query;

            var expected = new ComponentAsServiceDiagnostics().BindComplexObject(-1, misdiagnostic);

            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldBe(expected, "Expected MVC binding to bind A=-1 instead of A=A");
        }

        [Theory] 
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1.1&b=2", "1.12" /* string concatenation*/)]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2&c=3.3", "6.3")]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2&c=3", "6")]
        [InlineData("/ComponentAsServiceDiagnostics/Add?a=1&b=2", "3")]
        public async Task ResolvesOverloadsByTypeQuiteWellGivenAllInputsAreStrings(string url, string expected)
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
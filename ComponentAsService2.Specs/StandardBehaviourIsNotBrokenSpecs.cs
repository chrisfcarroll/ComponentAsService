using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using TestBase;
using Xunit;

namespace ComponentAsService2.Specs
{
    public class StandardBehaviourIsNotBrokenSpecs
    {
        [Theory]
        [InlineData("/", "Home Page")]
        [InlineData("/Home", "Home Page")]
        [InlineData("/Home/Index", "Home Page")]
        [InlineData("/Home/Privacy", "<p>Use this page to detail your site's privacy policy.</p>")]
        public async Task RoutesInTheAspNetCoreMvcTemplateWorkAsExpected(string url, string expectedSnippet)
        {
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            Console.WriteLine(stringResult);
            stringResult.ShouldContain(expectedSnippet);
        }
        
        public StandardBehaviourIsNotBrokenSpecs()
            => client
                   = new TestServer(Program.CreateWebHostBuilder(new string[0]))
                     .CreateClient()
                     .With(c => c.BaseAddress = new Uri("https://localhost"));

        HttpClient client;
    }
}
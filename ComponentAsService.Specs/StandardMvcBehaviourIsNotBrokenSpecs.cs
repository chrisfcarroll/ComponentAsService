using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using TestBase;
using Xunit;

namespace Component.As.Service.Specs
{
    public class StandardMvcBehaviourIsNotBrokenSpecs
    {
        [Theory]
        [InlineData("/")]
        [InlineData("/Home")]
        [InlineData("/Home/Index")]
        [InlineData("/Home/Index?a=1")]
        public async Task DefaultMvcRoutingWorksAsExpected(string url)
        {            
            var expected = url.Contains("?a=1") 
                ? new HomeController().Index(1)
                : new HomeController().Index();
            
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();
            Console.WriteLine(stringResult);
            
            stringResult.ShouldContain(expected);
        }
        
        
        public StandardMvcBehaviourIsNotBrokenSpecs()
            => client
                   = new TestServer(ProgramAndStartup.CreateWebHostBuilder(new string[0]))
                     .CreateClient()
                     .With(c => c.BaseAddress = new Uri("https://localhost"));

        HttpClient client;
    }
}
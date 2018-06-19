using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Example;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServeIt.Controllers;
using TestBase;
using Xunit;

namespace ServeIt.Tests
{
    public class ServeItControllerFacts : HostedMvcTestFixtureBase
    {
        [Fact]
        public async Task ServeItController_ReturnsJson__ByDefault()
        {
            var httpResult= await client.GetAsync($"Example/GetSomeNumbers?name=input");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            var output = JObject.Parse(stringResult);

            output.Count.ShouldBeGreaterThan(0);
        }
        
        [Fact]
        public async Task ExampleController_Routes_ExampleMethod()
        {
            var input = "fake";
            
            var httpResult= await client.GetAsync($"Example/GetSomeNumbers?name={input}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            var output = JsonConvert.DeserializeObject<List<int>>(stringResult);

            output.ShouldBe( new ExampleImp().GetSomeNumbers(input) );
        }

        public ServeItControllerFacts()
        {
            client = GivenClientForRunningServer<Startup>();
        }

        readonly HttpClient client;
    }
}

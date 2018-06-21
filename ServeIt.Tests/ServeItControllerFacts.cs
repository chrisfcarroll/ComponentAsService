using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Example;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServeIt.Controllers;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace ServeIt.Tests
{
    public class ServeItControllerFacts : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;

        [Theory]
        [InlineData("ComponentName","MethodName")]
        public async Task ServeItController_IdentifiesComponentAndMethodFromRoute(string componentName, string methodName)
        {
            var httpResult= await client.GetAsync($"{componentName}/{methodName}?name=input");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            dictionaryResult.ShouldHaveKey("component").ShouldBe(componentName);
            dictionaryResult.ShouldHaveKey("method").ShouldBe(methodName);
        }
        
        [Theory]
        [InlineData("p1", "p1", "p2",2, "p3",3)]
        public async Task ServeItController_IdentifiesParametersFromQueryString(params object[] data)
        {
            var paramDict= new Dictionary<string,object>();
            for (int i = 0; i < data.Length ; i += 2)
            {
                paramDict[(string)data[i]]=data[i+1];
            }
            
            var httpResult= await client.GetAsync("Example/Example?" + string.Join("&", paramDict.Select(kv=>$"{kv.Key}={kv.Value}") ));
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(string.Join("," , paramDict));
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            dictionaryResult.ShouldHaveKey("component").ShouldBe("Example");
            dictionaryResult.ShouldHaveKey("method").ShouldBe("Example");
            paramDict.ShouldAll(kv => dictionaryResult.ContainsKey(kv.Key));
            paramDict.ShouldAll(kv => dictionaryResult[kv.Key].ShouldBe(  $"{kv.Value}"));
        }
        
        public ServeItControllerFacts(ITestOutputHelper console)
        {
            this.console = console;
            client = GivenClientForRunningServer<Startup>();
        }

        readonly HttpClient client;
    }
}

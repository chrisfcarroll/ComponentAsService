using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using microServeIt.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace microServeIt.Tests
{
    public class ServeItRequestRoutingFacts : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;

        [Theory]
        [InlineData(nameof(IServeItDiagnostics),nameof(IServeItDiagnostics.ShowRouteValues))]
        public async Task ServeItController_IdentifiesServiceAndMethodFromRoute(string serviceName, string methodName)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?name=input&number=2");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            dictionaryResult.ShouldHaveKey("name").ShouldBe("input");
            dictionaryResult.ShouldHaveKey("number").ShouldBe("2");
        }
        
        [Theory]
        [InlineData(nameof(IServeItDiagnostics),nameof(IServeItDiagnostics.ShowRouteValues))]
        public async Task ServeItController_TreatsSpecialParameterNameForAllMvcRouteValuesSpecially(string serviceName, string methodName)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?name=input&number=2");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            foreach (var expectedRouteValueKey in ServeItController.ReservedRouteValueNames)
            {
                dictionaryResult.ShouldHaveKey(expectedRouteValueKey);
            }
            dictionaryResult.ShouldHaveKey("controller").ShouldBe("ServeIt");
            dictionaryResult.ShouldHaveKey("action").ShouldBe("Serve");
            dictionaryResult.ShouldHaveKey(SpecialNames.DefaultValues.RouteValueServiceName ).ShouldBe(serviceName);
            dictionaryResult.ShouldHaveKey(SpecialNames.DefaultValues.RouteValueMethodName  ).ShouldBe(methodName);
        }

        
        [Theory]
        [InlineData(nameof(IServeItDiagnostics),nameof(IServeItDiagnostics.ShowRouteValues), "p1", "p1", "p2",2, "p3",3)]
        public async Task ServeItController_GetsDictionaryKeyParametersFromQueryString(string serviceName, string methodName, params object[] data)
        {
            var paramDict= new Dictionary<string,object>();
            for (int i = 0; i < data.Length ; i += 2)
            {
                paramDict[(string)data[i]]=data[i+1];
            }
            
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?" + string.Join("&", paramDict.Select(kv=>$"{kv.Key}={kv.Value}") ));
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(string.Join("," , paramDict));
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            paramDict.ShouldAll(kv => dictionaryResult.ContainsKey(kv.Key));
            paramDict.ShouldAll(kv => dictionaryResult[kv.Key].ShouldBe(  $"{kv.Value}"));
        }

        [Theory]
        [InlineData(nameof(IServeItDiagnostics),nameof(IServeItDiagnostics.GetParameters), "a", "b", "c")]
        public async Task ServeItController_IdentifiesNamedParametersFromQueryString(string serviceName, string methodName, string a, string b, string c)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?a={a}&b={b}&c={c}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(string.Join("," , a,b,c));
            console.QuoteLine(stringResult);
            var result = JsonConvert.DeserializeObject<(string,string,string)>(stringResult);
            result.ShouldBe( (a, b, c) );
        }
        
        public ServeItRequestRoutingFacts(ITestOutputHelper console)
        {
            this.console = console;
            client = GivenClientForRunningServer<WhiteBoxStartup>();
        }

        readonly HttpClient client;
    }
}

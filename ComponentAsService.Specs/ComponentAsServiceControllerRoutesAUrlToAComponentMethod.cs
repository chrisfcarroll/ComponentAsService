using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace ComponentAsService.Specs
{
    public class ComponentAsServiceControllerRoutesAUrlToAComponentMethod : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;

        [Theory]
        [InlineData(nameof(IComponentAsServiceDiagnostics),nameof(IComponentAsServiceDiagnostics.ShowRouteValues))]
        public async Task Serve_IdentifiesServiceAndMethodFromRoute(string serviceName, string methodName)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?name=input&number=2");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            dictionaryResult.ShouldHaveKey("name").ShouldBe("input");
            dictionaryResult.ShouldHaveKey("number").ShouldBe("2");
        }
        
        [Theory]
        [InlineData(nameof(IComponentAsServiceDiagnostics),nameof(IComponentAsServiceDiagnostics.ShowRouteValues))]
        public async Task Serve_TreatsSpecialParameterNameForAllMvcRouteValuesSpecially(string serviceName, string methodName)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?name=input&number=2");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            foreach (var expectedRouteValueKey in ComponentAsServiceController.ReservedRouteValueNames)
            {
                dictionaryResult.ShouldHaveKey(expectedRouteValueKey);
            }
            dictionaryResult.ShouldHaveKey("controller").ShouldBe(SpecialNames.DefaultValues.ComponentAsServiceControllerName);
            dictionaryResult.ShouldHaveKey("action").ShouldBe(SpecialNames.DefaultValues.ComponentAsServiceControllerActionName);
            dictionaryResult.ShouldHaveKey(SpecialNames.DefaultValues.RouteValueServiceName ).ShouldBe(serviceName);
            dictionaryResult.ShouldHaveKey(SpecialNames.DefaultValues.RouteValueMethodName  ).ShouldBe(methodName);
        }

        
        [Theory]
        [InlineData(nameof(IComponentAsServiceDiagnostics),nameof(IComponentAsServiceDiagnostics.ShowRouteValues), "p1", "p1", "p2",2, "p3",3)]
        public async Task Serve_ParsesQueryStringToADictionary(string serviceName, string methodName, params object[] data)
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
        [InlineData(nameof(IComponentAsServiceDiagnostics),nameof(IComponentAsServiceDiagnostics.GetParameters), "a", "b", "c")]
        public async Task Serve_IdentifiesNamedParametersInQueryString(string serviceName, string methodName, string a, string b, string c)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?a={a}&b={b}&c={c}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(string.Join("," , a,b,c));
            console.QuoteLine(stringResult);
            var result = JsonConvert.DeserializeObject<(string,string,string)>(stringResult);
            result.ShouldBe( (a, b, c) );
        }
        
        public ComponentAsServiceControllerRoutesAUrlToAComponentMethod(ITestOutputHelper console)
        {
            this.console = console;
            client = GivenClientForRunningServer<WhiteBoxStartup>();
        }

        readonly HttpClient client;
    }
}

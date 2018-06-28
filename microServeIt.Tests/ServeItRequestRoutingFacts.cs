using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        [InlineData(nameof(ITestServeIt),nameof(ITestServeIt.GetParameters))]
        public async Task ServeItController_IdentifiesServiceAndMethodFromRoute(string serviceName, string methodName)
        {
            var httpResult= await client.GetAsync($"{serviceName}/{methodName}?name=input");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var dictionaryResult = JsonConvert.DeserializeObject<RouteValueDictionary>(stringResult);

            dictionaryResult.ShouldHaveKey("service").ShouldBe(serviceName);
            dictionaryResult.ShouldHaveKey("method").ShouldBe(methodName);
        }
        
        [Theory]
        [InlineData(nameof(ITestServeIt),nameof(ITestServeIt.GetParameters), "p1", "p1", "p2",2, "p3",3)]
        public async Task ServeItController_IdentifiesParametersFromQueryString(string serviceName, string methodName, params object[] data)
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

            dictionaryResult.ShouldHaveKey("service").ShouldBe(serviceName);
            dictionaryResult.ShouldHaveKey("method").ShouldBe(methodName);
            paramDict.ShouldAll(kv => dictionaryResult.ContainsKey(kv.Key));
            paramDict.ShouldAll(kv => dictionaryResult[kv.Key].ShouldBe(  $"{kv.Value}"));
        }

        [Fact]
        public async Task ServeItController_CanSelectAMethodByParameterName()
        {
            string p1 = "p1!";
            int p2 = 2;
            var httpResult = await client.GetAsync($"{nameof(ITestServeIt)}/{nameof(ITestServeIt.GetParameters)}?a={p1}&b={p2}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var result = JsonConvert.DeserializeObject<(string,string)>(stringResult);

            result.Item1.ShouldBe( p1.ToString());
            result.Item2.ShouldBe( p2.ToString());
        }

        public ServeItRequestRoutingFacts(ITestOutputHelper console)
        {
            this.console = console;
            client = GivenClientForRunningServer<WhiteBoxStartup>();
        }

        readonly HttpClient client;

    }
}

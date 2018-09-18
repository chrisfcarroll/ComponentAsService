using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace microServeIt.Specs
{
    public class ServeItRoutesAUrlToAMatchingMethodOverload : HostedMvcTestFixtureBase
    {
        readonly ITestOutputHelper console;
        
        [Fact]
        public async Task ServeItController_MatchesAMethodUsingParameterNames()
        {
            string a = "p1!";
            int b = 2;
            string c = "c";
            var httpResult = await client.GetAsync($"{nameof(IServeItDiagnostics)}/{nameof(IServeItDiagnostics.GetParameters)}?a={a}&b={b}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var result = JsonConvert.DeserializeObject<(string,string)>(stringResult);
            result.Item1.ShouldBe( a.ToString());
            result.Item2.ShouldBe( b.ToString());            
            
            httpResult = await client.GetAsync($"{nameof(IServeItDiagnostics)}/{nameof(IServeItDiagnostics.GetParameters)}?a={a}&b={b}&c={c}");
            stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            var result2 = JsonConvert.DeserializeObject<(string,string,string)>(stringResult);
            result2.Item1.ShouldBe( a.ToString());
            result2.Item2.ShouldBe( b.ToString());
            result2.Item3.ShouldBe( c.ToString());
        }

        [Fact]
        public async Task ServeItController_MatchesAMethodUsingNumberOfParameters()
        {
            string a = "p1!";
            int b = 2;
            string c = "c";
            var httpResult = await client.GetAsync($"{nameof(IServeItDiagnostics)}/{nameof(IServeItDiagnostics.GetParameterCount)}?a={a}&b={b}");
            var stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            JsonConvert.DeserializeObject<int>(stringResult).ShouldBe(2);
            
            httpResult = await client.GetAsync($"{nameof(IServeItDiagnostics)}/{nameof(IServeItDiagnostics.GetParameterCount)}?a={a}&b={b}&c={c}");
            stringResult = await httpResult.Content.ReadAsStringAsync();
            console.QuoteLine(stringResult);
            JsonConvert.DeserializeObject<int>(stringResult).ShouldBe(3);
        }

        public ServeItRoutesAUrlToAMatchingMethodOverload(ITestOutputHelper console)
        {
            this.console = console;
            client = GivenClientForRunningServer<WhiteBoxStartup>();
        }

        readonly HttpClient client;
    }
}

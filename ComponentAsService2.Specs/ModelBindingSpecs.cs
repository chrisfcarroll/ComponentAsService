using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using TestBase;
using Xunit;
using Xunit.Abstractions;

namespace Component.As.Service.Specs
{
    public class ModelBindingSpecs
    {
        readonly ITestOutputHelper console;

        [Theory]
        [InlineData("/" + nameof(ComponentAsServiceDiagnostics) 
                  + "/" + nameof(ComponentAsServiceDiagnostics.BindComplexObject) 
                  + "?diagnostic.A=AValue&diagnostic.b=1.1"
                  + "&diagnostic.recurse.A=diagnosticRecurse.AValue"
                  + "&diagnostic.recurse.recurse.B=2.2"
                  + "&a=9")]
        public async Task AppliesMvcModelBinding(string url)
        {
            var expected = new DiagnosticModel
            {
                A="AValue",
                B= 1.1f,
                Recurse = new DiagnosticModel
                {
                    A="diagnosticRecurse.AValue",
                    Recurse = new DiagnosticModel
                    {
                        B=2.2f
                    }
                }
            };
            
            var stringResult = await (await client.GetAsync(url)).Content.ReadAsStringAsync();

            console.WriteLine(stringResult);
            
            stringResult.ShouldBe(new ComponentAsServiceDiagnostics().BindComplexObject(9,expected) );
        }
        
        public ModelBindingSpecs(ITestOutputHelper console)
        {
            this.console = console;
            client
                = new TestServer(Program.CreateWebHostBuilder(new string[0]))
                 .CreateClient()
                 .With(c => c.BaseAddress = new Uri("https://localhost"));
        }

        HttpClient client;
    }
}
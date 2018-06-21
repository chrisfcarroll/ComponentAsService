using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ServeIt.Controllers
{
    public class ServeItController : Controller
    {
        public const string RegexIdentifier = "^_?[[A-Za-z_0-9]]+$";
        readonly IServiceProvider serviceProvider;
        
        public ServeItController(IServiceProvider serviceProvider  ) { this.serviceProvider = serviceProvider; }
        
        [Route(@"{component:regex(" + RegexIdentifier + ")}/{method:regex(" + RegexIdentifier + ")}")]
        public object Serve()
        {
            var query = Request.Query.ToDictionary(q=>q.Key, q=> (object)string.Join(",",q.Value));
            var values= RouteData.Values;
            return values.Union(query).ToDictionary(kv=>kv.Key, kv=>kv.Value);
        }
    }
}

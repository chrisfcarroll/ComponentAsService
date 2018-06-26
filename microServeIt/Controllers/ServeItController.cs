using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace microServeIt.Controllers
{
    public class ServeItController : Controller
    {
        readonly IServiceProvider serviceProvider;
        readonly ILogger logger;

        public ServeItController(IServiceProvider serviceProvider, ILogger<ServeItController> logger  )
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }
        
        public object Serve()
        {
            return Debug();
        }

        public object Debug()
        {
            var debug = serviceProvider.GetService<IDebugServeIt>();
            if (!methods.TryGetValue((debug.GetType(), nameof(IDebugServeIt.Method)), out MethodInfo method))
            {
                method = debug.GetType().GetMethod(nameof(IDebugServeIt.Method)) ;
                methods[(debug.GetType(), nameof(IDebugServeIt.Method))]= method;
            };
            var query = Request.Query.ToDictionary(q=>q.Key, q=> (object)string.Join(",",q.Value));
            var values= RouteData.Values;
            var args = values.Union(query).ToDictionary(kv=>kv.Key, kv=>kv.Value);
            try
            {
                var result = method.Invoke(debug, new object[]{args});
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e,"evaluating {method}({string.Join("," args)}");
                throw;
            }
        }
        
        static Dictionary<(Type,string),MethodInfo> methods= new Dictionary<(Type, string), MethodInfo>();
    }
}

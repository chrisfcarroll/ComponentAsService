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
            logger.LogDebug("ActionDescriptor {ActionDescriptor}", ControllerContext.ActionDescriptor);
            logger.LogDebug("RouteData {RouteData}", ControllerContext.RouteData);
            logger.LogDebug("ControllerContext {ControllerContext}", ControllerContext);

            var query   = Request.Query.ToDictionary(q => q.Key, q => (object)string.Join(",", q.Value));
            var values  = RouteData.Values;
            var args    = values.Union(query).ToDictionary(kv => kv.Key, kv => kv.Value);
            var service = RouteData.Values["service"] as string;
            var methodName  = RouteData.Values["method"] as string;
            var knownServices= serviceProvider;
            var typeFromService= Type.GetType(service);
            var method = typeFromService
                              .GetMethods()
                              .Where( m=>m.Name == methodName)
                              .Where( m=>m.GetParameters().Select(p=>p.Name).All(n=> args.ContainsKey(n)) )
                              .OrderByDescending(m=>m.GetParameters().Count())
                              .FirstOrDefault();

            methodsCache[(typeFromService.GetType(), nameof(ITestServeIt.GetParameters))]= method;
            try
            {
                var result = method.Invoke(typeFromService, new object[]{args});
                return result;
            }
            catch (Exception e)
            {
                logger.LogError(e,"evaluating {method}({string.Join("," args)}");
                throw;
            }
        }
        
        static Dictionary<(Type,string),MethodInfo> methodsCache= new Dictionary<(Type, string), MethodInfo>();
    }
}

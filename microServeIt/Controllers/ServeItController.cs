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
        public static readonly string[] ReservedRouteValueNames = new []{"service","method","controller","action"};
        
        readonly IServiceCollection services;
        readonly IServiceProvider serviceProvider;
        readonly ILogger logger;

        public ServeItController(IServiceCollection services, IServiceProvider serviceProvider ,ILogger<ServeItController> logger  )
        {
            this.services = services;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }
        
        public object Serve()
        {
            logger.LogDebug("ActionDescriptor {ActionDescriptor}", ControllerContext.ActionDescriptor);
            logger.LogDebug("RouteData {RouteData}", ControllerContext.RouteData);

            var serviceName = RouteData.Values["service"] as string;
            var methodName  = RouteData.Values["method"] as string;
            var serviceType = 
                services.Where(s => s.ServiceType.Name == serviceName).Select(s=>s.ServiceType).FirstOrDefault()
                     ?? throw new ArgumentException(
                            $"Didn't find a service of type {serviceName}. Use StartUp.ConfigureServices() to add it to the service collection?"
                           );
            var implementation = serviceProvider.GetService(serviceType);
            var routedValues= RouteData.Values.Where(kv => kv.Key.IsInList(ReservedRouteValueNames ));
            
            var query   = Request.Query.ToDictionary(q => q.Key, q => (object)string.Join(",", q.Value));
            var values  = RouteData.Values.Where(kv => kv.Key.IsNotInList(ReservedRouteValueNames ));
            var args    = values.Union(query).ToDictionary(kv => kv.Key, kv => kv.Value);
                        
            var method = serviceType.GetMethodsByNameAndParameterAssignability(methodName, args).FirstOrDefault();
            if (method != null)
            {
                var parameters= method.GetParameters().Select(p => args[p.Name]).ToArray();            
                try{ return method.Invoke(implementation, parameters); }
                catch (Exception e) {logger.LogError(e,$"evaluating {method}({string.Join(",", args)}");throw;}                        
            }
            else
            {
                method = serviceType.GetMethodsWithOneDictionaryParameter(methodName, args).FirstOrDefault();
                if (method != null)
                {
                    var argsToUse =
                        (method.GetParameters().First().Name == SpecialNames.DefaultValues.SpecialParameterNameForAllMvcRouteValues
                      && method.GetParameters().First().ParameterType == typeof(Dictionary<string, object>))
                            ? args.Union(routedValues).ToDictionary(kv => kv.Key, kv => kv.Value)
                            : args;
                    
                    try{ return method.Invoke(implementation, new object[]{argsToUse}); }
                    catch (Exception e) {logger.LogError(e,$"evaluating {method}(Dictionary<string,object> {string.Join(",", args)}");throw;}                        
                }
                else
                {
                    method = serviceType.GetMethodsWithOneObjectArrayParameter(methodName, args.Values.ToArray()).FirstOrDefault();
                    if (method != null)
                    {
                        try{ return method.Invoke(implementation, new object[]{args.Values.ToArray()}); }
                        catch (Exception e) {logger.LogError(e,$"evaluating {method}(object[] {string.Join(",", args)}");throw;}                        
                    }
                    
                }
            }

            var ex=new ArgumentException(
                $"Didn't find a method on {serviceName} with name {methodName} "
              + $"which can accept parameters from { string.Join(",", args) }");
            
            logger.LogError(ex, ex.Message);
            throw ex;
        }

        static Dictionary<(Type,string),MethodInfo> methodsCache= new Dictionary<(Type, string), MethodInfo>();
    }
}

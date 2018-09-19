using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComponentAsService
{
    public class ComponentAsServiceController : Controller
    {
        readonly IServiceCollection services;
        readonly IServiceProvider serviceProvider;
        readonly ILogger logger;
        readonly ComponentAsServiceConfiguration configuration;

        public ComponentAsServiceController(
            IServiceCollection services, 
            IServiceProvider serviceProvider,
            ILogger<ComponentAsServiceController> logger, 
            ComponentAsServiceConfiguration configuration)
        {
            this.services = services;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.configuration = configuration;
        }
        
        public object Serve()
        {
            logger.LogDebug("ActionDescriptor {ActionDescriptor}", ControllerContext.ActionDescriptor);
            logger.LogDebug("RouteData {RouteData}", ControllerContext.RouteData);

            var serviceName = RouteData.Values[configuration.RouteValueServiceNameKey] as string;
            var methodName  = RouteData.Values[configuration.RouteValueMethodNameKey] as string;
            var serviceType = SelectServiceTypeByName(serviceName);
            var implementation = serviceProvider.GetService(serviceType);
            var query   = Request.Query.ToDictionary(q => q.Key, q => (object)string.Join(",", q.Value));
            var form = Request.HasFormContentType
                ? Request.Form.ToDictionary(q => q.Key, q => (object) string.Join(",", q.Value))
                : new Dictionary<string, object>();
            var values  = RouteData.Values.Where(kv => kv.Key.IsNotInList(configuration.RouteValuesKeysIgnoredForParameterMatching ));
            var args    = values.Union(query).Union(form).ToDictionary(kv => kv.Key, kv => kv.Value);
                        
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
                    var nonParameterRouteValues= RouteData.Values.Where(kv => kv.Key.IsInList(configuration.RouteValuesKeysIgnoredForParameterMatching ));
                    var argsToUse =
                        (method.GetParameters().First().Name == ComponentAsServiceConfiguration.DefaultValues.DiagnosticParameterNameForAllMvcRouteValues
                      && method.GetParameters().First().ParameterType == typeof(Dictionary<string, object>))
                            ? args.Union(nonParameterRouteValues).ToDictionary(kv => kv.Key, kv => kv.Value)
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

        protected virtual Type SelectServiceTypeByName(string serviceName)
        {
            return services.Where(s => s.ServiceType.Name == serviceName)
                       .Select(s=>s.ServiceType).FirstOrDefault()
                   ?? services.Where(s => s.ServiceType.Name[0]=='I' && s.ServiceType.Name.Substring(1) == serviceName)
                       .Select(s=>s.ServiceType).FirstOrDefault()
                   ?? throw new ArgumentException(
                       $"Didn't find a service of type {serviceName}. Use StartUp.ConfigureServices() to add it to the service collection?");
        }

        static Dictionary<(Type,string),MethodInfo> methodsCache= new Dictionary<(Type, string), MethodInfo>();
    }
}

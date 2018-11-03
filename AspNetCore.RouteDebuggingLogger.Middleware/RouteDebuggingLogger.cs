using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Routes.DebuggingLoggerMiddleware
{
    /// <summary>
    /// Log the known routes. Re-logs each time that <see cref="ActionDescriptorCollection.Version"/> changes.
    /// </summary>
    public class RouteDebuggingLogger
    {
        public static int LastLoggedVersion { get; private set; } = -1;
        public static LogLevel LogLevel = LogLevel.Information;

        readonly RequestDelegate next;
        readonly ILogger<RouteDebuggingLogger> log;

        public RouteDebuggingLogger(RequestDelegate next, ILogger<RouteDebuggingLogger> log)
        {
            this.next = next;
            this.log = log;
        }

        public async Task Invoke(HttpContext httpContext, IActionDescriptorCollectionProvider actionsDescriber)
        {
            await next(httpContext);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                if (actionsDescriber.ActionDescriptors.Version > LastLoggedVersion)
                {
                    LastLoggedVersion = actionsDescriber.ActionDescriptors.Version;

                    log.Log(LogLevel, 0, actionsDescriber.ActionDescriptors,null,
                            (a, e) => $"{actionsDescriber.GetType()}.ActionDescriptors.Version={a.Version}. Actions.Count={a.Items.Count}");

                    foreach (var action in actionsDescriber.ActionDescriptors.Items)
                    {
                        log.Log(LogLevel, 0, action, null, InspectAndFormat);
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        string InspectAndFormat(Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor a, Exception e)
        {
            var timings = new List<string>();
            var timer = Stopwatch.StartNew();
            var displayName = a.DisplayName; timings.Add("Name:" + timer.ElapsedMilliseconds);
            var constraints = a.ActionConstraints.ToJson(); timings.Add("Constraints:" + timer.ElapsedMilliseconds);
            var attributeRouteInfo = a.AttributeRouteInfo?.ToJson(); timings.Add("AttributeRouteInfo:" + timer.ElapsedMilliseconds);
            var boundProperties = a.BoundProperties?.ToJson(); timings.Add("BoundProperties:" + timer.ElapsedMilliseconds);
            var filterDescriptors = a.FilterDescriptors?.Select(
                fd=>new{ 
                     FilterType=fd.Filter.GetType().FullName,
                     fd.Order,
                     fd.Scope
                    }).ToJson(); timings.Add("FilterDescriptors:" + timer.ElapsedMilliseconds);
            var parameters = a.Parameters?.Select(
                p=>new {
                        p.Name, 
                        TypeName=p.ParameterType.FullName, 
                        BindingInfo = new
                        {
                            p.BindingInfo?.BinderModelName,
                            BinderType=p.BindingInfo?.BinderType.FullName,
                            p.BindingInfo?.BindingSource,
                            PropertyFilterProviderType=p.BindingInfo?.PropertyFilterProvider.GetType().FullName,
                            RequestPredicateMethodDeclaringType= p.BindingInfo?.RequestPredicate.Method.DeclaringType?.FullName,
                        }
                    } 
                ).ToJson(); timings.Add("Parameters:" + timer.ElapsedMilliseconds);
            var properties = a.Properties?.ToJson(); timings.Add("Properties:" + timer.ElapsedMilliseconds);
            var routeValues = a.RouteValues?.ToJson(); timings.Add("RouteValues:" + timer.ElapsedMilliseconds);
            return string.Format(
                string.Join(System.Environment.NewLine,
                            "Action: {0} {1}",
                            "Constraints: {2}",
                            "AttributeRouteInfo : {3}",
                            "BoundProperties : {4}",
                            "FilterDescriptors : {5}",
                            "Parameters : {6}",
                            "Properties : {7}",
                            "RouteValues : {8}",
                            "Timings in milliseconds to inspect and format each attribute: {9}"),
                displayName, a.ToString(),
                constraints,
                attributeRouteInfo,
                boundProperties,
                filterDescriptors,
                parameters,
                properties,
                routeValues,
                string.Join(", ", timings)
                );
        }
    }
}

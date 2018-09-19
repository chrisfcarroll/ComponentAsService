using Microsoft.AspNetCore.Mvc;

namespace ComponentAsService
{
    public class ComponentAsServiceConfiguration
    {
        public static readonly ComponentAsServiceConfiguration DefaultValues = new ComponentAsServiceConfiguration();

        public ComponentAsServiceConfiguration(
            string routeValueServiceNameKey = "service", 
            string routeValueMethodNameKey = "method",
            string componentAsServiceControllerName = "ComponentAsService",
            string componentAsServiceActionName = "Serve",
            string diagnosticParameterNameForAllMvcRouteValues = "allMvcRouteValues")
        {
            ComponentAsServiceControllerName = componentAsServiceControllerName;
            ComponentAsServiceControllerActionName = componentAsServiceActionName;
            RouteValueServiceNameKey = routeValueServiceNameKey;
            RouteValueMethodNameKey = routeValueMethodNameKey;
            DiagnosticParameterNameForAllMvcRouteValues = diagnosticParameterNameForAllMvcRouteValues;
        }

        /// <summary>Effect: This is the controller name by which Asp.Net Routing can find a <see cref="ComponentAsServiceController"/>
        /// Only change this if you want to subclass or replace <see cref="ComponentAsServiceController"/></summary>
        public string ComponentAsServiceControllerName { get; }
        
        /// <summary>Effect: This is the action name by which Asp.Net Routing can find <see cref="ComponentAsServiceController.Serve"/>
        /// Only change this if you want to subclass or replace <see cref="ComponentAsServiceController"/></summary>
        public string ComponentAsServiceControllerActionName { get; }

        /// <summary>Effect: This is the identifier used in ComponentAsService's route template to identify the service to invoke
        /// It defaults to "service" so that the default route template is "{service}/{method}"
        /// </summary>
        public string RouteValueServiceNameKey { get; }
        
        /// <summary>Effect: This is the identifier used in ComponentAsService's route template to identify the method to invoke on the service being used
        /// It defaults to "method" so that the default route template is "{service}/{method}"
        /// </summary>
        public string RouteValueMethodNameKey { get; }

        /// <summary>Effect: if you invoke a method which has a single parameter of this name and of type Dictionary&lt;string,object&gt;, then ComponentAsService will populated it
        /// with all Values in <see cref="ActionContext.RouteData"/>.Values</summary>
        public string DiagnosticParameterNameForAllMvcRouteValues { get; }

        public string[] RouteValuesKeysIgnoredForParameterMatching => new[] {"controller", "action", RouteValueMethodNameKey, RouteValueServiceNameKey};
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace ComponentAsService
{
    public static class ComponentAsServiceExtensions
    {
        /// <summary>See C# language reference: valid characters for the first character of an identitier</summary>
        public const string RegexIdentifierStartCharacter = @"(_|\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl})";
        /// <summary>See C# language reference: valid characters for after the first character of an identitier</summary>
        public const string RegexIdentifierPartCharacters = @"(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}|\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc})*";
        /// <summary>See C# language reference: regex for a valid C# identifier</summary>
        public const string RegexValidCSharpIdentifier = "^" + RegexIdentifierStartCharacter + RegexIdentifierPartCharacters + "$";
        
        /// <summary>
        /// The default route template matched by microServeIt is {service}/{method}
        /// </summary>
        public const string RouteTemplateServiceSlashMethod = @"{service}/{method}";
        
        /// <summary>Used to hint to client IDEs that the default route uses a regex to constraint names to valid C# identifiers</summary>
        const string PseudoTemplateServiceSlashMethod = @"{service:regex(CSharpIdentifier)}/{method:regex(CSharpIdentifier)}";

        public static string MvcRoutingRequiredMessage { get; } = "ComponentAsService requires the MvcRouteHandler. Are references to Microsoft.AspNetCore.* missing?";

        public static string AddComponentAsServiceRequiredMessage { get; } = "Before you can UseComponentAsService() you must previous AddComponentAsService(configuration). See e.g. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-2.1";

        /// <summary>
        /// Adds
        /// <list type="bullet">
        ///     <item>Mvc (<see cref="MvcServiceCollectionExtensions.AddMvc(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>)</item>
        ///     <item><see cref="IComponentAsServiceDiagnostics"/></item>
        ///     <item><paramref name="services"/></item>
        /// </list>
        /// to the <paramref name="services"/> serviceCollection.
        /// To expose a Component as a Service, also add it to <paramref name="services"/> in the usual way, for instance:
        /// <code>services.AddScoped&lt;IMyService, MyImplementingComponent&gt;()</code> 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="componentAsServiceConfiguration"></param>
        /// <returns></returns>
        public static IServiceCollection AddComponentAsService(this IServiceCollection services,ComponentAsServiceConfiguration componentAsServiceConfiguration=null)
        {
            services.AddMvc();
            services.AddSingleton(services);
            services.AddScoped<IComponentAsServiceDiagnostics, ComponentAsServiceDiagnostics>();
            services.AddSingleton(componentAsServiceConfiguration??ComponentAsServiceConfiguration.DefaultValues);
            return services;
        }

        ///  <summary>
        ///  Adds a Route mapping to <paramref name="app"/> for <see cref="ComponentAsServiceController.<see cref="ComponentAsServiceController.Serve"/> action, 
        ///  so that ServeIt can handle incoming requests.
        ///  </summary>
        /// <param name="app">the application Builder you are building</param>
        /// <param name="routeTemplate">The route template which ServeIt will match.</param>
        /// <param name="defaults">The defaults for the route used by ServeIt.
        ///     By default, this is <code>new {controller="ServeIt", action="Serve"}</code>. If you override this, you must still include the 
        ///     defaults for <code>controller="ServeIt", action="Serve"</code>, otherwise your route will not be served by ServeIt
        /// </param>
        /// <param name="constraints">The constraints for the route used by ServeIt.
        ///     By default this is:
        ///     <code>new {
        ///      service= new RegexRouteConstraint(RegexValidCSharpIdentifier),
        ///      method= new RegexRouteConstraint(RegexValidCSharpIdentifier),
        ///  }</code>
        /// </param>
        /// <param name="routeName">The name for ServeIt's route.</param>
        /// <returns><paramref name="app"/></returns>
        ///  <exception cref="InvalidOperationException">
        ///  Thrown if you have not configured <paramref name="app"/>'s ServiceProvider with an MvcRouteHandler.
        ///  Avoid this by configuring <paramref name="app"/>.AddMvc() in the usual way. 
        ///  </exception>
        ///  <exception cref="ArgumentException">
        ///  Thrown if you override <paramref name="defaults"/> to not use controller=ServeIt, action=Serve
        ///  </exception>
        public static IApplicationBuilder UseComponentAsService(this IApplicationBuilder app,
            string routeTemplate = RouteTemplateServiceSlashMethod,
            object defaults = null,
            object constraints = null,
            string routeName = "ComponentAsServiceRoute")
        {
            var config = EnsureConfigurationElseThrow(app);

            if (routeTemplate == PseudoTemplateServiceSlashMethod) routeTemplate = RouteTemplateServiceSlashMethod;

            defaults = defaults ?? 
                       new
                       {
                           controller = config.ComponentAsServiceControllerName, 
                           action     = config.ComponentAsServiceControllerActionName, 
                       };
            constraints= constraints ?? 
                         new RouteValueDictionary
                         {
                             {config.RouteValueServiceNameKey, new RegexRouteConstraint(RegexValidCSharpIdentifier)},
                             {config.RouteValueMethodNameKey,  new RegexRouteConstraint(RegexValidCSharpIdentifier)},
                         };
            EnsureTemplateAndDefaultsRouteToComponentAsServiceElseThrow(defaults, config, routeTemplate);
            
            var mvcRouteHandler = EnsureMvcRouteHandlerElseThrow(app);
            app.UseRouter(routeBuilder =>
                          {
                              routeBuilder.DefaultHandler =mvcRouteHandler;
                              routeBuilder.MapRoute(routeName, routeTemplate, defaults, constraints);
                          });
            return app;
        }

        static ComponentAsServiceConfiguration EnsureConfigurationElseThrow(IApplicationBuilder app)
        {
            try
            {
                return app.ApplicationServices.GetRequiredService<ComponentAsServiceConfiguration>();
            }
            catch (Exception e){ throw new InvalidOperationException(AddComponentAsServiceRequiredMessage,e); }
        }

        static MvcRouteHandler EnsureMvcRouteHandlerElseThrow(IApplicationBuilder app)
        {
            try
            {
                return app.ApplicationServices.GetRequiredService<MvcRouteHandler>();
            }
            catch (Exception e){ throw new InvalidOperationException(MvcRoutingRequiredMessage, e); }
        }

        static void EnsureTemplateAndDefaultsRouteToComponentAsServiceElseThrow(object defaults, ComponentAsServiceConfiguration configuration, string routeTemplate)
        {
            if (!Regex.IsMatch(routeTemplate, "{"+configuration.RouteValueServiceNameKey+"[=:}]"))
            {
                throw new ArgumentException("If you specify ComponentAsServiceConfiguration.RouteValueServiceNameKey, " +
                                            "you most use that value in the routeTemplate, otherwise" +
                                            "no route can ever match it. RouteValueServiceNameKey= {"+
                                            configuration.RouteValueServiceNameKey + "} does not " +
                                            "appear in routeTemplate " + routeTemplate);
            }
            if (!Regex.IsMatch(routeTemplate, "{"+configuration.RouteValueMethodNameKey+"[=:}]"))
            {
                throw new ArgumentException("If you specify ComponentAsServiceConfiguration.RouteValueMethodNameKey, " +
                                            "you most use that value in the routeTemplate, otherwise" +
                                            "no route can ever match it. RouteValueMethodNameKey= {"+
                                            configuration.RouteValueMethodNameKey + "} does not " +
                                            "appear in routeTemplate " + routeTemplate);
            }
            var dict = ObjectToDictionary(defaults);
            if(   !dict.ContainsKey("controller") || dict["controller"] as string!=configuration.ComponentAsServiceControllerName
               || !dict.ContainsKey("action")     || dict["action"]     as string!=configuration.ComponentAsServiceControllerActionName)
            {
                if (configuration != ComponentAsServiceConfiguration.DefaultValues)
                {
                    throw new ArgumentException(
                                                "If you override the defaults for the ComponentAsService route, and also specify "
                                              + "the componentAsServiceConfiguration parameter, you must still include "
                                              + "{controller=\"componentAsServiceConfiguration.ServeItControllerName\", "
                                              + "{action=\"componentAsServiceConfiguration.ServeItActionName\"} in your defaults.",
                                                nameof(defaults));
                }
                else
                {
                    throw new ArgumentException(
                                                "If you override the defaults for the ComponentAsService route, you must still include "
                                              + "{controller=\"" + configuration.ComponentAsServiceControllerName + "\", "
                                              + "{action=\""     + configuration.ComponentAsServiceControllerActionName     + "\"} in your defaults.",
                                                nameof(defaults));
                }
            }   
        }

        static IDictionary<string, object> ObjectToDictionary(object value)
        {
            return value as IDictionary<string, object> ?? new RouteValueDictionary(value);
        }
        
    }
}
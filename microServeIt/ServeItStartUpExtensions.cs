using System;
using System.Collections.Generic;
using microServeIt.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace microServeIt
{
    public static class ServeItStartUpExtensions
    {
        public static IServiceCollection AddServeIt(this IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton(services);
            services.AddScoped<IServeItDiagnostics, ServeItDiagnostics>();
            return services;
        }
        
        ///  <summary>
        ///  Adds a Route mapping to <paramref name="app"/> for <see cref="ServeItController.<see cref="ServeItController.Serve"/> action, 
        ///  so that ServeIt can handle incoming requests.
        ///  </summary>
        ///  <param name="app">the application Builder you are building</param>
        ///  <param name="routeName">The name for ServeIt's route.</param>
        ///  <param name="routeTemplate">The route template which ServeIt will match.</param>
        ///  <param name="defaults">The defaults for the route used by ServeIt.
        ///  By default, this is <code>new {controller="ServeIt", action="Serve"}</code>. If you override this, you must still include the 
        ///  defaults for <code>controller="ServeIt", action="Serve"</code>, otherwise your route will not be served by ServeIt
        ///  </param>
        ///  <param name="constraints">The constraints for the route used by ServeIt.
        /// By default this is:
        ///  <code>new {
        ///      service= new RegexRouteConstraint(RegexValidCSharpIdentifier),
        ///      method= new RegexRouteConstraint(RegexValidCSharpIdentifier),
        ///  }</code>
        ///  </param>
        /// <param name="specialNames">Pass in a customised <see cref="SpecialNames"/> instance if you need to change the
        /// controller or action name, or the service or method constraint names, or the of a special parameter.
        /// The default value is <see cref="SpecialNames.DefaultValues"/></param>
        /// <returns><paramref name="app"/></returns>
        ///  <exception cref="InvalidOperationException">
        ///  Thrown if you have not configured <paramref name="app"/>'s ServiceProvider with an MvcRouteHandler.
        ///  Avoid this by configuring <paramref name="app"/>.AddMvc() in the usual way. 
        ///  </exception>
        ///  <exception cref="ArgumentException">
        ///  Thrown if you override <paramref name="defaults"/> to not use controller=ServeIt, action=Serve
        ///  </exception>
        public static IApplicationBuilder UseServeIt(this IApplicationBuilder app,
                                                    string routeName   = "ServeItRoute",
                                                    string routeTemplate    = PseudoTemplateServiceSlashMethod,
                                                    object defaults    = null,
                                                    object constraints = null,
                                                    SpecialNames specialNames=null)
        {
            specialNames = specialNames ?? SpecialNames.DefaultValues;
            
            if (routeTemplate == PseudoTemplateServiceSlashMethod) routeTemplate = RouteTemplateServiceSlashMethod;
            routeTemplate = routeTemplate ?? RouteTemplateServiceSlashMethod;
            
            defaults = defaults ?? 
                       new
                       {
                           controller = specialNames.ServeItControllerName, 
                           action     = specialNames.ServeItActionName, 
                       };
            
            constraints= constraints ?? 
                 new RouteValueDictionary
                 {
                     {specialNames.RouteValueServiceName, new RegexRouteConstraint(RegexValidCSharpIdentifier)},
                     {specialNames.RouteValueMethodName,  new RegexRouteConstraint(RegexValidCSharpIdentifier)},
                 };
            
            var mvcRouteHandler = EnsureMvcRouteHandlerElseThrow(app);
            EnsureControllerServeItAndActionServeElseThrow(defaults, specialNames);

            app.UseRouter(routeBuilder =>
                          {
                              routeBuilder.DefaultHandler =mvcRouteHandler;
                              routeBuilder.MapRoute(routeName, routeTemplate, defaults, constraints);
                          });
            return app;
        }
        
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

        public static string MvcRoutingRequiredMessage { get; } = "microServeIt requires the MvcRouteHandler. Use services.AddMvc() in the usual way to your ConfigureServices() method.";

        static MvcRouteHandler EnsureMvcRouteHandlerElseThrow(IApplicationBuilder app)
        {
            try
            {
                return app.ApplicationServices.GetRequiredService<MvcRouteHandler>();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(MvcRoutingRequiredMessage, e);
            }
        }

        static void EnsureControllerServeItAndActionServeElseThrow(object defaults, SpecialNames specialNames)
        {
            var dict = ObjectToDictionary(defaults);
            if(   !dict.ContainsKey("controller") || dict["controller"] as string!=specialNames.ServeItControllerName
               || !dict.ContainsKey("action")     || dict["action"]     as string!=specialNames.ServeItActionName)
            {
                if (specialNames != SpecialNames.DefaultValues)
                {
                    throw new ArgumentException(
                                                "If you override the defaults for the ServeIt route, and also specify "
                                              + "the specialNames parameter, you must still include "
                                              + "{controller=\"specialNames.ServeItControllerName\", "
                                              + "{action=\"specialNames.ServeItActionName\"} in your defaults.",
                                                nameof(defaults));
                }
                else
                {
                    throw new ArgumentException(
                                                "If you override the defaults for the ServeIt route, you must still include "
                                              + "{controller=\"" + specialNames.ServeItControllerName + "\", "
                                              + "{action=\""     + specialNames.ServeItActionName     + "\"} in your defaults.",
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
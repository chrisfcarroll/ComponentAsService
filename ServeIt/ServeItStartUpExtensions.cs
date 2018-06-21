using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ServeIt
{
    public static class ServeItStartUpExtensions
    {
        public static readonly string RegexIdentifierStartCharacter = @"(_|\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl})"
                                                                .Replace("{","{{").Replace("}","}}");
        public static readonly string RegexIdentifierPartCharacters =
            @"(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}|\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc})+"
               .Replace("{", "{{")
               .Replace("}", "}}");
        //public static readonly string RegexIdentifier = "^" + RegexIdentifierStartCharacter + RegexIdentifierPartCharacters + "$";

        public const string RegexIdentifier = "^_?[[A-Za-z0-9]]+$";
        
        public static readonly string RouteTemplateServiceSlashMethod = @"{component:regex(" + RegexIdentifier + ")}/{method:regex(" + RegexIdentifier + ")}";
        
        const string PseudoTemplateServiceSlashMethod = @"{component:regex(CSharpIdentifier)}/{method:regex(CSharpIdentifier)}";

        public static IRouteBuilder MapServeItWithDefaultComponentMethodRoute(this IRouteBuilder routeBuilder)
        {
            return routeBuilder.UseServeItWithRoute();
        }

        public static IRouteBuilder UseServeItWithRoute(this IRouteBuilder routeBuilder, 
                                                    string routeName   = "ServeItRoute",
                                                    string template    = PseudoTemplateServiceSlashMethod,
                                                    object defaults    = null,
                                                    object constraints = null)
        {
            if (template == PseudoTemplateServiceSlashMethod) template = RouteTemplateServiceSlashMethod;
            template = template ?? RouteTemplateServiceSlashMethod;
            defaults = defaults ?? new {controller="ServeIt", action="Serve"};

            EnsureControllerServeItAndActionServeElseThrow(defaults);
            
            return routeBuilder.MapRoute(routeName, template, defaults, constraints);
        }

        static void EnsureControllerServeItAndActionServeElseThrow(object defaults)
        {
            var dict = ObjectToDictionary(defaults);
            if(   !dict.ContainsKey("controller") || dict["controller"]!="ServeIt"
               || !dict.ContainsKey("action") || dict["action"] !="Serve")
            {
                throw new ArgumentException(
                    "If you override the defaults for the ServeIt route, you must still include {controller=\"ServeIt\", action=\"Serve\"} in your defaults",
                    nameof(defaults));
            }   
        }

        public static IDictionary<string, object> ObjectToDictionary(object value)
        {
            return value as IDictionary<string, object> ?? new RouteValueDictionary(value);
        }
        
    }
}
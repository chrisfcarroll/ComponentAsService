using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Component.As.Service.UseComponentAsService
{
    public static class FinerGrainedActionSelectorExtensions
    {
        internal static readonly Type ActionSelectorType = typeof(FinerGrainedActionSelector);

        /// <summary>Add the <see cref="FinerGrainedActionSelector"/> so that components can be served as controllers</summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="controllerTypesToAdd"></param>
        /// <returns><paramref name="mvcBuilder"/></returns>
        public static IMvcBuilder AddFinerGrainedActionSelector(this IMvcBuilder mvcBuilder, params TypeInfo[] controllerTypesToAdd)
        {
            mvcBuilder.Services.Replace(
                new ServiceDescriptor(typeof(IActionSelector), 
                    ActionSelectorType,ServiceLifetime.Singleton));
            return mvcBuilder;
        }        
        /// <summary>Add the <see cref="FinerGrainedActionSelector"/> so that components can be served as controllers</summary>
        /// <param name="services"></param>
        /// <param name="controllerTypesToAdd"></param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddFinerGrainedActionSelector(this IServiceCollection services, params TypeInfo[] controllerTypesToAdd)
        {
            return services.Replace(
                new ServiceDescriptor(typeof(IActionSelector), 
                    ActionSelectorType,ServiceLifetime.Transient /*Could try scoped, but either way it relies on an HttpContext*/  ));
        }        
    }
}
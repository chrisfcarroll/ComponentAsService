using System;
using System.Reflection;
using Component.As.Service.Pieces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Component.As.Service
{
    /// <summary>
    /// Extensions to <see cref="IMvcBuilder"/> and <see cref="IServiceCollection"/> which replace the
    /// default <see cref="ActionSelector"/> with a <see cref="FinerGrainedActionSelector"/>.
    /// </summary>
    public static class FinerGrainedActionSelectorExtensions
    {
        /// <summary>Add the <see cref="FinerGrainedActionSelector"/> so that components can be served as controllers</summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="controllerTypesToAdd"></param>
        /// <returns><paramref name="mvcBuilder"/></returns>
        public static IMvcBuilder AddFinerGrainedActionSelector(this IMvcBuilder mvcBuilder, params TypeInfo[] controllerTypesToAdd)
        {
            mvcBuilder.Services.Replace(
                new ServiceDescriptor(typeof(IActionSelector), 
                    typeof(FinerGrainedActionSelector),ServiceLifetime.Singleton));
            return mvcBuilder;
        }
        
        /// <summary>Add the <see cref="FinerGrainedActionSelector"/> so that components can be served as controllers</summary>
        /// <param name="services"></param>
        /// <param name="controllerTypesToAdd"></param>
        /// <returns><paramref name="services"/></returns>
        public static IServiceCollection AddFinerGrainedActionSelector(this IServiceCollection services, params TypeInfo[] controllerTypesToAdd)
        {
            return services.Replace(
                new ServiceDescriptor(
                        typeof(IActionSelector), 
                        typeof(FinerGrainedActionSelector),
                        ServiceLifetime.Singleton 
                        ));
        }        
    }
}
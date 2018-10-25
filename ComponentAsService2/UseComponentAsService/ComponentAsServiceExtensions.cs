using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ComponentAsService2.UseComponentAsService
{
    public static class ComponentAsServiceExtensions
    {
        /// <summary>Add the <see cref="AnythingCanBeControllersFeatureProvider"/> so that components can be served as controllers</summary>
        /// <param name="services"></param>
        /// <param name="componentTypesToServe"></param>
        /// <returns><paramref name="services"/></returns>
        public static IMvcBuilder AddComponentAsService(this IServiceCollection services, params TypeInfo[] componentTypesToServe)
        {
            var mvcBuilder = services.AddMvc();
            mvcBuilder.AddAnythingCanBeAController(componentTypesToServe);
            mvcBuilder.AddComponentAsService(typeof(ComponentAsServiceDiagnostics).GetTypeInfo());
            services.AddFinerGrainedActionSelector();
            return mvcBuilder;
        } 

        /// <summary>Add the <see cref="AnythingCanBeControllersFeatureProvider"/> so that components can be served as controllers</summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="componentTypesToServe"></param>
        /// <returns><paramref name="mvcBuilder"/></returns>
        public static IMvcBuilder AddComponentAsService(this IMvcBuilder mvcBuilder, params TypeInfo[] componentTypesToServe)
        {
            mvcBuilder.Services.AddFinerGrainedActionSelector();            
            return mvcBuilder.AddAnythingCanBeAController(componentTypesToServe);
        }

        /// <summary>Enable types <paramref name="componentTypesToServe"/> to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <param name="componentTypesToServe"></param>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService(this IApplicationBuilder app, params TypeInfo[] componentTypesToServe)
                => UseAsAController(app, componentTypesToServe);

        /// <summary>Enable type <typeparamref name="TComponent"/> to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <typeparam name="TComponent">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent>(this IApplicationBuilder app)
                => UseAsAController(app, typeof(TComponent).GetTypeInfo());

        /// <summary>Enable types <typeparamref name="TComponent1"/> ... <typeparamref name="TComponent2"/>  to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <typeparam name="TComponent1">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent2">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent1,TComponent2>(this IApplicationBuilder app)
            => UseAsAController(app, typeof(TComponent1).GetTypeInfo(),typeof(TComponent2).GetTypeInfo());

        /// <summary>Enable types <typeparamref name="TComponent1"/> ... <typeparamref name="TComponent3"/>  to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <typeparam name="TComponent1">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent2">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent3">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent1,TComponent2, TComponent3>(this IApplicationBuilder app)
            => UseAsAController(app, typeof(TComponent1).GetTypeInfo(),typeof(TComponent2).GetTypeInfo(),typeof(TComponent3).GetTypeInfo());

       
        /// <summary>Enable types <paramref name="moreControllers"/> to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <param name="moreControllers"></param>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder UseAsAController(this IApplicationBuilder app, params TypeInfo[] moreControllers)
        {
            app.ApplicationServices
               .GetService<AnythingCanBeControllersFeatureProvider>()
               .Add(moreControllers);
            return app;
        }
    }
}
using System;
using System.Reflection;
using Component.As.Service.Pieces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Component.As.Service
{
    /// <summary>
    /// Extensions to <see cref="IServiceCollection"/> and to <see cref="IMvcBuilder"/>
    /// to setup <see cref="Component.As.Service"/> features.
    /// </summary>
    public static class ComponentAsServiceExtensions
    {
        /// <summary>Add the <see cref="AnythingCanBeAControllerFeatureProvider"/> so that components can be served as controllers</summary>
        /// <param name="services"></param>
        /// <param name="componentTypesToServe"></param>
        /// <returns><paramref name="services"/></returns>
        public static IMvcBuilder AddComponentAsService(this IServiceCollection services, params TypeInfo[] componentTypesToServe)
        {
            var mvcBuilder = services.AddMvc();
            AddComponentAsService(mvcBuilder);
            return mvcBuilder;
        } 

        /// <summary>Add the <see cref="AnythingCanBeAControllerFeatureProvider"/> so that components can be served as controllers</summary>
        /// <param name="mvcBuilder"></param>
        /// <param name="componentTypesToServe"></param>
        /// <returns><paramref name="mvcBuilder"/></returns>
        public static IMvcBuilder AddComponentAsService(this IMvcBuilder mvcBuilder, params TypeInfo[] componentTypesToServe)
        {
            mvcBuilder.AddApplicationPart(typeof(ComponentAsServiceExtensions).Assembly);
            mvcBuilder.AddAnythingCanBeAController(componentTypesToServe);
            mvcBuilder.Services.AddFinerGrainedActionSelector();            
            return mvcBuilder;
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
        /// <param name="routeMapping">Optional: override the <see cref="DefaultComponentAsServiceRouteMapping"/></param>
        /// <typeparam name="TComponent">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent>(this IApplicationBuilder app, Action<IRouteBuilder> routeMapping=null)
                => UseAsAController(app, typeof(TComponent).GetTypeInfo());

        /// <summary>Enable types <typeparamref name="TComponent1"/> ... <typeparamref name="TComponent2"/>  to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <param name="routeMapping">Optional: override the <see cref="DefaultComponentAsServiceRouteMapping"/></param>
        /// <typeparam name="TComponent1">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent2">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent1,TComponent2>(this IApplicationBuilder app, Action<IRouteBuilder> routeMapping=null)
            => UseAsAController(app, typeof(TComponent1).GetTypeInfo(),typeof(TComponent2).GetTypeInfo());

        /// <summary>Enable types <typeparamref name="TComponent1"/> ... <typeparamref name="TComponent3"/>  to be served as Controllers.</summary>
        /// <param name="app"></param>
        /// <param name="routeMapping">Optional: override the <see cref="DefaultComponentAsServiceRouteMapping"/></param>
        /// <typeparam name="TComponent1">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent2">A component Type to be served by the web application</typeparam>
        /// <typeparam name="TComponent3">A component Type to be served by the web application</typeparam>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder 
            UseComponentAsService<TComponent1,TComponent2, TComponent3>(this IApplicationBuilder app, Action<IRouteBuilder> routeMapping=null)
            => UseAsAController(app, routeMapping, typeof(TComponent1).GetTypeInfo(),typeof(TComponent2).GetTypeInfo(),typeof(TComponent3).GetTypeInfo());

       
        /// <summary>Enable types <paramref name="moreControllers"/> to be served as Controllers.</summary> using the
        /// default Route <c>{ name="ComponentAsService" template= "{controller}/{action}" }</c>
        /// <param name="app"></param>
        /// <param name="moreControllers"></param>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder UseAsAController(this IApplicationBuilder app, params TypeInfo[] moreControllers)
        {
            return app.UseAsAController(DefaultComponentAsServiceRouteMapping, moreControllers);
        }

        /// <summary>Enable types <paramref name="moreControllers"/> to be served as Controllers at the
        /// route specified by <paramref name="routeMapping"/></summary>
        /// <param name="app"></param>
        /// <param name="routeMapping">If this is <c>null</c> then <see cref="DefaultComponentAsServiceRouteMapping"/> will be used.</param>
        /// <param name="moreControllers"></param>
        /// <returns><paramref name="app"/></returns>
        public static IApplicationBuilder UseAsAController(this IApplicationBuilder app, Action<IRouteBuilder> routeMapping,  params TypeInfo[] moreControllers)
        {
            app.UseMvc(routeMapping);
            var acbCFP=app.ApplicationServices.GetService<AnythingCanBeAControllerFeatureProvider>();
            acbCFP.Add(moreControllers);
            acbCFP.Add(typeof(ComponentAsServiceDiagnostics).GetTypeInfo());
            return app;
        }

        /// <summary>
        /// The default route mapping is <c>routes => routes.MapRoute(name: "ComponentAsService",template: "{controller}/{action}")</c>
        /// </summary>
        public static readonly Action<IRouteBuilder> DefaultComponentAsServiceRouteMapping = 
            routes => routes.MapRoute(name: "ComponentAsService",template: "{controller}/{action}");
   }
}
RouteContext requires

Inject HttpContext

Populate RouteData

MvcAttributeRouteHandler.RouteAsync populates RouteData.Values
actionDescriptor.RouteValues

MvcRouteHandler.RouteAsync doesnt

Microsoft.AspNetCore.Builder.RouterMiddleWare.Invoke(HttpContext)

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  public async Task Invoke(HttpContext httpContext)
  {
    RouteContext context = new RouteContext(httpContext);
    context.RouteData.Routers.Add(this._router);
    await this._router.RouteAsync(context);
    if (context.Handler == null)
    {
      this._logger.RequestDidNotMatchRoutes();
      await this._next(httpContext);
    }
    else
    {
      httpContext.Features[typeof (IRoutingFeature)] = (object) new RoutingFeature()
      {
        RouteData = context.RouteData
      };
      await context.Handler(context.HttpContext);
    }
  }
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Mvc(A)RouteHandler uses a IActionInvokerFactory (and a IActionContextAccessor
which can be null)

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  context.Handler = (HttpContext c) =>
  {
      var routeData = c.GetRouteData();
        // routeData is often just 2 keys: action, controller

      var actionContext = new ActionContext(context.HttpContext, routeData, actionDescriptor);
        // Action descriptor is already selected by calling _actionSelector.SelectBestCandidate
        // inside the same RouteAsync() method, a few lines above,
        // before the handler is called.

      if (_actionContextAccessor != null)
      {
          _actionContextAccessor.ActionContext = actionContext;
      }

      var invoker = _actionInvokerFactory.CreateInvoker(actionContext);
      if (invoker == null)
      {
          throw new InvalidOperationException(
              Resources.FormatActionInvokerFactory_CouldNotCreateInvoker(
                  actionDescriptor.DisplayName));
      }

      return invoker.InvokeAsync();
  };
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

`var invoker = _actionInvokerFactory.CreateInvoker(actionContext);` :

ActionInvokerFactory

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public class ActionInvokerFactory : IActionInvokerFactory
    {
        private readonly IActionInvokerProvider[] _actionInvokerProviders;

        public ActionInvokerFactory(IEnumerable<IActionInvokerProvider> actionInvokerProviders)
        {
            _actionInvokerProviders = actionInvokerProviders.OrderBy(item => item.Order).ToArray();
        }

        public IActionInvoker CreateInvoker(ActionContext actionContext)
        {
            var context = new ActionInvokerProviderContext(actionContext);

            foreach (var provider in _actionInvokerProviders)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = _actionInvokerProviders.Length - 1; i >= 0; i--)
            {
                _actionInvokerProviders[i].OnProvidersExecuted(context);
            }

            return context.Result;
        }
    }
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

DI provides IEnumerable which includes the

ControllerActionInvokerProvider

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
DI provides 
    IOptions<MvcOptions> optionsAccessor
        which provides the _valueProviderFactories (4 of them : Form,Route,QueryString,JQueryForm)
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
public void OnProvidersExecuting(ActionInvokerProviderContext context /* wraps the actionContext */)
{
    if (context == null)
    {
        throw new ArgumentNullException(nameof(context));
    }

    if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor)
    {
        var controllerContext = new ControllerContext(context.ActionContext);
        // PERF: These are rarely going to be changed, so let's go copy-on-write.
        controllerContext.ValueProviderFactories = new CopyOnWriteList<IValueProviderFactory>(_valueProviderFactories);
        controllerContext.ModelState.MaxAllowedErrors = _maxModelValidationErrors;

        var cacheResult = _controllerActionInvokerCache.GetCachedResult(controllerContext);

        var invoker = new ControllerActionInvoker(
            _logger,
            _diagnosticSource,
            _mapper,
            controllerContext,
            cacheResult.cacheEntry,
            cacheResult.filters);

        context.Result = invoker;
    }
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

ControllerActionInvokerCache

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
DI provides 
          IActionDescriptorCollectionProvider collectionProvider,
        ParameterBinder parameterBinder,
        IModelBinderFactory modelBinderFactory,
        IModelMetadataProvider modelMetadataProvider,
        IEnumerable<IFilterProvider> filterProviders,
        IControllerFactoryProvider factoryProvider,
        IOptions<MvcOptions> mvcOptions
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

GetCachedResult(ControllerContext )

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
  var parameterDefaultValues = ParameterDefaultValues
      .GetParameterDefaultValues(actionDescriptor.MethodInfo);

  var objectMethodExecutor = ObjectMethodExecutor.Create(
      actionDescriptor.MethodInfo,
      actionDescriptor.ControllerTypeInfo,
      parameterDefaultValues);

  var controllerFactory = _controllerFactoryProvider.CreateControllerFactory(actionDescriptor);
  var controllerReleaser = _controllerFactoryProvider.CreateControllerReleaser(actionDescriptor);
  var propertyBinderFactory = ControllerBinderDelegateProvider.CreateBinderDelegate(
      _parameterBinder,
      _modelBinderFactory,
      _modelMetadataProvider,
      actionDescriptor,
      _mvcOptions);

  var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

  cacheEntry = new ControllerActionInvokerCacheEntry(
      filterFactoryResult.CacheableFilters,
      controllerFactory,
      controllerReleaser,
      propertyBinderFactory,
      objectMethodExecutor,
      actionMethodExecutor);
  cacheEntry = cache.Entries.GetOrAdd(actionDescriptor, cacheEntry);
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
ControllerBinderDelegateProvider.CreateBinderDelegate is public static.
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

ControllerBinderDelegate CreateBinderDelegate( ParameterBinder parameterBinder,
\<---- Came from DI IModelBinderFactory modelBinderFactory, \<---- Came from DI
IModelMetadataProvider modelMetadataProvider, \<---- Came from DI
ControllerActionDescriptor actionDescriptor, \^-- Came from the
ControllerContext from ActionInvokerProviderContext from ActionContext from the
ActionDescriptoer selected by calling actionSelector.SelectBestCandidate()
during RouteHandlerRouteAsync() before creating the Handler MvcOptions
mvcOptions \<---- Came from DI )

calls

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
private static BinderItem[] GetParameterBindingInfo(
            IModelBinderFactory modelBinderFactory, <----  Came from DI
            IModelMetadataProvider modelMetadataProvider, <----  Came from DI
            ControllerActionDescriptor actionDescriptor, 
                    ^-- Came from calling actionSelector.SelectBestCandidate()
            MvcOptions mvcOptions  <----  Came from DI
            )
    )
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

ActionContext= { HttpContext \<--From RC RouteData \<-- from RC ActionDescriptor
\<-- given ModelState= new }

cc = new ControllerContext(ActionContext){ ValueProviderFactories = new
CopyOnWriteList( MvcOptions.ValueProviderFactories.ToArray());
ModelState.MaxAllowedErrors= MvcOptions.MaxModelValidationErrors

}

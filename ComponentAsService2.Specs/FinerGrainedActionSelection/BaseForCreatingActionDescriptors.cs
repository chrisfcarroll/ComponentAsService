using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure;
using ComponentAsService2.UseComponentAsService;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using TestBase;
using Assert = Xunit.Assert;

namespace ComponentAsService2.Specs.FinerGrainedActionSelection
{
    public class BaseForCreatingActionDescriptors
    {
        public static FinerGrainedActionSelector CreateFinerGrainedActionSelector(IReadOnlyList<ActionDescriptor> actions, ILoggerFactory loggerFactory = null)
        {
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            var actionProvider = CreateMockIActionDescriptorCollectionProvider(actions);

            var actionConstraintProviders = new IActionConstraintProvider[] {
                    new DefaultActionConstraintProvider(),
                    new BooleanConstraintProvider(),
                };

            return new FinerGrainedActionSelector(
                actionProvider.Object,
                GetActionConstraintCache(actionConstraintProviders),
                ModelingBindingParameterBinderTestBase.CreateParameterBinder(),
                new ModelBinderFactory(
                    TestModelMetadataProvider.CreateDefaultProvider(),
                    ModelingBindingParameterBinderTestBase.MvcOptionsWrapper,
                    GetServiceProvider(loggerFactory)
                ),
                TestModelMetadataProvider.CreateDefaultProvider(),
                ModelingBindingParameterBinderTestBase.MvcOptionsWrapper,
                loggerFactory);
        }

        public static FinerGrainedActionSelector CreateFinerGrainedActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, 
            IActionConstraintProvider[] actionConstraintProviders, 
            ILoggerFactory loggerFactory=null)
        {
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            return new FinerGrainedActionSelector(
                actionDescriptorCollectionProvider,
                GetActionConstraintCache(actionConstraintProviders),
                ModelingBindingParameterBinderTestBase.CreateParameterBinder(),
                new ModelBinderFactory(
                    TestModelMetadataProvider.CreateDefaultProvider(),
                    ModelingBindingParameterBinderTestBase.MvcOptionsWrapper,
                    GetServiceProvider(loggerFactory)
                ),
                TestModelMetadataProvider.CreateDefaultProvider(),
                ModelingBindingParameterBinderTestBase.MvcOptionsWrapper,
                loggerFactory);
        }

        public ControllerActionDescriptor InvokeActionSelector(RouteContext context)
        {
            var actionDescriptorProvider = GetActionDescriptorProvider();
            var actionDescriptorCollectionProvider = new DefaultActionDescriptorCollectionProvider(
                new[] { actionDescriptorProvider },
                Enumerable.Empty<IActionDescriptorChangeProvider>());

            var actionConstraintProviders = new[]
            {
                new DefaultActionConstraintProvider()
            };

            var actionSelector = CreateFinerGrainedActionSelector(actionDescriptorCollectionProvider, actionConstraintProviders);

            var candidates = actionSelector.SelectCandidates(context);
            return (ControllerActionDescriptor)actionSelector.SelectBestCandidate(context, candidates);
        }

        static IServiceProvider GetServiceProvider(ILoggerFactory loggerFactory=null, params object[] instances)
        {
            var services = new ServiceCollection();
            services.AddSingleton(loggerFactory??NullLoggerFactory.Instance);
            return services.BuildServiceProvider();
        }

        static Mock<IActionDescriptorCollectionProvider> CreateMockIActionDescriptorCollectionProvider(IReadOnlyList<ActionDescriptor> actions)
        {
            var actionProvider = new Mock<IActionDescriptorCollectionProvider>(MockBehavior.Strict);
            actionProvider
                .Setup(p => p.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(actions, 0));
            return actionProvider;
        }


        public ControllerActionDescriptorProvider GetActionDescriptorProvider()
        {
            var controllerTypes = typeof(ActionSelectorTest)
                .GetNestedTypes(BindingFlags.NonPublic)
                .Select(t => t.GetTypeInfo())
                .ToList();

            var options = Options.Create(new MvcOptions());

            var manager = GetApplicationManager(controllerTypes);

            var modelProvider = new DefaultApplicationModelProvider(options, new EmptyModelMetadataProvider());

            var provider = new ControllerActionDescriptorProvider(
                manager,
                new[] { modelProvider },
                options);

            return provider;
        }

        public static ApplicationPartManager GetApplicationManager(List<TypeInfo> controllerTypes)
        {
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new TestApplicationPart(controllerTypes));
            manager.FeatureProviders.Add(new TestFeatureProvider());
            return manager;
        }

        public static HttpContext GetHttpContext(string httpMethod)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Method = httpMethod;
            return httpContext;
        }

        public static ActionDescriptor[] GetActions()
        {
            return new[]
            {
                // Like a typical RPC controller
                CreateAction(area: null, controller: "Home", action: "Index"),
                CreateAction(area: null, controller: "Home", action: "Edit"),

                // Like a typical REST controller
                CreateAction(area: null, controller: "Product", action: null),
                CreateAction(area: null, controller: "Product", action: null),

                // RPC controller in an area with the same name as home
                CreateAction(area: "Admin", controller: "Home", action: "Index"),
                CreateAction(area: "Admin", controller: "Home", action: "Diagnostics")
            };
        }

        public static IEnumerable<ActionDescriptor> GetActions(
            IEnumerable<ActionDescriptor> actions,
            string area,
            string controller,
            string action)
        {
            var comparer = new RouteValueEqualityComparer();

            return
                actions
                    .Where(a => a.RouteValues.Any(kvp => kvp.Key == "area" && comparer.Equals(kvp.Value, area)))
                    .Where(a => a.RouteValues.Any(kvp => kvp.Key == "controller" && comparer.Equals(kvp.Value, controller)))
                    .Where(a => a.RouteValues.Any(kvp => kvp.Key == "action" && comparer.Equals(kvp.Value, action)));
        }

        public static VirtualPathContext CreateContext(object routeValues)
        {
            return CreateContext(routeValues, ambientValues: null);
        }

        public static VirtualPathContext CreateContext(object routeValues, object ambientValues)
        {
            return new VirtualPathContext(
                new Mock<HttpContext>(MockBehavior.Strict).Object,
                new RouteValueDictionary(ambientValues),
                new RouteValueDictionary(routeValues));
        }

        public static RouteContext CreateRouteContext(string httpMethod)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(new Mock<IRouter>(MockBehavior.Strict).Object);

            var httpContext = CreateHttpContext(httpMethod, routeData);

            return new RouteContext(httpContext)
            {
                RouteData = routeData
            };
        }

        static DefaultHttpContext CreateHttpContext(string httpMethod, RouteData routeData)
        {
            var features = new FeatureCollection();
            features.Set((IRoutingFeature) new RoutingFeature {RouteData = routeData});
            var httpRequestFeature = (IHttpRequestFeature) new HttpRequestFeature
            {
                Method = httpMethod,
                Path = new PathString("/here"),
                Headers = new HeaderDictionary(),
                Protocol = "https",
                Body = new MemoryStream(new byte[0]),
                PathBase = "/",
                QueryString = "",
                RawTarget = "",
                Scheme = "https"
            };
            features.Set(httpRequestFeature);
            features.Set((IHttpResponseFeature) new HttpResponseFeature
            {
                Body = new MemoryStream(),
                Headers = new HeaderDictionary(),
                StatusCode = 200
            });

            var httpContext = new DefaultHttpContext(features);

            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var engineAsScopeFactory = typeof(ServiceProvider)
                .GetField("_engine", BindingFlags.Instance|BindingFlags.NonPublic)
                .GetValue(serviceProvider) as IServiceScopeFactory;
            features.Set(
                (IServiceProvidersFeature)new RequestServicesFeature(httpContext,engineAsScopeFactory));

            httpContext.Request
                .ShouldEqualByValueOnMembers(
                    features[typeof(IHttpRequestFeature)], 
                    new []{"Method","Protocol","Scheme"} );

            return httpContext;
        }

        public static ActionDescriptor CreateAction(string area, string controller, string action)
        {
            var actionDescriptor = new ControllerActionDescriptor
            {
                ActionName = string.Format("Area: {0}, Controller: {1}, Action: {2}", area, controller, action),
                Parameters = new List<ParameterDescriptor>()
            };

            actionDescriptor.RouteValues.Add("area", area);
            actionDescriptor.RouteValues.Add("controller", controller);
            actionDescriptor.RouteValues.Add("action", action);
            return actionDescriptor;
        }

        public static ActionConstraintCache GetActionConstraintCache(IActionConstraintProvider[] actionConstraintProviders = null)
        {
            var descriptorProvider = new DefaultActionDescriptorCollectionProvider(
                Enumerable.Empty<IActionDescriptorProvider>(),
                Enumerable.Empty<IActionDescriptorChangeProvider>());
            return new ActionConstraintCache(descriptorProvider, actionConstraintProviders.AsEnumerable() ?? new List<IActionConstraintProvider>());
        }

        protected class BooleanConstraint : IActionConstraint
        {
            public bool Pass { get; set; }

            public int Order { get; set; }

            public bool Accept(ActionConstraintContext context)
            {
                return Pass;
            }
        }

        protected class ConstraintFactory : IActionConstraintFactory
        {
            public IActionConstraint Constraint { get; set; }

            public bool IsReusable => true;

            public IActionConstraint CreateInstance(IServiceProvider services)
            {
                return Constraint;
            }
        }

        protected class BooleanConstraintMarker : IActionConstraintMetadata
        {
            public bool Pass { get; set; }
        }

        protected class BooleanConstraintProvider : IActionConstraintProvider
        {
            public int Order { get; set; }

            public void OnProvidersExecuting(ActionConstraintProviderContext context)
            {
                foreach (var item in context.Results)
                {
                    if (item.Metadata is BooleanConstraintMarker marker)
                    {
                        Assert.Null(item.Constraint);
                        item.Constraint = new BooleanConstraint { Pass = marker.Pass };
                    }
                }
            }

            public void OnProvidersExecuted(ActionConstraintProviderContext context)
            {
            }
        }

        protected class NonActionController
        {
            [NonAction]
            public void Put()
            {
            }

            [NonAction]
            public void RPCMethod()
            {
            }

            [NonAction]
            [HttpGet]
            public void RPCMethodWithHttpGet()
            {
            }
        }

        protected class ActionNameController
        {
            [ActionName("CustomActionName_Verb")]
            public void Put()
            {
            }

            [ActionName("CustomActionName_DefaultMethod")]
            public void Index()
            {
            }

            [ActionName("CustomActionName_RpcMethod")]
            public void RPCMethodWithHttpGet()
            {
            }
        }

        protected class HttpMethodAttributeTests_RestOnlyController
        {
            [HttpGet]
            [HttpPut]
            [HttpPost]
            [HttpDelete]
            [HttpPatch]
            [HttpHead]
            public void Put()
            {
            }

            [AcceptVerbs("PUT", "post", "GET", "delete", "pATcH")]
            public void Patch()
            {
            }
        }

    }
}
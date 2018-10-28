using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComponentAsService2.UseComponentAsService
{
    public class FinerGrainedActionSelector : ActionSelector
    {
        readonly ParameterBinder parameterBinder;
        readonly IModelBinderFactory modelBinderFactory;
        readonly IModelMetadataProvider modelMetadataProvider;
        readonly MvcOptions mvcOptions;
        RouteContext routeContext;

        readonly ILogger<FinerGrainedActionSelector> logger;

        /// <summary>
        /// Creates a new <see cref="FinerGrainedActionSelector"/>, which
        /// overrides <see cref="Microsoft.AspNetCore.Mvc.Internal.ActionSelector.SelectBestActions" />.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">
        /// The <see cref="T:Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider" />.
        /// </param>
        /// <param name="actionConstraintCache">The <see cref="T:Microsoft.AspNetCore.Mvc.Internal.ActionConstraintCache" /> that
        /// providers a set of <see cref="T:Microsoft.AspNetCore.Mvc.ActionConstraints.IActionConstraint" /> instances.</param>
        /// <param name="mvcOptions"></param>
        /// <param name="loggerFactory">The <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
        /// <param name="parameterBinder"></param>
        /// <param name="modelBinderFactory"></param>
        /// <param name="modelMetadataProvider"></param>
        /// <remarks>The constructor parameters are passed up to the base constructor</remarks>
        public FinerGrainedActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            ParameterBinder parameterBinder, 
            IModelBinderFactory modelBinderFactory, 
            IModelMetadataProvider modelMetadataProvider, 
            IOptions<MvcOptions> mvcOptions,
            ILoggerFactory loggerFactory) : base(actionDescriptorCollectionProvider, actionConstraintCache, loggerFactory)
        {
            if (parameterBinder == null)throw new ArgumentNullException(nameof(parameterBinder));
            if (modelBinderFactory == null)throw new ArgumentNullException(nameof(modelBinderFactory));
            if (modelMetadataProvider == null)throw new ArgumentNullException(nameof(modelMetadataProvider));
            if (mvcOptions == null)throw new ArgumentNullException(nameof(mvcOptions));
            this.parameterBinder = parameterBinder;
            this.modelBinderFactory = modelBinderFactory;
            this.modelMetadataProvider = modelMetadataProvider;
            this.mvcOptions = mvcOptions.Value;
            logger = loggerFactory.CreateLogger<FinerGrainedActionSelector>();
        }

        public new ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            routeContext = context;

            var controllerActionDescriptors = candidates?.OfType<ControllerActionDescriptor>();

            if ( controllerActionDescriptors.Count() <2)
                return candidates.Single();

            return controllerActionDescriptors
                    .OrderByDescending(
                        actionDescriptor => ScoreByParameterNameAndConvertibility.Score(
                            GetBindingInfo(actionDescriptor).ConfigureAwait(false).GetAwaiter().GetResult().Item1,
                            routeContext,
                            actionDescriptor)
                    ).First();
        }

        /// <inheritdoc />
        /// <summary>Return the single best matching action.</summary>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected override IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
        {
            var controllerActionDescriptors = actions?.OfType<ControllerActionDescriptor>();

            if ( actions==null ||  controllerActionDescriptors.Count() < 2)
                return actions;

            return Array.AsReadOnly( controllerActionDescriptors
                .OrderByDescending(
                    actionDescriptor => ScoreByParameterNameAndConvertibility.Score(
                                GetBindingInfo(actionDescriptor).ConfigureAwait(false).GetAwaiter().GetResult().Item1,
                                routeContext,
                                actionDescriptor)
                ).Take(1).ToArray());
        }

         async Task<(Dictionary<string, object>, Dictionary<ModelMetadata,object>)> GetBindingInfo(
            ControllerActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)throw new ArgumentNullException(nameof(actionDescriptor));

            var bindingResult = (new Dictionary<string, object>(), new Dictionary<ModelMetadata, object>());

            var parameterBindingInfo = 
                    GetParameterBindingInfo(
                        modelBinderFactory,
                        modelMetadataProvider,
                        actionDescriptor,
                        mvcOptions);

            var propertyBindingInfo = 
                    GetPropertyBindingInfo(modelBinderFactory, modelMetadataProvider, actionDescriptor);

            if (parameterBindingInfo == null && propertyBindingInfo == null)
                return bindingResult;

            //Bind(ControllerContext controllerContext, object controller, Dictionary<string, object> arguments)

            var actionContext = new ActionContext(routeContext.HttpContext, routeContext.RouteData, actionDescriptor);

            var controllerContext = new ControllerContext(actionContext)
            {
                ValueProviderFactories = 
                    new CopyOnWriteList<IValueProviderFactory>(mvcOptions.ValueProviderFactories.ToArray()),                
            };
            controllerContext.ModelState.MaxAllowedErrors = mvcOptions.MaxModelValidationErrors;

            var valueProvider = await CompositeValueProvider.CreateAsync(controllerContext);

            var parameters = actionDescriptor.Parameters;

            if(parameterBindingInfo!=null)for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                var bindingInfo = parameterBindingInfo[i];
                var modelMetadata = bindingInfo.ModelMetadata;

                if (!modelMetadata.IsBindingAllowed)
                {
                    continue;
                }

                var result = await parameterBinder.BindModelAsync(
                    controllerContext,
                    bindingInfo.ModelBinder,
                    valueProvider,
                    parameter,
                    modelMetadata,
                    value: null);

                if (result.IsModelSet)
                {
                    bindingResult.Item1[parameter.Name] = result.Model;
                }
            }

            var properties = actionDescriptor.BoundProperties;
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var bindingInfo = propertyBindingInfo[i];
                var modelMetadata = bindingInfo.ModelMetadata;

                if (!modelMetadata.IsBindingAllowed)
                {
                    continue;
                }

                var result = await parameterBinder.BindModelAsync(
                   controllerContext,
                   bindingInfo.ModelBinder,
                   valueProvider,
                   property,
                   modelMetadata,
                   value: null);

                if (result.IsModelSet)
                {
                    //PropertyValueSetter.SetValue(bindingInfo.ModelMetadata, controller, result.Model);
                    bindingResult.Item2[bindingInfo.ModelMetadata] = result.Model;
                }
            }

            return bindingResult;
        }

        static BinderItem[] GetParameterBindingInfo(
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider,
            ControllerActionDescriptor actionDescriptor,
            MvcOptions mvcOptions)
        {
            var parameters = actionDescriptor.Parameters;
            if (parameters==null || parameters.Count == 0){return null;}

            var parameterBindingInfo = new BinderItem[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                ModelMetadata metadata;
                if (mvcOptions.AllowValidatingTopLevelNodes &&
                    modelMetadataProvider is ModelMetadataProvider modelMetadataProviderBase &&
                    parameter is ControllerParameterDescriptor controllerParameterDescriptor)
                {
                    // The default model metadata provider derives from ModelMetadataProvider
                    // and can therefore supply information about attributes applied to parameters.
                    metadata = modelMetadataProviderBase.GetMetadataForParameter(controllerParameterDescriptor.ParameterInfo);
                }
                else
                {
                    // For backward compatibility, if there's a custom model metadata provider that
                    // only implements the older IModelMetadataProvider interface, access the more
                    // limited metadata information it supplies. In this scenario, validation attributes
                    // are not supported on parameters.
                    metadata = modelMetadataProvider.GetMetadataForType(parameter.ParameterType);
                }

                var binder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
                {
                    BindingInfo = parameter.BindingInfo,
                    Metadata = metadata,
                    CacheToken = parameter,
                });

                parameterBindingInfo[i] = new BinderItem(binder, metadata);
            }

            return parameterBindingInfo;
        }

        static BinderItem[] GetPropertyBindingInfo(
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider,
            ControllerActionDescriptor actionDescriptor)
        {
            var properties = actionDescriptor.BoundProperties;
            if (properties==null || properties.Count == 0){return null;}

            var propertyBindingInfo = new BinderItem[properties.Count];
            var controllerType = actionDescriptor.ControllerTypeInfo.AsType();
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var metadata = modelMetadataProvider.GetMetadataForProperty(controllerType, property.Name);
                var binder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
                {
                    BindingInfo = property.BindingInfo,
                    Metadata = metadata,
                    CacheToken = property,
                });

                propertyBindingInfo[i] = new BinderItem(binder, metadata);
            }

            return propertyBindingInfo;
        }

        struct BinderItem
        {
            public BinderItem(IModelBinder modelBinder, ModelMetadata modelMetadata)
            {
                ModelBinder = modelBinder;
                ModelMetadata = modelMetadata;
            }

            public IModelBinder ModelBinder { get; }

            public ModelMetadata ModelMetadata { get; }
        }
   }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Component.As.Service.Pieces
{
    /// <summary>
    /// A class that uses a scoring function such as <see cref="ScoreByParameterNameAndConvertibility.Score"/>
    /// inside <see cref="Choose"/> to disambiguate Controller Methods with the same name, by inspecting how well
    /// the incoming model values can be bound and fitted to the Method's parameters.
    /// </summary>
    public class ActionDisambiguatorForOverloadedMethods
    {
        public delegate int ScoreSignature(RouteContext rc, ActionDescriptor action);

        readonly IModelBinderFactory modelBinderFactory;
        readonly IModelMetadataProvider modelMetadataProvider;
        readonly MvcOptions mvcOptions;
        readonly ParameterBinder parameterBinder;
        readonly ScoreSignature scoreDelegate;

        public ActionDisambiguatorForOverloadedMethods(
            IModelBinderFactory modelBinderFactory, 
            IModelMetadataProvider modelMetadataProvider, 
            MvcOptions mvcOptions, 
            ParameterBinder parameterBinder,
            ScoreSignature scoreDelegate=null)
        {
            this.modelBinderFactory = modelBinderFactory;
            this.modelMetadataProvider = modelMetadataProvider;
            this.mvcOptions = mvcOptions;
            this.parameterBinder = parameterBinder;
            this.scoreDelegate = scoreDelegate??Score;
        }

        /// <summary>
        /// Given a <paramref name="routeContext"/> (which encapsulates candidate parameter values in an incoming <c>Request</c>)
        /// and <paramref name="candidateActions"/>, choose the <c>action</c> that is best-fitted to process the
        /// candidate parameter values.
        ///
        /// The default <see cref="Score"/> method uses MVC model binding and <see cref="ScoreByParameterNameAndConvertibility.Score"/>
        /// against each <paramref name="candidateActions"/> to evaluate how well it can handle the candidate parameter values in
        /// the <paramref name="routeContext"/>.
        /// </summary>
        /// <param name="routeContext"></param>
        /// <param name="candidateActions"></param>
        /// <returns>The best matched of the <paramref name="candidateActions"/></returns>
        /// <remarks>
        /// Override the default scoring by passing a <see cref="ScoreSignature"/> parameter to the constructor.
        /// </remarks>
        public IReadOnlyList<ActionDescriptor> Choose(RouteContext routeContext, IReadOnlyList<ActionDescriptor> candidateActions)
        {
            if (candidateActions==null || candidateActions.Count < 2) return candidateActions;

            var rankedChoices = 
                candidateActions.Select(a=> new {
                        Action= a, 
                        Score= scoreDelegate(routeContext, a)})
                .OrderByDescending(a=>a.Score).ToArray();

            return Array.AsReadOnly( new []{rankedChoices[0].Action}  );
        }

        internal int Score(RouteContext routeContext, ActionDescriptor a)
        {
            return ScoreByParameterNameAndConvertibility.Score(
                GetBindingInfo(a, routeContext).ConfigureAwait(false).GetAwaiter().GetResult().Item1,
                a);
        }

        async Task<(Dictionary<string, object>, Dictionary<ModelMetadata, object>)> 
            GetBindingInfo(ActionDescriptor actionDescriptor, RouteContext routeContext)
        {
            if (actionDescriptor == null)throw new ArgumentNullException(nameof(actionDescriptor));

            var bindingResult = (new Dictionary<string, object>(), new Dictionary<ModelMetadata, object>());

            var parameterBindingInfo = 
                GetParameterBindingInfo(
                    modelBinderFactory,
                    modelMetadataProvider,
                    actionDescriptor,
                    mvcOptions);

            BinderItem[] propertyBindingInfo=null;
            if(actionDescriptor is ControllerActionDescriptor)
            {
                propertyBindingInfo = 
                    GetPropertyBindingInfo(
                        modelBinderFactory, 
                        modelMetadataProvider, 
                        actionDescriptor as ControllerActionDescriptor);
            }

            if (parameterBindingInfo == null && propertyBindingInfo == null) return bindingResult;

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
            
            if(propertyBindingInfo!=null && properties!=null)
            {
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
            }
            return bindingResult;
        }

        static BinderItem[] GetParameterBindingInfo(
            IModelBinderFactory modelBinderFactory,
            IModelMetadataProvider modelMetadataProvider,
            ActionDescriptor actionDescriptor,
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
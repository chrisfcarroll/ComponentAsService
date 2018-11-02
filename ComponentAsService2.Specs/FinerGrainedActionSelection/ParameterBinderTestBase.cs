using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Component.As.Service.Specs.FinerGrainedActionSelection
{
    class ModelingBindingParameterBinderTestBase
    {
        public static readonly IOptions<MvcOptions> MvcOptionsWrapper = Options.Create(new MvcOptions
        {
            AllowValidatingTopLevelNodes = true,
            ModelBinderProviders = { }
        });

        public static ControllerContext GetControllerContext()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = services.BuildServiceProvider()
                }
            };
        }

        public static IEnumerable<KeyValuePair<Type, ModelMetadata>> CreateMetadataForTypes<T>() => CreateMetadataForTypes(typeof(T));

        public static IEnumerable<KeyValuePair<Type, ModelMetadata>> CreateMetadataForTypes(params Type[] types)
        {
            var provider = new EmptyModelMetadataProvider();
            var detailsProvider = new DefaultCompositeMetadataDetailsProvider(
                Enumerable.Empty<IMetadataDetailsProvider>());

            foreach (var type in types)
            {
                var key = ModelMetadataIdentity.ForType(type);
                var cache = new DefaultMetadataDetails(key, ModelAttributes.GetAttributesForType(type));
                var metadata = new DefaultModelMetadata(provider, detailsProvider, cache);
                Xunit.Assert.Equal(type, metadata.ModelType);
                Xunit.Assert.Null(metadata.PropertyName);
                Xunit.Assert.Null(metadata.ContainerType);
                yield return KeyValuePair.Create<Type,ModelMetadata>(type, metadata);
            }
        }

        public static IModelBinder CreateMockModelBinder(ModelBindingResult modelBinderResult)
        {
            var mockBinder = new Mock<IModelBinder>(MockBehavior.Strict);
            mockBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns<ModelBindingContext>(context =>
                {
                    context.Result = modelBinderResult;
                    return Task.CompletedTask;
                });
            return mockBinder.Object;
        }

        public static ParameterBinder CreateParameterBinder(IModelValidator validator = null,
            IOptions<MvcOptions> optionsAccessor = null,
            ILoggerFactory loggerFactory = null,
            params Type[] prepareMetadataForTypes)
        {
            var modelMetadata = new Dictionary<Type,ModelMetadata>(CreateMetadataForTypes(prepareMetadataForTypes));

            var mockModelMetadataProvider = new Mock<IModelMetadataProvider>(MockBehavior.Strict);
            mockModelMetadataProvider
                .Setup(o => o.GetMetadataForType( It.IsAny<Type>()))
                .Returns( (Type t) => modelMetadata[t] );

            var mockModelBinderFactory = new Mock<IModelBinderFactory>(MockBehavior.Strict);

            optionsAccessor = optionsAccessor ?? MvcOptionsWrapper;

            return new ParameterBinder(
                mockModelMetadataProvider.Object,
                mockModelBinderFactory.Object,
                new DefaultObjectValidator(
                    mockModelMetadataProvider.Object,
                    new[] { GetModelValidatorProvider(validator) }),
                optionsAccessor,
                loggerFactory ?? NullLoggerFactory.Instance);
        }

        public static IModelValidatorProvider GetModelValidatorProvider(IModelValidator validator = null)
        {
            if (validator == null)
            {
                validator = Mock.Of<IModelValidator>();
            }

            var validatorProvider = new Mock<IModelValidatorProvider>();
            validatorProvider
                .Setup(p => p.CreateValidators(It.IsAny<ModelValidatorProviderContext>()))
                .Callback<ModelValidatorProviderContext>(context =>
                {
                    foreach (var result in context.Results)
                    {
                        result.Validator = validator;
                        result.IsReusable = true;
                    }
                });
            return validatorProvider.Object;
        }

        static ParameterBinder CreateBackCompatParameterBinder(
            ModelMetadata modelMetadata,
            IObjectModelValidator validator)
        {
            var mockModelMetadataProvider = new Mock<IModelMetadataProvider>(MockBehavior.Strict);
            mockModelMetadataProvider
                .Setup(o => o.GetMetadataForType(typeof(Person)))
                .Returns(modelMetadata);

            var mockModelBinderFactory = new Mock<IModelBinderFactory>(MockBehavior.Strict);
#pragma warning disable CS0618 // Type or member is obsolete
            return new ParameterBinder(
                mockModelMetadataProvider.Object,
                mockModelBinderFactory.Object,
                validator);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static IValueProvider CreateMockValueProvider()
        {
            var mockValueProvider = new Mock<IValueProvider>(MockBehavior.Strict);
            mockValueProvider
                .Setup(o => o.ContainsPrefix(It.IsAny<string>()))
                .Returns(true);
            return mockValueProvider.Object;
        }

        public static IModelValidatorProvider CreateMockValidatorProvider(IModelValidator validator = null)
        {
            var mockValidator = new Mock<IModelValidatorProvider>();
            mockValidator
                .Setup(o => o.CreateValidators(
                    It.IsAny<ModelValidatorProviderContext>()))
                .Callback<ModelValidatorProviderContext>(context =>
                {
                    if (validator != null)
                    {
                        foreach (var result in context.Results)
                        {
                            result.Validator = validator;
                        }
                    }
                });
            return mockValidator.Object;
        }

        class Person : IEquatable<Person>, IEquatable<object>
        {
            public string Name { get; set; }

            public bool Equals(Person other)
            {
                return other != null && string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            bool IEquatable<object>.Equals(object obj)
            {
                return Equals(obj as Person);
            }
        }

        class Family
        {
            public Person Dad { get; set; }

            public Person Mom { get; set; }

            public IList<Person> Kids { get; } = new List<Person>();
        }

        class DerivedPerson : Person
        {
            [Required]
            public string DerivedProperty { get; set; }
        }

        [Required]
        Person PersonProperty { get; set; }

        public abstract class FakeModelMetadata : ModelMetadata
        {
            public FakeModelMetadata(): base(ModelMetadataIdentity.ForType(typeof(string))){}
            public FakeModelMetadata(bool isBindingAllowed=true, Type forType=null)
                    : base(ModelMetadataIdentity.ForType( forType ?? typeof(string)))
            {
                IsBindingAllowed = isBindingAllowed;
            }

            public override bool IsBindingAllowed { get; } = true;
        }

        void TestMethodWithoutAttributes(Person person) { }

        void TestMethodWithAttributes([Required][AlwaysInvalid] Person person) { }

        class TestController
        {
            public BaseModel Model { get; set; }
        }

        class TestControllerWithValidatedProperties
        {
            [AlwaysInvalid]
            [Required]
            public BaseModel Model { get; set; }
        }

        class BaseModel
        {
        }

        class DerivedModel
        {
            [Required]
            public string DerivedProperty { get; set; }
        }

        class AlwaysInvalidAttribute : ValidationAttribute
        {
            public AlwaysInvalidAttribute()
            {
                ErrorMessage = "Always Invalid";
            }

            public override bool IsValid(object value)
            {
                return false;
            }
        }
    }
}

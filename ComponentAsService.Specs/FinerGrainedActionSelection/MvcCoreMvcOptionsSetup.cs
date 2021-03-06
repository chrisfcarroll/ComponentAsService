﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Component.As.Service.Specs.FinerGrainedActionSelection
{
    /// <summary>
    /// Sets up default options for <see cref="MvcOptions"/>.
    /// </summary>
    public class MvcCoreMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        readonly IHttpRequestStreamReaderFactory _readerFactory;
        readonly ILoggerFactory _loggerFactory;

        // Used in tests
        public MvcCoreMvcOptionsSetup(IHttpRequestStreamReaderFactory readerFactory)
            : this(readerFactory, NullLoggerFactory.Instance)
        {
        }

        public MvcCoreMvcOptionsSetup(IHttpRequestStreamReaderFactory readerFactory, ILoggerFactory loggerFactory)
        {
            if (readerFactory == null)
            {
                throw new ArgumentNullException(nameof(readerFactory));
            }

            _readerFactory = readerFactory;
            _loggerFactory = loggerFactory;
        }

        public void Configure(MvcOptions options)
        {
            // Set up ModelBinding
            options.ModelBinderProviders.Add(new BinderTypeModelBinderProvider());
            options.ModelBinderProviders.Add(new ServicesModelBinderProvider());
            options.ModelBinderProviders.Add(new BodyModelBinderProvider(options.InputFormatters, _readerFactory, _loggerFactory, options));
            options.ModelBinderProviders.Add(new HeaderModelBinderProvider());
            options.ModelBinderProviders.Add(new FloatingPointTypeModelBinderProvider());
            options.ModelBinderProviders.Add(new EnumTypeModelBinderProvider(options));
            options.ModelBinderProviders.Add(new SimpleTypeModelBinderProvider());
            options.ModelBinderProviders.Add(new CancellationTokenModelBinderProvider());
            options.ModelBinderProviders.Add(new ByteArrayModelBinderProvider());
            options.ModelBinderProviders.Add(new FormFileModelBinderProvider());
            options.ModelBinderProviders.Add(new FormCollectionModelBinderProvider());
            options.ModelBinderProviders.Add(new KeyValuePairModelBinderProvider());
            options.ModelBinderProviders.Add(new DictionaryModelBinderProvider());
            options.ModelBinderProviders.Add(new ArrayModelBinderProvider());
            options.ModelBinderProviders.Add(new CollectionModelBinderProvider());
            options.ModelBinderProviders.Add(new ComplexTypeModelBinderProvider());

            // Set up filters
            options.Filters.Add(new UnsupportedContentTypeFilter());

            // Set up default output formatters.
            options.OutputFormatters.Add(new HttpNoContentOutputFormatter());
            options.OutputFormatters.Add(new StringOutputFormatter());
            options.OutputFormatters.Add(new StreamOutputFormatter());

            // Set up ValueProviders
            options.ValueProviderFactories.Add(new FormValueProviderFactory());
            options.ValueProviderFactories.Add(new RouteValueProviderFactory());
            options.ValueProviderFactories.Add(new QueryStringValueProviderFactory());
            options.ValueProviderFactories.Add(new JQueryFormValueProviderFactory());

            // Set up metadata providers
            ConfigureAdditionalModelMetadataDetailsProvider(options.ModelMetadataDetailsProviders);

            // Set up validators
            options.ModelValidatorProviders.Add(new DefaultModelValidatorProvider());
        }

        internal static void ConfigureAdditionalModelMetadataDetailsProvider(IList<IMetadataDetailsProvider> modelMetadataDetailsProviders)
        {
            // Don't bind the Type class by default as it's expensive. A user can override this behavior
            // by altering the collection of providers.
            modelMetadataDetailsProviders.Add(new ExcludeBindingMetadataProvider(typeof(Type)));

            modelMetadataDetailsProviders.Add(new DefaultBindingMetadataProvider());
            modelMetadataDetailsProviders.Add(new DefaultValidationMetadataProvider());

            modelMetadataDetailsProviders.Add(new BindingSourceMetadataProvider(typeof(CancellationToken), BindingSource.Special));
            modelMetadataDetailsProviders.Add(new BindingSourceMetadataProvider(typeof(IFormFile), BindingSource.FormFile));
            modelMetadataDetailsProviders.Add(new BindingSourceMetadataProvider(typeof(IFormCollection), BindingSource.FormFile));
            modelMetadataDetailsProviders.Add(new BindingSourceMetadataProvider(typeof(IFormFileCollection), BindingSource.FormFile));

            // Add types to be excluded from Validation
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Type)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Uri)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(CancellationToken)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(IFormFile)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(IFormCollection)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(IFormFileCollection)));
            modelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Stream)));
        }
    }
}
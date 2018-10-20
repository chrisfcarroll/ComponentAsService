using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;

namespace ComponentAsService2.UseComponentAsService
{
    public class FinerGrainedActionSelector : ActionSelector
    {
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
        /// <param name="loggerFactory">The <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
        /// <remarks>The constructor parameters are passed up to the base constructor</remarks>
        public FinerGrainedActionSelector(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            ILoggerFactory loggerFactory) : base(actionDescriptorCollectionProvider, actionConstraintCache, loggerFactory)
        {
            logger = loggerFactory.CreateLogger<FinerGrainedActionSelector>();
        }


        /// <inheritdoc />
        /// <summary>Returns the set of best matching actions.</summary>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected override IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
        {
            var count = actions.Count;
            logger.LogDebug($"SelectBestActions( resolving best of {count} actions)");
            if(count!=1)logger.LogDebug( actions.ToJson());
            return base.SelectBestActions(actions);
        }

        class StringArrayComparer : IEqualityComparer<string[]>
        {
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer(StringComparer.Ordinal);

            public static readonly StringArrayComparer OrdinalIgnoreCase =
                new StringArrayComparer(StringComparer.OrdinalIgnoreCase);

            readonly StringComparer _valueComparer;

            StringArrayComparer(StringComparer valueComparer)
            {
                _valueComparer = valueComparer;
            }

            public bool Equals(string[] x, string[] y)
            {
                if (x == y)
                    return true;
                if (x == null ^ y == null || x.Length != y.Length)
                    return false;
                for (int index = 0; index < x.Length; ++index)
                {
                    if ((!string.IsNullOrEmpty(x[index]) || !string.IsNullOrEmpty(y[index])) && !_valueComparer.Equals(x[index], y[index]))
                        return false;
                }

                return true;
            }

            public int GetHashCode(string[] obj)
            {
                if (obj == null)
                    return 0;
                var hashCodeCombiner = new HashCodeCombiner();
                for (int index = 0; index < obj.Length; ++index)
                    hashCodeCombiner.Add(obj[index], _valueComparer);
                return hashCodeCombiner.CombinedHash;
            }
        }
    }
}
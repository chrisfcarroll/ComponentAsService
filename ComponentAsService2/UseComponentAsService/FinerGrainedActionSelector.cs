using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;

namespace ComponentAsService2.UseComponentAsService
{
    public class FinerGrainedActionSelector : ActionSelector
    {
        public delegate ActionDescriptor SelectBestOneOfActionsDelegate(ILogger logger, IReadOnlyList<ActionDescriptor> actions);

        readonly ILogger<FinerGrainedActionSelector> logger;

        SelectBestOneOfActionsDelegate _selectBestOneOfSeveralActionsStrategy = (logger, actions) => actions.First();

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

        /// <summary>This Strategy is called by <see cref="SelectBestActions"/> if and only if there is
        /// more than one action to choose from. 
        /// The default implementation simply picks the first one.
        /// </summary>
        public SelectBestOneOfActionsDelegate SelectBestOneOfSeveralActionsStrategy
        {
            get { return _selectBestOneOfSeveralActionsStrategy;}
            set { 
                _selectBestOneOfSeveralActionsStrategy = value ?? ((l, a) => a.First());
            }
        }


        /// <inheritdoc />
        /// <summary>Calls <see cref="SelectBestOneOfSeveralActionsStrategy"/> to returns the single best matching action.</summary>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected override IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
            => actions?.Count>1 ? new []{ _selectBestOneOfSeveralActionsStrategy(logger, actions)} : actions;

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

    public class SelectActionByParameterNameAndConvertibility
    {
        public static FinerGrainedActionSelector.SelectBestOneOfActionsDelegate Apply
            = (logger, actions) => actions.OrderByDescending(Score).First();

        static object TryConvert(Type toType, string fromString)
        {
            try
            {
                return TypeDescriptor.GetConverter(toType).ConvertFromString(fromString);
            }
            catch{ return null; }
        }

        public static int Score(ActionDescriptor action)
        {
            var actualParameters = action.RouteValues;
            var expectedParameters = action.Parameters?.Select(p => new {p.Name, p.ParameterType});
            var convertibleMatches =
                expectedParameters==null
                    ? 0 
                    :actualParameters
                        .Join(expectedParameters,
                            kv => kv.Key, nt => nt.Name,
                            (a, e) => new
                            {
                                a.Key,
                                e.ParameterType,
                                Value = TryConvert(e.ParameterType,a.Value),
                            })
                        .Count(x=>x.Value!=null);

            var mismatches = actualParameters.Count + (expectedParameters?.Count()??0) - 2 * convertibleMatches;
            return -mismatches;
        }
    }
}
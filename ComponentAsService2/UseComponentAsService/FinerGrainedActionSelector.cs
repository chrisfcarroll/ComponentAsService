using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComponentAsService2.UseComponentAsService
{
    public class FinerGrainedActionSelector : IActionSelector
    {
        static readonly IReadOnlyList<ActionDescriptor> EmptyActions = Array.Empty<ActionDescriptor>();
        readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        readonly ActionConstraintCache actionConstraintCache;
        readonly ILogger<FinerGrainedActionSelector> logger;
        readonly ActionDisambiguatorForOverloadedMethods actionDisambiguator;
        Cache _cache;

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
            ILoggerFactory loggerFactory)
        {
            if (parameterBinder == null) throw new ArgumentNullException(nameof(parameterBinder));
            if (modelBinderFactory == null) throw new ArgumentNullException(nameof(modelBinderFactory));
            if (modelMetadataProvider == null) throw new ArgumentNullException(nameof(modelMetadataProvider));
            if (mvcOptions == null) throw new ArgumentNullException(nameof(mvcOptions));
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.actionConstraintCache = actionConstraintCache;
            logger = loggerFactory.CreateLogger<FinerGrainedActionSelector>();
            actionDisambiguator =
                new ActionDisambiguatorForOverloadedMethods(modelBinderFactory, modelMetadataProvider, mvcOptions.Value, parameterBinder);
        }

        /// <summary>Return the single best matching action.</summary>
        /// <param name="routeContext"></param>
        /// <param name="actions">The set of actions that satisfy all constraints.</param>
        /// <returns>A list of the best matching actions.</returns>
        protected virtual IReadOnlyList<ActionDescriptor> SelectBestActions(RouteContext routeContext, IReadOnlyList<ActionDescriptor> actions)
        {
            if (actions == null || actions.Count() < 2)
                return actions;

            return actionDisambiguator.Choose(routeContext, actions);
        }

        Cache Current
        {
            get
            {
                var actions = actionDescriptorCollectionProvider.ActionDescriptors;
                var cache = Volatile.Read(ref _cache);

                if (cache != null && cache.Version == actions.Version)
                {
                    return cache;
                }

                cache = new Cache(actions);
                Volatile.Write(ref _cache, cache);
                return cache;
            }
        }

        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            if (context == null){throw new ArgumentNullException(nameof(context));}

            var cache = Current;

            // The Cache works based on a string[] of the route values in a pre-calculated order. This code extracts
            // those values in the correct order.
            var keys = cache.RouteKeys;
            var values = new string[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                context.RouteData.Values.TryGetValue(keys[i], out object value);

                if (value != null)
                {
                    values[i] = value as string ?? Convert.ToString(value);
                }
            }

            if (cache.OrdinalEntries.TryGetValue(values, out var matchingRouteValues) ||
                cache.OrdinalIgnoreCaseEntries.TryGetValue(values, out matchingRouteValues))
            {
                Debug.Assert(matchingRouteValues != null);
                return matchingRouteValues;
            }

            logger.NoActionsMatched(context.RouteData.Values);
            return EmptyActions;
        }

        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            if (context == null)throw new ArgumentNullException(nameof(context));
            if (candidates == null)throw new ArgumentNullException(nameof(candidates));

            var matches = EvaluateActionConstraints(context, candidates);

            var finalMatches = SelectBestActions(context, matches);
            if (finalMatches == null || finalMatches.Count == 0)
            {
                return null;
            }
            else if (finalMatches.Count == 1)
            {
                var selectedAction = finalMatches[0];

                return selectedAction;
            }
            else
            {
                var actionNames = string.Join(
                    Environment.NewLine,
                    finalMatches.Select(a => a.DisplayName));

                logger.AmbiguousActions(actionNames);

                var message = string.Format("Multiple actions matched. The following actions matched route data and had all constraints satisfied:{0}{0}{1}",
                    Environment.NewLine,
                    actionNames);

                throw new AmbiguousActionException(message);
            }
        }

        IReadOnlyList<ActionDescriptor> EvaluateActionConstraints(
            RouteContext context,
            IReadOnlyList<ActionDescriptor> actions)
        {
            var candidates = new List<ActionSelectorCandidate>();

            // Perf: Avoid allocations
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                var constraints = actionConstraintCache.GetActionConstraints(context.HttpContext, action);
                candidates.Add(new ActionSelectorCandidate(action, constraints));
            }

            var matches = EvaluateActionConstraintsCore(context, candidates, startingOrder: null);

            List<ActionDescriptor> results = null;
            if (matches != null)
            {
                results = new List<ActionDescriptor>(matches.Count);
                // Perf: Avoid allocations
                for (var i = 0; i < matches.Count; i++)
                {
                    var candidate = matches[i];
                    results.Add(candidate.Action);
                }
            }

            return results;
        }

        IReadOnlyList<ActionSelectorCandidate> EvaluateActionConstraintsCore(
            RouteContext context,
            IReadOnlyList<ActionSelectorCandidate> candidates,
            int? startingOrder)
        {
            // Find the next group of constraints to process. This will be the lowest value of
            // order that is higher than startingOrder.
            int? order = null;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.Constraints != null)
                {
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if ((startingOrder == null || constraint.Order > startingOrder) &&
                            (order == null || constraint.Order < order))
                        {
                            order = constraint.Order;
                        }
                    }
                }
            }

            // If we don't find a next then there's nothing left to do.
            if (order == null)
            {
                return candidates;
            }

            // Since we have a constraint to process, bisect the set of actions into those with and without a
            // constraint for the current order.
            var actionsWithConstraint = new List<ActionSelectorCandidate>();
            var actionsWithoutConstraint = new List<ActionSelectorCandidate>();

            var constraintContext = new ActionConstraintContext();
            constraintContext.Candidates = candidates;
            constraintContext.RouteContext = context;

            // Perf: Avoid allocations
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var isMatch = true;
                var foundMatchingConstraint = false;

                if (candidate.Constraints != null)
                {
                    constraintContext.CurrentCandidate = candidate;
                    for (var j = 0; j < candidate.Constraints.Count; j++)
                    {
                        var constraint = candidate.Constraints[j];
                        if (constraint.Order == order)
                        {
                            foundMatchingConstraint = true;

                            if (!constraint.Accept(constraintContext))
                            {
                                isMatch = false;
                                MvcCoreLoggerExtensions.ConstraintMismatch(logger,
                                    candidate.Action.DisplayName,
                                    candidate.Action.Id,
                                    constraint);
                                break;
                            }
                        }
                    }
                }

                if (isMatch && foundMatchingConstraint)
                {
                    actionsWithConstraint.Add(candidate);
                }
                else if (isMatch)
                {
                    actionsWithoutConstraint.Add(candidate);
                }
            }

            // If we have matches with constraints, those are better so try to keep processing those
            if (actionsWithConstraint.Count > 0)
            {
                var matches = EvaluateActionConstraintsCore(context, actionsWithConstraint, order);
                if (matches?.Count > 0)
                {
                    return matches;
                }
            }

            // If the set of matches with constraints can't work, then process the set without constraints.
            if (actionsWithoutConstraint.Count == 0)
            {
                return null;
            }
            else
            {
                return EvaluateActionConstraintsCore(context, actionsWithoutConstraint, order);
            }
        }

        // The action selector cache stores a mapping of route-values -> action descriptors for each known set of
        // of route-values. We actually build two of these mappings, one for case-sensitive (fast path) and one for
        // case-insensitive (slow path).
        //
        // This is necessary because MVC routing/action-selection is always case-insensitive. So we're going to build
        // a case-sensitive dictionary that will behave like the a case-insensitive dictionary when you hit one of the
        // canonical entries. When you don't hit a case-sensitive match it will try the case-insensitive dictionary
        // so you still get correct behaviors.
        //
        // The difference here is because while MVC is case-insensitive, doing a case-sensitive comparison is much 
        // faster. We also expect that most of the URLs we process are canonically-cased because they were generated
        // by Url.Action or another routing api.
        //
        // This means that for a set of actions like:
        //      { controller = "Home", action = "Index" } -> HomeController::Index1()
        //      { controller = "Home", action = "index" } -> HomeController::Index2()
        //
        // Both of these actions match "Index" case-insensitively, but there exist two known canonical casings,
        // so we will create an entry for "Index" and an entry for "index". Both of these entries match **both**
        // actions.
        class Cache
        {
            public Cache(ActionDescriptorCollection actions)
            {
                // We need to store the version so the cache can be invalidated if the actions change.
                Version = actions.Version;

                // We need to build two maps for all of the route values.
                OrdinalEntries = new Dictionary<string[], List<ActionDescriptor>>(StringArrayComparer.Ordinal);
                OrdinalIgnoreCaseEntries = new Dictionary<string[], List<ActionDescriptor>>(StringArrayComparer.OrdinalIgnoreCase);

                // We need to first identify of the keys that action selection will look at (in route data). 
                // We want to only consider conventionally routed actions here.
                var routeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < actions.Items.Count; i++)
                {
                    var action = actions.Items[i];
                    if (action.AttributeRouteInfo == null)
                    {
                        // This is a conventionally routed action - so make sure we include its keys in the set of
                        // known route value keys.
                        foreach (var kvp in action.RouteValues)
                        {
                            routeKeys.Add(kvp.Key);
                        }
                    }
                }

                // We need to hold on to an ordered set of keys for the route values. We'll use these later to
                // extract the set of route values from an incoming request to compare against our maps of known
                // route values.
                RouteKeys = routeKeys.ToArray();

                for (var i = 0; i < actions.Items.Count; i++)
                {
                    var action = actions.Items[i];
                    if (action.AttributeRouteInfo != null)
                    {
                        // This only handles conventional routing. Ignore attribute routed actions.
                        continue;
                    }

                    // This is a conventionally routed action - so we need to extract the route values associated
                    // with this action (in order) so we can store them in our dictionaries.
                    var routeValues = new string[RouteKeys.Length];
                    for (var j = 0; j < RouteKeys.Length; j++)
                    {
                        action.RouteValues.TryGetValue(RouteKeys[j], out routeValues[j]);
                    }

                    if (!OrdinalIgnoreCaseEntries.TryGetValue(routeValues, out var entries))
                    {
                        entries = new List<ActionDescriptor>();
                        OrdinalIgnoreCaseEntries.Add(routeValues, entries);
                    }

                    entries.Add(action);

                    // We also want to add the same (as in reference equality) list of actions to the ordinal entries.
                    // We'll keep updating `entries` to include all of the actions in the same equivalence class -
                    // meaning, all conventionally routed actions for which the route values are equalignoring case.
                    //
                    // `entries` will appear in `OrdinalIgnoreCaseEntries` exactly once and in `OrdinalEntries` once
                    // for each variation of casing that we've seen.
                    if (!OrdinalEntries.ContainsKey(routeValues))
                    {
                        OrdinalEntries.Add(routeValues, entries);
                    }
                }
            }

            public int Version { get; }

            public string[] RouteKeys { get; }

            public Dictionary<string[], List<ActionDescriptor>> OrdinalEntries { get; }

            public Dictionary<string[], List<ActionDescriptor>> OrdinalIgnoreCaseEntries { get; }
        }

        class StringArrayComparer : IEqualityComparer<string[]>
        {
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer(StringComparer.Ordinal);

            public static readonly StringArrayComparer OrdinalIgnoreCase = new StringArrayComparer(StringComparer.OrdinalIgnoreCase);

            readonly StringComparer _valueComparer;

            StringArrayComparer(StringComparer valueComparer)
            {
                _valueComparer = valueComparer;
            }

            public bool Equals(string[] x, string[] y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null ^ y == null)
                {
                    return false;
                }

                if (x.Length != y.Length)
                {
                    return false;
                }

                for (var i = 0; i < x.Length; i++)
                {
                    if (string.IsNullOrEmpty(x[i]) && string.IsNullOrEmpty(y[i]))
                    {
                        continue;
                    }

                    if (!_valueComparer.Equals(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(string[] obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                var hash = new HashCodeCombiner();
                for (var i = 0; i < obj.Length; i++)
                {
                    var o = obj[i];

                    // Route values define null and "" to be equivalent.
                    if (string.IsNullOrEmpty(o))
                    {
                        o = null;
                    }
                    hash.Add(o, _valueComparer);
                }

                return hash.CombinedHash;
            }
        }
    }

    static class MvcCoreLoggerExtensions
    {
        public static void AmbiguousActions(this ILogger logger, string actionNames)
        {
            (LoggerMessage.Define<string>(
                LogLevel.Error,
                1,
                "Request matched multiple actions resulting in ambiguity. Matching actions: {AmbiguousActions}"))
              (logger, actionNames, null);
        }

        public static void ConstraintMismatch(
            this ILogger logger,
            string actionName,
            string actionId,
            IActionConstraint actionConstraint)
        {
            LoggerMessage.Define<string, string, IActionConstraint>(
                    LogLevel.Debug,
                    2,
                    "Action '{ActionName}' with id '{ActionId}' did not match the constraint '{ActionConstraint}'")
                (logger, actionName, actionId, actionConstraint, null);
        }
        public static void NoActionsMatched(this ILogger logger, IDictionary<string, object> routeValueDictionary)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                string[] routeValues = null;
                if (routeValueDictionary != null)
                {
                    routeValues = routeValueDictionary
                        .Select(pair => pair.Key + "=" + Convert.ToString(pair.Value))
                        .ToArray();
                }

                (LoggerMessage.Define<string[]>(
                    LogLevel.Debug,
                    3,
                    "No actions matched the current request. Route values: {RouteValues}"))
                  (logger, routeValues, null);
            }
        }
    }
}
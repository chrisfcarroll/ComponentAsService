// Decompiled with JetBrains decompiler
// Type: Microsoft.AspNetCore.Mvc.Internal.DefaultActionSelector
// Assembly: Microsoft.AspNetCore.Mvc.Core, Version=2.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: ED07561A-F00F-4966-8EBC-547EA3E7CE17
// Assembly location: C:\Program Files\dotnet\sdk\NuGetFallbackFolder\microsoft.aspnetcore.mvc.core\2.1.1\lib\netstandard2.0\Microsoft.AspNetCore.Mvc.Core.dll


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Component.As.Service.UseComponentAsService
{

    // ReSharper disable All 

  /// <summary>
  /// A default <see cref="T:Microsoft.AspNetCore.Mvc.Infrastructure.IActionSelector" /> implementation.
  /// </summary>
  /// <remarks>Copy-pasted from https://github.com/aspnet/Mvc/tree/release/2.1/src </remarks>
  public class DefaultActionSelector : IActionSelector
  {
    private static readonly IReadOnlyList<ActionDescriptor> EmptyActions = (IReadOnlyList<ActionDescriptor>) Array.Empty<ActionDescriptor>();
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly ActionConstraintCache _actionConstraintCache;
    private readonly ILogger _logger;
    private DefaultActionSelector.Cache _cache;

    /// <summary>
    /// Creates a new <see cref="T:Microsoft.AspNetCore.Mvc.Internal.DefaultActionSelector" />.
    /// </summary>
    /// <param name="actionDescriptorCollectionProvider">
    /// The <see cref="T:Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider" />.
    /// </param>
    /// <param name="actionConstraintCache">The <see cref="T:Microsoft.AspNetCore.Mvc.Internal.ActionConstraintCache" /> that
    /// providers a set of <see cref="T:Microsoft.AspNetCore.Mvc.ActionConstraints.IActionConstraint" /> instances.</param>
    /// <param name="loggerFactory">The <see cref="T:Microsoft.Extensions.Logging.ILoggerFactory" />.</param>
    public DefaultActionSelector(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider, ActionConstraintCache actionConstraintCache, ILoggerFactory loggerFactory)
    {
      this._actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
      this._logger = (ILogger) loggerFactory.CreateLogger<DefaultActionSelector>();
      this._actionConstraintCache = actionConstraintCache;
    }

    private DefaultActionSelector.Cache Current
    {
      get
      {
        ActionDescriptorCollection actionDescriptors = this._actionDescriptorCollectionProvider.ActionDescriptors;
        DefaultActionSelector.Cache cache1 = Volatile.Read<DefaultActionSelector.Cache>(ref this._cache);
        if (cache1 != null && cache1.Version == actionDescriptors.Version)
          return cache1;
        DefaultActionSelector.Cache cache2 = new DefaultActionSelector.Cache(actionDescriptors);
        Volatile.Write<DefaultActionSelector.Cache>(ref this._cache, cache2);
        return cache2;
      }
    }

    public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
    {
      if (context == null)throw new ArgumentNullException(nameof (context));
      DefaultActionSelector.Cache current = this.Current;
      string[] routeKeys = current.RouteKeys;
      string[] key = new string[routeKeys.Length];
      for (int index = 0; index < routeKeys.Length; ++index)
      {
        object obj;
        context.RouteData.Values.TryGetValue(routeKeys[index], out obj);
        if (obj != null)key[index] = obj as string ?? Convert.ToString(obj);
      }
      List<ActionDescriptor> actionDescriptorList;
      if (current.OrdinalEntries.TryGetValue(key, out actionDescriptorList) || current.OrdinalIgnoreCaseEntries.TryGetValue(key, out actionDescriptorList))
        return (IReadOnlyList<ActionDescriptor>) actionDescriptorList;
      this._logger.LogDebug("NoActionsMatched {0}",(IDictionary<string, object>) context.RouteData.Values);
      return DefaultActionSelector.EmptyActions;
    }

    public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
    {
      if (context == null)
        throw new ArgumentNullException(nameof (context));
      if (candidates == null)
        throw new ArgumentNullException(nameof (candidates));
      IReadOnlyList<ActionDescriptor> source = this.SelectBestActions(this.EvaluateActionConstraints(context, candidates));
      if (source == null || source.Count == 0)
        return (ActionDescriptor) null;
      if (source.Count == 1)
        return source[0];
      string actionNames = string.Join(Environment.NewLine, source.Select<ActionDescriptor, string>((Func<ActionDescriptor, string>) (a => a.DisplayName)));
      this._logger.LogDebug("AmbiguousActions {0}",actionNames);
      throw new AmbiguousActionException( $"DefaultActionSelector_AmbiguousActions  {actionNames}");
    }

    /// <summary>Returns the set of best matching actions.</summary>
    /// <param name="actions">The set of actions that satisfy all constraints.</param>
    /// <returns>A list of the best matching actions.</returns>
    protected virtual IReadOnlyList<ActionDescriptor> SelectBestActions(IReadOnlyList<ActionDescriptor> actions)
    {
      return actions;
    }

    private IReadOnlyList<ActionDescriptor> EvaluateActionConstraints(RouteContext context, IReadOnlyList<ActionDescriptor> actions)
    {
      List<ActionSelectorCandidate> selectorCandidateList = new List<ActionSelectorCandidate>();
      for (int index = 0; index < actions.Count; ++index)
      {
        ActionDescriptor action = actions[index];
        IReadOnlyList<IActionConstraint> actionConstraints = this._actionConstraintCache.GetActionConstraints(context.HttpContext, action);
        selectorCandidateList.Add(new ActionSelectorCandidate(action, actionConstraints));
      }
      IReadOnlyList<ActionSelectorCandidate> actionConstraintsCore = this.EvaluateActionConstraintsCore(context, (IReadOnlyList<ActionSelectorCandidate>) selectorCandidateList, new int?());
      List<ActionDescriptor> actionDescriptorList = (List<ActionDescriptor>) null;
      if (actionConstraintsCore != null)
      {
        actionDescriptorList = new List<ActionDescriptor>(actionConstraintsCore.Count);
        for (int index = 0; index < actionConstraintsCore.Count; ++index)
        {
          ActionSelectorCandidate selectorCandidate = actionConstraintsCore[index];
          actionDescriptorList.Add(selectorCandidate.Action);
        }
      }
      return (IReadOnlyList<ActionDescriptor>) actionDescriptorList;
    }

    private IReadOnlyList<ActionSelectorCandidate> EvaluateActionConstraintsCore(RouteContext context, IReadOnlyList<ActionSelectorCandidate> candidates, int? startingOrder)
    {
      int? startingOrder1 = new int?();
      int? nullable;
      for (int index1 = 0; index1 < candidates.Count; ++index1)
      {
        ActionSelectorCandidate candidate = candidates[index1];
        if (candidate.Constraints != null)
        {
          for (int index2 = 0; index2 < candidate.Constraints.Count; ++index2)
          {
            IActionConstraint constraint = candidate.Constraints[index2];
            if (startingOrder.HasValue)
            {
              int order = constraint.Order;
              nullable = startingOrder;
              int valueOrDefault = nullable.GetValueOrDefault();
              if ((order > valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                continue;
            }
            if (startingOrder1.HasValue)
            {
              int order = constraint.Order;
              nullable = startingOrder1;
              int valueOrDefault = nullable.GetValueOrDefault();
              if ((order < valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) == 0)
                continue;
            }
            startingOrder1 = new int?(constraint.Order);
          }
        }
      }
      if (!startingOrder1.HasValue)
        return candidates;
      List<ActionSelectorCandidate> selectorCandidateList1 = new List<ActionSelectorCandidate>();
      List<ActionSelectorCandidate> selectorCandidateList2 = new List<ActionSelectorCandidate>();
      ActionConstraintContext context1 = new ActionConstraintContext();
      context1.Candidates = candidates;
      context1.RouteContext = context;
      for (int index1 = 0; index1 < candidates.Count; ++index1)
      {
        ActionSelectorCandidate candidate = candidates[index1];
        bool flag1 = true;
        bool flag2 = false;
        if (candidate.Constraints != null)
        {
          context1.CurrentCandidate = candidate;
          for (int index2 = 0; index2 < candidate.Constraints.Count; ++index2)
          {
            IActionConstraint constraint = candidate.Constraints[index2];
            int order = constraint.Order;
            nullable = startingOrder1;
            int valueOrDefault = nullable.GetValueOrDefault();
            if ((order == valueOrDefault ? (nullable.HasValue ? 1 : 0) : 0) != 0)
            {
              flag2 = true;
              if (!constraint.Accept(context1))
              {
                flag1 = false;
                this._logger.LogDebug($"ConstraintMismatch( {candidate.Action.DisplayName}, {candidate.Action.Id}, {constraint}");
                break;
              }
            }
          }
        }
        if (flag1 & flag2)
          selectorCandidateList1.Add(candidate);
        else if (flag1)
          selectorCandidateList2.Add(candidate);
      }
      if (selectorCandidateList1.Count > 0)
      {
        IReadOnlyList<ActionSelectorCandidate> actionConstraintsCore = this.EvaluateActionConstraintsCore(context, (IReadOnlyList<ActionSelectorCandidate>) selectorCandidateList1, startingOrder1);
        if (actionConstraintsCore != null && actionConstraintsCore.Count > 0)
          return actionConstraintsCore;
      }
      if (selectorCandidateList2.Count == 0)
        return (IReadOnlyList<ActionSelectorCandidate>) null;
      return this.EvaluateActionConstraintsCore(context, (IReadOnlyList<ActionSelectorCandidate>) selectorCandidateList2, startingOrder1);
    }

    private class Cache
    {
      public Cache(ActionDescriptorCollection actions)
      {
        this.Version = actions.Version;
        this.OrdinalEntries = new Dictionary<string[], List<ActionDescriptor>>((IEqualityComparer<string[]>) DefaultActionSelector.StringArrayComparer.Ordinal);
        this.OrdinalIgnoreCaseEntries = new Dictionary<string[], List<ActionDescriptor>>((IEqualityComparer<string[]>) DefaultActionSelector.StringArrayComparer.OrdinalIgnoreCase);
        HashSet<string> source = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < actions.Items.Count; ++index)
        {
          ActionDescriptor actionDescriptor = actions.Items[index];
          if (actionDescriptor.AttributeRouteInfo == null)
          {
            foreach (KeyValuePair<string, string> routeValue in (IEnumerable<KeyValuePair<string, string>>) actionDescriptor.RouteValues)
              source.Add(routeValue.Key);
          }
        }
        this.RouteKeys = source.ToArray<string>();
        for (int index1 = 0; index1 < actions.Items.Count; ++index1)
        {
          ActionDescriptor actionDescriptor = actions.Items[index1];
          if (actionDescriptor.AttributeRouteInfo == null)
          {
            string[] key = new string[this.RouteKeys.Length];
            for (int index2 = 0; index2 < this.RouteKeys.Length; ++index2)
              actionDescriptor.RouteValues.TryGetValue(this.RouteKeys[index2], out key[index2]);
            List<ActionDescriptor> actionDescriptorList;
            if (!this.OrdinalIgnoreCaseEntries.TryGetValue(key, out actionDescriptorList))
            {
              actionDescriptorList = new List<ActionDescriptor>();
              this.OrdinalIgnoreCaseEntries.Add(key, actionDescriptorList);
            }
            actionDescriptorList.Add(actionDescriptor);
            if (!this.OrdinalEntries.ContainsKey(key))
              this.OrdinalEntries.Add(key, actionDescriptorList);
          }
        }
      }

      public int Version { get; }

      public string[] RouteKeys { get; }

      public Dictionary<string[], List<ActionDescriptor>> OrdinalEntries { get; }

      public Dictionary<string[], List<ActionDescriptor>> OrdinalIgnoreCaseEntries { get; }
    }

    private class StringArrayComparer : IEqualityComparer<string[]>
    {
      public static readonly DefaultActionSelector.StringArrayComparer Ordinal = new DefaultActionSelector.StringArrayComparer(StringComparer.Ordinal);
      public static readonly DefaultActionSelector.StringArrayComparer OrdinalIgnoreCase = new DefaultActionSelector.StringArrayComparer(StringComparer.OrdinalIgnoreCase);
      private readonly StringComparer _valueComparer;

      private StringArrayComparer(StringComparer valueComparer)
      {
        this._valueComparer = valueComparer;
      }

      public bool Equals(string[] x, string[] y)
      {
        if (x == y)
          return true;
        if (x == null ^ y == null || x.Length != y.Length)
          return false;
        for (int index = 0; index < x.Length; ++index)
        {
          if ((!string.IsNullOrEmpty(x[index]) || !string.IsNullOrEmpty(y[index])) && !this._valueComparer.Equals(x[index], y[index]))
            return false;
        }
        return true;
      }

      public int GetHashCode(string[] obj)
      {
        if (obj == null)
          return 0;
        HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
        for (int index = 0; index < obj.Length; ++index)
          hashCodeCombiner.Add<string>(obj[index], (IEqualityComparer<string>) this._valueComparer);
        return hashCodeCombiner.CombinedHash;
      }

        internal struct HashCodeCombiner
        {
            private long _combinedHash64;

            public int CombinedHash
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)] get
                {
                    return this._combinedHash64.GetHashCode();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private HashCodeCombiner(long seed)
            {
                this._combinedHash64 = seed;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(IEnumerable e)
            {
                if (e == null)
                {
                    this.Add(0);
                }
                else
                {
                    int i = 0;
                    foreach (object o in e)
                    {
                        this.Add(o);
                        ++i;
                    }
                    this.Add(i);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator int(HashCodeCombiner self)
            {
                return self.CombinedHash;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(int i)
            {
                this._combinedHash64 = (this._combinedHash64 << 5) + this._combinedHash64 ^ (long) i;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(string s)
            {
                this.Add(s != null ? s.GetHashCode() : 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(object o)
            {
                this.Add(o != null ? o.GetHashCode() : 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add<TValue>(TValue value, IEqualityComparer<TValue> comparer)
            {
                this.Add((object) value != null ? comparer.GetHashCode(value) : 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static HashCodeCombiner Start()
            {
                return new HashCodeCombiner(5381L);
            }
        }

    }
  }
}

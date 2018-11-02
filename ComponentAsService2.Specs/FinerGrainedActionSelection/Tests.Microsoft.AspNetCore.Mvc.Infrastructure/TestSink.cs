 // Cutnpasted from 
 // Type: Microsoft.Extensions.Logging.Testing.TestSink
// Assembly: Microsoft.Extensions.Logging.Testing, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: A61E84DF-D849-4DE9-9021-E179F96DB0BF
// Assembly location: /Users/chris/.nuget/packages/microsoft.extensions.logging.testing/2.2.0-preview3-35359/lib/netstandard2.0/Microsoft.Extensions.Logging.Testing.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Component.As.Service.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure
{
  public class TestLoggerFactory : ILoggerFactory, IDisposable
  {
    private readonly ITestSink _sink;
    private readonly bool _enabled;

    public TestLoggerFactory(ITestSink sink, bool enabled)
    {
      this._sink = sink;
      this._enabled = enabled;
    }

    public ILogger CreateLogger(string name)
    {
      return (ILogger) new TestLogger(name, this._sink, this._enabled);
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }

    public void Dispose()
    {
    }
  }
  
  public interface ITestSink
  {
    Func<WriteContext, bool> WriteEnabled { get; set; }

    Func<BeginScopeContext, bool> BeginEnabled { get; set; }

    IProducerConsumerCollection<BeginScopeContext> Scopes { get; set; }

    IProducerConsumerCollection<WriteContext> Writes { get; set; }

    void Write(WriteContext context);

    void Begin(BeginScopeContext context);
  }
  
  public class TestSink : ITestSink
  {
    private ConcurrentQueue<BeginScopeContext> _scopes;
    private ConcurrentQueue<WriteContext> _writes;

    public TestSink(Func<WriteContext, bool> writeEnabled = null, Func<BeginScopeContext, bool> beginEnabled = null)
    {
      this.WriteEnabled = writeEnabled;
      this.BeginEnabled = beginEnabled;
      this._scopes = new ConcurrentQueue<BeginScopeContext>();
      this._writes = new ConcurrentQueue<WriteContext>();
    }

    public Func<WriteContext, bool> WriteEnabled { get; set; }

    public Func<BeginScopeContext, bool> BeginEnabled { get; set; }

    public IProducerConsumerCollection<BeginScopeContext> Scopes
    {
      get
      {
        return (IProducerConsumerCollection<BeginScopeContext>) this._scopes;
      }
      set
      {
        this._scopes = new ConcurrentQueue<BeginScopeContext>((IEnumerable<BeginScopeContext>) value);
      }
    }

    public IProducerConsumerCollection<WriteContext> Writes
    {
      get
      {
        return (IProducerConsumerCollection<WriteContext>) this._writes;
      }
      set
      {
        this._writes = new ConcurrentQueue<WriteContext>((IEnumerable<WriteContext>) value);
      }
    }

    public void Write(WriteContext context)
    {
      if (this.WriteEnabled != null && !this.WriteEnabled(context))
        return;
      this._writes.Enqueue(context);
    }

    public void Begin(BeginScopeContext context)
    {
      if (this.BeginEnabled != null && !this.BeginEnabled(context))
        return;
      this._scopes.Enqueue(context);
    }

    public static bool EnableWithTypeName<T>(WriteContext context)
    {
      return context.LoggerName.Equals(typeof (T).FullName);
    }

    public static bool EnableWithTypeName<T>(BeginScopeContext context)
    {
      return context.LoggerName.Equals(typeof (T).FullName);
    }
  }
}

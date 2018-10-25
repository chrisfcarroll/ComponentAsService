// CutnPaste from nuget/packages/microsoft.extensions.logging.testing
// Type: Microsoft.Extensions.Logging.Testing.TestLogger
// Assembly: Microsoft.Extensions.Logging.Testing, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// MVID: A61E84DF-D849-4DE9-9021-E179F96DB0BF
// Assembly location: /Users/chris/.nuget/packages/microsoft.extensions.logging.testing/2.2.0-preview3-35359/lib/netstandard2.0/Microsoft.Extensions.Logging.Testing.dll

using System;
using Microsoft.Extensions.Logging;

namespace ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure
{
  public class WriteContext
  {
    public LogLevel LogLevel { get; set; }

    public EventId EventId { get; set; }

    public object State { get; set; }

    public Exception Exception { get; set; }

    public Func<object, Exception, string> Formatter { get; set; }

    public object Scope { get; set; }

    public string LoggerName { get; set; }

    public string Message
    {
      get
      {
        return this.Formatter(this.State, this.Exception);
      }
    }
  }
  
  public class BeginScopeContext
  {
    public object Scope { get; set; }

    public string LoggerName { get; set; }
  }
  
  public class TestLogger : ILogger
  {
    private object _scope;
    private readonly ITestSink _sink;
    private readonly string _name;
    private readonly Func<LogLevel, bool> _filter;

    public TestLogger(string name, ITestSink sink, bool enabled)
      : this(name, sink, (Func<LogLevel, bool>) (_ => enabled))
    {
    }

    public TestLogger(string name, ITestSink sink, Func<LogLevel, bool> filter)
    {
      this._sink = sink;
      this._name = name;
      this._filter = filter;
    }

    public string Name { get; set; }

    public IDisposable BeginScope<TState>(TState state)
    {
      this._scope = (object) state;
      this._sink.Begin(new BeginScopeContext()
      {
        LoggerName = this._name,
        Scope = (object) state
      });
      return (IDisposable) TestLogger.TestDisposable.Instance;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!this.IsEnabled(logLevel))
        return;
      this._sink.Write(new WriteContext()
      {
        LogLevel = logLevel,
        EventId = eventId,
        State = (object) state,
        Exception = exception,
        Formatter = (Func<object, Exception, string>) ((s, e) => formatter((TState) s, e)),
        LoggerName = this._name,
        Scope = this._scope
      });
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      if (logLevel != LogLevel.None)
        return this._filter(logLevel);
      return false;
    }

    private class TestDisposable : IDisposable
    {
      public static readonly TestLogger.TestDisposable Instance = new TestLogger.TestDisposable();

      public void Dispose()
      {
      }
    }
  }
}

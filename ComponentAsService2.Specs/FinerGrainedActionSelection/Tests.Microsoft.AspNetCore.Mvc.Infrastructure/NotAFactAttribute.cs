using System;

namespace ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure
{
    /// <summary>Does not indicate a Fact</summary>
    public class NotAFactAttribute : Attribute
    {
        public NotAFactAttribute(string comment) => Comment = comment;
        public string Comment { get; }
    }
}
using System;

namespace ComponentAsService2.Specs.FinerGrainedActionSelection.Tests.Microsoft.AspNetCore.Mvc.Infrastructure
{
    /// <summary>Would like to be a Fact but isn't yet</summary>
    public class NotYetAFactAttribute : Attribute
    {
        public NotYetAFactAttribute(string comment="WIP") => Comment = comment;
        public string Comment { get; }
    }

    /// <summary>Would like to be a Theory but isn't yet</summary>
    public class NotYetATheoryAttribute : Attribute
    {
        public NotYetATheoryAttribute(string comment) => Comment = comment;
        public string Comment { get; }
    }

    /// <summary>Does not indicate a Fact at all.</summary>
    public class NotAFactAttribute : Attribute
    {
        public NotAFactAttribute(string comment) => Comment = comment;
        public string Comment { get; }
    }

}
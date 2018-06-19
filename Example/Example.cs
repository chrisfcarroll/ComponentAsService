using System;
using System.Collections.Generic;
using System.Linq;

namespace Example
{
    public interface IExemplify
    {
        List<int> GetSomeNumbers(string name);
    }
    
    public class ExampleImp : IExemplify
    {
        public List<int> GetSomeNumbers(string name) { return name.Select(c => (int) c).ToList(); }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace ExampleComponent
{
    public interface IExemplify
    {
        List<int> StringToNumbers(string @string);
    }
    
    public class ExampleImp : IExemplify
    {
        public List<int> StringToNumbers(string @string) { return @string.Select(c => (int) c).ToList(); }
    }
}

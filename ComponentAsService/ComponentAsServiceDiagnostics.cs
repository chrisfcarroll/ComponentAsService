using System.Linq;
using System.Reflection;
using Component.As.Service.Pieces;

namespace Component.As.Service
{
    /// <summary>
    /// A class that can be used to help diagnose how querystring and form values
    /// are processed by the framework.
    /// </summary>
    public class ComponentAsServiceDiagnostics
    {
        public string Index() 
            => string.Join(
                 "\r\n",
                 typeof(ComponentAsServiceDiagnostics)
                    .GetMethods(BindingFlags.Instance |BindingFlags.Public)
                    .Select(
                            m=> (m.ReturnParameter?.ParameterType.Name) + " " + m.Name 
                              + "(" + 
                                string.Join(",", 
                                            m.GetParameters()
                                             .Select(p=>p.ParameterType.Name + " " + p.Name))
                              + ")"));
        public bool   And(bool a, bool b) => a && b;
        public int    Add(int a, int b) => a +b;
        public int    Add(int a, int b, int c) => a +b +c;
        public float  Add(float a, float b, float c) => a +b +c;
        public string Add(string a, string b) => a +b;

        public string BindComplexObject(int a, DiagnosticModel diagnostic) => $"{diagnostic.ToJson()} {a}";

        public string BindComplexObjects<T1,T2>(T1 diagnosticA, T2 diagnosticB) => $"{diagnosticA.ToJson()} \r\n{diagnosticB.ToJson()}";
    }

    /// <summary>
    /// A class that can be used by <see cref="ComponentAsServiceDiagnostics.BindComplexObject"/> to
    /// help diagnose how querystring and form values are processed by the framework.
    /// </summary>
    public class DiagnosticModel
    {
        public string A { get; set; }
        public float B { get; set; }
        public DiagnosticModel Recurse { get; set; }
    }
}

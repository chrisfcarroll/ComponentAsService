using Component.As.Service.UseComponentAsService;

namespace Component.As.Service
{
    public class ComponentAsServiceDiagnostics
    {
        public bool   And(bool a, bool b) => a && b;
        public int    Add(int a, int b) => a +b;
        public int    Add(int a, int b, int c) => a +b +c;
        public float  Add(float a, float b, float c) => a +b +c;
        public string Add(string a, string b) => a +b;

        public string BindComplexObject(int a, DiagnosticModel diagnostic) => $"{diagnostic.ToJson()} {a}";
    }

    public class DiagnosticModel
    {
        public string A { get; set; }
        public float B { get; set; }
        public DiagnosticModel Recurse { get; set; }
    }
}

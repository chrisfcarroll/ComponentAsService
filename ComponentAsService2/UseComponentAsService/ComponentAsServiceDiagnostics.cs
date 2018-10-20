namespace ComponentAsService2.UseComponentAsService
{
    public class ComponentAsServiceDiagnostics
    {
        public int Add(int a, int b, int c) => a +b +c;
        public string BindComplexObject(int a, DiagnosticModel diagnostic) => $"{diagnostic.ToJson()} {a}";
    }

    public class DiagnosticModel
    {
        public string A { get; set; }
        public float B { get; set; }
        public DiagnosticModel Recurse { get; set; }
    }
}

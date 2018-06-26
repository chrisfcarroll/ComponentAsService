using System.Text;
using Xunit.Abstractions;

namespace microServeIt.Tests
{
    public static class XUnitExtensions
    {
        public static void WriteLine(this ITestOutputHelper console, StringBuilder stringBuilder){ console.WriteLine(stringBuilder.ToString());}

        public static void QuoteLine(this ITestOutputHelper console, string lines, string quoteLine="–--")
        {
            console.WriteLine(new StringBuilder(quoteLine).AppendLine().AppendLine(lines).AppendLine(quoteLine));
        }
    }
}

using System;
using System.IO;

namespace CNUnit.Tools
{
    public static class Constants
    {
        public const string JUnitXslt = "https://raw.githubusercontent.com/nunit/nunit-transforms/master/nunit3-junit/nunit3-junit.xslt";

        public static readonly string OutDirDefault = Path.Combine(Environment.CurrentDirectory, "cnunit-reports");
        public static readonly string JUnitXsltFile = Path.Combine(Environment.CurrentDirectory, "nunit3-junit.xslt");
    }
}

using System.IO;
using CNUnit.Test.Utils;
using NUnit.Framework;

namespace CNUnit.Test.Tests
{
    [TestFixture]
    public class WhereTests : BaseTest
    {
        [Test, TestCaseSource(typeof(DataClass), nameof(DataClass.WhereValidation))]
        public void WhereTest(string where, int thread, int files)
        {
            var d = D;
            d.Add("--where", where);
            d.Add("--tlGenerate", null);
            d.Add("--workers", thread);
            Execute(d);
            Assert.AreEqual(files, new DirectoryInfo(Dir).GetFiles().Length, ArgsBuilder(d));
        }
    }
}
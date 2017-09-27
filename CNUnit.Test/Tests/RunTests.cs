using System.IO;
using CNUnit.Test.Utils;
using NUnit.Framework;

namespace CNUnit.Test.Tests
{
    [TestFixture]
    public class RunTests : BaseTest
    {
        [Test, TestCaseSource(typeof(DataClass), nameof(DataClass.ThreadRunValidation))]
        public void CheckOutFiles(int thread, int files)
        {
            var d = D;
            d.Add("--workers", thread);
            d.Add("--tlKeep", null);
            Execute(d);
            Assert.AreEqual(2 * thread, new DirectoryInfo(Dir).GetFiles().Length, ArgsBuilder(d));
        }
    }
}
using System.IO;
using CNUnit.Test.Utils;
using NUnit.Framework;

namespace CNUnit.Test.Tests
{
    [TestFixture]
    public class GenerateTests : BaseTest
    {
        [Test, TestCaseSource(typeof(DataClass), nameof(DataClass.ThreadNotRunValidation))]
        public void ValidateGenerationFeature(object thread, int files)
        {
            var d = D;
            d.Add("--tlGenerate", null);
            d.Add("--workers", thread);
            d.Add("-s", null);
            Execute(d);
            Assert.AreEqual(files, new DirectoryInfo(Dir).GetFiles().Length, ArgsBuilder(d));
        }
    }

}
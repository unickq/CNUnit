using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;

namespace DebugTests
{
    [TestFixture]
    public class Examples
    {
        [Test, TestCaseSource(typeof(MyDataClass), "TestCases")]
        [Category("Debug")]
        public void T1(int n, int d)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"I'm test {TestContext.CurrentContext.Test.FullName}");
            Thread.Sleep(3000);
            Console.WriteLine("DONE");
        }

        [Test, TestCaseSource(typeof(MyDataClass), "TestCases")]
        [Category("Release")]
        public void T2(int n, int d)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"I'm test {TestContext.CurrentContext.Test.FullName}");
            Thread.Sleep(3000);
            Console.WriteLine("DONE");
        }

        [Category("Release")]
        [Test]
        public void FailedTest()
        {
            Assert.Fail("I'm failded test");
        }

        [Category("Release")]
        [Test]
        public void IgnoredTest()
        {
            Assert.Ignore("I'm ignored");
        }
    }

    public class MyDataClass
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(12, 3);
                yield return new TestCaseData(12, 2);
                yield return new TestCaseData(12, 4);
                yield return new TestCaseData(12, 1);
                yield return new TestCaseData(0, 11);
                yield return new TestCaseData(12, 5);
                yield return new TestCaseData(43, 5);
                yield return new TestCaseData(43, 2);
                yield return new TestCaseData(3, 5);
            }
        }
    }
}
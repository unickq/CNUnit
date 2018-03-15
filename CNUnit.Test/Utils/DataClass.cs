using System.Collections;
using NUnit.Framework;

namespace CNUnit.Test.Utils
{
    public class DataClass
    {
        public static IEnumerable ThreadNotRunValidation
        {
            get
            {
                yield return new TestCaseData(1, 1);
                yield return new TestCaseData(-1, 1);
                yield return new TestCaseData(0, 1);
                yield return new TestCaseData(20, 20);
                yield return new TestCaseData(32000, 20);
            }
        }

        public static IEnumerable ThreadRunValidation
        {
            get
            {
                yield return new TestCaseData(4, 8);
                yield return new TestCaseData(5, 10);
                yield return new TestCaseData(2, 4);
            }
        }

        public static IEnumerable WhereValidation
        {
            get
            {
                yield return new TestCaseData("\"test =~ T1\"", 1, 1);
                yield return new TestCaseData("\"test =~ T1\"", 2, 2);          
                yield return new TestCaseData("\"test =~ T1\"", 9, 9);             
                yield return new TestCaseData("\"test =~ T1\"", 10, 9);             
                yield return new TestCaseData("\"test =~ Fail\"", 2, 1);             
                yield return new TestCaseData("\"test =~ F\"", 1, 1);             
                yield return new TestCaseData("\"test =~ F or test =~ I\"", 2, 2);             
                yield return new TestCaseData("\"test =~ F or test =~ I\"", 1, 1);             
            }
        }

    }
}
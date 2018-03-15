//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using NUnit.Framework;
//
//namespace CNUnit.Test.Tests
//{
//    class FixTheFRegex
//    {
//        public string rStr = "^(?=.*[\\w])(?=.*?[\\d])(.*)$";
////        public string rStr = "^(?=.*[A-Z])(?=.*[a-z])(?=.*?[0-9])(.*)$";
//
//
//        [TestCaseSource(nameof(Passwords))]
//        public void Test(string pass, bool result)
//        {
//            var regex = new Regex(rStr);
//            Assert.AreEqual(result, regex.IsMatch(pass));
//        }
//
//        static object[] Passwords =
//        {
//            new object[] { "qweRTY123", true },
//            new object[] { "qWe1", true },
//            new object[] { "qwe1", false },
//            new object[] { "QWE1", false },
//            new object[] { "qwertyQ123$", true },
//            new object[] { "qwertyQ123!", true },
//            new object[] { "qwertyQ123@", true },
//            new object[] { "qwertyQ123#", true },
//            new object[] { "qwertyQ123%", true },
//            new object[] { "qwertyQ123^", true },
//            new object[] { "qwertyQ123&", true },
//            new object[] { "qwertyQ123*", true },
//            new object[] { "qwertyQ123)", true },
//            new object[] { "qwertyQ123(", true },
//            new object[] { "qwertyQ123-", true },
//            new object[] { "qwertyQ123_", true },
//            new object[] { "qwertyQ123+", true },
//            new object[] { "qwertyQ123=", true },
//            new object[] { "qwertyQ123{", true },
//            new object[] { "qwertyQ123}", true },
//            new object[] { "qwertyQ123[", true },
//            new object[] { "qwertyQ123]", true },
//            new object[] { "qwertyQ123 ", true },
//        };
//    }
//}

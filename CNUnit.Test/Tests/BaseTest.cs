using System;
using System.Collections.Generic;
using System.IO;
using CNUnit.Test.Utils;
using NUnit.Framework;

namespace CNUnit.Test.Tests
{
    public class BaseTest : TestUtils
    {
        protected string Dir;


        protected Dictionary<string, object> D;

        [SetUp]
        public void SetUp()
        {     
            Dir = GetTemporaryDirectory();
            Console.WriteLine(Dir);
            D = new Dictionary<string, object>
            {
                {"-e", NUnitConsole},
                {"-t", Tests},
                {"--outdir", Dir}
            };
            
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(Dir, true);
        }
    }
}

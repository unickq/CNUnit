using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;

namespace CNUnit.Test.Utils
{
    public class TestUtils
    {
        public static string CNUnit =>
            Path.Combine(TestContext.CurrentContext.TestDirectory.Replace(".Test", ""), "net45\\CNUnit.exe");

        public static string Tests =>
            Path.Combine(TestContext.CurrentContext.TestDirectory.Replace("CNUnit.Test", "DebugTests"),
                "DebugTests.dll");

        private string OutDir => Path.Combine(TestContext.CurrentContext.TestDirectory, "OutDir");

        public static string NUnitConsole
        {
            get
            {
                try
                {
                    var version = "NA";
                    var doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,
                        "packages.config")));
                    var xmlAttributeCollection = doc.DocumentElement?.SelectNodes("//*[@id='NUnit.ConsoleRunner']")?[0]
                        .Attributes;
                    if (xmlAttributeCollection != null)
                        version = xmlAttributeCollection["version"].Value;
                    var path = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent?.Parent?.Parent
                        ?.FullName;
                    return Path.Combine(path, "packages", $"NUnit.ConsoleRunner.{version}", "tools",
                        "nunit3-console.exe");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        public static string ArgsBuilder(Dictionary<string, object> dic)
        {
            var sb = new StringBuilder();
            sb.Append(" ");
            foreach (var d in dic)
            {
                sb.Append(d.Key);
                if (d.Value != null)
                {
                    sb.Append(" ");
                    sb.Append(d.Value);
                }

                sb.Append(" ");
            }
            return sb.ToString();
        }

        public void Execute(Dictionary<string, object> args)
        {
            var pr = new Process
            {
                StartInfo =
                {
                    FileName = CNUnit,
                    UseShellExecute = true,
                    Arguments = ArgsBuilder(args)
                }
            };
            pr.Start();
            pr.WaitForExit();
            Thread.Sleep(1000);
        }

        

        public static string GetTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
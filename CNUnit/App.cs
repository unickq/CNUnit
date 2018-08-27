using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CNUnit
{
    public class App
    {
        public const string JUnitXslt =
            "https://raw.githubusercontent.com/nunit/nunit-transforms/master/nunit3-junit/nunit3-junit.xslt";

        private static readonly string OutDirDefault = Path.Combine(Environment.CurrentDirectory, "cnunit-reports");
        public static readonly string JUnitXsltFile = Path.Combine(Environment.CurrentDirectory, "nunit3-junit.xslt");
        private Options Options { get; }
        private readonly List<string> _testLists = new List<string>();

        public App(Options options)
        {
            Options = options;
        }

        public int Run()
        {
            try
            {
                TryToSetNUnitConsole();
                CheckTestsDll();
                SetOutDir();
                GenerateTestLists();
                RunTests();
                RemoveTestLists();
                return 0;
            }
            catch (SystemException e)
            {
                Utils.WriteLine(e.Message, ConsoleColor.Red);
                return 1;
            }
            catch (Exception e)
            {
                e.DebugCNunit(Options.IsDebug, e.Message);
                return -1;
            }
            finally
            {
                Utils.BeginEndPrint();
            }
        }

        private void TryToSetNUnitConsole()
        {
            if (Options.NUnitPath == null)
                Options.NUnitPath = Utils.TryToFindNUnit();
         
            if (!Utils.IsValidNunitConsole(Options.NUnitPath))
                throw new SystemException("Error: Invalid NUnit Console executable");
        }

        /// <summary>
        /// Validates whether Tests DLL file is available
        /// </summary>
        /// <exception cref="SystemException">Error: Tests dll is not found</exception>
        private void CheckTestsDll()
        {
            Console.WriteLine(Options.TestDllPath);
            if (!File.Exists(Options.TestDllPath))
                throw new SystemException("Error: Tests dll is not found");
        }

        /// <summary>
        /// Validates output directory
        /// </summary>
        private void SetOutDir()
        {
            if (Options.OutDir == null)
            {
                Options.OutDir = OutDirDefault;
            }
            else
            {
                try
                {
                    Path.GetDirectoryName(Options.OutDir);
                }
                catch (Exception)
                {
                    Utils.WriteLine("Path from options is invalid. Using default one");
                    Options.OutDir = OutDirDefault;
                }
            }

            if (Directory.Exists(Options.OutDir))
                Directory.Delete(Options.OutDir, true);
            Thread.Sleep(100);
            Directory.CreateDirectory(Options.OutDir);
            Utils.WriteLine($"Working directory: {Options.OutDir}\n", ConsoleColor.Cyan);
        }


        /// <summary>
        /// Generates the test lists from NUnit3 console --explore.
        /// </summary>
        private void GenerateTestLists()
        {
            Utils.WriteLine("\nParsing NUnit tests...\n", ConsoleColor.Green);
            var arguments = $"{Options.NUnitPath} {Options.TestDllPath} --explore:cnunit.cases;format=cases";

            if (!string.IsNullOrEmpty(Options.NUnitWhere))
            {
                arguments += $" --where \"{Options.NUnitWhere}\"";
            }

            if (Options.IsDebug)
            {
                Utils.WriteLine($"\nGenerate test list arguments: {arguments}", ConsoleColor.Blue);
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = Options.NUnitPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = arguments
                }
            };

            process.Start();
            process.WaitForExit();
            var nameFile = Options.TestDllPath.BuildTestName();
            var exploreResult = Path.Combine(Environment.CurrentDirectory, "cnunit.cases");
            var list = File.ReadAllLines(exploreResult).ToList();
            File.Delete(exploreResult);

            if (!string.IsNullOrEmpty(Options.CNUnitWhere))
            {
                var parsedList = new List<string>();
                var parseArray = Options.CNUnitWhere.Split(';');
                foreach (var parsing in parseArray)
                {
                    parsedList.AddRange(list.Where(v => v.Contains(parsing)));
                }

                list = parsedList;
            }

            if (Options.IsShuffle) list.Shuffle();

            Utils.WriteLine($"Total tests count: {list.Count}", ConsoleColor.Cyan);
            Utils.WriteLine($"\nDividing tests by {Options.Workers} and saving to to lists:", ConsoleColor.Cyan);
            int i = 0;
            foreach (var listPart in list.Split(Options.Workers))
            {
                var fileTestBuilder = new StringBuilder();
                var listPartElements = listPart as IList<string> ?? listPart.ToList();
                foreach (var listPartElement in listPartElements)
                {
                    fileTestBuilder.AppendLine(listPartElement);
                }

                var outPut = fileTestBuilder.ToString().TrimEnd('\n', '\r');
                var path = Path.Combine(Options.OutDir, $"{nameFile}{++i}.txt");
                _testLists.Add(path);
                Utils.WriteLine($"Saved {listPartElements.Count()} tests into {path}", ConsoleColor.Green);
                File.WriteAllText(path, outPut);
            }
        }


        /// <summary>
        /// Launch NUnit console instances and runs the tests.
        /// </summary>
        private void RunTests()
        {
            if (Options.IsGenerateOnly) return;
            var xmlList = new List<string>();
            var allProcesses = Task.Factory.StartNew(() =>
            {
                Utils.WriteLine("\nExecuting tests...", ConsoleColor.Cyan);
                var processes = new List<Process>();
                foreach (var test in _testLists)
                {
                    var pr = new Process
                    {
                        StartInfo =
                        {
                            FileName = Options.NUnitPath,
                            Arguments = BuildNUnitTestListArguments(test)
                        }
                    };

                    pr.StartInfo.UseShellExecute = Options.IsNUnitWait;
                    pr.Start();
                    if (Options.IsDebug)
                        Utils.WriteLine("CNUnit: Run tests params " + pr.StartInfo.Arguments, ConsoleColor.Blue);
                    processes.Add(pr);
                }

                foreach (var pr in processes)
                {
                    pr.WaitForExit();
                    pr.Dispose();
                }

                foreach (var test in _testLists)
                {
                    var path = Path.Combine(Options.OutDir, test.BuildTestName() + ".xml");
                    xmlList.Add(path);
                    Utils.WriteLine($"Test report saved - {path}", ConsoleColor.Green);
                }
            });
            allProcesses.Wait();
            Utils.WriteLine("Test execution finished", ConsoleColor.Cyan);
            NUnitXmlUpdate(xmlList);
        }

        /// <summary>
        /// Builds NUnit console run parameters.
        /// </summary>
        /// <returns></returns>
        private string BuildNUnitTestListArguments(string test)
        {
            var sb = new StringBuilder();
            sb.Append(Options.TestDllPath);
            sb.Append($" --testlist=\"{test}\"");

            switch (Options.OutputFortmat)
            {
                case ReportType.NUnit2:
                    sb.Append($" --result=\"{test.BuildTestName()}.xml;format=nunit2\" ");
                    break;
                case ReportType.JUnit:
                    if (!File.Exists(JUnitXsltFile))
                        Utils.DownloadJUnitTransform();
                    sb.Append(
                        $" --result=\"{test.BuildTestName()}.xml\";transform=\"{JUnitXsltFile}\" ");
                    break;
                default:
                    sb.Append($" --result=\"{test.BuildTestName()}.xml\" ");
                    break;
            }

            if (!string.IsNullOrEmpty(Options.OutDir)) sb.Append($" --work=\"{Options.OutDir}\"").Append(" ");
            if (Options.IsNUnitWait) sb.Append(" --wait ");
            return sb.ToString();
        }


        /// <summary>
        /// Remoevs tests list files from disk.
        /// </summary>
        private void RemoveTestLists()
        {
            if (Options.IsGenerateOnly) return;
            if (Options.IsKeepTestCases) return;
            if (Options.IsDebug)
                Utils.WriteLine("\nRemoving test lists:", ConsoleColor.Blue);
            foreach (var testList in _testLists)
            {
                File.Delete(testList);
                if (Options.IsDebug)
                    Utils.WriteLine($"Removed {testList}", ConsoleColor.DarkBlue);
            }
        }

        private void NUnitXmlUpdate(IEnumerable<string> xmls)
        {
            try
            {
                foreach (var xml in xmls)
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(xml));
                    if (Options.OutputFortmat == ReportType.JUnit)
                    {
                        var testSuiteNodeList = doc.DocumentElement?.SelectNodes("//testsuite");
                        if (testSuiteNodeList != null)
                            foreach (XmlNode testSuiteNode in testSuiteNodeList)
                            {
                                if (testSuiteNode.Attributes != null)
                                    testSuiteNode.Attributes["name"].Value =
                                        Path.GetFileNameWithoutExtension(xml) + "." + Path.GetRandomFileName();
                            }
                    }
                    else
                    {
                        var attributeCollection = doc.DocumentElement?.SelectNodes("//test-suite")?[0].Attributes;
                        if (attributeCollection != null)
                        {
                            var xmlAttributeCollection = attributeCollection?["name"];
                            xmlAttributeCollection.Value = Path.GetFileNameWithoutExtension(xml);
                        }
                    }

                    doc.Save(xml);
                }
            }
            catch (Exception e)
            {
                e.DebugCNunit(Options.IsDebug, "Unable to parse NUnit xml results");
            }
        }
    }
}
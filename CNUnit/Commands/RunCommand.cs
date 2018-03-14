using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CNUnit.Tools;

// ReSharper disable InconsistentNaming

namespace CNUnit.Commands
{
    public class RunCommand
    {
        private const int Success = 0;
        private const int Error = 1;
        private const int Failure = -1;

        private readonly List<string> _testLists = new List<string>();

        public bool CNUnit_Debug;
        public string CNUnit_Outdir = Constants.OutDirDefault;
        public ReportType CNUnit_ReportType = ReportType.NUnit3;
        public string CNUnit_Parse_Rules;

        public string Tests_Dll_Path;
        public bool Tests_Skip;
        public bool Tests_Keep_Cases;
        public bool Tests_Shuffle;

        public string NUnit_Where;
        public string NUnit_Executable = Utils.TryToFindNUnit();
        public int NUnit_Workers = 1;
        public bool NUnit_Wait;
        public bool NUnit_No_Output;


       


        public int Run(string[] remainingArguments)
        {
            try
            {
                if (!Utils.IsValidNunitConsole(NUnit_Executable))
                {
                    Utils.WriteLine("Error: Invalid NUnit Console executable", ConsoleColor.Red);
                    return Error;
                }

                if (!File.Exists(Tests_Dll_Path))
                {
                    Utils.WriteLine("Error: Test dll is not found", ConsoleColor.Red);
                    return Error;
                }

                Utils.OutDirSetup(CNUnit_Outdir);

                if (NUnit_Wait) NUnit_No_Output = true;

                GenerateTestLists();

                if (!Tests_Skip)
                {
                    RunTests();
                    if (!Tests_Keep_Cases) RemoveTestLists();
                }

                return Success;
            }
            catch (Exception e)
            {
                e.DebugCNunit(CNUnit_Debug, e.Message);
                return Failure;
            }
            finally
            {
                Utils.BeginEndPrint();
            }
        }


        /// <summary>
        /// Generates the test lists from NUnit3 console --explore.
        /// </summary>
        private void GenerateTestLists()
        {
            Utils.WriteLine("\nParsing NUnit tests...\n", ConsoleColor.Green);
            var arguments = $"{NUnit_Executable} {Tests_Dll_Path} --explore:cnunit.cases;format=cases";

            if (!string.IsNullOrEmpty(NUnit_Where))
            {
                arguments += $" --where \"{NUnit_Where}\"";
            }

            if (CNUnit_Debug)
            {
                Utils.WriteLine($"\nGenerate test list arguments: {arguments}", ConsoleColor.Blue);
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = NUnit_Executable,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = arguments
                }
            };

            process.Start();
            process.WaitForExit();
            var nameFile = Tests_Dll_Path.BuildTestName();
            var exploreResult = Path.Combine(Environment.CurrentDirectory, "cnunit.cases");
            var list = File.ReadAllLines(exploreResult).ToList();
            File.Delete(exploreResult);

            if (!string.IsNullOrEmpty(CNUnit_Parse_Rules))
            {
                var parsedList = new List<string>();
                var parseArray = CNUnit_Parse_Rules.Split(';');
                foreach (var parsing in parseArray)
                {
                    parsedList.AddRange(list.Where(v => v.Contains(parsing)));
                }
                list = parsedList;
            }

            if (Tests_Shuffle) list.Shuffle();

            Utils.WriteLine($"Total tests count: {list.Count}", ConsoleColor.Cyan);
            Utils.WriteLine($"\nDividing tests by {NUnit_Workers} and saving to to lists:", ConsoleColor.Cyan);
            int i = 0;
            foreach (var listPart in list.Split(NUnit_Workers))
            {
                var fileTestBuilder = new StringBuilder();
                var listPartElements = listPart as IList<string> ?? listPart.ToList();
                foreach (var listPartElement in listPartElements)
                {
                    fileTestBuilder.AppendLine(listPartElement);
                }
                var outPut = fileTestBuilder.ToString().TrimEnd('\n', '\r');
                var path = Path.Combine(CNUnit_Outdir, $"{nameFile}{++i}.txt");
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
            List<string> xmlList = new List<string>();
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
                            FileName = NUnit_Executable,
                            Arguments = BuildNUnitTestListArguments(test)
                        }
                    };

                    pr.StartInfo.UseShellExecute = NUnit_No_Output;
                    pr.Start();
                    if (CNUnit_Debug) Utils.WriteLine("CNUnit: Run tests params " + pr.StartInfo.Arguments, ConsoleColor.Blue);
                    processes.Add(pr);
                }
                foreach (var pr in processes)
                {
                    pr.WaitForExit();
                    pr.Dispose();
                }
                foreach (var test in _testLists)
                {
                    var path = Path.Combine(CNUnit_Outdir, test.BuildTestName() + ".xml");
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
            sb.Append(Tests_Dll_Path);
            sb.Append($" --testlist=\"{test}\"");

            switch (CNUnit_ReportType)
            {
                case ReportType.Nunit2:
                    sb.Append($" --result=\"{test.BuildTestName()}.xml;format=nunit2\" ");
                    break;
                case ReportType.JUnit:
                    if (!File.Exists(Constants.JUnitXsltFile))
                        Utils.DownloadJUnitTransform();
                    sb.Append(
                        $" --result=\"{test.BuildTestName()}.xml\";transform=\"{Constants.JUnitXsltFile}\" ");
                    break;
                default:
                    sb.Append($" --result=\"{test.BuildTestName()}.xml\" ");
                    break;
            }
            if (!string.IsNullOrEmpty(CNUnit_Outdir)) sb.Append($" --work=\"{CNUnit_Outdir}\"").Append(" ");
            if (NUnit_Wait) sb.Append(" --wait ");        
            return sb.ToString();
        }


        /// <summary>
        /// Remoevs tests list files from disk.
        /// </summary>
        private void RemoveTestLists()
        {
            if (CNUnit_Debug)
                Utils.WriteLine("\nRemoving test lists:", ConsoleColor.Blue);
            foreach (var testList in _testLists)
            {
                File.Delete(testList);
                if (CNUnit_Debug)
                    Utils.WriteLine($"Removed {testList}", ConsoleColor.DarkBlue);
            }
        }

        private void NUnitXmlUpdate(List<string> xmls)
        {
            try
            {
                foreach (var xml in xmls)
                {
                    Console.WriteLine("!!!!");
                    var doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(xml));
                    if (CNUnit_ReportType == ReportType.JUnit)
                    {
                        var testSuiteNodeList = doc.DocumentElement?.SelectNodes("//testsuite");
                        if (testSuiteNodeList != null)
                            foreach (XmlNode testSuiteNode in testSuiteNodeList)
                            {
                                if (testSuiteNode.Attributes != null)
                                    testSuiteNode.Attributes["name"].Value =
                                        Path.GetFileNameWithoutExtension(xml)+ "."+ Path.GetRandomFileName();
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
                e.DebugCNunit(CNUnit_Debug, "Unable to parse NUnit xml results");
            }    
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNUnit.Tools;
using ManyConsole;

// ReSharper disable InconsistentNaming

namespace CNUnit.Commands
{
    public class RunCommand : ConsoleCommand
    {
        private const int Success = 0;
        private const int Error = 1;
        private const int Failure = -1;

        private readonly List<string> _testLists = new List<string>();

        public bool CNUnit_Debug;
        public string CNUnit_Outdir;
        public ReportType CNUnit_ReportType = ReportType.NUnit3;
        public string CNUnit_Parse_Rules;

        public string Tests_Dll_Path;
        public bool Tests_Skip;
        public bool Tests_Keep_Cases;
        public bool Tests_Shuffle;

        public string NUnit_Where;
        public string NUnit_Executable = Utils.TryToFindNUnit();
        public string NUnit_Workers = "1";
        public bool NUnit_Wait;
        public bool NUnit_No_Output;


        public RunCommand()
        {
            IsCommand(".");

            HasLongDescription("CNUnit - tool to generate testlists, run tests and transform results using NUnit console runner.");

            HasOption("e|exe=", "NUnit3-console executable path.", p => NUnit_Executable = p);
            HasRequiredOption("t|dll=", "Dll with NUnit tests.", v => Tests_Dll_Path = v);           
            HasOption("w|workers=", "Thread count for tests execution.", v => NUnit_Workers = v);
            HasOption("s|shuffle", "Shuffle tests in test list.", v => Tests_Shuffle = v != null);
            HasOption("q|quite", "Hide NUnit console output.", v => NUnit_No_Output = v != null);
            HasOption("f|format=", "Output xml format. junit, nunit2, nunit3 - by default.", v => CNUnit_ReportType = Utils.GetReportType(v));
            HasOption("where=",
                "NUnit selection EXPRESSION indicating what tests will be run.\nSee https://github.com/nunit/docs/wiki/Test-Selection-Language",
                v => NUnit_Where = v);
            HasOption("parse=",
                "Own selection rules. --parse=Chrome;Firefox will find tests containings Chrome and Firefox in test name.",
                v => CNUnit_Parse_Rules = v);
            HasOption("outdir=",
                "Path of the directory to use for output files. If  not specified, defaults to the current directory.",
                v => CNUnit_Outdir = v);
         
            HasOption("tlGenerate", "Generate test lists without execution.", v => Tests_Skip = v != null);
            HasOption("tlKeep", "Keep test lists after exection.", v => Tests_Keep_Cases = v != null);
            HasOption("debug", "Debug CNUnit output.", v => CNUnit_Debug = v != null);
            HasOption("wait", "NUnit3-console won't be closed after tests finished.", v => NUnit_Wait = v != null);
            
        }


        public override int Run(string[] remainingArguments)
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
                Utils.WriteLine(e.Message, ConsoleColor.Red);
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

            var dividerLists = list.Divide(int.Parse(NUnit_Workers));
            Utils.WriteLine($"Total tests count: {list.Count}", ConsoleColor.Cyan);
            Utils.WriteLine($"\nDividing tests by {NUnit_Workers} and saving to to lists:", ConsoleColor.Cyan);
            for (var i = 0; i < dividerLists.Length; i++)
            {
                var fileTestBuilder = new StringBuilder();
                foreach (var line in dividerLists[i])
                {
                    fileTestBuilder.AppendLine(line);
                }
                var outPut = fileTestBuilder.ToString();
                if (outPut.Length <= 0) continue;
                var path = Path.Combine(CNUnit_Outdir, $"{nameFile}{i}.txt");
                _testLists.Add(path);
                Utils.WriteLine($"Saved {dividerLists[i].Count} tests into {path}", ConsoleColor.Green);
                File.WriteAllText(path, outPut);
            }
        }


        /// <summary>
        /// Launch NUnit console instances and runs the tests.
        /// </summary>
        private void RunTests()
        {
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
                    processes.Add(pr);
                }
                foreach (var pr in processes)
                {
                    pr.WaitForExit();
                    pr.Dispose();
                }
                foreach (var test in _testLists)
                {
                    Utils.WriteLine($"Test report saved - {CNUnit_Outdir}\\{test.BuildTestName()}.xml",
                        ConsoleColor.Green);
                }
            });
            allProcesses.Wait();
            Utils.WriteLine("Test execution finished", ConsoleColor.Cyan);
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
            if (CNUnit_Debug) Utils.WriteLine("!NUnit params: " + sb, ConsoleColor.Blue);
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
    }
}
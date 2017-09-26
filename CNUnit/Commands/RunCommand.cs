using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNUnit.Tools;
using ManyConsole;

namespace CNUnit.Commands
{
    public class RunCommand : ConsoleCommand
    {
        private readonly List<string> _testLists = new List<string>();
        public string NunitPath = Utils.TryToFindNUnit();
        public string Outdir;
        public string TestDllPath;

        public string Threads = "1";

//        public string Where;
        public string NunitOutputParseRules = string.Empty;

        public bool JUnit;
        public bool Nunit2;
        public bool NotRun;
        public bool PrintNunitOutput;
        public bool KeepTestLists;
        public bool Debug;
        public bool Shuffle;
        public bool WaitNunit;

        public RunCommand()
        {
            IsCommand("run", "Generate test lists and run tests");

            HasOption("e|exe=", "NUnit3-console executable path.", v => NunitPath = v);
            HasOption("w|threads=", "Thread count for tests execution.", v => Threads = v);
            HasOption("t|dll=", "Dll with NUnit tests", v => TestDllPath = v);
//            HasOption("w|where=", "Test selection EXPRESSION indicating what tests will be run. See NUnit3 docs", v => Where = v);
            HasOption("parse=",
                "Tests parsing rule. --parse=Chrome;Firefox will find tests containings Chrome and Firefox in test name.",
                v => NunitOutputParseRules = v);
            HasOption("o|outdir=",
                "Path of the directory to use for output files. If  not specified, defaults to the current directory.",
                v => Outdir = v);
            HasOption("s|shuffle", "Shuffle tests before execution.", v => Shuffle = v != null);
            HasOption("nunit2", "NUnit 2 test output format.", v => Nunit2 = v != null);
            HasOption("junit", "JUnit test output format. Using JUnit xlst file.", v => JUnit = v != null);
            HasOption("tlCreate", "Generate test lists without execution.", v => NotRun = v != null);
            HasOption("tlKeep", "Keep test lists after exection.", v => KeepTestLists = v == null);
            HasOption("print", "Print test output after tests execution.", v => PrintNunitOutput = v != null);
            HasOption("debug", "Debug CNUnit output.", v => Debug = v != null);
            HasOption("wait", "NUnit3-console won't be closed after tests finished.", v => WaitNunit = v != null);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                OutDirSetup();
                if (!IsFileExist(NunitPath, "NUnit executable"))
                {
                    Utils.WriteLine("NUnit executable is not set", ConsoleColor.Red);
                    return 1;
                }
                if (!IsFileExist(TestDllPath, "DLL with NUnit tests"))
                {
                    Utils.WriteLine("DLL with NUnit tests is not set", ConsoleColor.Red);
                    return 1;
                }
                if (JUnit & Nunit2)
                {
                    Utils.WriteLine("Only one output format is alowed. Please use NUnit2 or JUnit only",
                        ConsoleColor.Red);
                    return 1;
                }

                SaveFiles();
                if (!NotRun) RunTests();
                if (!KeepTestLists) RemoveTestLists();
                return 0;
            }
            catch (Exception e)
            {
                Utils.WriteLine(e.Message, ConsoleColor.Red);
                return -1;
            }
            finally
            {
                Utils.BeginEndPrint();
            }
        }

        private void SaveFiles()
        {
            Utils.WriteLine("\nParsing NUnit tests...\n", ConsoleColor.Green);
            var process = new Process
            {
                StartInfo =
                {
                    FileName = NunitPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"{NunitPath} {TestDllPath} --explore:cnunit.cases;format=cases"
                }
            };
            process.Start();
            process.WaitForExit();
            var nameFile = Path.GetFileNameWithoutExtension(TestDllPath);
            var exploreResult = Path.Combine(Environment.CurrentDirectory, "cnunit.cases");
            var list = File.ReadAllLines(exploreResult).ToList();
            File.Delete(exploreResult);

            if (!string.IsNullOrEmpty(NunitOutputParseRules))
            {
                var parsedList = new List<string>();
                var parseArray = NunitOutputParseRules.Split(';');
                foreach (var parsing in parseArray)
                {
                    parsedList.AddRange(list.Where(v => v.Contains(parsing)));
                }
                list = parsedList;
            }

            if (Shuffle) list.Shuffle();

            var dividerLists = list.Divide(int.Parse(Threads));
            Utils.WriteLine($"Total tests count: {list.Count}", ConsoleColor.Cyan);
            Utils.WriteLine($"\nDividing tests by {Threads} and saving to to lists:", ConsoleColor.Cyan);
            for (var i = 0; i < dividerLists.Length; i++)
            {
                var fileTestBuilder = new StringBuilder();
                foreach (var line in dividerLists[i])
                {
                    fileTestBuilder.AppendLine(line);
                }
                var outPut = fileTestBuilder.ToString();
                if (outPut.Length <= 0) continue;
                var path = Path.Combine(Outdir, $"{nameFile}{i}.txt");
                _testLists.Add(path);
                Utils.WriteLine($"Saved {dividerLists[i].Count} tests into {path}", ConsoleColor.Green);
                File.WriteAllText(path, outPut);
            }
        }


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
                            FileName = NunitPath,
                            Arguments = BuildNUnitTestListArguments(test)
                        }
                    };

                    if (PrintNunitOutput)
                    {
                        pr.StartInfo.RedirectStandardInput = true;
                        pr.StartInfo.UseShellExecute = false;
                    }
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
                    Utils.WriteLine(
                        $"Test report saved - {Outdir}\\{Path.GetFileNameWithoutExtension(test)}.xml",
                        ConsoleColor.Green);
                }
            });
            allProcesses.Wait();
            Utils.WriteLine("Test execution finished", ConsoleColor.Cyan);
        }

        private string BuildNUnitTestListArguments(string test)
        {
            var sb = new StringBuilder();
            sb.Append(TestDllPath);
            sb.Append($" --testlist=\"{test}\"");
            if (Nunit2)
            {
                sb.Append($" --result=\"{Path.GetFileNameWithoutExtension(test)}.xml;format=nunit2\" ");
            }
            else if (JUnit)
            {
                if (!File.Exists(Constants.JUnitXsltFile))
                {
                    Utils.DownloadJUnitTransform();
                    sb.Append(
                        $" --result=\"{Path.GetFileNameWithoutExtension(test)}.xml\";transform=\"{Constants.JUnitXsltFile}\" ");
                }
            }
            else
            {
                sb.Append($" --result=\"{Path.GetFileNameWithoutExtension(test)}.xml\" ");
            }
            if (!string.IsNullOrEmpty(Outdir)) sb.Append($" --work=\"{Outdir}\"").Append(" ");
            if (WaitNunit) sb.Append(" --wait ");
            if (Debug) Utils.WriteLine("!NUnit params: " + sb, ConsoleColor.Blue);
            return sb.ToString();
        }

        private void OutDirSetup()
        {
            if (string.IsNullOrWhiteSpace(Outdir))
                Outdir = Constants.OutDirDefault;

            try
            {
                if (Directory.Exists(Outdir))
                {
                    Directory.Delete(Outdir, true);
                }
                Directory.CreateDirectory(Outdir);
            }
            catch (IOException ioex)
            {
                Utils.WriteLine(ioex.Message, ConsoleColor.Red);
            }

            Utils.WriteLine($"Working directory: {Outdir}\n", ConsoleColor.Cyan);
        }

        private void RemoveTestLists()
        {
            if (Debug)
                Utils.WriteLine("\nRemoving test lists:", ConsoleColor.Blue);
            foreach (var testList in _testLists)
            {
                File.Delete(testList);
                if (Debug)
                    Utils.WriteLine($"Removed {testList}", ConsoleColor.DarkBlue);
            }
        }

        private static bool IsFileExist(string parameter, string message)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return false;
            if (File.Exists(parameter)) return true;
            Utils.WriteLine($"Unable to find {message}", ConsoleColor.Red);
            return false;
        }
    }
}
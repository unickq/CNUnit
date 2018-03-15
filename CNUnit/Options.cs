using CommandLine;

namespace CNUnit
{
    public class Options
    {
        [Option('e',"exe", HelpText = "NUnit3-console executable path")]
        public string NUnitPath { get; set; }

        [Option('t',"dll", HelpText = "NUnit test dll", Required = true)]
        public string TestDllPath { get; set; }

        [Option('w',"workers", Default = 1, HelpText = "Thread count for tests execution")]
        public int Workers { get; set; }

        [Option('s',"shuffle", Default = false, HelpText = "Shuffle tests in test list")]
        public bool IsShuffle { get; set; }

//        [Option('q',"quite", Default = false, HelpText = "Hide NUnit console output")]
//        public bool IsQuite { get; set; }
    
        [Option('f',"format", Default = ReportType.NUnit3, HelpText =  "Output xml format. junit, nunit2, nunit3 - by default")]
        public ReportType OutputFortmat { get; set; }

        [Option("where", HelpText = "NUnit selection EXPRESSION indicating what tests will be run.\nSee https://github.com/nunit/docs/wiki/Test-Selection-Language")]
        public string NUnitWhere { get; set; }

        [Option("parse", HelpText = "Own selection rules. --parse=Chrome;Firefox will find tests containings Chrome and Firefox in test name")]
        public string CNUnitWhere { get; set; }

        [Option("outdir", HelpText = "Path of the directory to use for output files. If  not specified, defaults to the current directory")]
        public string OutDir { get; set; }

        [Option("tlGenerate", Default = false, HelpText = "Generate test lists without execution")]
        public bool IsGenerateOnly { get; set; }

        [Option("tlKeep", Default = false, HelpText = "Keep test lists after exection")]
        public bool IsKeepTestCases { get; set; }

        [Option("debug", Default = false, HelpText = "Debug CNUnit output")]
        public bool IsDebug { get; set; }

        [Option("wait", Default = false, HelpText = "NUnit3-console won't be closed after tests finished")]
        public bool IsNUnitWait { get; set; }
    }
}
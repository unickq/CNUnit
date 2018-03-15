using System;
using CommandLine;

namespace CNUnit
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            var appResult = -2;
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                Utils.PrintInfo();
                appResult = new App(options).Run();
            });
            return appResult;
        }
    }
}
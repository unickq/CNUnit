using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace CNUnit.Tools
{
    public static class Utils
    {
        public static void BeginEndPrint()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(new string('-', 30) + $" {DateTime.Now:G} " + new string('-', 30) + "\n");
            Console.ResetColor();
        }

        public static void PrintCopyright()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n" + new string('-', 82));
            Console.WriteLine(
                $"{new string('-', 15)} CNunit {Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/unickq/CNUnit {new string('-', 16)}");
            Console.WriteLine(new string('-', 82));
        }

        public static void WriteLine(object obj, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(obj);
            Console.ResetColor();
        }

        public static bool IsFileExist(string parameter, string message)
        {
            if (String.IsNullOrWhiteSpace(parameter))
                return false;
            if (File.Exists(parameter)) return true;
            WriteLine($"Unable to find {message}", ConsoleColor.Red);
            return false;
        }

        public static void OutDirSetup(string outdir)
        {
            Console.WriteLine(outdir);
            if (Directory.Exists(outdir))
                Directory.Delete(outdir, true);
            Thread.Sleep(100);
            Directory.CreateDirectory(outdir);
            WriteLine($"Working directory: {outdir}\n", ConsoleColor.Cyan);
        }

        public static ReportType GetReportType(string report)
        {
            switch (report)
            {
                case "nunit2":
                    return ReportType.Nunit2;
                case "junit":
                    return ReportType.JUnit;
                default:
                    return ReportType.NUnit3;
            }
        }

        public static string TryToFindNUnit(string name = "NUnit3-console.exe")
        {
            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (envPath == null) return null;
            var envPathDirs = envPath.Split(';');
            foreach (var dir in envPathDirs)
            {
                if (!Directory.Exists(dir)) continue;
                if (Directory.GetFiles(dir, name).Length == 0) continue;
                name = Path.Combine(dir, name);
                break;
            }
            if (!File.Exists(name)) return null;
            return IsValidNunitConsole(name) ? name : null;
        }

        public static bool IsValidNunitConsole(string path)
        {
            if (!File.Exists(path)) return false;
            var fileVer = FileVersionInfo.GetVersionInfo(path);
            return fileVer.ProductName != null && fileVer.ProductName.Equals("NUnit 3");
        }

        public static void DownloadJUnitTransform()
        {
            var file = Constants.JUnitXsltFile;
            try
            {
                if (File.Exists(file)) File.Delete(file);
                var wc = new WebClient();
                wc.DownloadFile(new Uri(Constants.JUnitXslt), Constants.JUnitXsltFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"JUnit transform file couldn't be found. - {e.Message}");
            }
        }
    }
}
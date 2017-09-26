using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;

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

        public static void WriteLine(object obj, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(obj);
            Console.ResetColor();
        }

        private static readonly Random Rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<T>[] Divide<T>(this List<T> list, int parts)
        {
            if (list == null)
                throw new Exception("List == NULL");
            if (parts < 1)
                throw new Exception("Partiotions count < 1");

            var partitions = new List<T>[parts];
            var maxSize = (int) Math.Ceiling(list.Count / (double) parts);
            var k = 0;

            for (var i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (var j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }
            return partitions;
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
            return FileVersionInfo.GetVersionInfo(name).LegalCopyright.Contains("Charlie Poole") ? name : null;
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
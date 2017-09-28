using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CNUnit.Tools
{
    public static class Extensions
    {
        public static string BuildTestName(this string str)
        {
            return Path.GetFileNameWithoutExtension(str);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = new Random().Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            var i = 0;
            var splits = from item in list
                group item by i++ % parts
                into part
                select part.AsEnumerable();
            return splits;
        }

        public static void DebugCNunit(this Exception e, bool isTrue, string message)
        {
            Utils.WriteLine($"CNunit exception: {message}", ConsoleColor.Red);
            if (!isTrue) return;
            Utils.WriteLine(e.GetType(), ConsoleColor.DarkRed);
            Utils.WriteLine(e.StackTrace, ConsoleColor.DarkRed);
        }
    }
}
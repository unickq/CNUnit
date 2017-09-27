using System;
using System.Collections.Generic;
using System.IO;

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

        public static List<T>[] Divide<T>(this List<T> list, int parts)
        {
            if (list == null)
                throw new Exception("List == NULL");
            if (parts < 1)
                throw new Exception("Partiotions count < 1");

            var partitions = new List<T>[parts];
            var maxSize = (int)Math.Ceiling(list.Count / (double)parts);
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
    }
}
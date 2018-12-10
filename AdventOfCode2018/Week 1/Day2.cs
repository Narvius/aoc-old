using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day2 : ISolution
    {
        // Count the number of lines that match a specific condition. Twice. Then multiply those two counts.
        public string PartOne(string[] lines)
        {
            var boxes = (from line in lines
                         let groups = line.GroupBy(c => c)
                         select (groups.Any(g => g.Count() == 2), groups.Any(g => g.Count() == 3)))
                         .ToList();

            return (boxes.Count(b => b.Item1) * boxes.Count(b => b.Item2)).ToString();
        }

        // Find two strings that match in all but one spots, and return a string that contains everything but that one spot.
        public string PartTwo(string[] lines)
        {
            int Similarity(string a, string b)
                => a.Zip(b, (ca, cb) => ca == cb).Count(x => x);

            string ExciseDifference(string a, string b)
                => new string(a.Zip(b, (ca, cb) => (ca == cb, ca)).Where(x => x.Item1).Select(x => x.Item2).ToArray());

            for (int i = 0; i < lines.Length; i++)
                for (int j = i + 1; j < lines.Length; j++)
                    if (Similarity(lines[i], lines[j]) == (lines[i].Length - 1))
                        return ExciseDifference(lines[i], lines[j]);

            throw new Exception();
        }
    }
}

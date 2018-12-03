using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day1 : ISolution
    {
        public string PartOne(string[] lines)
            => lines.Select(int.Parse).Sum().ToString();

        public string PartTwo(string[] lines)
        {
            IEnumerable<int> ChangesForever()
            {
                while (true)
                    foreach (var line in lines)
                        yield return int.Parse(line);
            }

            var previous = new HashSet<int>() { 0 };
            int current = 0;
            foreach (var change in ChangesForever())
            {
                current += change;
                if (previous.Contains(current))
                    return current.ToString();

                previous.Add(current);
            }

            throw new Exception();
        }
    }
}

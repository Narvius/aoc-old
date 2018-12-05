using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day5 : ISolution
    {
        public string PartOne(string[] lines)
            => FullyReact(lines[0]).Length.ToString();

        public string PartTwo(string[] lines)
        {
            var shortest = (from letter in "abcdefghijklmnopqrstuvwxyz"
                            let without = new string(lines[0].Where(c => char.ToLower(c) != letter).ToArray())
                            let length = FullyReact(without).Length
                            orderby length ascending
                            select length).First();

            return shortest.ToString();
        }

        private string FullyReact(string s)
        {
            int? position = null;
            while (true)
            {
                position = GetReactionPosition(s, position);
                if (!position.HasValue)
                    return s;
                s = s.Remove(position.Value, 2);
            }
        }

        private int? GetReactionPosition(string s, int? start = null)
        {
            int i = Math.Max(0, (start ?? 1) - 1);
            for (; i < (s.Length - 1); i++)
                if (s[i] != s[i + 1] && char.ToLower(s[i]) == char.ToLower(s[i + 1]))
                    return i;

            return null;
        }
    }
}

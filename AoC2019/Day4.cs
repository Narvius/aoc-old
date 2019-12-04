using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day4 : ISolution
    {
        public string PartOne(string[] lines)
        {
            Bounds(lines, out int lower, out int upper);
            return CountInRange(lower, upper, MatchesCriteria).ToString();
        }

        public string PartTwo(string[] lines)
        {
            Bounds(lines, out int lower, out int upper);
            return CountInRange(lower, upper, MatchesStricterCriteria).ToString();
        }

        private int CountInRange(int lower, int upper, Func<int, bool> criteria)
        {
            int count = 0;

            // Ambiguous whether to include upper bound or not, but since the given upper
            // bound (746315) doesn't get counted either way, it doesn't matter.
            for (int i = lower; i < upper; i++)
                if (criteria(i))
                    count++;

            return count;
        }

        private bool MatchesCriteria(int input)
        {
            char last = (char)('0' - 1);
            bool hasDouble = false;

            foreach (char c in input.ToString())
            {
                if (c == last) hasDouble = true;
                if (c < last) return false;
                last = c;
            }

            return hasDouble;
        }

        private bool MatchesStricterCriteria(int input)
        {
            var doubles = new HashSet<char>();
            var triples = new HashSet<char>();

            char last = (char)('0' - 1);

            foreach (char c in input.ToString())
            {
                if (c == last) (doubles.Contains(c) ? triples : doubles).Add(c);
                if (c < last) return false;
                last = c;
            }

            doubles.ExceptWith(triples);
            return doubles.Any();
        }

        private void Bounds(string[] input, out int lower, out int upper)
        {
            lower = int.Parse(input[0]);
            upper = int.Parse(input[1]);
        }
    }
}

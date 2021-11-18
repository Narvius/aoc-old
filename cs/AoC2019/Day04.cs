using System;
using System.Linq;

namespace AoC2019
{
    public class Day04 : ISolution
    {
        // Find all integers in a given range that match the criteria given in the problem description.
        public string PartOne(string[] lines)
        {
            Bounds(lines, out int lower, out int upper);
            return CountInRange(lower, upper, MatchesCriteria).ToString();
        }

        // As part one, with a slightly stricter criterion added.
        public string PartTwo(string[] lines)
        {
            Bounds(lines, out int lower, out int upper);
            return CountInRange(lower, upper, MatchesStricterCriteria).ToString();
        }

        private int CountInRange(int lower, int upper, Func<string, bool> criteria)
        {
            int count = 0;

            // Ambiguous whether to include upper bound or not, but since the given upper
            // bound (746315) doesn't get counted either way, it doesn't matter.
            for (int i = lower; i < upper; i++)
                if (criteria(i.ToString()))
                    count++;

            return count;
        }

        private bool MatchesCriteria(string input)
            => MonotonicallyIncreasing(input) && DigitCounts(input).Any(count => count >= 2);

        private bool MatchesStricterCriteria(string input)
            => MonotonicallyIncreasing(input) && DigitCounts(input).Any(count => count == 2);

        private bool MonotonicallyIncreasing(string input)
            => input.Zip(input.Skip(1), (a, b) => a <= b).All(t => t);

        private int[] DigitCounts(string input)
        {
            var result = new int[10];
            foreach (var c in input)
                result[c - '0']++;
            return result;
        }

        private void Bounds(string[] input, out int lower, out int upper)
        {
            lower = int.Parse(input[0]);
            upper = int.Parse(input[1]);
        }
    }
}

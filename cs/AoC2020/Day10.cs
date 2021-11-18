using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day10 : ISolution
    {
        // Arrange the chain and count the joltage differences. Compute a checksum of the difference counts.
        public string PartOne(string[] lines)
            => new ChargingChain(lines).ChainChecksum().ToString();

        // Find the number of possible ways to arrange the charging chain.
        public string PartTwo(string[] lines)
            => new ChargingChain(lines).ArrangementCount().ToString();
    }

    public class ChargingChain
    {
        // All relevant joltages, sorted in ascending order.
        private readonly int[] joltages;

        public ChargingChain(string[] adapters)
        {
            var numbers = adapters.Select(int.Parse);
            int max = numbers.Max();
            // 0 corresponds to the charging port, max + 3 corresponds to the "device's built-in joltage adapter".
            joltages = numbers.Prepend(0).Prepend(max + 3).OrderBy(x => x).ToArray();
        }

        /// <summary>
        /// Counts how often which joltage difference shows up, and computes the checksum, that is, the product of the number of one-differences
        /// and three-differences.
        /// </summary>
        /// <returns>The checksum.</returns>
        public int ChainChecksum()
        {
            var diffs = joltages.Zip(joltages.Skip(1), (a, b) => b - a);
            return diffs.Count(n => n == 1) * diffs.Count(n => n == 3);
        }

        /// <summary>
        /// Calculates the number of possible arrangements of the charging chain.
        /// </summary>
        /// <remarks>
        /// Uses Dynamic Programming to efficiently compute the answer.
        /// 
        /// An observation: Let's take the joltages 1, 3, 4, 5, 6. How many ways of arranging them are there?
        /// We can actually systematically construct all valid arrangements. We go backwards from the last element, and prepend valid numbers
        /// until we at some point include the 1, which will then be a valid solution.
        /// 
        /// Due to this systematic nature, we can count the amount of solutions without ever actually producing the solutions.
        /// 
        /// Example for 1 3 4 5 6:
        /// 
        /// Subchain     6: One valid solution: '6'.
        /// Subchain    56: One valid solution: '56' (prepend 5 to all possible solutions for subchain 6).
        /// Subchain   456: Two valid solutions: '46' (prepend 4 to subchain 6) and '456' (prepend 4 to subchain 56).
        /// Subchain  3456: Four valid solutions: '36' (prepend 3 to subchain 6), '356' (prepend 3 to subchain 56), '346' and '3456' (prepend 3 to subchain 456).
        /// Chain    13456: Six valid solutions: '146', '1456' (prepend 1 to subchain 456), '136', '1356', '1346' and '13456' (prepend 1 to subchain 3456).
        /// 
        /// Therefore:
        /// solutions[4] = 1                                          = 1
        /// solutions[3] = solutions[4]                               = 1
        /// solutions[2] = solutions[3] + solutions[4]                = 1 + 1     = 2
        /// solutions[1] = solutions[2] + solutions[3] + solutions[4] = 2 + 1 + 1 = 4
        /// solutions[0] = solutions[1] + solutions[2]                = 4 + 2     = 6
        /// 
        /// The amount of elements in those sums above is governed by the restrictions of the puzzle: The maximum allowed joltage difference is 3, so for example
        /// we can only prepend 1 to subchains 3456 and 456, because prepending to subchains 56 and 6 would result in a joltage difference of 4 and 5, respectively.
        /// 
        /// So in short: The solution for a subchain is equal to the sum of the solutions for all shorter subchains with one new joltage added.
        /// So we just keep building bigger subchains until our "subchain" is actually the full chain.
        /// </remarks>
        /// <returns>The number of possible arrangements for the chain.</returns>
        public long ArrangementCount()
        {
            var cache = new long[joltages.Length]; // The amount of possible solutions for a sub-chain starting at the i'th element of "joltages".
            cache[joltages.Length - 1] = 1; // There is only one solution for a chain made of one element.

            for (int i = joltages.Length - 2; 0 <= i; i--)
                cache[i] = Enumerable.Range(1, 3)                                                // within the three next-highest joltages
                    .Where(o => (i + o) < joltages.Length && joltages[i + o] - joltages[i] <= 3) // pick the ones that exist and are at most 3 larger
                    .Sum(o => cache[i + o]);                                                     // and sum their cached sub-solutions to get the new subsolution

            return cache[0]; // The "subchain starting at the 0'th element" is actually just the full chain.
        }
    }
}

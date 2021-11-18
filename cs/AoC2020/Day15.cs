using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day15 : ISolution
    {
        // Find the 2020th memory game number for the given starting numbers.
        public string PartOne(string[] lines)
            => new MemoryGame().NthNumber(2020 - 1, lines[0].Split(',').Select(int.Parse).ToArray()).ToString();

        // Find the 30000000th memory game number for the given starting numbers.
        public string PartTwo(string[] lines)
            => new MemoryGame().NthNumber(30000000 - 1, lines[0].Split(',').Select(int.Parse).ToArray()).ToString();
    }

    public class MemoryGame
    {
        /// <summary>
        /// Runs the memory game until enough numbers are calculated, then returns the requested one.
        /// </summary>
        /// <param name="targetIndex">Index of the requested number.</param>
        /// <param name="input">The given starting numbers.</param>
        /// <returns>The number at the given index of the memory game.</returns>
        public int NthNumber(int targetIndex, int[] input)
        {
            var result = new int[targetIndex + 1];    // stores the actual produced numbers
            var lastPositions = new int[targetIndex]; // when indexed by 'n', the last time 'n' has shown up

            Array.Copy(input, result, input.Length);
            Array.Fill(lastPositions, -1);

            for (int i = 0; i < input.Length; i++)
                lastPositions[result[i]] = i;

            for (int i = input.Length; i <= targetIndex; i++)
            {
                var toWrite = lastPositions[result[i - 1]] == -1 ? 0 : i - 1 - lastPositions[result[i - 1]];
                lastPositions[result[i - 1]] = i - 1;
                result[i] = toWrite;
            }

            return result[targetIndex];
        }
    }
}

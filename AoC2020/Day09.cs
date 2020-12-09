using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day09 : ISolution
    {
        // Find the number that is not the sum of any two of the previous 25 numbers.
        public string PartOne(string[] lines)
            => new XmasDecryptor(25, lines).FindWeakness().ToString();

        // For the number from part 1, find the continuous range (of arbitrary size) that sums to it; sum the smallest and largest number from the range.
        public string PartTwo(string[] lines)
            => new XmasDecryptor(25, lines).FindWeaknessIntervalCode().ToString();
    }

    /// <summary>
    /// Wraps functionality for dealing with XMAS number streams.
    /// </summary>
    public class XmasDecryptor
    {
        private readonly int preambleSize;
        private readonly long[] data;

        public XmasDecryptor(int preambleSize, string[] rawData)
        {
            this.preambleSize = preambleSize;
            data = rawData.Select(long.Parse).ToArray();
        }

        /// <summary>
        /// Finds the weakness. "Weakness" is defined as the first number in the stream that can not be built as a sum of the preceding n numbers
        /// (where n = <see cref="preambleSize"/>).
        /// </summary>
        /// <returns>The weakness.</returns>
        public long FindWeakness()
        {
            for (int i = preambleSize; i < data.Length; i++)
            {
                bool found = false;

                for (int a = 1; !found && a <= preambleSize; a++)
                    for (int b = a + 1; !found && b <= preambleSize; b++)
                        if (data[i - a] + data[i - b] == data[i])
                            found = true;

                if (!found)
                    return data[i];
            }

            throw new Exception("input data violates assumption");
        }

        /// <summary>
        /// Convenience function that calls <see cref="FindIntervalCode(long)"/> with the result of <see cref="FindWeakness"/>.
        /// </summary>
        /// <returns></returns>
        public long FindWeaknessIntervalCode() => FindIntervalCode(FindWeakness());

        /// <summary>
        /// For the given number, finds a continuous range of numbers in the stream that sums up to it and
        /// finds it's "code" (the sum of the lowest and highest number in it).
        /// </summary>
        /// <param name="sum">The sum of the range to be found.</param>
        /// <returns>The sum of the lowest and highest number in the found range.</returns>
        public long FindIntervalCode(long sum)
        {
            long total;

            for (int start = 0; start < data.Length; start++)
            {
                total = data[start];
                for (int n = 1; (start + n) < data.Length; n++)
                {
                    total += data[start + n];
                    if (total < sum) continue;
                    if (total > sum) break;

                    var subrange = new ArraySegment<long>(data, start, n + 1);
                    return subrange.Min() + subrange.Max();
                }
            }

            throw new Exception("input data violates assumption");
        }
    }
}

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
            => new XmasDecryptor(25, lines).FindInvalidNumber().ToString();

        // For the number from part 1, find the continuous range (of arbitrary size) that sums to it; sum the smallest and largest number from the range.
        public string PartTwo(string[] lines)
            => new XmasDecryptor(25, lines).FindEncryptionWeakness().ToString();
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
        /// Finds the number in the stream that isn't the sum of any of the previous n numbers (n = <see cref="preambleSize"/>).
        /// </summary>
        /// <returns>The invalid number.</returns>
        public long FindInvalidNumber()
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
        /// Finds the encryption weakness of the data stream; the sum of the lowest and highest number from
        /// the only continuous range (size 2 or more) of numbers that add up to the invalid number in the stream.
        /// </summary>
        /// <returns>The encryption weakness.</returns>
        public long FindEncryptionWeakness() => FindEncryptionWeaknessForGivenSum(FindInvalidNumber());

        /// <summary>
        /// For the given number, finds a continuous range of numbers in the stream that sums up to it and
        /// finds it's "code" (the sum of the lowest and highest number in it).
        /// </summary>
        /// <remarks>Only guaranteed to have valid output with the result of <see cref="FindInvalidNumber"/>.</remarks>
        /// <param name="sum">The sum of the range to be found.</param>
        /// <returns>The sum of the lowest and highest number in the found range.</returns>
        private long FindEncryptionWeaknessForGivenSum(long sum)
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

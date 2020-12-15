using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day14 : ISolution
    {
        // Get the sum of all memory cells after running the initialization program.
        public string PartOne(string[] lines)
            => new DockInterpreter().RunInitialization(lines).Sum(kvp => kvp.Value).ToString();

        // Get the sum of all memory cells after running the initialization program with revised bitmask logic.
        public string PartTwo(string[] lines)
            => new DockInterpreter().RunInitializationV2(lines).Sum(kvp => kvp.Value).ToString();
    }

    public class DockInterpreter
    {
        /// <summary>
        /// Produces the state of the memory after the initialization program has been run.
        /// </summary>
        /// <param name="program">The puzzle input.</param>
        /// <returns>A dictionary representing the memory state.</returns>
        public Dictionary<long, long> RunInitialization(string[] program)
        {
            var output = new Dictionary<long, long>();
            long andMask = long.MaxValue;
            long orMask = 0;

            foreach (var line in program)
                if (line.StartsWith("mask = "))
                {
                    andMask = Convert.ToInt64(line.Substring(7).Replace('X', '1'), 2);
                    orMask = Convert.ToInt64(line.Substring(7).Replace('X', '0'), 2);
                }
                else
                {
                    var match = Regex.Match(line, @"mem\[(\d+)\] = (\d+)");
                    output[long.Parse(match.Groups[1].Value)] = long.Parse(match.Groups[2].Value) & andMask | orMask;
                }

            return output;
        }

        /// <summary>
        /// Produces the state of the memory after the initialization program has been run. Uses the alternate bitmask
        /// rules from part 2.
        /// </summary>
        /// <param name="program">The puzzle input.</param>
        /// <returns>A dictionary representing the memory state.</returns>
        public Dictionary<long, long> RunInitializationV2(string[] program)
        {
            var output = new Dictionary<long, long>();
            long orMask = 0;
            var redirections = new int[0];
            int possibilities = 0;

            foreach (var line in program)
                if (line.StartsWith("mask = "))
                {
                    orMask = Convert.ToInt64(line.Substring(7).Replace('X', '0'), 2);
                    redirections = BuildRedirections(line.Substring(7));
                    possibilities = (int)Math.Pow(2, redirections.Length);
                }
                else
                {
                    var match = Regex.Match(line, @"mem\[(\d+)\] = (\d+)");
                    var target = orMask | long.Parse(match.Groups[1].Value);
                    var value = long.Parse(match.Groups[2].Value);

                    // Creates all possible target addresses and sets them.
                    for (int i = 0; i < possibilities; i++)
                    {
                        for (int n = 0; n < redirections.Length; n++)
                            SetBit(ref target, redirections[n], (i & (1 << n)) > 0);

                        output[target] = value;
                    }
                }

            return output;
        }

        /// <summary>
        /// Overwrites a single bit in the given number.
        /// </summary>
        /// <param name="number">The number to modify.</param>
        /// <param name="index">The index of the bit, with 0 being the least significant bit.</param>
        /// <param name="on">If true, sets the bit to 1; sets it to 0 otherwise.</param>
        private void SetBit(ref long number, int index, bool on)
        {
            if (on)
                number &= ~(1L << index);
            else
                number |= 1L << index;
        }

        /// <summary>
        /// Given a mask with "floating bits", produces an array holding the positions of those bits. This array
        /// can than be used as a mapping of the indices [0 .. n - 1 ] to those floating bit positions.
        /// </summary>
        /// <param name="mask">The input floating bit mask.</param>
        /// <returns>An array holding all floating bit positions.</returns>
        private int[] BuildRedirections(string mask)
            => mask.Reverse().Select((c, i) => (c, i)).Where(p => p.c == 'X').Select(p => p.i).ToArray();
    }
}

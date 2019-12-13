using System.Collections.Generic;
using System.Linq;

using CPU = AoC2019.Computer.V3;

namespace AoC2019
{
    public class Day07 : ISolution
    {
        // Run an amplifier series for all possible phase settings, get the best result.
        public string PartOne(string[] lines)
        {
            var source = new CPU(lines[0]);
            const int permutations = 120;
            return Enumerable.Range(0, permutations).Max(seed => new AmplifierSeries(source, seed).RunAll()).ToString();
        }

        // Run an amplifier series in feedback mode for all possible phase settings, get the best result.
        public string PartTwo(string[] lines)
        {
            var source = new CPU(lines[0]);
            const int permutations = 120;
            return Enumerable.Range(0, permutations).Max(seed => new AmplifierSeries(source, seed, true).RunAll()).ToString();
        }
    }

    // Describes a series of five amplifiers.
    public class AmplifierSeries
    {
        private readonly CPU[] amplifiers;

        // Takes a "model" amplifier, which is copied to create instances, a "seed" (a number between 0 inclusive and
        // 120 exclusive) which describes the phase settings, and a switch that, if true, links up the final amplifier
        // back with the first one (as well as adjusting the final phase settings to use feedback mode).
        public AmplifierSeries(CPU amplifierModel, int seed, bool withFeedback = false)
        {
            amplifiers = Enumerable.Repeat(0, 5).Select(_ => new Computer.V3(amplifierModel)).ToArray();

            for (int i = 0; i < 4; i++)
                amplifiers[i].OutputTo(amplifiers[i + 1]);

            if (withFeedback)
                amplifiers[4].OutputTo(amplifiers[0]);

            var phaseSettings = PermutationFromIndex(seed);
            for (int i = 0; i < 5; i++)
                amplifiers[i].Input.Enqueue(phaseSettings[i] + (withFeedback ? 5 : 0));
            amplifiers[0].Input.Enqueue(0);
        }

        // Repeatedly runs all amplifiers in sequence until a halt is detected, at which point
        // it returns the last output of the last amplifier that ran.
        public int RunAll()
        {
            for (int i = 0; ; i++)
            {
                var amp = amplifiers[i % 5];
                if (amp.State == CPU.ExecutionState.Halted)
                {
                    var prevAmp = amplifiers[(i + 4) % 5];
                    return prevAmp.Output.Dequeue();
                }

                amp.RunWhilePossible();
            }
        }

        // Maps a single integer between 0 (inclusive) and 5! (exclusive) to one of the permutations of the 
        // set {0, 1, 2, 3, 4}. The indices do not produce permutations in lexicographical order, but that's
        // irrelevant for this task.
        private int[] PermutationFromIndex(int index)
        {
            var items = new List<int> { 0, 1, 2, 3, 4 };
            var result = new int[5];
            for (int i = 5; i > 0; i--)
            {
                result[i - 1] = items[index % i];
                items.RemoveAt(index % i);
                index /= i;
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day02 : ISolution
    {
        // Implement the virtual machine specified in the problem.
        // Run it with the given initial state, and retrieve the output.
        public string PartOne(string[] lines)
            => new Computer.V1(lines[0], (1, 12), (2, 2)).RunUntilHalted().ToString();

        // Find an initial setting for the virtual machine that returns a specific result.
        public string PartTwo(string[] lines)
            => (from noun in Enumerable.Range(0, 100)
                from verb in Enumerable.Range(0, 100)
                let result = new Computer.V1(lines[0], (1, noun), (2, verb)).RunUntilHalted()
                where result == 19690720
                select 100 * noun + verb).First().ToString();
    }
}

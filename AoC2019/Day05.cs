using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day05 : ISolution
    {
        // Run ship computer with input 1, get diagnostic code (last output).
        public string PartOne(string[] lines)
            => new Computer.V2(lines[0]).RunUntilHalted(1).Last().ToString();

        // Run ship computer with input 5, get diagnostic code (last output).
        public string PartTwo(string[] lines)
            => new Computer.V2(lines[0]).RunUntilHalted(5).Last().ToString();
    }
}

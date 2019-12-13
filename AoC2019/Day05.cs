using System.Linq;

using CPU = AoC2019.Computer.V2;

namespace AoC2019
{
    public class Day05 : ISolution
    {
        // Run ship computer with input 1, get diagnostic code (last output).
        public string PartOne(string[] lines)
            => new CPU(lines[0]).RunUntilHalted(1).Last().ToString();

        // Run ship computer with input 5, get diagnostic code (last output).
        public string PartTwo(string[] lines)
            => new CPU(lines[0]).RunUntilHalted(5).Last().ToString();
    }
}

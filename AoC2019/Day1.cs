using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day1 : ISolution
    {
        public string PartOne(string[] lines)
            => (from line in lines
                let mass = long.Parse(line)
                select mass / 3 - 2).Sum().ToString();

        public string PartTwo(string[] lines)
            => (from line in lines
                let mass = long.Parse(line)
                let fuel = FuelSequence(mass).Sum()
                select fuel).Sum().ToString();

        private IEnumerable<long> FuelSequence(long fuel)
        {
            while (fuel > 0)
            {
                fuel = Math.Max(0, fuel / 3 - 2);
                yield return fuel;
            } 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day01 : ISolution
    {
        // Each input line is the mass of one component.
        // Calculate the total required fuel, as a function of component mass.
        public string PartOne(string[] lines)
            => (from line in lines
                let mass = long.Parse(line)
                select mass / 3 - 2).Sum().ToString();

        // As part one, but the fuel itself also requires fuel the same way components do.
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

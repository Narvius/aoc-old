using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day06 : ISolution
    {
        // Get the total amount of all direct and indirect orbits.
        public string PartOne(string[] lines)
        {
            var data = (from line in lines
                        let split = line.Split(')')
                        select (key: split[1], value: split[0])).ToDictionary(kvp => kvp.key, kvp => kvp.value);

            var orbits = new Dictionary<string, int> { { "COM", 0 } };

            return data.Keys.Sum(k => GetOrbitCount(k, data, orbits)).ToString();
        }

        // Get the amount of jumps between YOU and SAN.
        public string PartTwo(string[] lines)
        {
            var data = (from line in lines
                        let split = line.Split(')')
                        select (key: split[1], value: split[0])).ToDictionary(kvp => kvp.key, kvp => kvp.value);

            var mine = PathToRoot("YOU", data).ToList();
            var his = PathToRoot("SAN", data).ToList();

            var intersect = mine.Intersect(his).First();

            return (mine.IndexOf(intersect) + his.IndexOf(intersect)).ToString();
        }

        private int GetOrbitCount(string planet, Dictionary<string, string> links, Dictionary<string, int> memoizedResults)
        {
            if (!memoizedResults.ContainsKey(planet))
                memoizedResults.Add(planet, GetOrbitCount(links[planet], links, memoizedResults) + 1);

            return memoizedResults[planet];
        }

        private IEnumerable<string> PathToRoot(string planet, Dictionary<string, string> data)
        {
            while (planet != "COM")
            {
                planet = data[planet];
                yield return planet;
            }
        }
    }
}

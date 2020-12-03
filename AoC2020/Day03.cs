using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day03 : ISolution
    {
        // Find the number of trees on the given slope.
        public string PartOne(string[] lines)
            => new TreeMap(lines).TreesOnSlope((3, 1)).ToString();

        // Find the product of the numbers of trees on all given slopes.
        public string PartTwo(string[] lines)
            => new TreeMap(lines).TreesOnSlopes((1, 1), (3, 1), (5, 1), (7, 1), (1, 2)).Aggregate((a, b) => a * b).ToString();
    }

    /// <summary>
    /// Provides the means of querying the amount of trees on slopes starting at (0, 0).
    /// </summary>
    public class TreeMap
    {
        /// <summary>
        /// terrain[y][x] contains '#' if there is a tree at coordinates (x, y), '.' otherwise.
        /// </summary>
        private readonly string[] terrain;

        public TreeMap(string[] terrain)
        {
            this.terrain = terrain;
        }

        /// <summary>
        /// Counts the number of trees along the given slope.
        /// </summary>
        /// <param name="slope">The orientation of the slope given as mutually-prime changes in horizontal and
        /// vertical position per step, with an assumed starting point of (0, 0).</param>
        /// <returns>The number of trees on the slope.</returns>
        public long TreesOnSlope((int dx, int dy) slope)
            => ProduceSlope(slope.dx, slope.dy, terrain[0].Length, terrain.Length)
            .LongCount(c => terrain[c.y][c.x] == '#');

        /// <summary>
        /// Counts the number of trees along a number of given slopes.
        /// </summary>
        /// <param name="slopes">A list of slopes, each one given as their orientation in the form of mutually-prime
        /// changes in horizontal and vertical position per step, with an assumed starting point of (0, 0) each.</param>
        /// <returns>A sequence of numbers, where each number is the amount of trees on the corresponding slope</returns>
        public IEnumerable<long> TreesOnSlopes(params (int dx, int dy)[] slopes)
            => slopes.Select(TreesOnSlope);

        /// <summary>
        /// Produces the coordinates of all points on a slope.
        /// </summary>
        /// <param name="dx">Change along the x axis in each step of the slope.</param>
        /// <param name="dy">Change along the y axis in each step of the slope.</param>
        /// <param name="mx">The maximum value of x. All x coordinates in the output will be coerced into [0, mx) via modulo.</param>
        /// <param name="my">The maximum value of y. This determines when the enumeration terminates.</param>
        /// <returns>An enumeration of all valid coordinates for the given slope, in order.</returns>
        private IEnumerable<(int x, int y)> ProduceSlope(int dx, int dy, int mx, int my)
            => Enumerable.Range(0, my / dy).Select(n => ((dx * n) % mx, dy * n));
    }
}

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
            => TreesOnSlope(3, 1, lines).ToString();

        // Find the product of the numbers of trees on all given slopes.
        public string PartTwo(string[] lines)
            => ((long)TreesOnSlope(1, 1, lines)
              * TreesOnSlope(3, 1, lines)
              * TreesOnSlope(5, 1, lines)
              * TreesOnSlope(7, 1, lines)
              * TreesOnSlope(1, 2, lines)).ToString();

        /// <summary>
        /// Counts the number of trees along the given slope for the given terrain.
        /// </summary>
        /// <param name="dx">Change along the x axis in each step of the slope.</param>
        /// <param name="dy">Change along the y axis in each step of the slope.</param>
        /// <param name="terrain">The terrain. It infinitely repeats horizontally.</param>
        /// <returns>The number of trees on the slope.</returns>
        private int TreesOnSlope(int dx, int dy, string[] terrain)
            => ProduceSlope(dx, dy, terrain[0].Length, terrain.Length).Count(c => terrain[c.y][c.x] == '#');

        /// <summary>
        /// Produces the coordinates of all points on a slope.
        /// </summary>
        /// <param name="dx">Change along the x axis in each step of the slope.</param>
        /// <param name="dy">Change along the y axis in each step of the slope.</param>
        /// <param name="mx">The maximum value of x. All x coordinates in the output will be coerced into [0, mx) via modulo.</param>
        /// <param name="my">The maximum value of y. This determines when the enumeration terminates.</param>
        /// <returns>An enumeration of all valid coordinates for the given slope, in order.</returns>
        private IEnumerable<(int x, int y)> ProduceSlope(int dx, int dy, int mx, int my)
        {
            int x = 0, y = 0;

            while (y < my)
            {
                yield return (x, y);
                x = (x + dx) % mx;
                y += dy;
            }
        }
    }
}

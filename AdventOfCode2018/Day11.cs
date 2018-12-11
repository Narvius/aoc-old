using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day11 : ISolution
    {
        // Highest-power 3x3 sub-grid.
        public string PartOne(string[] lines)
        {
            var serial = int.Parse(lines[0]);
            var point = (from p in PointsOnGrid()
                         where MaxSquareSizeAtCoordinate(p) >= 3
                         let power = PowerOfSquare(p, serial, 3)
                         orderby power descending
                         select p).First();
            return $"{point.X},{point.Y}";
        }

        // Highest-power sub-grid of any size.
        public string PartTwo(string[] lines)
        {
            var serial = int.Parse(lines[0]);
            var result = (from p in PointsOnGrid()
                          from power in PowerLevels(p, serial)
                          orderby power.power descending
                          select (point: p, size: power.size)).First();
            return $"{result.point.X},{result.point.Y},{result.size}";
        }

        // A list of all points on the 300x300 grid.
        private IEnumerable<Point> PointsOnGrid()
        {
            for (int y = 1; y < 299; y++)
                for (int x = 1; x < 299; x++)
                    yield return (x, y);
        }

        // The largest possible square starting at the given point that still fits into the 300x300 grid.
        private int MaxSquareSizeAtCoordinate(Point topleft)
            => Math.Min(300 - topleft.X, 300 - topleft.Y);

        // The total power of a square of a given size at the given starting point.
        private int PowerOfSquare(Point topleft, int serial, int size)
            => (from i in Enumerable.Range(0, size * size)
                let point = topleft + (i % size, i / size)
                select PowerLevel(point, serial)).Sum();

        // A list of the total power of each possible square at the given starting point.
        private IEnumerable<(int size, int power)> PowerLevels(Point topleft, int serial)
        {
            int result = 0;
            for (int layer = 0; layer < MaxSquareSizeAtCoordinate(topleft); layer++)
            {
                result += PowerLevel(topleft + (layer, layer), serial);
                for (int i = 0; i < layer; i++)
                {
                    result += PowerLevel(topleft + (layer, i), serial);
                    result += PowerLevel(topleft + (i, layer), serial);
                }
                yield return (layer + 1, result);
            }
        }

        // The raw power level calculation for a cell.
        private int PowerLevel(Point index, int serial)
            => ((serial + ((index.X + 10) * index.Y)) * (index.X + 10) / 100 % 10) - 5;
    }
}

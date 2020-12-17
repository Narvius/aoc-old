using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day11 : ISolution
    {
        // Find the number of taken seats given the first ruleset.
        public string PartOne(string[] lines)
            => new CellularAutomaton(lines).RunUntilNoChange(CellularAutomaton.CountingMode.Neighbour, 4).ToString();

        // Find the number of taken seats given the second ruleset.
        public string PartTwo(string[] lines)
            => new CellularAutomaton(lines).RunUntilNoChange(CellularAutomaton.CountingMode.Visible, 5).ToString();
    }

    /// <summary>
    /// Naive implementation of a cellular automaton with puzzle-specific rules.
    /// </summary>
    public class CellularAutomaton
    {
        public enum Cell : byte { On, Off, Blocked }
        
        public enum CountingMode
        { 
            Neighbour, // Counts the 8 adjacent tiles
            Visible    // Counts the closest tile in each of the 8 directions that isn't Blocked
        }

        private Cell[,] buffer;
        private Cell[,] map;
        
        public CellularAutomaton(string[] input, char on = '#', char off = 'L', char blocked = '.')
        {
            map = new Cell[input.Length, input[0].Length];
            buffer = new Cell[input.Length, input[0].Length];

            for (int y = 0; y < input.Length; y++)
                for (int x = 0; x < input[y].Length; x++)
                    map[y, x] = input[y][x] switch
                    {
                        var c when c == on => Cell.On,
                        var c when c == off => Cell.Off,
                        var c when c == blocked => Cell.Blocked,
                        _ => throw new Exception("invalid character in input")
                    };
        }

        /// <summary>
        /// Runs the cellular automaton until it reaches equilibrium.
        /// </summary>
        /// <param name="mode">Decides which cells count as "seen" from any set of coordinates.</param>
        /// <param name="thresholdForOff">The amount of seen <see cref="Cell.On"/> cells for the <see cref="Cell.On"/> -> <see cref="Cell.Off"/> rule to kick in.</param>
        /// <returns>The number of <see cref="Cell.On"/> cells.</returns>
        public int RunUntilNoChange(CountingMode mode, int thresholdForOff)
        {
            var targetCache = BuildTargetCache(mode == CountingMode.Neighbour ? (Func<int, int, IEnumerable<(int x, int y)>>)AdjacentTargets : VisibleTargets);

            while (Step(targetCache, thresholdForOff)) ;

            return map.Cast<Cell>().Count(c => c == Cell.On);
        }

        /// <summary>
        /// Advances the cellular automaton by one step.
        /// </summary>
        /// <param name="targetCache">See <see cref="BuildTargetCache(Func{int, int, IEnumerable{(int x, int y)}})"/> for more information.</param>
        /// <param name="thresholdForOff">The amount of seen <see cref="Cell.On"/> cells for the <see cref="Cell.On"/> -> <see cref="Cell.Off"/> rule to kick in.</param>
        /// <returns>Whether the state of the automaton changed from before the step.</returns>
        private bool Step((int x, int y)[,][] targetCache, int thresholdForOff)
        {
            bool changed = false;
            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    var newState = (cell: map[y, x], count: targetCache[y, x].Count(p => map[p.y, p.x] == Cell.On)) switch
                    {
                        (Cell.Off, 0)                              => Cell.On,
                        (Cell.On, var n) when n >= thresholdForOff => Cell.Off,
                        _                                          => map[y, x]
                    };

                    buffer[y, x] = newState;

                    if (map[y, x] != newState)
                        changed = true;
                }

            (map, buffer) = (buffer, map);

            return changed;
        }

        /// <summary>
        /// Builds the target cache. When indexed with the same coordinates as <see cref="map"/>, it returns
        /// a list of coordinates that are counted when determining the behaviour of this cell.
        /// </summary>
        /// <remarks>Essentially an optimization that trades space for time. Computing the targets each time
        /// resulted in a >1s runtime the first time through, which I considered unacceptable.</remarks>
        /// <param name="coordinateProducer">The function to cache the output of.</param>
        /// <returns>A target cache built from the given coordinate producer</returns>
        private (int x, int y)[,][] BuildTargetCache(Func<int, int, IEnumerable<(int x, int y)>> coordinateProducer)
        {
            var result = new (int x, int y)[map.GetLength(0), map.GetLength(1)][];
            for (int y = 0; y < map.GetLength(0); y++)
                for (int x = 0; x < map.GetLength(1); x++)
                    result[y, x] = coordinateProducer(x, y).ToArray();
            return result;
        }

        // Vectors for the 8 cardinal/ordinal directions.
        private readonly (int dx, int dy)[] Directions = new[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };

        /// <summary>
        /// Produces coordinates for all valid neighbour coordinates. A set of coordinates is valid if it exists on the map,
        /// and the corresponding cell is not <see cref="Cell.Blocked"/>.
        /// </summary>
        /// <param name="x">X coordinate of the cell to find targets for.</param>
        /// <param name="y">Y coordinate of the cell to find targets for.</param>
        /// <returns>Up to eight coordinates corresponding to valid neighbour coordinates.</returns>
        private IEnumerable<(int x, int y)> AdjacentTargets(int x, int y)
        {
            foreach (var (dx, dy) in Directions)
                if (map.ValidIndex(y + dy, x + dx) && map[y + dy, x + dx] != Cell.Blocked)
                    yield return (x + dx, y + dy);
        }

        /// <summary>
        /// Produces coordinates for all valid visible coordinates. A set of coordinates is valid if it exists on the map,
        /// and the corresponding cell is not <see cref="Cell.Blocked"/>. A set of coordinates is visible if it is the closest
        /// non-<see cref="Cell.Blocked"/> cell in one of the cardinal or ordinal directions on the map.
        /// </summary>
        /// <param name="x">X coordinate of the cell to find targets for.</param>
        /// <param name="y">Y coordinate of the cell to find targets for.</param>
        /// <returns>Up to eight coordinates corresponding to valid visible coordinates.</returns>
        private IEnumerable<(int x, int y)> VisibleTargets(int x, int y)
        {
            foreach (var (dx, dy) in Directions)
                for (int n = 1; ; n++)
                {
                    var tx = x + dx * n;
                    var ty = y + dy * n;

                    if (!map.ValidIndex(ty, tx))
                        break;

                    if (map[ty, tx] != Cell.Blocked)
                    {
                        yield return (tx, ty);
                        break;
                    }
                }
        }
    }
}

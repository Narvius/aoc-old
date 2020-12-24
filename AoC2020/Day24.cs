using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day24 : ISolution
    {
        // Count the number of black tiles given in the input.
        public string PartOne(string[] lines)
            => Tiles.CountBlackTiles(lines).ToString();

        // Count the number of black tiles after 100 steps of the "art exhibit rules" applied.
        public string PartTwo(string[] lines)
            => Tiles.ArtExhibitAfter100Days(lines).ToString();
    }

    public static class Tiles
    {
        private static readonly IReadOnlyDictionary<string, Vec> directionDeltas = new Dictionary<string, Vec>
        {
            { "w", (-1, 0) },
            { "nw", (-1, -1) },
            { "ne", (0, -1) },
            { "e", (1, 0) },
            { "se", (1, 1) },
            { "sw", (0, 1) }
        };

        /// <summary>
        /// Calculates which tiles are black based on the puzzle input.
        /// </summary>
        /// <param name="input">The puzzle input.</param>
        /// <returns>A hashset containing the coordinates of every black tile.</returns>
        private static HashSet<Vec> BlackTiles(string[] input)
        {
            var blackTiles = new HashSet<Vec>();
            foreach (var line in input)
            {
                Vec p = Vec.Zero;
                foreach (var dir in line.ChunkBy(c => c == 'e' || c == 'w').Select(cs => new string(cs.ToArray())).Where(cs => cs.Length > 0).ToArray())
                    p += directionDeltas[dir];
                if (blackTiles.Contains(p))
                    blackTiles.Remove(p);
                else
                    blackTiles.Add(p);
            }
            return blackTiles;
        }

        /// <summary>
        /// Counts the number of tiles that are black after applying all the rules from the input.
        /// </summary>
        /// <param name="input">The puzzle input.</param>
        /// <returns>The number of black tiles.</returns>
        public static int CountBlackTiles(string[] input)
            => BlackTiles(input).Count;

        /// <summary>
        /// Returns the number of black tiles ("live cells") after 100 steps of the "art exhibit rules" (cellular automaton rules).
        /// </summary>
        /// <param name="input">The puzzle input.</param>
        /// <returns>The number of black tiles after 100 steps.</returns>
        public static int ArtExhibitAfter100Days(string[] input)
        {
            var blackTiles = BlackTiles(input);

            for (int i = 0; i < 100; i++)
            {
                var whiteCandidates = new Dictionary<Vec, int>();
                var toFlip = new HashSet<Vec>();

                // Since only the number of black neighbours is relevant for the rules, we just use existing black
                // cells to figure out which tiles might be affected. This saves us iterating over a whole area of
                // hexagon-shaped cells.

                foreach (var tile in blackTiles)
                {
                    int blackCount = 0;
                    foreach (var neighbour in directionDeltas.Values.Select(delta => tile + delta))
                        if (!blackTiles.Contains(neighbour))
                            whiteCandidates[neighbour] = whiteCandidates.GetValueOrDefault(neighbour) + 1;
                        else
                            blackCount++;
                    if (blackCount == 0 || blackCount > 2)
                        toFlip.Add(tile);
                }

                foreach (var kvp in whiteCandidates)
                    if (kvp.Value == 2)
                        blackTiles.Add(kvp.Key);

                foreach (var tile in toFlip)
                    if (blackTiles.Contains(tile))
                        blackTiles.Remove(tile);
                    else
                        blackTiles.Add(tile);
            }

            return blackTiles.Count;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day06 : ISolution
    {
        // Find letters contained in ANY line within a chunk. Count those letters for each chunk, sum the totals.
        public string PartOne(string[] lines)
            => lines.ChunkBy(string.IsNullOrEmpty)
            .Sum(chunk => string.Join("", chunk).Distinct().Count())
            .ToString();

        // Find letters contained in ALL lines within a chunk. Count htose letters for each chunk, sum the totals.
        public string PartTwo(string[] lines)
            => lines.ChunkBy(string.IsNullOrEmpty)
            .Select(chunk => chunk.Where(s => !string.IsNullOrEmpty(s)).ToArray())
            .Sum(chunk => "abcdefghijklmnopqrstuvwxyz".Count(c => chunk.All(line => line.Contains(c))))
            .ToString();
    }
}

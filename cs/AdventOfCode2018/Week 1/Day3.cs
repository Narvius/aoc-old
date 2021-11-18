﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2018
{
    public class Day3 : ISolution
    {
        const int Width = 1000;
        const int Height = 1000;

        // Count the number of tiles with more than one tile to them.
        public string PartOne(string[] lines)
            => GetField(lines.Select(Claim.FromInput))
            .Count(x => x > 1)
            .ToString();

        // Find the one claim that doesn't overlap with any other.
        // That is, the one for which there is 1 claim on all tiles it claims.
        public string PartTwo(string[] lines)
        {
            var claims = lines.Select(Claim.FromInput).ToList();
            var field = GetField(claims);
            return (from claim in claims
                    // Here, the Rectangle type (claim.Area) is treated as a collection of all points it contains.
                    let nonoverlapping = claim.Area.All(p => field[p.As1D(Width)] == 1)
                    where nonoverlapping
                    select claim.Id).Single().ToString();
        }

        // Returns a [Width x Height] array which, indexed by [x, y] returns the number of claims to that tile.
        // Note that the 2D array is flattened into 1D so it's easier to iterate over.
        private int[] GetField(IEnumerable<Claim> claims)
        {
            var field = new int[Width * Height];
            foreach (var claim in claims)
                foreach (var point in claim.Area)
                    field[point.As1D(Width)]++;
            return field;
        }
    }

    public class Claim
    {
        public int Id { get; }
        public Rectangle Area { get; }

        private static readonly Regex LineParse = new Regex(@"^#(\d+)\s@\s(\d+),(\d+):\s(\d+)x(\d+)$");

        public Claim(int id, Rectangle area)
        {
            Id = id;
            Area = area;
        }

        public static Claim FromInput(string input)
        {
            var match = LineParse.Match(input);
            if (!match.Success)
                throw new ArgumentException("invalid format", nameof(input));

            return new Claim(
                int.Parse(match.Groups[1].Value),
                new Rectangle(
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value),
                    int.Parse(match.Groups[5].Value)));
        }
    }
}

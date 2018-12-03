using System;
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

        private int[] GetField(IEnumerable<Claim> claims)
        {
            var field = new int[Width * Height];
            foreach (var claim in claims)
                for (int cy = 0; cy < claim.Size.y; cy++)
                    for (int cx = 0; cx < claim.Size.x; cx++)
                    {
                        var x = claim.Offset.x + cx;
                        var y = claim.Offset.y + cy;
                        field[Width * y + x]++;
                    }

            return field;
        }

        public string PartOne(string[] lines)
            => GetField(lines.Select(Claim.FromInput))
            .Count(x => x > 1)
            .ToString();


        public string PartTwo(string[] lines)
        {
            var claims = lines.Select(Claim.FromInput).ToList();
            var field = GetField(claims);

            bool Nonoverlapping(Claim claim)
            {
                for (int y = 0; y < claim.Size.y; y++)
                    for (int x = 0; x < claim.Size.x; x++)
                        if (field[(claim.Offset.y + y) * Width + claim.Offset.x + x] != 1)
                            return false;

                return true;
            }

            return claims.First(Nonoverlapping).Id.ToString();
        }
    }

    public class Claim
    {
        public int Id { get; }
        public (int x, int y) Offset { get; }
        public (int x, int y) Size { get; }

        private static readonly Regex ParseRegex = new Regex(@"^#(\d+)\s@\s(\d+),(\d+):\s(\d+)x(\d+)$");

        public Claim(int id, (int x, int y) offset, (int x, int y) size)
        {
            Id = id;
            Offset = offset;
            Size = size;
        }

        public static Claim FromInput(string input)
        {
            var match = ParseRegex.Match(input);
            if (!match.Success)
                throw new ArgumentException("invalid format", nameof(input));

            return new Claim(
                int.Parse(match.Groups[1].Value),
                (int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value)),
                (int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value)));
        }
    }
}

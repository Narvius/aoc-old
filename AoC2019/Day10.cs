using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day10 : ISolution
    {
        // Find the coordinates from which the most asteroids are visible.
        public string PartOne(string[] lines)
        {
            var asteroids = new Asteroids(lines);
            return Enumerable.Range(0, asteroids.Width * asteroids.Height)
                .Max(asteroids.CountVisibleFrom)
                .ToString();
        }

        // Given the coordinates from part 1, get the 200th asteroid when enumerating
        // them as a spiral, starting at true north.
        public string PartTwo(string[] lines)
        {
            var asteroids = new Asteroids(lines);
            // Get the coordinates from part 1.
            var coords = Enumerable.Range(0, asteroids.Width * asteroids.Height)
                .OrderByDescending(asteroids.CountVisibleFrom)
                .First();

            var (x, y) = asteroids.EnumerateAsClockwiseSpiralFrom(coords % asteroids.Width, coords / asteroids.Width)
                .Skip(199).First();
            return (x * 100 + y).ToString();
        }
    }

    public class Asteroids
    {
        private readonly int[] original;

        public int Width { get; }
        public int Height { get; }

        private int this[int x, int y] => original[As1D(x, y)];

        const int SPACE = 0;
        const int CANDIDATE = 1;
        const int RULED_OUT = 2;

        public Asteroids(string[] data)
        {
            Width = data[0].Length;
            Height = data.Length;

            original = new int[Width * Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    original[y * Width + x] = data[y][x] == '#' ? CANDIDATE : SPACE;
        }

        public int CountVisibleFrom(int i)
            => CountVisibleFrom(i % Width, i / Width);

        public int CountVisibleFrom(int x, int y)
        {
            if (this[x, y] != CANDIDATE)
                return 0;

            return VisibleFrom(original, x, y).Count(n => n == CANDIDATE);
        }

        private int[] VisibleFrom(int[] source, int x, int y)
        {
            var field = new int[original.Length];
            Array.Copy(original, field, field.Length);

            field[As1D(x, y)] = RULED_OUT;

            for (int oy = 0; oy < Height; oy++)
                for (int ox = 0; ox < Width; ox++)
                    if (field[As1D(ox, oy)] == CANDIDATE)
                        RuleOutLine(field, (x, y), (ox, oy));

            return field;
        }

        private void RuleOutLine(int[] field, (int x, int y) observer, (int x, int y) obstacle)
        {
            int dx = obstacle.x - observer.x;
            int dy = obstacle.y - observer.y;
            int factor = GCD(Math.Abs(dx), Math.Abs(dy));

            dx /= factor;
            dy /= factor;

            for (int i = factor + 1; ; i++)
            {
                var x = observer.x + dx * i;
                var y = observer.y + dy * i;

                if (0 <= x && x < Width && 0 <= y && y < Height)
                    field[As1D(x, y)] = RULED_OUT;
                else
                    return;
            }
        }

        public IEnumerable<(int x, int y)> EnumerateAsClockwiseSpiralFrom(int sx, int sy)
        {
            var field = new int[original.Length];
            Array.Copy(original, field, field.Length);

            field[As1D(sx, sy)] = RULED_OUT;
            // (1) find ALL currently visible
            // (2) order them by angle towards north

            var found = new List<(int x, int y)>();
            while (true)
            {
                var data = VisibleFrom(field, sx, sy);
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == CANDIDATE)
                        found.Add((i % Width, i / Width));
                
                foreach (var (x, y) in found.OrderBy(p => AngleBetween((sx, sy), (sx, 0), p)))
                {
                    yield return (x, y);
                    field[As1D(x, y)] = RULED_OUT;
                }

                found.Clear();
            }
        }

        private int As1D(int x, int y) => y * Width + x;
        private int GCD(int a, int b) => b == 0 ? a : GCD(b, a % b);

        // Returns the angle between two points as seen from 'origin'; clamped into the interval [0, 2pi).
        private double AngleBetween((int x, int y) origin, (int x, int y) p1, (int x, int y) p2)
            => (Math.PI * 2 + Math.Atan2(p2.y - origin.y, p2.x - origin.x) - Math.Atan2(p1.y - origin.y, p1.x - origin.x)) % (Math.PI * 2);
    }
}

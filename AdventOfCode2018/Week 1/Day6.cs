using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day6 : ISolution
    {
        // Flood-fill a 2D map from a number of starting point.
        // See which point claimed the highest area, and return the size of that area.
        public string PartOne(string[] lines)
            => new FloodFillMap(400, 400, from line in lines
                                          let data = line.Split(", ")
                                          select new Point(int.Parse(data[0]), int.Parse(data[1])))
            .LargestSurface()
            .ToString();

        // For each point on the grid, check the sum of the Manhattan distances to all the starting points.
        // Count the number of elements that are smaller than a constant.
        public string PartTwo(string[] lines)
        {
            const int distance = 10000;

            var points = (from line in lines
                          let data = line.Split(", ")
                          select new Point(int.Parse(data[0]), int.Parse(data[1])))
                          .ToList();

            return (from p in new Rectangle(-100, -100, 600, 600)
                    where points.Sum(p.ManhattanDistance) < distance
                    select 1).Sum().ToString();
        }
    }

    // Given a size and number of starting points, performs a flood fill in that plane.
    // Each point expands at the same speed, orthogonally.
    // Also keeps track of which points expand "infinitely" (== touch the edge).
    public class FloodFillMap
    {
        public int Width, Height;

        // The actual 2D array. It's flattened to a 1D array to be easier to iterate over.
        private int[] buffer;

        // The indices of starting points that ended up "going infinite" (touching the edge).
        private HashSet<int> infinites = new HashSet<int>();

        // The number of starting points.
        private readonly int startSeedCount;

        private const int Empty = 0;
        private const int Contested = -1;

        public FloodFillMap(int width, int height, IEnumerable<Point> startSeeds)
        {
            Width = width;
            Height = height;
            buffer = new int[width * height];

            var seeds = startSeeds.ToList();
            startSeedCount = seeds.Count;

            int x = 1;
            foreach (var pos in startSeeds)
                buffer[pos.As1D(Width)] = x++;

            while (seeds.Any())
                seeds = FloodFillStep(seeds);
        }

        // The size of the largest area that is not "infinite".
        public int LargestSurface()
            => Enumerable.Range(1, startSeedCount)
            .Select(x => infinites.Contains(x) ? -1 : buffer.Count(b => b == x))
            .Max();

        // Performs a single step of floodfill (from the "seeds"), and returns a list of new seeds.
        // New seeds are tiles that are 1) freshly-filled 2) not contested.
        // A tile is contested if multiple flood fills reach it simultaneously.
        private List<Point> FloodFillStep(List<Point> seeds)
        {
            var changes = new HashSet<Point>();
            var newSeeds = new List<Point>();

            foreach (var seed in seeds)
            {
                var c = buffer[seed.As1D(Width)];

                if (seed.X == 0 || seed.X == Width - 1 || seed.Y == 0 || seed.Y == Height - 1)
                    infinites.Add(c);

                foreach (var neighbour in Neighbours(seed))
                {
                    var dc = buffer[neighbour.As1D(Width)];
                    if (dc == Empty)
                    {
                        buffer[neighbour.As1D(Width)] = c;
                        changes.Add(neighbour);
                        newSeeds.Add(neighbour);
                    }
                    else if (dc != c && changes.Contains(neighbour))
                    {
                        buffer[neighbour.As1D(Width)] = Contested;
                        newSeeds.Remove(neighbour);
                    }
                }
            }

            return newSeeds;
        }

        // A list of all in-bounds neighbours.
        private IEnumerable<Point> Neighbours(Point p)
        {
            if (p.X > 0)
                yield return p + (-1, 0);
            if (p.Y > 0)
                yield return p + (0, -1);
            if (p.X < (Width - 1))
                yield return p + (1, 0);
            if (p.Y < (Height - 1))
                yield return p + (0, 1);
        }
    }
}

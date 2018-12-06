using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day6 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var map = new FloodFillMap(400, 400, from line in lines
                                                 let data = line.Split(", ")
                                                 select new Point(int.Parse(data[0]), int.Parse(data[1])));

            while (map.FillStep()) ;

            return map.Surfaces().Max(x => x.Value).ToString();
        }

        public string PartTwo(string[] lines)
        {
            var points = (from line in lines
                          let data = line.Split(", ")
                          select new Point(int.Parse(data[0]), int.Parse(data[1])))
                          .ToList();

            const int distance = 10000;

            int count = 0;
            for (int x = -100; x < 500; x++)
                for (int y = -100; y < 500; y++)
                    if (points.Sum(p => Math.Abs(p.X - x) + Math.Abs(p.Y - y)) < distance)
                        count++;

            return count.ToString();
        }
    }

    public class FloodFillMap
    {
        public int Width, Height;
        private int[] buffer;
        private List<Point> seeds;
        private HashSet<int> infinites = new HashSet<int>();

        private readonly int startSeedCount;

        private const int Empty = -1;
        private const int Contested = -2;

        public FloodFillMap(int width, int height, IEnumerable<Point> seeds)
        {
            Width = width;
            Height = height;
            buffer = new int[width * height];
            Array.Fill(buffer, Empty);
            this.seeds = new List<Point>(seeds);
            startSeedCount = this.seeds.Count;

            int x = 0;
            foreach (var pos in seeds)
                Set(pos, x++);
        }

        public int this[int x, int y] => Get((x, y));

        public Dictionary<int, int> Surfaces()
        {
            var result = new Dictionary<int, int>();
            for (int i = 0; i < startSeedCount; i++)
                result[i] = infinites.Contains(i)
                    ? -1
                    : buffer.Count(x => x == i);
            return result;
        }

        public bool FillStep()
        {
            var changes = new HashSet<Point>();
            var newSeeds = new List<Point>();
            foreach (var seed in seeds)
            {
                var c = Get(seed);

                if (seed.X == 0 || seed.X == Width - 1 || seed.Y == 0 || seed.Y == Height - 1)
                    infinites.Add(c);

                foreach (var neighbour in Neighbours(seed))
                {
                    var dc = Get(neighbour);
                    if (dc == Empty)
                    {
                        Set(neighbour, c);
                        changes.Add(neighbour);
                        newSeeds.Add(neighbour);
                    }
                    else if (dc != c && changes.Contains(neighbour))
                    {
                        Set(neighbour, Contested);
                        newSeeds.Remove(neighbour);
                    }
                }
            }

            seeds = newSeeds;
            return seeds.Any();
        }

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

        private void Set(Point p, int c) => buffer[p.Y * Width + p.X] = c;
        private int Get(Point p) => buffer[p.Y * Width + p.X];
    }
}

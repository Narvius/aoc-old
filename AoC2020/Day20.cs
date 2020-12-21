using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day20 : ISolution
    {
        public string PartOne(string[] lines)
            => new SatelliteImage(lines).GetCornerTiles().Aggregate(1L, (n, t) => n * t.Id).ToString();

        public string PartTwo(string[] lines)
            => new SatelliteImage(lines).AssemblePicture().CalculateSeaRoughness().ToString();
    }

    public class SatelliteImage
    {
        private readonly ImageTile[] tiles;
        private Dictionary<Vec, ImageTile> image = new Dictionary<Vec, ImageTile>();
        private Vec bounds;
        private readonly Vec[] deltas = new Vec[] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        public SatelliteImage(string[] input)
        {
            tiles = input.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => new ImageTile(chunk.ToArray())).ToArray();
        }

        public ImageTile[] GetCornerTiles()
        {
            AssemblePicture();
            Vec bounds = (image.Keys.Max(p => p.X), image.Keys.Max(p => p.Y));
            return new[] { image[(0, 0)], image[(bounds.X, 0)], image[(0, bounds.Y)], image[(bounds.X, bounds.Y)] };
        }

        public SatelliteImage AssemblePicture()
        {
            var toPlace = tiles.ToList();
            image[Vec.Zero] = toPlace[0];
            toPlace.RemoveAt(0);
            var candidates = new Queue<Vec>(deltas);

            while (candidates.TryDequeue(out var candidate))
                if (PlaceTile(candidate, toPlace))
                    foreach (var delta in deltas)
                        candidates.Enqueue(candidate + delta);

            var offset = (image.Keys.Min(p => p.X), image.Keys.Min(p => p.Y));

            image = image.ToDictionary(
                kvp => kvp.Key - offset,
                kvp => kvp.Value);

            bounds = ((image.Keys.Max(p => p.X) + 1) * ImageTile.ContentSize, (image.Keys.Max(p => p.Y) + 1) * ImageTile.ContentSize);

            return this;
        }

        private bool PlaceTile(Vec p, List<ImageTile> candidates)
        {
            if (image.ContainsKey(p))
                return false;

            var options = from candidate in candidates
                          from tr in Transform.AllUniqueTransforms
                          let matchesL = !image.TryGetValue(p - (1, 0), out var left) || left.Right().SequenceEqual(candidate.Left(tr))
                          let matchesT = !image.TryGetValue(p - (0, 1), out var top) || top.Bottom().SequenceEqual(candidate.Top(tr))
                          let matchesR = !image.TryGetValue(p + (1, 0), out var right) || right.Left().SequenceEqual(candidate.Right(tr))
                          let matchesB = !image.TryGetValue(p + (0, 1), out var bottom) || bottom.Top().SequenceEqual(candidate.Bottom(tr))
                          where matchesL && matchesT && matchesR && matchesB
                          select (candidate, tr);

            var (tile, transform) = options.FirstOrDefault();

            if (tile != null)
            {
                image[p] = tile;
                tile.Transform = transform;
                candidates.Remove(tile);
                return true;
            }

            return false;
        }

        public int CalculateSeaRoughness()
        {
            int roughness = 0, monsters = 0;
            for (int y = 0; y < bounds.Y; y++)
                for (int x = 0; x < bounds.X; x++)
                {
                    if (RoughAt((x, y))) roughness++;
                    if (SeaMonsterAt((x, y))) monsters++;
                }

            return roughness - monsters * SeaMonsterOffsets.Length;
        }

        private bool RoughAt(Vec p, Transform t = default)
        {
            p = t.Apply(p, bounds);
            if (image.TryGetValue(p / ImageTile.ContentSize, out var tile))
                return tile.IsRoughAt(p % ImageTile.ContentSize);
            return false;
        }

        readonly Vec[] SeaMonsterOffsets = new Vec[]
        {
            (0, 1), (1, 2),
            (4, 2), (5, 1), (6, 1), (7, 2),
            (10, 2), (11, 1), (12, 1), (13, 2),
            (16, 2), (17, 1), (18, 0), (18, 1), (19, 1)
        };

        private bool SeaMonsterAt(Vec p)
            => Transform.AllUniqueTransforms.Any(t => SeaMonsterOffsets.All(o => RoughAt(p + o, t)));
    }

    public class ImageTile
    {
        public const int TileSize = 10;
        public const int ContentSize = 8;

        private bool[,] data;

        public long Id { get; }
        public Transform Transform { get; set; }

        public ImageTile(string[] chunk)
        {
            Id = int.Parse(chunk[0].Trim('T', 'i', 'l', 'e', ':'));
            data = new bool[TileSize, TileSize];

            for (int y = 0; y < TileSize; y++)
                for (int x = 0; x < TileSize; x++)
                    data[x, y] = chunk[y + 1][x] == '#';
        }

        public bool IsRoughAt(Vec p)
        {
            var tp = Transform.Apply(p, (ContentSize, ContentSize));
            return data[1 + tp.X, 1 + tp.Y];
        }

        private bool this[Vec p] => data[p.X, p.Y];
        private static Vec T(Transform t, Vec v) => t.Apply(v, (TileSize, TileSize));

        public IEnumerable<bool> Left(Transform? t = null) => Enumerable.Range(0, TileSize).Select(y => this[T(t ?? Transform, new Vec(0, y))]);
        public IEnumerable<bool> Top(Transform? t = null) => Enumerable.Range(0, TileSize).Select(x => this[T(t ?? Transform, new Vec(x, 0))]);
        public IEnumerable<bool> Right(Transform? t = null) => Enumerable.Range(0, TileSize).Select(y => this[T(t ?? Transform, new Vec(TileSize - 1, y))]);
        public IEnumerable<bool> Bottom(Transform? t = null) => Enumerable.Range(0, TileSize).Select(x => this[T(t ?? Transform, new Vec(x, TileSize - 1))]);
    }

    public readonly struct Transform
    {
        public readonly bool R;
        public readonly bool H;
        public readonly bool V;

        public Transform(bool r, bool h, bool v)
            => (R, H, V) = (r, h, v);

        public static IEnumerable<Transform> AllUniqueTransforms
            => from r in new[] { false, true }
               from h in new[] { false, true }
               from v in new[] { false, true }
               select new Transform(r, h, v);

        public Transform Rotate => new Transform(!R, V ^ R, H ^ R);
        public Transform FlipH => new Transform(R, !H, V);
        public Transform FlipV => new Transform(R, H, !V);

        public Vec Apply(Vec p, Vec bounds)
        {
            int Flip(int v, int b) => (2 * b - v - 1) % b;

            if (R) p = (Flip(p.Y, bounds.Y), p.X);
            if (H) p = (Flip(p.X, bounds.X), p.Y);
            if (V) p = (p.X, Flip(p.Y, bounds.Y));

            return p;
        }

        public static Transform operator *(Transform lhs, Transform rhs)
        {
            var result = lhs;
            if (rhs.R) result = result.Rotate;
            if (rhs.H) result = result.FlipH;
            if (rhs.V) result = result.FlipV;
            return result;
        }
    }
}

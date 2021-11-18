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
        // Identify the corner tiles of the image and multiply together their Ids.
        public string PartOne(string[] lines)
            => new SatelliteImage(lines).GetCornerTiles().Aggregate(1L, (n, t) => n * t.Id).ToString();

        // Identify the number of rough (#) tiles that aren't part of a sea monster.
        public string PartTwo(string[] lines)
            => new SatelliteImage(lines).CalculateSeaRoughness().ToString();
    }

    /// <summary>
    /// The full satellite image.
    /// </summary>
    public class SatelliteImage
    {
        private readonly ImageTile[] tiles;
        private Dictionary<Vec, ImageTile> image = new Dictionary<Vec, ImageTile>();
        private Vec bounds;
        private readonly Vec[] deltas = new Vec[] { (1, 0), (0, 1), (-1, 0), (0, -1) };

        public SatelliteImage(string[] input)
        {
            tiles = input.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => new ImageTile(chunk.ToArray())).ToArray();
            AssemblePicture();
        }

        /// <summary>
        /// Gets the corner tiles of the image.
        /// </summary>
        /// <returns>An array containins the corner tiles of the image.</returns>
        public ImageTile[] GetCornerTiles()
        {
            Vec bounds = (image.Keys.Max(p => p.X), image.Keys.Max(p => p.Y));
            return new[] { image[(0, 0)], image[(bounds.X, 0)], image[(0, bounds.Y)], image[(bounds.X, bounds.Y)] };
        }

        /// <summary>
        /// Builds the picture by picking a random tile to start with and just attaching other tiles to it.
        /// </summary>
        private void AssemblePicture()
        {
            var candidates = tiles.ToList();
            var openCoordinates = new Queue<Vec>(new[] { Vec.Zero });

            while (openCoordinates.TryDequeue(out var p))
                if (PlaceTile(p, candidates))
                    foreach (var delta in deltas)
                        openCoordinates.Enqueue(p + delta);

            // Shift entire image to start at (0, 0) and use positive coordinates.
            var offset = (image.Keys.Min(p => p.X), image.Keys.Min(p => p.Y));
            image = image.ToDictionary(kvp => kvp.Key - offset, kvp => kvp.Value);

            bounds = ((image.Keys.Max(p => p.X) + 1) * ImageTile.ContentSize, (image.Keys.Max(p => p.Y) + 1) * ImageTile.ContentSize);
        }

        /// <summary>
        /// Places a tile in the image.
        /// </summary>
        /// <param name="p">The coordinate to fill</param>
        /// <param name="candidates">Tiles that still need to be placed.</param>
        /// <returns>True if a tile was placed, false otherwise.</returns>
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

        /// <summary>
        /// Counts the number of # tiles and sea monsters in the image, and calculates the total sea roughness based on those.
        /// </summary>
        /// <returns>The total sea roughness.</returns>
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

        /// <summary>
        /// Checks if, under the given transform, the given coordinates are rough (ie. contain a '#').
        /// </summary>
        /// <param name="p">The coordinates to check.</param>
        /// <returns>True if the tile is rough, false otherwise.</returns>
        private bool RoughAt(Vec p)
            => image.TryGetValue(p / ImageTile.ContentSize, out var tile) && tile.IsRoughAt(p % ImageTile.ContentSize);

        /// <summary>
        /// Checks if the given tile is the (possibly-transformed) top-left corner of a sea monster.
        /// </summary>
        /// <remarks>The monster may be oriented any of the 8 possible ways, hance "possibly-transformed".</remarks>
        /// <param name="p">The coordinates to check.</param>
        /// <returns>True if the tile is the top-left corner of a sea monster, false otherwise.</returns>
        private bool SeaMonsterAt(Vec p)
            => Transform.AllUniqueTransforms.Any(t => SeaMonsterOffsets.All(o => RoughAt(t.Apply(p + o, bounds))));

        // Describes the shape of a sea monster.
        readonly Vec[] SeaMonsterOffsets = new Vec[]
        {
            (0, 1), (1, 2),
            (4, 2), (5, 1), (6, 1), (7, 2),
            (10, 2), (11, 1), (12, 1), (13, 2),
            (16, 2), (17, 1), (18, 0), (18, 1), (19, 1)
        };
    }

    /// <summary>
    /// A piece of the satellite image.
    /// </summary>
    public class ImageTile
    {
        public const int TileSize = 10;
        public const int ContentSize = 8;

        private bool[,] data;

        public long Id { get; }

        /// <summary>
        /// The current transform for this tile. Accesses through <see cref="IsRoughAt"/> as well as the four border-querying
        /// calls will all have this applied already.
        /// </summary>
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

        /// <summary>
        /// Returns the left border of this tile under the given transform.
        /// </summary>
        /// <param name="t">Transform to use. Defaults to <see cref="Transform"/>.</param>
        /// <returns>The border as an enumeration of bools (true = '#', false = '.').</returns>
        public IEnumerable<bool> Left(Transform? t = null) => Enumerable.Range(0, TileSize).Select(y => this[T(t ?? Transform, new Vec(0, y))]);

        /// <summary>
        /// Returns the top border of this tile under the given transform.
        /// </summary>
        /// <param name="t">Transform to use. Defaults to <see cref="Transform"/>.</param>
        /// <returns>The border as an enumeration of bools (true = '#', false = '.').</returns>
        public IEnumerable<bool> Top(Transform? t = null) => Enumerable.Range(0, TileSize).Select(x => this[T(t ?? Transform, new Vec(x, 0))]);

        /// <summary>
        /// Returns the right border of this tile under the given transform.
        /// </summary>
        /// <param name="t">Transform to use. Defaults to <see cref="Transform"/>.</param>
        /// <returns>The border as an enumeration of bools (true = '#', false = '.').</returns>
        public IEnumerable<bool> Right(Transform? t = null) => Enumerable.Range(0, TileSize).Select(y => this[T(t ?? Transform, new Vec(TileSize - 1, y))]);

        /// <summary>
        /// Returns the bottom border of this tile under the given transform.
        /// </summary>
        /// <param name="t">Transform to use. Defaults to <see cref="Transform"/>.</param>
        /// <returns>The border as an enumeration of bools (true = '#', false = '.').</returns>
        public IEnumerable<bool> Bottom(Transform? t = null) => Enumerable.Range(0, TileSize).Select(x => this[T(t ?? Transform, new Vec(x, TileSize - 1))]);
    }

    /// <summary>
    /// Describes a spatial transformation allowing for rotations by 90 degrees, as well as flips.
    /// </summary>
    public readonly struct Transform
    {
        public readonly bool R; // Rotation right by 90 degrees.
        public readonly bool H; // Horizontal flip.
        public readonly bool V; // Vertical flip.

        public Transform(bool r, bool h, bool v)
            => (R, H, V) = (r, h, v);

        /// <summary>
        /// Gets every possible spatial transformation given the restrictions.
        /// There are 8 valid ones.
        /// </summary>
        public static IEnumerable<Transform> AllUniqueTransforms
            => from r in new[] { false, true }
               from h in new[] { false, true }
               from v in new[] { false, true }
               select new Transform(r, h, v);

        /// <summary>
        /// This transform but with a 90 degree rotation right added.
        /// </summary>
        public Transform Rotate => new Transform(!R, V ^ R, H ^ R);

        /// <summary>
        /// This transform but with a horizontal flip added.
        /// </summary>
        public Transform FlipH => new Transform(R, !H, V);

        /// <summary>
        /// This transform buy with a vertical flip added.
        /// </summary>
        public Transform FlipV => new Transform(R, H, !V);

        /// <summary>
        /// Applies the transform to a set of coordinates.
        /// </summary>
        /// <param name="p">The coordinates.</param>
        /// <param name="bounds">The bounds of the coordinate space.</param>
        /// <returns>The transformed coordinates.</returns>
        public Vec Apply(Vec p, Vec bounds)
        {
            int Flip(int v, int b) => (2 * b - v - 1) % b;

            if (R) p = (Flip(p.Y, bounds.Y), p.X);
            if (H) p = (Flip(p.X, bounds.X), p.Y);
            if (V) p = (p.X, Flip(p.Y, bounds.Y));

            return p;
        }
    }
}

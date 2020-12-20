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
        private int tileSize;

        public SatelliteImage(string[] input)
        {
            tiles = input.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => new ImageTile(chunk.ToArray())).ToArray();
        }

        public ImageTile[] GetCornerTiles()
            => (from tile in GetMatchableBorders()
                where tile.matchableBorders.Length == 2
                select tile.tile).ToArray();

        private IEnumerable<(ImageTile tile, Direction[] matchableBorders)> GetMatchableBorders()
            => from tile in tiles
               from other in tiles
               where tile != other
               from border in other.Border.Variations
               let direction = tile.Border.SharedEdgeWith(border)
               where direction != null
               group direction.Value by tile into matchedBorders
               select (matchedBorders.Key, matchedBorders.Distinct().ToArray());

        public SatelliteImage AssemblePicture()
        {
            foreach (var (tile, matchableDirections) in GetMatchableBorders())
                tile.AssignRestrictedDirections(matchableDirections);

            Vec tileBounds = (int.MaxValue, int.MaxValue);

            var toPlace = tiles.ToList();

            var start = toPlace.First(t => t.LeftMostCornerDirection != null);
            toPlace.Remove(start);
            image[Vec.Zero] = start;
            start.T = start.WaysToFit(null, new[] { Direction.Left, Direction.Up }).First();

            var candidates = new List<Vec> { (1, 0), (0, 1) };

            while (candidates.Count > 0)
                for (int i = 0; i < candidates.Count; i++)
                {
                    var p = candidates[i];
                    if (image.ContainsKey(p))
                    {
                        candidates.RemoveAt(i);
                        break;
                    }

                    var result = PlaceTile(p, tileBounds, toPlace);
                    if (result == PlacementResult.Succeeded)
                    {
                        candidates.AddRange(NewCandidates(p, tileBounds));
                        candidates.RemoveAt(i);

                        // Nail down the border coordinates after placing the bottom left or top right corner.
                        if (image[p].LeftMostCornerDirection.HasValue)
                            if (p.X == 0)
                            {
                                tileBounds = (tileBounds.X, p.Y + 1);
                                candidates.RemoveAll(v => v.Y > p.Y);
                            }
                            else if (p.Y == 0)
                            {
                                tileBounds = (p.X + 1, tileBounds.Y);
                                candidates.RemoveAll(v => v.X > p.X);
                            }
                        break;
                    }
                    else if (result == PlacementResult.Failed)
                        candidates.RemoveAt(i);
                }

            bounds = tileBounds * image[(0, 0)].ContentSize;
            tileSize = image[(0, 0)].ContentSize;

            return this;
        }

        const int SeaMonsterHeight = 3;
        const int SeaMonsterWidth = 20;
        readonly Vec[] SeaMonsterOffsets = new Vec[]
        {
            (0, 1), (1, 2),
            (4, 2), (5, 1), (6, 1), (7, 2),
            (10, 2), (11, 1), (12, 1), (13, 2),
            (16, 2), (17, 1), (18, 0), (18, 1), (19, 1)
        };

        private void PrintImage(string name, Transform t = default)
        {
            using var f = File.Open(name, FileMode.Create);
            using var sw = new StreamWriter(f);

            for (int y = 0; y < bounds.Y; y++)
            {
                for (int x = 0; x < bounds.X; x++)
                {
                    var p = t.Apply((x, y), bounds);
                    sw.Write(image[p / 8][p % 8]);
                }
                sw.WriteLine();
            }
        }

        public int CalculateSeaRoughness()
        {
            return CalculateSeaRoughnessOld();

            var counts = Transform.AllUniqueTransforms.Select(t => CountMonsters(t)).ToArray();

            return image.Sum(p => p.Value.RoughnessScore) - counts.Max() * SeaMonsterOffsets.Length;
        }

        public int CalculateSeaRoughnessOld()
        {
            PrintImage("map.txt");
            foreach (var transform in Transform.AllUniqueTransforms)
                MarkMonsters(transform);
            PrintImage("monster-map.txt");

            return image.Sum(p => p.Value.RoughnessScore);
        }

        private void MarkMonsters(Transform t)
        {
            for (int y = 0; y < bounds.Y; y++)
                for (int x = 0; x < bounds.X; x++)
                    if (SeaMonsterAt(t, (x, y)))
                        foreach (var offset in SeaMonsterOffsets)
                        {
                            var po = t.Apply((x, y) + offset, bounds);
                            image[po / 8][po % 8] = 'O';
                        }
        }

        private int CountMonsters(Transform t)
        {
            int result = 0;
            for (int y = 0; y < bounds.Y; y++)
                for (int x = 0; x < bounds.X; x++)
                    if (SeaMonsterAt(t, (x, y)))
                        result++;
            return result;
        }

        private bool SeaMonsterAt(Transform t, Vec p)
            => SeaMonsterOffsets.All(o =>
            {
                var po = t.Apply(p + o, bounds);
                return image.TryGetValue(po / tileSize, out var tile) ? tile[po % 8] == '#' : false;
            });

        private IEnumerable<Vec> NewCandidates(Vec p, Vec bounds)
            => from delta in new[] { (-1, 0), (0, -1), (1, 0), (0, 1) }
               let n = p + delta
               where 0 <= n.X && n.X < bounds.X && 0 <= n.Y && n.Y < bounds.Y
               where !image.ContainsKey(n)
               select n;

        private PlacementResult PlaceTile(Vec p, Vec bounds, List<ImageTile> candidates)
        {
            var anchors = GetAnchors(p.X, p.Y);
            var bannedDirections = GetBannedDirections(p, bounds);

            var items = (from candidate in candidates
                         from transform in candidate.WaysToFit(anchors, bannedDirections)
                         select (tile: candidate, transform)).ToArray();

            switch (items.Length)
            {
                case 0: return PlacementResult.Failed;
                case 1:
                    image[p] = items[0].tile;
                    items[0].tile.T = items[0].transform;
                    candidates.Remove(items[0].tile);
                    return PlacementResult.Succeeded;
                default: return PlacementResult.Ambiguous;
            }
        }

        private (ImageTile tile, Direction moving)[] GetAnchors(int x, int y)
            => (from (int x, int y, Direction moving) delta in new[] { (-1, 0, Direction.Right), (0, -1, Direction.Down), (1, 0, Direction.Left), (0, 1, Direction.Up) }
                let key = (x + delta.x, y + delta.y)
                where image.ContainsKey(key)
                select (image[key], delta.moving)).ToArray();

        private Direction[] GetBannedDirections(Vec p, Vec bounds)
            => (from (Direction d, int a, int r) option
                in new[] { (Direction.Left, 0, p.X), (Direction.Up, 0, p.Y), (Direction.Right, bounds.X - 1, p.X), (Direction.Down, bounds.Y - 1, p.Y) }
                where option.a == option.r
                select option.d).ToArray();
    }

    public class ImageTile
    {
        private char[,] content;
        private ImageBorder border;

        public long Id { get; }
        public Transform T;
        public Direction? LeftMostCornerDirection { get; set; }
        public Direction? BannedEdgeDirection { get; set; }
        public int RoughnessScore => content.Cast<char>().Count(c => c == '#');

        public ImageBorder Border => border.TransformedBy(T);
        public int ContentSize => content.GetLength(0);

        public ImageTile(string[] chunk)
        {
            Id = long.Parse(chunk[0].Trim('T', 'i', 'l', 'e', ' ', ':'));
            border = new ImageBorder(chunk);
            content = new char[chunk.Length - 3, chunk[1].Length - 2];
            for (int x = 0; x < chunk[1].Length - 2; x++)
                for (int y = 0; y < chunk.Length - 3; y++)
                    content[x, y] = chunk[y + 2][x + 1];
        }

        public char this[Vec p]
        {
            get
            {
                var tp = T.Apply(p, (ContentSize, ContentSize));
                return content[tp.X, tp.Y];
            }
            set
            {
                var tp = T.Apply(p, (ContentSize, ContentSize));
                content[tp.X, tp.Y] = value;
            }
        }

        public IEnumerable<Transform> WaysToFit((ImageTile tile, Direction moving)[] anchors, Direction[] bannedDirections)
        {
            // Can only put corners and edges along edges.
            if (bannedDirections.Length > 0 && LeftMostCornerDirection == null && BannedEdgeDirection == null)
                return Enumerable.Empty<Transform>();

            bool AvoidsBannedDirections(Transform t)
            {
                if (BannedEdgeDirection.HasValue)
                {
                    var B = t.Apply(BannedEdgeDirection.Value);
                    return bannedDirections.Length == 1 && B == bannedDirections[0];
                }
                else if (LeftMostCornerDirection.HasValue)
                {
                    var L1 = t.Apply(LeftMostCornerDirection.Value);
                    var L2 = t.Apply((Direction)(((int)LeftMostCornerDirection.Value + 1) % 4));
                    return !bannedDirections.Contains(L1) && !bannedDirections.Contains(L2);
                }

                return true;
            }

            return (from transform in Transform.AllUniqueTransforms
                    where AvoidsBannedDirections(transform)
                    let transformedBorder = border.TransformedBy(transform)
                    where anchors?.All(anchor => anchor.tile.Border.SharedEdgeWith(transformedBorder) == anchor.moving) ?? true
                    select transform).ToArray(); 
        }

        public void AssignRestrictedDirections(Direction[] matchableDirections)
        {
            switch (matchableDirections.Length)
            {
                case 2: LeftMostCornerDirection = Enum.GetValues(typeof(Direction)).Cast<Direction>().First(d => matchableDirections.Contains(d) && matchableDirections.Contains((Direction)((1 + (int)d) % 4))); return;
                case 3: BannedEdgeDirection = Enum.GetValues(typeof(Direction)).Cast<Direction>().First(d => !matchableDirections.Contains(d)); return;
                default: return;
            }
        }
    }

    [DebuggerDisplay("({Left}, {Top}, {Right}, {Bottom}) [{T}]")]
    public readonly struct ImageBorder
    {
        public readonly ushort BaseLeft;
        public readonly ushort BaseTop;
        public readonly ushort BaseRight;
        public readonly ushort BaseBottom;
        public readonly Transform T;

        public ushort Left => EdgeFromTransformation(Direction.Left);
        public ushort Top => EdgeFromTransformation(Direction.Up);
        public ushort Right => EdgeFromTransformation(Direction.Right);
        public ushort Bottom => EdgeFromTransformation(Direction.Down);

        public ImageBorder(
            ushort left, ushort top, ushort right, ushort bottom, Transform transform = default)
            =>  (BaseLeft, BaseTop, BaseRight, BaseBottom, T) = (left, top, right, bottom, transform);

        public ImageBorder(string[] chunk)
        {
            string MakeBinaryString(IEnumerable<char> chars)
                => new string(chars.Reverse().ToArray()).Replace('#', '1').Replace('.', '0');

            var leftString = MakeBinaryString(chunk.Skip(1).Select(line => line[0]));
            var topString = MakeBinaryString(chunk[1]);
            var rightString = MakeBinaryString(chunk.Skip(1).Select(line => line.Last()));
            var bottomString = MakeBinaryString(chunk.Last());

            (BaseLeft, BaseTop, BaseRight, BaseBottom, T) = (Convert.ToUInt16(leftString, 2), Convert.ToUInt16(topString, 2), Convert.ToUInt16(rightString, 2), Convert.ToUInt16(bottomString, 2), default);
        }

        public ImageBorder TransformedBy(Transform t) => new ImageBorder(BaseLeft, BaseTop, BaseRight, BaseBottom, t);
        public ImageBorder Rotate => TransformedBy(T.Rotate);
        public ImageBorder FlipH => TransformedBy(T.FlipH);
        public ImageBorder FlipV => TransformedBy(T.FlipV);

        public ImageBorder[] Variations
        {
            get
            {
                var @this = this;
                return Transform.AllUniqueTransforms.Select(t => @this.TransformedBy(t)).ToArray();
            }
        }

        private ushort EdgeFromTransformation(Direction direction)
        {
            var (d, f) = T.MapEdge(direction);

            var raw = d switch { Direction.Left => BaseLeft, Direction.Up => BaseTop, Direction.Right => BaseRight, Direction.Down => BaseBottom, _ => throw new Exception() };

            return f ? Reverse(raw) : raw;
        }

        private static ushort Reverse(ushort value)
        {
            int result = 0;
            for (int i = 0; i < 10; i++)
            {
                var bit = (value & (1 << i)) >> i;
                result |= bit << (9 - i);
            }
            return (ushort)result;
        }

        public Direction? SharedEdgeWith(ImageBorder other)
        {
            if (Left == other.Right) return Direction.Left;
            if (Top == other.Bottom) return Direction.Up;
            if (Right == other.Left) return Direction.Right;
            if (Bottom == other.Top) return Direction.Down;
            return null;
        }
    }

    [DebuggerDisplay("{StatusString}")]
    public readonly struct Transform
    {
        public readonly bool R;
        public readonly bool H;
        public readonly bool V;

        public string StatusString => $"{(R ? "R" : ".")}{(H ? "H" : ".")}{(V ? "V" : ".")}";

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

        public Direction Apply(Direction direction)
        {
            var horizontal = R ^ ((int)direction % 2) == 0;

            if (R) direction = (Direction)(((int)direction + 1) % 4);
            if (H && horizontal) direction = 2 - direction;
            if (V && !horizontal) direction = 4 - direction;

            return direction;
        }

        public (Direction direction, bool flipped) MapEdge(Direction direction)
        {
            var flipped = IsBorderEdgeFlipped(direction);

            if (R) direction = (Direction)(((int)direction + 3) % 4);
            var horizontal = ((int)direction % 2) == 0;
            var (h, v) = R ? (V, H) : (H, V);
            if (h && horizontal) direction = 2 - direction;
            if (v && !horizontal) direction = 4 - direction;

            return (direction, flipped);
        }

        public bool IsBorderEdgeFlipped(Direction direction)
        {
            var horizontal = ((int)direction % 2) == 0;
            var affectedByRotation = R & !horizontal;
            var affectedByH = H && !horizontal;
            var affectedByV = V && horizontal;

            return affectedByRotation ^ affectedByH ^ affectedByV;
        }

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

    public enum PlacementResult : byte { Failed, Ambiguous, Succeeded }
    public enum Direction : byte { Left, Up, Right, Down }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day20 : ISolution
    {
        public string PartOne(string[] lines)
            => new SatelliteImage(lines).GetCornerTiles().Aggregate(1L, (n, t) => n * t.Id).ToString();

        public string PartTwo(string[] lines)
        {
            throw new NotImplementedException();
        }
    }

    public class SatelliteImage
    {
        private readonly ImageTile[] tiles;
        private ImageTile[,] image;

        public SatelliteImage(string[] input)
        {
            tiles = input.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => new ImageTile(chunk.ToArray())).ToArray();
        }

        public ImageTile[] GetCornerTiles()
        {
            return (from count in GetMatchableDirections()
                    where count.matchedDirections.Length == 2
                    select count.tile).ToArray();
        }

        private IEnumerable<(ImageTile tile, Direction[] matchedDirections)> GetMatchableDirections()
            => from tile in tiles
               from other in tiles
               where tile != other
               let variations = tile.Border.AllVariations
               from border in variations
               let direction = border.CanAttachTo(other.Border)
               where direction != null
               group border.Current(direction.Value) by tile into matchedBorders
               select (matchedBorders.Key, Items: matchedBorders.Distinct().ToArray());

        public SatelliteImage AssemblePicture()
        {
            var groups = (from item in GetMatchableDirections()
                          group item by item.matchedDirections.Length into g
                          select g).ToArray();

            var corners = groups.First(g => g.Key == 2);
            var borders = groups.First(g => g.Key == 3);
            var body = groups.First(g => g.Key == 4);

            // Rotate the first tile such that 

            return this;
        }
    }

    public class ImageTile
    {
        public long Id { get; }
        private char[,] content;
        private ImageBorder border;

        public ImageBorder Border => border.TransformedBy(Rotation, FlippedH, FlippedV);
        public Rotation Rotation { get; set; } = Rotation.None;
        public bool FlippedH { get; set; } = false;
        public bool FlippedV { get; set; } = false;

        public ImageTile(string[] chunk)
        {
            Id = long.Parse(chunk[0].Trim('T', 'i', 'l', 'e', ' ', ':'));
            border = new ImageBorder(chunk);
            content = new char[chunk.Length - 3, chunk[1].Length - 2];
            for (int x = 0; x < chunk[1].Length - 2; x++)
                for (int y = 0; y < chunk.Length - 3; y++)
                    content[x, y] = chunk[y + 2][x + 1];
        }


    }

    public readonly struct ImageBorder
    {
        public readonly ushort Left;
        public readonly ushort Top;
        public readonly ushort Right;
        public readonly ushort Bottom;

        public readonly Direction CurrentLeft;
        public readonly Direction CurrentTop;
        public readonly Direction CurrentRight;
        public readonly Direction CurrentBottom;

        public ImageBorder(
            ushort left, ushort top, ushort right, ushort bottom,
            Direction currentLeft = Direction.Left, Direction currentTop = Direction.Top, Direction currentRight = Direction.Right, Direction currentBottom = Direction.Bottom)
        {
            (Left, Top, Right, Bottom) = (left, top, right, bottom);
            (CurrentLeft, CurrentTop, CurrentRight, CurrentBottom) = (currentLeft, currentTop, currentRight, currentBottom);
        }

        public ImageBorder(string[] chunk)
        {
            string MakeBinaryString(IEnumerable<char> chars)
                => new string(chars.Reverse().ToArray()).Replace('#', '1').Replace('.', '0');

            var leftString = MakeBinaryString(chunk.Skip(1).Select(line => line[0]));
            var topString = MakeBinaryString(chunk[1]);
            var rightString = MakeBinaryString(chunk.Skip(1).Select(line => line.Last()));
            var bottomString = MakeBinaryString(chunk.Last());

            (Left, Top, Right, Bottom) = (Convert.ToUInt16(leftString, 2), Convert.ToUInt16(topString, 2), Convert.ToUInt16(rightString, 2), Convert.ToUInt16(bottomString, 2));
            (CurrentLeft, CurrentTop, CurrentRight, CurrentBottom) = (Direction.Left, Direction.Top, Direction.Right, Direction.Bottom);
        }

        public Direction Current(Direction direction)
            => direction switch { Direction.Left => CurrentLeft, Direction.Top => CurrentTop, Direction.Right => CurrentRight, Direction.Bottom => CurrentBottom, _ => throw new Exception() };

        public ImageBorder TransformedBy(Rotation r, bool horizontalFlip, bool verticalFlip)
        {
            var result = r switch { Rotation.None => this, Rotation.R => Rotate, Rotation.RR => Rotate.Rotate, Rotation.RRR => Rotate.Rotate.Rotate, _ => throw new Exception() };
            if (horizontalFlip)
                result = result.FlipH;
            if (verticalFlip)
                result = result.FlipV;
            return result;
        }

        public ImageBorder FlipV
            => new ImageBorder(Reverse(Left), Bottom, Reverse(Right), Top, CurrentLeft, CurrentBottom, CurrentRight, CurrentTop);

        public ImageBorder FlipH
            => new ImageBorder(Right, Reverse(Top), Left, Reverse(Bottom), CurrentRight, CurrentTop, CurrentLeft, CurrentBottom);

        public ImageBorder Rotate
            => new ImageBorder(Bottom, Reverse(Left), Top, Reverse(Right), CurrentBottom, CurrentLeft, CurrentTop, CurrentRight);


        public ImageBorder[] AllVariations
        {
            get
            {
                var @this = this;
                return (from rotation in new[] { Rotation.None, Rotation.R, Rotation.RR, Rotation.RRR }
                        from flipH in new[] { false, true }
                        from flipV in new[] { false, true }
                        select @this.TransformedBy(rotation, flipH, flipV)).ToArray();
            }
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

        public Direction? CanAttachTo(ImageBorder other)
        {
            if (Right == other.Left) return Direction.Left;
            if (Bottom == other.Top) return Direction.Top;
            if (Left == other.Right) return Direction.Right;
            if (Top == other.Bottom) return Direction.Bottom;
            return null;
        }
    }

    public enum Direction : byte { Left, Top, Right, Bottom }
    public enum Rotation : byte { None, R, RR, RRR }
}

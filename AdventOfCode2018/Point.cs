using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2018
{
    public struct Point : IEquatable<Point>
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point lhs, Point rhs) => new Point(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static implicit operator Point((int x, int y) p) => new Point(p.x, p.y);

        public bool Equals(Point other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is Point p && Equals(p);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
}

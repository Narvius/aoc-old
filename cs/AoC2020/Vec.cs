using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AoC2020
{
    [DebuggerDisplay("({X}, {Y})")]
    public readonly struct Vec : IEquatable<Vec>
    {
        /// <summary>
        /// The x-component of the <see cref="Vec"/>.
        /// </summary>
        public int X { get; }
        /// <summary>
        /// The y-component of the <see cref="Vec"/>.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Whether this <see cref="Vec"/> represents a cardinal direction: North, east, south or west.
        /// </summary>
        public bool Cardinal => (X == 0 || Y == 0) && Math.Abs(X + Y) == 1;
        /// <summary>
        /// Whether this <see cref="Vec"/> represents an ordinal (intercardinal) direction: North-west, north-east, south-east or north-west.
        /// </summary>
        public bool Ordinal => Math.Abs(X) == 1 && Math.Abs(Y) == 1;

        public Vec(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The taxicab distance between two <see cref="Vec"/>s.
        /// </summary>
        public int FlatDistanceTo(Vec other)
            => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        /// <summary>
        /// Returns the <see cref="Vec"/> obtained by rotating this one to the right by 90 degrees around the origin.
        /// </summary>
        public Vec RotatedRight() => (-Y, X);

        /// <summary>
        /// Returns the <see cref="Vec"/> obtained by rotating this one to the left by 90 degrees around the origin.
        /// </summary>
        /// <returns></returns>
        public Vec RotatedLeft() => (Y, -X);

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public override bool Equals(object obj) => obj is Vec v && Equals(v);
        public bool Equals(Vec other)
            => X == other.X
            && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static readonly Vec Zero = new Vec(0, 0);

        public static Vec operator +(Vec lhs, Vec rhs) => new Vec(lhs.X + rhs.X, lhs.Y + rhs.Y);
        public static Vec operator -(Vec lhs, Vec rhs) => new Vec(lhs.X - rhs.X, lhs.Y - rhs.Y);
        public static Vec operator *(Vec lhs, int rhs) => new Vec(lhs.X * rhs, lhs.Y * rhs);
        public static Vec operator *(int lhs, Vec rhs) => new Vec(lhs * rhs.X, lhs * rhs.Y);
        public static Vec operator /(Vec lhs, int rhs) => new Vec(lhs.X / rhs, lhs.Y / rhs);
        public static Vec operator %(Vec lhs, int rhs) => new Vec(lhs.X % rhs, lhs.Y % rhs);

        public static Vec operator -(Vec @this) => new Vec(-@this.X, -@this.Y);

        public static bool operator ==(Vec lhs, Vec rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(Vec lhs, Vec rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;

        public static implicit operator Vec((int x, int y) vec)
            => new Vec(vec.x, vec.y);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2018
{
    public struct Rectangle : IEnumerable<Point>
    {
        public int X { get; }
        public int Y { get; }
        public int W { get; }
        public int H { get; }

        public int Area => W * H;

        public Rectangle(int x, int y, int w, int h) : this()
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public static Rectangle FromLTRB(int left, int top, int right, int bottom)
            => new Rectangle(left, top, right - left, bottom - top);

        private IEnumerable<Point> Points()
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    yield return new Point(X + x, Y + y);
        }

        public IEnumerator<Point> GetEnumerator() => Points().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

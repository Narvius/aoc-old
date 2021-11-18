﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC2019
{
    public class Day03 : ISolution
    {
        // On a grid with two polylines, find the intersection that is closest to the origin (0, 0).
        public string PartOne(string[] lines)
        {
            var a = AllLinePoints(lines[0]);
            var b = AllLinePoints(lines[1]);

            var intersections = a.Keys.Intersect(b.Keys);

            return (from point in intersections
                    let distance = Math.Abs(point.x) + Math.Abs(point.y)
                    orderby distance ascending
                    select distance).First().ToString();
        }

        // On a grid with two polylines, find the intersection that forms the shortest loop
        // with the polyline segments from (0, 0) to the intersection.
        public string PartTwo(string[] lines)
        {
            var a = AllLinePoints(lines[0]);
            var b = AllLinePoints(lines[1]);

            var intersectionSteps = a.Keys.Intersect(b.Keys).Select(k => a[k] + b[k]);

            return intersectionSteps.Min().ToString();
        }

        private Dictionary<(int x, int y), int> AllLinePoints(string line)
        {
            return AllLinePoints(line.Split(',').Select(ParseDirection));

            static (int x, int y) ParseDirection(string s)
            {
                var value = int.Parse(s.Substring(1));
                return (s[0]) switch
                {
                    'L' => (-value, 0),
                    'U' => (0, -value),
                    'R' => (value, 0),
                    'D' => (0, value),
                    _ => throw new Exception("invalid direction letter"),
                };
            }
        }

        private Dictionary<(int x, int y), int> AllLinePoints(IEnumerable<(int x, int y)> deltas)
        {
            int x = 0, y = 0, t = 0;
            var result = new Dictionary<(int, int), int>();
            foreach (var (dx, dy) in deltas)
            {
                int sx = Math.Sign(dx), sy = Math.Sign(dy), steps = Math.Abs(dx) + Math.Abs(dy);
                for (int i = 0; i < steps; i++)
                {
                    x += sx;
                    y += sy;
                    t++;
                    if (!result.ContainsKey((x, y)))
                        result[(x, y)] = t;
                }
            }
            return result;
        }
    }
}

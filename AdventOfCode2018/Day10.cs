using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2018
{
    public class Day10 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var starfield = new Starfield(lines.Select(Star.Parse));
            starfield.StepUntilLikelyCandidate();
            starfield.Interactive();

            return "<manually scan for the answer>";
        }

        public string PartTwo(string[] lines) => "<manually scan for the answer>";
    }

    // Represents a single light (position and velocity pair).
    public class Star
    {
        public Point Position { get; set; }
        public Point Velocity { get; }

        public Star(Point position, Point velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        private static readonly Regex StarParse = new Regex(@"\<\s*(-?\d+),\s*(-?\d+)\>\ velocity=\<\s*(-?\d+),\s*(-?\d+)");
        public static Star Parse(string line)
        {
            var match = StarParse.Match(line);
            return new Star(
                (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)),
                (int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value)));
        }
    }

    // Represents a collection of lights.
    public class Starfield
    {
        private readonly List<Star> Stars;
        private int stepCount = 0;

        public Starfield(IEnumerable<Star> stars)
        {
            Stars = stars.ToList();
        }

        // Advances the simulation by one step.
        public void Step()
        {
            foreach (var star in Stars)
                star.Position += star.Velocity;
            stepCount++;
        }

        // Rolls the simulation back by one step.
        public void StepBack()
        {
            foreach (var star in Stars)
                star.Position -= star.Velocity;
            stepCount--;
        }

        // Advance the starfield until a likely candidate frame.
        // This is done by finding the first instance of the sum of distances to first light INCREASING rather than decreasing.
        public void StepUntilLikelyCandidate()
        {
            int a = 0, b = DistanceSum();
            do
            {
                a = b;
                Step();
                b = DistanceSum();
            }
            while ((b - a) < 0);
        }
        
        // The sum of distances to the first light.
        public int DistanceSum()
            => Stars.Sum(s => Stars[0].Position.ManhattanDistance(s.Position));

        // The light at the given position.
        public Star ByPosition(Point p)
            => Stars.FirstOrDefault(s => s.Position == p);

        // Allows the user to step through the simulation forward and back,
        // and prints out the most relevant rectangle of it.
        public void Interactive()
        {
            var left = Stars.Min(s => s.Position.X);
            var top = Stars.Min(s => s.Position.Y);

            while (true)
            {
                Console.Clear();
                for (int y = 0; y < 25; y++)
                {
                    for (int x = 0; x < 80; x++)
                        Console.Write(ByPosition((left + x, top + y)) != null ? "#" : ".");
                    Console.WriteLine();
                }
                Console.WriteLine($"Steps: {stepCount}");

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.LeftArrow: StepBack(); break;
                    case ConsoleKey.RightArrow: Step(); break;
                    case ConsoleKey.Escape: return;
                    default: break;
                }
            }
        }
    }
}

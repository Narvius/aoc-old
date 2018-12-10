using System;
using System.Collections.Generic;
using System.IO;
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

            return $"{Environment.NewLine}{starfield.PrintField()}";
        }

        public string PartTwo(string[] lines)
        {
            var starfield = new Starfield(lines.Select(Star.Parse));
            starfield.StepUntilLikelyCandidate();
            return starfield.CurrentStep.ToString();
        }
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
        public int CurrentStep { get; private set; }

        public Starfield(IEnumerable<Star> stars)
        {
            Stars = stars.ToList();
        }

        // Advances the simulation by one step.
        public void Step()
        {
            foreach (var star in Stars)
                star.Position += star.Velocity;
            CurrentStep++;
        }

        // Rolls the simulation back by one step.
        public void StepBack()
        {
            foreach (var star in Stars)
                star.Position -= star.Velocity;
            CurrentStep--;
        }

        // Advance the starfield until a likely candidate frame.
        // This is done by finding the first step on which the bounding rectangle is minimal.
        public void StepUntilLikelyCandidate()
        {
            int a = 0, b = BoundingRect().Area;
            do
            {
                a = b;
                Step();
                b = BoundingRect().Area;
            }
            while ((b - a) < 0);
            StepBack();
        }
        
        // The smallest rectangle that contains all lights.
        public Rectangle BoundingRect()
            => Rectangle.FromLTRB(
                Stars.Min(s => s.Position.X),
                Stars.Min(s => s.Position.Y),
                Stars.Max(s => s.Position.X),
                Stars.Max(s => s.Position.Y));

        // The light at the given position.
        public Star ByPosition(Point p)
            => Stars.FirstOrDefault(s => s.Position == p);

        public string PrintField()
        {
            var rect = BoundingRect();

            using (var stream = new MemoryStream())
            using (var sw = new StreamWriter(stream))
            {
                for (int y = 0; y <= rect.H; y++)
                {
                    for (int x = 0; x <= rect.W; x++)
                        sw.Write(ByPosition((rect.X + x, rect.Y + y)) != null ? "#" : ".");
                    sw.WriteLine();
                }
                sw.Flush();

                return new string(Encoding.ASCII.GetString(stream.ToArray()));
            }
        }

        // Allows the user to step through the simulation forward and back,
        // and prints out the most relevant rectangle of it.
        public void Interactive()
        {
            var left = Stars.Min(s => s.Position.X);
            var top = Stars.Min(s => s.Position.Y);

            while (true)
            {
                Console.Clear();
                
                Console.WriteLine($"Steps: {CurrentStep}");

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

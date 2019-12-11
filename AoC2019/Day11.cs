using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day11 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var field = new byte[1001, 1001];
            var robot = new Robot(lines[0], (500, 500));
            var visited = new HashSet<(int, int)>();

            while (robot.Step(field))
                visited.Add(robot.Position);

            return visited.Count.ToString();
        }

        public string PartTwo(string[] lines)
        {
            var field = new byte[1001, 1001];
            var robot = new Robot(lines[0], (500, 500));

            field[500, 500] = 1;

            while (robot.Step(field)) ;

            File.WriteAllLines("Day11_output.txt", OutputLines(field));
            return "check file 'Day11_output.txt'";
        }

        IEnumerable<string> OutputLines(byte[,] field)
        {
            // find rectangle containing the actual solution
            var topLeft = (x: int.MaxValue, y: int.MaxValue);
            var bottomRight = (x: int.MinValue, y: int.MinValue);

            for (int y = 0; y < field.GetLength(1); y++)
                for (int x = 0; x < field.GetLength(0); x++)
                    if (field[x, y] > 0)
                    {
                        topLeft.x = Math.Min(topLeft.x, x);
                        topLeft.y = Math.Min(topLeft.y, y);
                        bottomRight.x = Math.Max(bottomRight.x, x);
                        bottomRight.y = Math.Max(bottomRight.y, y);
                    }

            // print only that rectangle
            var width = bottomRight.x - topLeft.x;
            for (int y = topLeft.y; y <= bottomRight.y; y++)
                yield return new string(Enumerable.Range(topLeft.x, width + 1).Select(x => field[x, y] == 1 ? '█' : '.').ToArray());
        }
    }

    public class Robot
    {
        private readonly Computer.V4 Brain;

        private (int x, int y) position;
        private Dir direction;

        public (int x, int y) Position => position;

        public Robot(string program, (int x, int y) startPosition)
        {
            Brain = new Computer.V4(program, 10000);
            position = startPosition;
        }

        public bool Step(byte[,] field)
        {
            if (Brain.State == Computer.V4.ExecutionState.Halted)
                return false;

            Brain.Input.Enqueue(field[position.x, position.y]);
            Brain.RunWhilePossible();
            var tileColor = Brain.Output.Dequeue();
            var right = Brain.Output.Dequeue() == 1;

            field[position.x, position.y] = (byte)tileColor;
            Advance(right, ref position, ref direction);

            return true;
        }

        private static Dir TurnedLeft(Dir dir) => (Dir)(((int)dir + 3) % 4);
        private static Dir TurnedRight(Dir dir) => (Dir)(((int)dir + 1) % 4);
        private void Advance(bool right, ref (int x, int y) pos, ref Dir dir)
        {
            dir = right ? TurnedRight(dir) : TurnedLeft(dir);
            switch (dir)
            {
                case Dir.Left: pos.x -= 1; return;
                case Dir.Up: pos.y -= 1; return;
                case Dir.Right: pos.x += 1; return;
                case Dir.Down: pos.y += 1; return;
            }
        }

        public enum Dir { Left, Up, Right, Down }
    }
}

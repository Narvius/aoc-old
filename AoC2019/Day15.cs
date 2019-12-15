using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day15 : ISolution
    {
        // Find the shortest path between the starting position and the goal position.
        public string PartOne(string[] lines)
            => new RobotMap(lines[0]).ShortestPathBetweenStartAndGoal().ToString();

        // Floodfill the map starting at the goal position; find longest path.
        public string PartTwo(string[] lines)
            => new RobotMap(lines[0]).TimeToFillUpCompletely().ToString();
    }

    // Constructs the map stores in the program without running it.
    // See Data\Day15_ANNOTATED.txt to make sense of the comments in this.
    public class RobotMap
    {
        public readonly int Width;
        public readonly int Height;

        public readonly (int x, int y) Goal;
        public readonly (int x, int y) StartPosition;

        private readonly char[,] map;

        public RobotMap(string program)
        {
            var cells = program.Split(',').Select(int.Parse).ToArray();

            // Fact 1: The right and bottom edges ([width - 1] and [height - 1], respectively) are stored in
            //         cells 132 (width) and 139 (height).
            // Reason: That code is part of BOUNDS_CHECK, which performs bounds checking. If the new x/y coordinates match
            //         those cells, the move fails--which mean the width/height are larger than those by 1.
            Width = cells[132] + 1;
            Height = cells[139] + 1;
            map = new char[Width, Height];

            // Fact 2: The initial position is stored in cells 1034 (x) and 1035 (y).
            // Reason: Those cells are used in the program to keep track of the player position.
            StartPosition = (cells[1034], cells[1035]);

            // Fact 3: The position of the goal is stored in cells 146 (x) and 153 (y);
            // Reason: Those cells are part of the code for BOUNDS_CHECK, where bounds and goal checking is done.
            //         If those two coordinates are matched, the program outputs 2, which is the code for the goal.
            Goal = (cells[146], cells[153]);

            // Fact 4: The border is solid.
            // Reason: BOUNDS_CHECK checks for border coordinates, and fails the move if any of the 4 border conditions is met.
            for (int x = 0; x < Width; x++)
            {
                map[x, 0] = '#';
                map[x, Height - 1] = '#';
            }
            for (int y = 0; y < Height; y++)
            {
                map[0, y] = '#';
                map[Width - 1, y] = '#';
            }

            // Fact 5: A quarter of all tiles are guaranteed walls.
            // Reason: In GET_MOVE_RESULT, if the horizontal and vertical flip flags are 0, the move fails.
            for (int x = 2; x < Width; x += 2)
                for (int y = 2; y < Height; y += 2)
                    map[x, y] = '#';

            // Fact 6: A quarter of all tiles are guaranteed floor.
            // Reason: In NORMAL_MOVE, if both the horizontal and vertical flip flags are 1, the move succeeds.
            for (int x = 1; x < (Width - 1); x += 2)
                for (int y = 1; y < (Height - 1); y += 2)
                    map[x, y] = '.';

            // Fact 7: The type of cell for all other cells is stored in the data blob.
            // Reason: GET_MOVE_RESULT has a complex calculation involving the current "temp r" and "temp x" values,
            //         the result of which is an index into the data section.
            const int DATA_OFFSET = 252;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (map[x, y] == '\0')
                    {
                        var r = y / 2; // Reconstructs the value of "r", which is kept track of separately from "y" in the program.
                        var v = y % 2; // Reconstructs the "v", which is kept track of separately from "y" in the program.

                        map[x, y] = 45 > cells[(r + v - 1) * 39 + x - 1 + DATA_OFFSET] ? '.' : '#';
                    }

            Print();
        }

        // Floodfills the map starting at the given point.
        // If an end point is given and found, returns the shortest path between those two.
        // If no end point is given, the maximum value reached in the floodfill is returned.
        private int FloodFill((int x, int y) start, (int x, int y)? end)
        {
            var fill = new int[Width, Height];
            var toCheck = new Stack<(int x, int y)>();
            toCheck.Push(start);

            while (toCheck.Count > 0)
            {
                var (x, y) = toCheck.Pop();
                foreach (var (nx, ny) in Neighbours(x, y))
                    if ((nx, ny) == end)
                        return fill[x, y] + 1;
                    else if ((nx, ny) == start)
                        continue;
                    else if (map[nx, ny] == '#')
                        continue;
                    else if (fill[nx, ny] != 0)
                        continue;
                    else
                    {
                        fill[nx, ny] = fill[x, y] + 1;
                        toCheck.Push((nx, ny));
                    }
            }

            return (from int item in fill
                    select item).Max();
        }

        private static IEnumerable<(int x, int y)> Neighbours(int x, int y)
        {
            yield return (x - 1, y);
            yield return (x, y - 1);
            yield return (x + 1, y);
            yield return (x, y + 1);
        }

        public int ShortestPathBetweenStartAndGoal() => FloodFill(StartPosition, Goal);
        public int TimeToFillUpCompletely() => FloodFill(Goal, null);

        private void Print()
        {
            Console.Clear();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    Console.Write(map[x, y]);
                Console.WriteLine();
            }

            Console.SetCursorPosition(21, 21);
            Console.Write('@');

            Console.SetCursorPosition(Goal.x, Goal.y);
            Console.Write('<');

            Console.ReadKey();
        }
    }
}

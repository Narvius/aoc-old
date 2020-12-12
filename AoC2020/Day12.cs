using System;
using System.Collections.Generic;
using System.Text;

namespace AoC2020
{
    public class Day12 : ISolution
    {
        // Use instructions to move the ship; get final position.
        public string PartOne(string[] lines)
            => new Ferry().RunCourse(lines).FlatDistanceTo((0, 0)).ToString();

        // Use instructions to move the waypoint (and sometimes the ship); get final position.
        public string PartTwo(string[] lines)
            => new Ferry().RunWaypointBasedCourse(lines).FlatDistanceTo((0, 0)).ToString();
    }

    /// <summary>
    /// Holds methods to run the ferry according to puzzle specifications.
    /// </summary>
    public class Ferry
    {
        /// <summary>
        /// Using the input as instructions for the ship, simulates that course.
        /// </summary>
        /// <param name="course">The puzzle input.</param>
        /// <returns>The final position of the ship.</returns>
        public Vec RunCourse(string[] course)
        {
            Vec heading = (1, 0);
            Vec position = (0, 0);

            foreach (var line in course)
            {
                int argument = int.Parse(line.Substring(1));
                heading = Rotate(heading, line[0], argument);
                position += argument * GetHeading(line[0], heading);
            }

            return position;
        }

        /// <summary>
        /// Using the input as instructions for the waypoint, simulates that course.
        /// </summary>
        /// <param name="course">The puzzle input.</param>
        /// <returns>The final position of the ship.</returns>
        public Vec RunWaypointBasedCourse(string[] course)
        {
            Vec waypoint = (10, -1);
            Vec position = (0, 0);

            foreach (var line in course)
            {
                int argument = int.Parse(line.Substring(1));
                waypoint = Rotate(waypoint, line[0], argument);
                if (line[0] == 'F')
                    position += argument * waypoint;
                else
                    waypoint += argument * GetHeading(line[0]);
            }

            return position;
        }

        /// <summary>
        /// Rotates the vector in the provided direction in 90 degree increments the correct amount of times.
        /// </summary>
        /// <param name="vec">The vector to rotate.</param>
        /// <param name="direction">The direction to rotate in. Values other than 'L' and 'R' will result in no rotation.</param>
        /// <param name="degrees">The amount of degrees to rotate by. Only increments of 90 matter; more precision than that will be discarded.</param>
        /// <returns>The rotated vector.</returns>
        private Vec Rotate(Vec vec, char direction, int degrees)
        {
            for (int i = 0; i < degrees / 90; i++)
                vec = direction switch
                {
                    'L' => vec.RotatedLeft(),
                    'R' => vec.RotatedRight(),
                    _ => vec
                };

            return vec;
        }
        
        /// <summary>
        /// Returns a cardinal vector corresponding to the given letter.
        /// </summary>
        /// <param name="direction">The direction. Values other than 'N', 'S', 'E', 'W' and 'F' will result in a zero vector. 'F' is only valid if <paramref name="forward"/> is given.</param>
        /// <param name="forward">The vector to return when the direction is 'F'.</param>
        /// <returns>The vector corresponding to the given direction letter.</returns>
        private Vec GetHeading(char direction, Vec? forward = null)
            => direction switch
            {
                'N' => (0, -1),
                'S' => (0, 1),
                'E' => (1, 0),
                'W' => (-1, 0),
                'F' => forward ?? throw new Exception($"forward heading with null '{forward}' argument"),
                _ => (0, 0)
            };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day12 : ISolution
    {
        // Use instructions to move the ship; get final position.
        public string PartOne(string[] lines)
            => new Ferry().RunCourseF(lines).FlatDistanceTo((0, 0)).ToString();

        // Use instructions to move the waypoint (and sometimes the ship); get final position.
        public string PartTwo(string[] lines)
            => new Ferry().RunWaypointBasedCourseF(lines).FlatDistanceTo((0, 0)).ToString();
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
        public Vec RunCourseF(string[] course)
        {
            (Vec p, Vec d) Step((Vec p, Vec d) state, string instruction)
            {
                int argument = int.Parse(instruction.Substring(1));
                return instruction[0] switch
                {
                    'L' => (state.p, Enumerable.Range(0, argument / 90).Aggregate(state.d, (d, _) => d.RotatedLeft())),
                    'R' => (state.p, Enumerable.Range(0, argument / 90).Aggregate(state.d, (d, _) => d.RotatedRight())),
                    'N' => (state.p + (0, -argument), state.d),
                    'E' => (state.p + (argument, 0), state.d),
                    'S' => (state.p + (0, argument), state.d),
                    'W' => (state.p + (-argument, 0), state.d),
                    'F' => (state.p + argument * state.d, state.d),
                    _ => throw new Exception("invalid instruction")
                };
            }

            (Vec position, Vec direction) initialState = ((0, 0), (1, 0));
            return course.Aggregate(initialState, Step).position;
        }

        /// <summary>
        /// Using the input as instructions for the waypoint, simulates that course.
        /// </summary>
        /// <param name="course">The puzzle input.</param>
        /// <returns>The final position of the ship.</returns>
        public Vec RunWaypointBasedCourseF(string[] course)
        {
            (Vec p, Vec w) Step((Vec p, Vec w) state, string instruction)
            {
                int argument = int.Parse(instruction.Substring(1));
                return instruction[0] switch
                {
                    'L' => (state.p, Enumerable.Range(0, argument / 90).Aggregate(state.w, (w, _) => w.RotatedLeft())),
                    'R' => (state.p, Enumerable.Range(0, argument / 90).Aggregate(state.w, (w, _) => w.RotatedRight())),
                    'N' => (state.p, state.w + (0, -argument)),
                    'E' => (state.p, state.w + (argument, 0)),
                    'S' => (state.p, state.w + (0, argument)),
                    'W' => (state.p, state.w + (-argument, 0)),
                    'F' => (state.p + argument * state.w, state.w),
                    _ => throw new Exception("invalid instruction")
                };
            }

            (Vec position, Vec waypoint) initialState = ((0, 0), (10, -1));
            return course.Aggregate(initialState, Step).position;
        }
    }
}

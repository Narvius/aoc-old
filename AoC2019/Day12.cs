using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2019
{
    public class Day12 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var system = new System1D(lines);
            system.StepTimes(1000);
            return system.TotalEnergy().ToString();
        }

        public string PartTwo(string[] lines)
        {
            var system = new System1D(lines);
            return Enumerable.Range(0, 3).Select(system.StepAxisUntilSame).Aggregate(1L, LCM).ToString();
        }

        private long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
        private long LCM(long a, long b) => a / GCD(a, b) * b;
    }

    // Each axis can be treated completely separately, as they don't influence each other.
    public class System1D
    {
        private readonly Regex matchInput = new Regex(@"^<x=(-?\d+), y=(-?\d+), z=(-?\d+)>$");
        // three indices:
        //   axis (0..2): x, y or z
        //   body (0..i): the given moon
        //   value (0..2): 0 = position, 1 = velocity, 2 = original position
        private readonly int[,,] bodies;

        public System1D(string[] lines)
        {
            bodies = new int[3, lines.Length, 3];
            int i = 0;
            foreach (var line in lines)
            {
                var match = matchInput.Match(line);
                int x = int.Parse(match.Groups[1].Value), y = int.Parse(match.Groups[2].Value), z = int.Parse(match.Groups[3].Value);
                bodies[0, i, 0] = x;
                bodies[0, i, 1] = 0;
                bodies[0, i, 2] = x;

                bodies[1, i, 0] = y;
                bodies[1, i, 1] = 0;
                bodies[1, i, 2] = y;

                bodies[2, i, 0] = z;
                bodies[2, i, 1] = 0;
                bodies[2, i, 2] = z;

                i++;
            }
        }

        public int TotalEnergy()
        {
            int result = 0;
            for (int i = 0; i < bodies.GetLength(1); i++)
            {
                int potential = 0, kinetic = 0;
                for (int axis = 0; axis < 3; axis++)
                {
                    potential += Math.Abs(bodies[axis, i, 0]);
                    kinetic += Math.Abs(bodies[axis, i, 1]);
                }
                result += (potential * kinetic);
            }
            return result;
        }

        public void StepTimes(int amount)
        {
            for (int axis = 0; axis < 3; axis++)
                for (int i = 0; i < amount; i++)
                    StepAxis(axis);
        }

        public long StepAxisUntilSame(int axis)
        {   
            for (int generation = 0; ; generation++)
            {
                StepAxis(axis);

                // Compare state to original
                bool finished = true;
                for (int i = 0; i < bodies.GetLength(1); i++)
                    if (bodies[axis, i, 0] != bodies[axis, i, 2] || bodies[axis, i, 1] != 0)
                    {
                        finished = false;
                        break;
                    }

                if (finished)
                    return generation + 1;
            }
        }

        private void StepAxis(int axis)
        {
            var count = bodies.GetLength(1);

            // Update velocities
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                    bodies[axis, i, 1] += Math.Sign(bodies[axis, j, 0] - bodies[axis, i, 0]);

            // Update positions
            for (int i = 0; i < count; i++)
                bodies[axis, i, 0] += bodies[axis, i, 1];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day17 : ISolution
    {
        // Run the 3d automaton for 6 steps, and count the number of alive cubes.
        public string PartOne(string[] lines)
            => new ConwayCubeSpace(lines, 6).RunToCompletionAndCountCubes().ToString();

        // Run the 4d automaton for 6 steps, and count the number of alive hypercubes.
        public string PartTwo(string[] lines)
            => new ConwayCubeHyperspace(lines, 6).RunToCompletionAndCountCubes().ToString();
    }

    /// <summary>
    /// 3d cellular automaton.
    /// </summary>
    public class ConwayCubeSpace
    {
        public enum CubeState : byte { Off, On }

        private int cyclesLeft;
        private CubeState[,,] space;
        private CubeState[,,] buffer;
        private readonly int sx, sy, sz; // Sizes of each dimension.

        public ConwayCubeSpace(string[] input, int cycles = 6)
        {
            // Sizes grow by 1 either direction each step. We know the amount of steps ahead of time, so we
            // can precompute the final size for simplicity in processing.
            (sx, sy, sz) = (input.Length + 2 * cycles, input[0].Length + 2 * cycles, 1 + 2 * cycles);

            cyclesLeft = cycles;
            space = new CubeState[sx, sy, sz];
            buffer = new CubeState[sx, sy, sz];

            // Place input in the very middle of our space.
            for (int y = 0; y < input.Length; y++)
                for (int x = 0; x < input[y].Length; x++)
                    space[cycles + x, cycles + y, cycles] = input[y][x] == '#' ? CubeState.On : CubeState.Off;
        }

        /// <summary>
        /// Runs the simulation for the amount of steps provided in the constructor. Steps do not
        /// repeat on consecutive calls.
        /// </summary>
        /// <returns>The amount of <see cref="CubeState.On"/> cells.</returns>
        public int RunToCompletionAndCountCubes()
        {
            for (; cyclesLeft > 0; cyclesLeft--)
                Step();

            return space.Cast<CubeState>().Count(s => s == CubeState.On);
        }

        /// <summary>
        /// Simulates a single step for the cellular automaton.
        /// </summary>
        private void Step()
        {
            for (int x = 0; x < space.GetLength(0); x++)
                for (int y = 0; y < space.GetLength(1); y++)
                    for (int z = 0; z < space.GetLength(2); z++)
                        buffer[x, y, z] = (space[x, y, z], NeighbourCount(space, x, y, z)) switch
                        {
                            (CubeState.On, 2) => CubeState.On,
                            (_, 3)            => CubeState.On,
                            _                 => CubeState.Off
                        };

            (space, buffer) = (buffer, space);
        }

        /// <summary>
        /// Checks if the given coordinates are a valid index into <see cref="space"/> and <see cref="buffer"/>.
        /// </summary>
        /// <returns>True if the arguments are a valid index, false otherwise.</returns>
        private bool ValidIndex(int x, int y, int z)
            => 0 <= x && x < sx && 0 <= y && y < sy && 0 <= z && z < sz;

        /// <summary>
        /// Counts the amount of <see cref="CubeState.On"/> cells adjacent to the given coordinates.
        /// </summary>
        /// <returns>The amount of <see cref="CubeState.On"/> neighbours.</returns>
        private int NeighbourCount(CubeState[,,] space, int x, int y, int z)
        {
            int result = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                        if ((dx != 0 || dy != 0 || dz != 0) && ValidIndex(x + dx, y + dy, z + dz))
                            if (space[x + dx, y + dy, z + dz] == CubeState.On)
                                result++;
            return result;
        }
    }

    /// <summary>
    /// 4d cellular automaton. Implementation is a natural extension of <see cref="ConwayCubeSpace"/>.
    /// </summary>
    public class ConwayCubeHyperspace
    {
        public enum CubeState : byte { Off, On }

        private int cyclesLeft;
        private CubeState[,,,] space;
        private CubeState[,,,] buffer;
        private readonly int sx, sy, sz, sw;

        public ConwayCubeHyperspace(string[] input, int cycles = 6)
        {
            // Sizes grow by 1 either direction each step. We know the amount of steps ahead of time, so we
            // can precompute the final size for simplicity in processing.
            (sx, sy, sz, sw) = (input.Length + 2 * cycles, input[0].Length + 2 * cycles, 1 + 2 * cycles, 1 + 2 * cycles);

            cyclesLeft = cycles;
            space = new CubeState[sx, sy, sz, sw];
            buffer = new CubeState[sx, sy, sz, sw];

            // Place input in the very middle of our space.
            for (int y = 0; y < input.Length; y++)
                for (int x = 0; x < input[y].Length; x++)
                    space[cycles + x, cycles + y, cycles, cycles] = input[y][x] == '#' ? CubeState.On : CubeState.Off;
        }

        /// <summary>
        /// Runs the simulation for the amount of steps provided in the constructor. Steps do not
        /// repeat on consecutive calls.
        /// </summary>
        /// <returns>The amount of <see cref="CubeState.On"/> cells.</returns>
        public int RunToCompletionAndCountCubes()
        {
            for (; cyclesLeft > 0; cyclesLeft--)
                Step();

            return space.Cast<CubeState>().Count(s => s == CubeState.On);
        }

        /// <summary>
        /// Simulates a single step for the cellular automaton.
        /// </summary>
        private void Step()
        {
            for (int x = 0; x < space.GetLength(0); x++)
                for (int y = 0; y < space.GetLength(1); y++)
                    for (int z = 0; z < space.GetLength(2); z++)
                        for (int w = 0; w < space.GetLength(3); w++)
                            buffer[x, y, z, w] = (space[x, y, z, w], NeighbourCount(space, x, y, z, w)) switch
                            {
                                (CubeState.On, 2) => CubeState.On,
                                (_, 3) => CubeState.On,
                                _ => CubeState.Off
                            };

            (space, buffer) = (buffer, space);
        }

        /// <summary>
        /// Checks if the given coordinates are a valid index into <see cref="space"/> and <see cref="buffer"/>.
        /// </summary>
        /// <remarks>Using <see cref="ArrayExtensions.ValidIndex(Array, int[])"/> instead resulted in a >2s running time, which prompted me to write this.</remarks>
        /// <returns>True if the arguments are a valid index, false otherwise.</returns>
        private bool ValidIndex(int x, int y, int z, int w)
            => 0 <= x && x < sx && 0 <= y && y < sy
            && 0 <= z && z < sz && 0 <= w && w < sw;

        /// <summary>
        /// Counts the amount of <see cref="CubeState.On"/> cells adjacent to the given coordinates.
        /// </summary>
        /// <returns>The amount of <see cref="CubeState.On"/> neighbours.</returns>
        private int NeighbourCount(CubeState[,,,] space, int x, int y, int z, int w)
        {
            int result = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                        for (int dw = -1; dw <= 1; dw++)
                            if ((dx != 0 || dy != 0 || dz != 0 || dw != 0) && ValidIndex(x + dx, y + dy, z + dz, w + dw))
                                if (space[x + dx, y + dy, z + dz, w + dw] == CubeState.On)
                                    result++;
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019.Computer
{
    // Basic implementation of the intcode computer.
    // Only has two operations plus halt, and returns the first memory cell as a "result" after halting.
    public class V1
    {
        private readonly int[] Memory;
        private int ProgramCounter = 0;

        public V1(string initial, params (int p, int v)[] overwrites)
        {
            Memory = initial.Split(',').Select(int.Parse).ToArray();
            foreach (var (p, v) in overwrites)
                Memory[p] = v;
        }

        public void RunOnce()
        {
            void ApplyOp(Func<int, int, int> op)
            {
                var target = Memory[ProgramCounter + 3];
                var a = Memory[ProgramCounter + 1];
                var b = Memory[ProgramCounter + 2];

                Memory[target] = op(Memory[a], Memory[b]);
            }

            switch (Memory[ProgramCounter])
            {
                case 99: ProgramCounter = int.MaxValue; return;
                case 1: ApplyOp((a, b) => a + b); ProgramCounter += 4; return;
                case 2: ApplyOp((a, b) => a * b); ProgramCounter += 4; return;
            }
        }

        public int RunUntilHalted()
        {
            while (0 <= ProgramCounter && ProgramCounter < Memory.Length)
                RunOnce();

            return Memory[0];
        }
    }
}

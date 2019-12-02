using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day2 : ISolution
    {
        public string PartOne(string[] lines)
            => new ShipComputer(lines[0], (1, 12), (2, 2)).RunUntilHalted().ToString();

        public string PartTwo(string[] lines)
            => (from noun in Enumerable.Range(0, 100)
                from verb in Enumerable.Range(0, 100)
                let zero = new ShipComputer(lines[0], (1, noun), (2, verb)).RunUntilHalted()
                where zero == 19690720
                select 100 * noun + verb).First().ToString();
    }

    public class ShipComputer
    {
        private readonly int[] Memory;
        private int ProgramCounter = 0;

        public int this[int p]
        {
            get => Memory[p];
            set => Memory[p] = value;
        }

        public ShipComputer(string initial, params (int p, int v)[] overwrites)
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
                case 2: ApplyOp((a, b) => a * b); ProgramCounter += 4;  return;
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

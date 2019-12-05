using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day5 : ISolution
    {
        // Run ship computer with input 1, get diagnostic code (last output).
        public string PartOne(string[] lines)
            => new ShipComputerV2(lines[0]).RunUntilHalted(1).Last().ToString();

        // Run ship computer with input 5, get diagnostic code (last output).
        public string PartTwo(string[] lines)
            => new ShipComputerV2(lines[0]).RunUntilHalted(5).Last().ToString();
    }

    public class ShipComputerV2
    {
        private readonly int[] Memory;
        private int ProgramCounter = 0;

        private readonly Queue<int> input = new Queue<int>();
        private readonly Queue<int> output = new Queue<int>();

        public ShipComputerV2(string initial, params (int p, int v)[] overwrites)
        {
            Memory = initial.Split(',').Select(int.Parse).ToArray();
            foreach (var (p, v) in overwrites)
                Memory[p] = v;
        }

        public IEnumerable<int> RunUntilHalted(params int[] inputs)
            => RunUntilHalted(inputs.AsEnumerable());

        public IEnumerable<int> RunUntilHalted(IEnumerable<int> inputs)
        {
            foreach (var input in inputs)
                this.input.Enqueue(input);

            while (0 <= ProgramCounter && ProgramCounter < Memory.Length)
            {
                RunOnce();
                while (output.Count > 0)
                    yield return output.Dequeue();
            }
        }

        private void RunOnce()
        {
            DecomposeOp(Memory[ProgramCounter], out Op code, out int mode1, out int mode2, out int mode3);
            switch (code)
            {
                case Op.Halt: ProgramCounter = int.MaxValue; return;

                case Op.Add:
                    Write(3, Read(1, mode1) + Read(2, mode2));
                    ProgramCounter += 4;
                    break;

                case Op.Multiply:
                    Write(3, Read(1, mode1) * Read(2, mode2));
                    ProgramCounter += 4;
                    break;

                case Op.Read:
                    Write(1, input.Dequeue());
                    ProgramCounter += 2;
                    return;

                case Op.Output:
                    output.Enqueue(Read(1, mode1));
                    ProgramCounter += 2; 
                    return;

                case Op.JumpIfTrue:
                    if (Read(1, mode1) != 0)
                        ProgramCounter = Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return;

                case Op.JumpIfFalse:
                    if (Read(1, mode1) == 0)
                        ProgramCounter = Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return;

                case Op.LessThan:
                    Write(3, Read(1, mode1) < Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return;

                case Op.Equals:
                    Write(3, Read(1, mode1) == Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return;
            }
        }

        private int Read(int offset, int mode = 0)
        {
            int parameter = Memory[ProgramCounter + offset];
            switch (mode)
            {
                case 0: return Memory[parameter];
                case 1: return parameter;
                default: throw new Exception();
            }
        }

        private void Write(int offset, int value) => Memory[Memory[ProgramCounter + offset]] = value;

        private void DecomposeOp(int op, out Op code, out int mode1, out int mode2, out int mode3)
        {
            code = (Op)(op % 100);
            mode1 = (op / 100) % 10;
            mode2 = (op / 1000) % 10;
            mode3 = (op / 10000) % 10;
        }

        private enum Op { Add = 1, Multiply = 2, Read = 3, Output = 4, JumpIfTrue = 5, JumpIfFalse = 6, LessThan = 7, Equals = 8, Halt = 99 };
    }
}

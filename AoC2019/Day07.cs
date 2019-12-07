using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day07 : ISolution
    {
        // Run an amplifier series for all possible phase settings, get the best result.
        public string PartOne(string[] lines)
        {
            var source = new ShipComputerV3(lines[0]);
            const int permutations = 120;
            return Enumerable.Range(0, permutations).Max(seed => new AmplifierSeries(source, seed).RunAll()).ToString();
        }

        // Run an amplifier series in feedback mode for all possible phase settings, get the best result.
        public string PartTwo(string[] lines)
        {
            var source = new ShipComputerV3(lines[0]);
            const int permutations = 120;
            return Enumerable.Range(0, permutations).Max(seed => new AmplifierSeries(source, seed, true).RunAll()).ToString();
        }
    }

    // Describes a series of five amplifiers.
    public class AmplifierSeries
    {
        private ShipComputerV3[] amplifiers;

        // Takes a "model" amplifier, which is copied to create instances, a "seed" (a number between 0 inclusive and
        // 120 exclusive) which describes the phase settings, and a switch that, if true, links up the final amplifier
        // back with the first one (as well as adjusting the final phase settings to use feedback mode).
        public AmplifierSeries(ShipComputerV3 amplifierModel, int seed, bool withFeedback = false)
        {
            amplifiers = Enumerable.Repeat(0, 5).Select(_ => new ShipComputerV3(amplifierModel)).ToArray();

            for (int i = 0; i < 4; i++)
                amplifiers[i].OutputTo(amplifiers[i + 1]);

            if (withFeedback)
                amplifiers[4].OutputTo(amplifiers[0]);

            var phaseSettings = PermutationFromIndex(seed);
            for (int i = 0; i < 5; i++)
                amplifiers[i].Input.Enqueue(phaseSettings[i] + (withFeedback ? 5 : 0));
            amplifiers[0].Input.Enqueue(0);
        }

        // Repeatedly runs all amplifiers in sequence until a halt is detected, at which point
        // it returns the last output of the last amplifier that ran.
        public int RunAll()
        {
            for (int i = 0; ; i++)
            {
                var amp = amplifiers[i % 5];
                if (amp.State == ShipComputerV3.ExecutionState.Halted)
                {
                    var prevAmp = amplifiers[(i + 4) % 5];
                    return prevAmp.Output.Dequeue();
                }

                amp.RunWhilePossible();
            }
        }

        // Maps a single integer between 0 (inclusive) and 5! (exclusive) to one of the permutations of the 
        // set {0, 1, 2, 3, 4}. The indices do not produce permutations in lexicographical order, but that's
        // irrelevant for this task.
        private int[] PermutationFromIndex(int index)
        {
            var items = new List<int> { 0, 1, 2, 3, 4 };
            var result = new int[5];
            for (int i = 5; i > 0; i--)
            {
                result[i - 1] = items[index % i];
                items.RemoveAt(index % i);
                index /= i;
            }
            return result;
        }
    }

    public class ShipComputerV3
    {
        private readonly int[] Memory;
        private int ProgramCounter = 0;

        public Queue<int> Input { get; private set; } = new Queue<int>();
        public Queue<int> Output { get; private set; } = new Queue<int>();

        public ExecutionState State { get; private set; } = ExecutionState.Running;

        public ShipComputerV3(string initial)
        {
            Memory = initial.Split(',').Select(int.Parse).ToArray();
        }

        public ShipComputerV3(ShipComputerV3 source)
        {
            Memory = new int[source.Memory.Length];
            Array.Copy(source.Memory, Memory, Memory.Length);
        }

        // Runs the program until paused or halted.
        public void RunWhilePossible()
        {
            if (State == ExecutionState.Halted)
                return;

            if (State == ExecutionState.Paused)
                State = ExecutionState.Running;

            while (State == ExecutionState.Running)
                State = RunOnce();
        }

        // Links the output of this computer to the input of the target computer.
        public void OutputTo(ShipComputerV3 target)
        {
            target.Input = Output;
        }

        // Executes the next instruction and returns whether to continue executing, pause or halt.
        private ExecutionState RunOnce()
        {
            DecomposeOp(Memory[ProgramCounter], out Op code, out int mode1, out int mode2, out int mode3);
            switch (code)
            {
                case Op.Halt: return ExecutionState.Halted;

                case Op.Add:
                    Write(3, Read(1, mode1) + Read(2, mode2));
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Multiply:
                    Write(3, Read(1, mode1) * Read(2, mode2));
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Read:
                    if (Input.Count == 0)
                        return ExecutionState.Paused;

                    Write(1, Input.Dequeue());
                    ProgramCounter += 2;
                    return ExecutionState.Running;

                case Op.Output:
                    Output.Enqueue(Read(1, mode1));
                    ProgramCounter += 2;
                    return ExecutionState.Running;

                case Op.JumpIfTrue:
                    if (Read(1, mode1) != 0)
                        ProgramCounter = Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return ExecutionState.Running;

                case Op.JumpIfFalse:
                    if (Read(1, mode1) == 0)
                        ProgramCounter = Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return ExecutionState.Running;

                case Op.LessThan:
                    Write(3, Read(1, mode1) < Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Equals:
                    Write(3, Read(1, mode1) == Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return ExecutionState.Running;
            }

            throw new Exception("invalid op");
        }

        // Reads a memory cell relative to the current program counter, and then resolves it, respecting the provided addressing mode.
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

        // Write to the memory cell pointed to by the memory cell ahead by "offset" from the program counter.
        private void Write(int offset, int value) => Memory[Memory[ProgramCounter + offset]] = value;

        // Takes apart an opcode of the shape EDCBA, where each letter corresponds to a digit.
        // BA = two-digit operation code
        // C, D, E = addressing mode for the first, second and third parameter, respectively
        private void DecomposeOp(int op, out Op code, out int mode1, out int mode2, out int mode3)
        {
            code = (Op)(op % 100);
            mode1 = (op / 100) % 10;
            mode2 = (op / 1000) % 10;
            mode3 = (op / 10000) % 10;
        }

        private enum Op { Add = 1, Multiply = 2, Read = 3, Output = 4, JumpIfTrue = 5, JumpIfFalse = 6, LessThan = 7, Equals = 8, Halt = 99 };

        public enum ExecutionState { Running, Paused, Halted }
    }
}

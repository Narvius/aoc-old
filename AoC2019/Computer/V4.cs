using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AoC2019.Computer
{
    // Changelog:
    // - relative addressing
    // - memory size constructor argument
    // - BigInteger memory
    public class V4
    {
        private readonly BigInteger[] Memory;
        private int ProgramCounter = 0;
        private int RelativeBase = 0;

        public Queue<BigInteger> Input { get; private set; } = new Queue<BigInteger>();
        public Queue<BigInteger> Output { get; private set; } = new Queue<BigInteger>();

        public ExecutionState State { get; private set; } = ExecutionState.Running;

        public V4(string initial, int? memorySize = null)
        {
            var data = initial.Split(',').Select(BigInteger.Parse).ToArray();
            Memory = new BigInteger[memorySize ?? data.Length];

            Array.Copy(data, Memory, data.Length);
        }

        public V4(V4 source)
        {
            Memory = new BigInteger[source.Memory.Length];
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
        public void OutputTo(V4 target)
        {
            target.Input = Output;
        }

        // Executes the next instruction and returns whether to continue executing, pause or halt.
        private ExecutionState RunOnce()
        {
            DecomposeOp((int)Memory[ProgramCounter], out Op code, out int mode1, out int mode2, out int mode3);
            switch (code)
            {
                case Op.Halt: return ExecutionState.Halted;

                case Op.Add:
                    Write(3, mode3, Read(1, mode1) + Read(2, mode2));
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Multiply:
                    Write(3, mode3, Read(1, mode1) * Read(2, mode2));
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Read:
                    if (Input.Count == 0)
                        return ExecutionState.Paused;

                    Write(1, mode1, Input.Dequeue());
                    ProgramCounter += 2;
                    return ExecutionState.Running;

                case Op.Output:
                    Output.Enqueue(Read(1, mode1));
                    ProgramCounter += 2;
                    return ExecutionState.Running;

                case Op.JumpIfTrue:
                    if (Read(1, mode1) != 0)
                        ProgramCounter = (int)Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return ExecutionState.Running;

                case Op.JumpIfFalse:
                    if (Read(1, mode1) == 0)
                        ProgramCounter = (int)Read(2, mode2);
                    else
                        ProgramCounter += 3;
                    return ExecutionState.Running;

                case Op.LessThan:
                    Write(3, mode3, Read(1, mode1) < Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.Equals:
                    Write(3, mode3, Read(1, mode1) == Read(2, mode2) ? 1 : 0);
                    ProgramCounter += 4;
                    return ExecutionState.Running;

                case Op.OffsetRelative:
                    RelativeBase += (int)Read(1, mode1);
                    ProgramCounter += 2;
                    return ExecutionState.Running;
            }

            throw new Exception("invalid op");
        }

        // Reads a memory cell relative to the current program counter, and then resolves it, respecting the provided addressing mode.
        private BigInteger Read(int offset, int mode = 0)
        {
            var parameter = Memory[ProgramCounter + offset];
            switch (mode)
            {
                case 0: return Memory[(int)parameter];
                case 1: return parameter;
                case 2: return Memory[(int)parameter + RelativeBase];
                default: throw new Exception();
            }
        }

        // Write to the memory cell pointed to by the memory cell ahead by "offset" from the program counter.
        private void Write(int offset, int mode, BigInteger value)
        {
            switch (mode)
            {
                case 0: Memory[(int)Memory[ProgramCounter + offset]] = value; break;
                case 2: Memory[(int)Memory[ProgramCounter + offset] + RelativeBase] = value; break;

                case 1: throw new Exception("cannot use literal addressing mode when writing");
                default: throw new Exception();
            }
        }

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

        private enum Op { Add = 1, Multiply = 2, Read = 3, Output = 4, JumpIfTrue = 5, JumpIfFalse = 6, LessThan = 7, Equals = 8, OffsetRelative = 9, Halt = 99 };

        public enum ExecutionState { Running, Paused, Halted }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day08 : ISolution
    {
        // Run the program until it repeats, find the accumulator just before the repetition.
        public string PartOne(string[] lines)
            => new GameConsole(lines).RunUntilTerminated().ToString();

        // Fix one NOP or JMP instruction so that the described program terminates; find the accumulator when it's done.
        public string PartTwo(string[] lines)
            => new GameConsole(lines).FixAndRunProgram().ToString();
    }

    /// <summary>
    /// Runs the "programming language" from the puzzle input.
    /// </summary>
    public class GameConsole
    {
        private readonly Instruction[] instructions;

        public GameConsole(string[] code)
        {
            instructions = code.Select(instruction => new Instruction(instruction)).ToArray();
        }

        /// <summary>
        /// Runs a program until it terminates. The program terminates when an instruction is repeated.
        /// </summary>
        /// <returns>The final value of the accumulator.</returns>
        public int RunUntilTerminated()
            => Execute(instructions).accumulator;

        /// <summary>
        /// Fix a program by making it actually terminate, by changing just one NOP to JMP or vice-versa.
        /// The correct NOP or JMP to change is found via brute force.
        /// </summary>
        /// <returns>The final value of the accumulator.</returns>
        public int FixAndRunProgram()
        {
            for (int i = 0; i < instructions.Length; i++)
                if (instructions[i].CanSwap)
                {
                    var p = (Instruction[])instructions.Clone();
                    p[i].Swap();
                    var (pointer, accumulator) = Execute(p);
                    if (pointer >= instructions.Length)
                        return accumulator;
                }

            throw new Exception("no working fix found");
        }

        /// <summary>
        /// Keeps running the provided program until an instruction repeats or it terminates.
        /// </summary>
        /// <param name="p">The program to run.</param>
        /// <returns>The final values for the pointer and accumulator.</returns>
        private (int pointer, int accumulator) Execute(Instruction[] p)
        {
            int pointer = 0, accumulator = 0;
            while (pointer < p.Length && !p[pointer].Executed)
                p[pointer].Run(
                    nop: _ => pointer++,
                    acc: n => { accumulator += n; pointer++; },
                    jmp: n => pointer += n);

            return (pointer, accumulator);
        }
    }

    /// <summary>
    /// Describes a single instruction for the <see cref="GameConsole"/>.
    /// </summary>
    /// <remarks>
    /// Making this a struct allows for real copies of programs with <see cref="Array.Clone"/>, but requires careful use,
    /// otherwise unintended behavior may occur.
    /// </remarks>
    /// <example>
    /// Unintended behavior:<br/>
    /// <code>
    /// Instruction[] program = /* set it */;
    /// var instruction = program[5];
    /// instruction.Executed = true; // program[5].Executed is still false after this, because "instruction" is a copy
    /// </code>
    /// </example>
    public struct Instruction
    {
        public string Type;
        public readonly int Argument;
        public bool Executed;

        public Instruction(string line)
        {
            var data = line.Split(' ');

            Type = data[0];
            Argument = int.Parse(data[1]);
            Executed = false;
        }

        /// <summary>
        /// Picks the correct provided delegate to run, based on <see cref="Type"/>; and runs it, with <see cref="Argument"/> as argument.
        /// </summary>
        public void Run(Action<int> nop, Action<int> acc, Action<int> jmp)
        {
            Executed = true;
            (Type switch { "nop" => nop, "acc" => acc, "jmp" => jmp, _ => throw new Exception("invalid operation") })
                .Invoke(Argument);
        }

        public bool CanSwap => Type != "acc";
        public void Swap() => Type = (Type == "nop" ? "jmp" : "nop");
    }
}

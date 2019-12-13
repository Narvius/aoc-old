using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CPU = AoC2019.Computer.V4;

namespace AoC2019
{
    public class Day13 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var cpu = new CPU(lines[0], 10000);
            cpu.RunWhilePossible();

            int result = 0;
            while (cpu.Output.Count > 0)
            {
                cpu.Output.Dequeue();
                cpu.Output.Dequeue();
                if (cpu.Output.Dequeue() == 2)
                    result++;
            }

            return result.ToString();
        }

        public string PartTwo(string[] lines)
        {
            var cpu = new CPU(lines[0], 10000);
            cpu.OverwriteMemoryAt(0, 2);
            cpu.RunWhilePossible();

            return RunGameLoop(cpu).ToString();
        }

        private void UpdateGameState(CPU cpu, ref int score, ref (int x, int y) paddle, ref (int x, int y) ball)
        {
            while (cpu.Output.Count > 0)
            {
                var (x, y) = ((int)cpu.Output.Dequeue(), (int)cpu.Output.Dequeue());
                var value = (int)cpu.Output.Dequeue();

                if (x == -1 && y == 0)
                    score = value;
                else
                {
                    if (value == 3)
                        paddle = (x, y);
                    else if (value == 4)
                        ball = (x, y);
                }
            }
        }

        private int RunGameLoop(CPU cpu)
        {
            var paddleState = (x: 0, y: 0);
            var ballState = (x: 0, y: 0);
            var score = 0;

            UpdateGameState(cpu, ref score, ref paddleState, ref ballState);

            while (cpu.State != CPU.ExecutionState.Halted)
            {
                cpu.Input.Enqueue(Math.Sign(ballState.x - paddleState.x));
                cpu.RunWhilePossible();
                UpdateGameState(cpu, ref score, ref paddleState, ref ballState);
            }
            return score;
        }
    }
}

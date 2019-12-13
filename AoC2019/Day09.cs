using CPU = AoC2019.Computer.V4;

namespace AoC2019
{
    public class Day09 : ISolution
    {
        // Run program with input 1.
        public string PartOne(string[] lines)
        {
            var c = new CPU(lines[0], 10000);
            c.Input.Enqueue(1);
            c.RunWhilePossible();
            return c.Output.Dequeue().ToString();
        }

        // Run program with input 2.
        public string PartTwo(string[] lines)
        {
            var c = new CPU(lines[0], 10000);
            c.Input.Enqueue(2);
            c.RunWhilePossible();
            return c.Output.Dequeue().ToString();
        }
    }
}

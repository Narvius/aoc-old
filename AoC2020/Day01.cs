using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day01 : ISolution
    {
        // Find product of two entries that sum to 2020.
        public string PartOne(string[] lines)
        {
            var ns = lines.Select(int.Parse).ToArray();

            for (int i = 0; i < lines.Length; i++)
                for (int j = i + 1; j < lines.Length; j++)
                    if ((ns[i] + ns[j]) == 2020)
                        return (ns[i] * ns[j]).ToString();

            throw new Exception("input data violates assumptions");
        }

        // Find product of three entries that sum to 2020.
        public string PartTwo(string[] lines)
        {
            var ns = lines.Select(int.Parse).ToArray();

            for (int i = 0; i < lines.Length; i++)
                for (int j = i + 1; j < lines.Length; j++)
                    for (int k = j + 1; k < lines.Length; k++)
                        if ((ns[i] + ns[j] + ns[k]) == 2020)
                            return (ns[i] * ns[j] * ns[k]).ToString();

            throw new Exception("input data violates assumptions");
        }
    }
}

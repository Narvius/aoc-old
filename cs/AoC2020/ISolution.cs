using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AoC2020
{
    public interface ISolution
    {
        string PartOne(string[] lines);
        string PartTwo(string[] lines);
    }
}

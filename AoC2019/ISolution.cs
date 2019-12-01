using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AoC2019
{
    public interface ISolution
    {
        string PartOne(string[] lines);
        string PartTwo(string[] lines);
    }
}

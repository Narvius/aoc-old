using System.IO;
using System.Linq;

namespace AoC2019
{
    public class Day08 : ISolution
    {
        // Find the layer with the fewest zeroes, and calculate a checksum.
        public string PartOne(string[] lines)
        {
            const int LayerSize = 25 * 6;
            var leastZeroes = lines[0]
                .Select((c, i) => (c, group: i / LayerSize))
                .GroupBy(v => v.group)
                .OrderBy(v => v.Count(c => c.c == '0'))
                .First()
                .Select(v => v.c);

            return (leastZeroes.Count(c => c == '1') * leastZeroes.Count(c => c == '2')).ToString();
        }

        // Merge all the layers in the image data, and interpret the final image.
        // I'll skip the part where I automatically recognize the image contents, lol.
        public string PartTwo(string[] lines)
        {
            const int LayerSize = 25 * 6;
            var line = lines[0];
            var result = new char[LayerSize];

            for (int i = 0; i < LayerSize; i++)
                for (int j = i; j < line.Length; j += LayerSize)
                    if (line[j] != '2')
                    {
                        result[i] = line[j] == '1' ? '█' : ' ';
                        break;
                    }

            File.WriteAllLines("Day08_output.txt", Enumerable.Range(0, 6).Select(i => new string(result, 25 * i, 25)));
            return "check file 'Day08_output.txt'";
        }
    }
}

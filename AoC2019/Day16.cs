using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day16 : ISolution
    {
        public string PartOne(string[] lines)
            => new FftTransformer().AppliedRepeatedly(lines[0], 100).Substring(0, 8);

        public string PartTwo(string[] lines)
            => new FftTransformer().AppliedRepeatedly(string.Join("", Enumerable.Repeat(lines[0], 10000)), 100)
            .Substring(int.Parse(lines[0].Substring(0, 7)), 8);
    }

    public class FftTransformer
    {
        private readonly int[] basePattern = new[] { 1, 0, -1, 0 };

        public string AppliedRepeatedly(string input, int times)
        {
            var digits = AsDigits(input);

            for (int i = 0; i < times; i++)
                Fast_Fft(digits);

            return AsString(digits);
        }

        private string AsString(int[] digits)
            => new string(digits.Select(n => (char)('0' + n)).ToArray());

        private int[] AsDigits(string input)
            => input.Select(c => c - '0').ToArray();

        private void ApplyFft(int[] digits, int[] original, int digitCount)
        {
            for (int i = 0; i < digitCount; i++)
                digits[i] = MergeIntoDigit(original, i, digitCount);
        }

        private int MergeIntoDigit(int[] digits, int index, int digitCount)
        {
            int stride = 4 * (index + 1);
            int subOffset = 2 * (index + 1);

            int result = 0;
            for (int i = index; i < digitCount; i += stride)
                for (int n = 0; n <= index; n++)
                    result += digits[i + n] - digits[i + n + subOffset];

            return Math.Abs(result % 10);
        }

        private void Fast_Fft(int[] digits)
        {
            /* Concept: digit[n + 1] = digit[n] +/- some changes
             
+0-0+0-0+0-0+0-0+0-
 ++00--00++00--00++
  +++000---000+++00
   ++++0000----0000
    +++++00000-----
     ++++++000000--
    
0th window shift: 1
1st window shift: 3
2nd window shift: 5
3rd window shift: 7

For each window, find the overlap between steps:
>> shift = 2 * window_index + 1
>> stride = step

current_start(step) = 2 * window_index + shift * step
current_end(step) = 2 * window_index + shift * step + stride
next_start(step) = 2 * window_index + shift * (step + 1)
next_end(step) = 2 * window_index + shift * (step + 1) + (stride + 1)
    */
            for (int i = 0; i < digits.Length; i++)
            {
                //digits[i]
            }
        }
    }
}

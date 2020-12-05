using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day05 : ISolution
    {
        // Find the highest seat id.
        public string PartOne(string[] lines)
            => lines.Select(line => new Seating(line)).Max(s => s.SeatId).ToString();

        // Find the only missing seat id between two other ones.
        public string PartTwo(string[] lines)
        {
            var ids = lines.Select(line => new Seating(line).SeatId).OrderBy(id => id);

            return (from consecutiveIds in ids.Zip(ids.Skip(1))
                    let difference = consecutiveIds.Second - consecutiveIds.First
                    where difference == 2
                    select consecutiveIds.Second - 1).Single().ToString();
            
            //return ids.Zip(ids.Skip(1), (a, b) => (b - a, b - 1)).First(p => p.Item1 == 2).Item2.ToString();
        }
    }

    /// <summary>
    /// Decodes the actual row, column and seat ID for a given seating string.
    /// Note that the seating strings are literally just two binary numbers stuck together.
    /// </summary>
    public readonly struct Seating
    {
        public readonly int Row;
        public readonly int Column;

        public int SeatId => Row * 8 + Column;

        public Seating(string seatingString)
        {
            Row = Convert.ToInt32(seatingString.Substring(0, 7).Replace('F', '0').Replace('B', '1'), 2);
            Column = Convert.ToInt32(seatingString.Substring(7, 3).Replace('L', '0').Replace('R', '1'), 2);
        }
    }
}

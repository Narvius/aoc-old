using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day02 : ISolution
    {
        // Count passwords that match letter count policy.
        public string PartOne(string[] lines)
            => (from line in lines
                let data = ParseLine(line)
                let count = data.password.Count(c => c == data.c)
                where data.p1 <= count && count <= data.p2
                select line).Count().ToString();

        // Count passwords that match exclusive letter position policy.
        public string PartTwo(string[] lines)
            => (from line in lines
                let data = ParseLine(line)
                where (data.password[data.p1 - 1] == data.c) ^ (data.password[data.p2 - 1] == data.c)
                select line).Count().ToString();

        /// <summary>
        /// Parses the content of a "policy: password" line.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>A tuple containing the two policy numbers (p1, p2), the restricted character (c)
        /// and the password to match the policy against (password).</returns>
        private (int p1, int p2, char c, string password) ParseLine(string line)
        {
            const string regex = @"^(\d+)-(\d+).(\w)..(\w+)$";

            var match = Regex.Match(line, regex);
            return (
                int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value),
                match.Groups[3].Value[0], match.Groups[4].Value);
        }
    }
}

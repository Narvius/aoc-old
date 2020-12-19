using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    // I'm doing the same thing again as last the last two attempts. The main difference is that instead of writing my
    // own state machine, I'm using regexes to do it for me, more efficiently. The important insight imported from my
    // second attempt is that I can "unroll" the loops in the state machine, because I know the maximum input size.
    // Since I manually handle the loop cases, this would be the first solution I wrote that is, by design, not universal for all possible inputs.
    public class Day19 : ISolution
    {
        // Count how many strings match rule 0.
        public string PartOne(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var compiler = new RuleToRegexCompiler(chunks[0]);
            return chunks[1].Count(compiler.Match).ToString();
        }

        // Count how many strings match rule 0 if rules 8 and 11 were modified to loop with themselves.
        public string PartTwo(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var compiler = new RuleToRegexCompiler(chunks[0], loopRules: true, longestInputSize: chunks[1].Max(s => s.Length));
            return chunks[1].Count(compiler.Match).ToString();
        }
    }

    /// <summary>
    /// Converts the rules into regexes.
    /// </summary>
    public class RuleToRegexCompiler
    {
        private readonly string[] regexes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawRules"></param>
        /// <param name="loopRules">Whether rules 8 and 11 are modified to loop (according to part two). If given, <paramref name="longestInputSize"/> needs to be provided.
        /// The changes are:<br/>
        /// "8: 42" becomes "8: 42 | 42 8"<br/>
        /// "11: 42 31" becomes "11: 42 31 | 42 11 31"<br/></param>
        /// <param name="longestInputSize">The longest input size the regexes should be designed to handle. Only matters if <paramref name="loopRules"/> is true</param>
        public RuleToRegexCompiler(string[] rawRules, bool loopRules = false, int longestInputSize = 0)
        {
            // Convert the format of the rules from "[i] => n: rule" to "[n] => rule" for convenience.
            var rules = new string[rawRules.Length];
            foreach (var rawRule in rawRules)
            {
                var items = rawRule.Split(": ");
                rules[int.Parse(items[0])] = items[1].Trim('"', ' ');
            }

            regexes = new string[rawRules.Length];
            void BuildRegexFor(int i)
            {
                if (regexes[i] != null) return;

                if (!rules[i].Any(char.IsDigit))
                {
                    regexes[i] = rules[i].Trim('"');
                    return;
                }

                // Manually unroll the loops introduced for rules 8 and 11.
                // I empirically checked that rules 42 and 31 (which are used in the loops) always consume a constant amount
                // of input (6 and 8, respectively). This allows me to severely limit the amount of unroll steps required.
                if (loopRules && i == 8)
                {
                    BuildRegexFor(42);
                    var part = $"(?:{regexes[42]})";

                    regexes[8] = part;
                    for (int n = 2; n < longestInputSize / 6; n++)
                        regexes[8] = $"(?:{string.Concat(regexes[8], "|", string.Join("", Enumerable.Repeat(part, n)))})";
                }
                else if (loopRules && i == 11)
                {
                    BuildRegexFor(42);
                    BuildRegexFor(31);
                    var pre = $"(?:{regexes[42]})";
                    var post = $"(?:{regexes[31]})";

                    regexes[11] = string.Concat(pre, post);
                    for (int n = 2; n < longestInputSize / 14; n++)
                        regexes[11] = string.Concat(regexes[11], "|",
                            $"(?:{string.Concat(Enumerable.Repeat(pre, n).Concat(Enumerable.Repeat(post, n)))})");
                }
                // Substitute all rule numbers with their constructed regex, recursively constructing them
                // first if necessary.
                else
                    regexes[i] = Regex.Replace(rules[i], @"\d+", match =>
                    {
                        var target = int.Parse(match.Value);
                        BuildRegexFor(target);
                        var hasAlternative = regexes[target].Contains('|');
                        return hasAlternative
                            ? $"(?:{regexes[target]})"
                            : regexes[target];
                    }).Replace(" ", "");
            }
            BuildRegexFor(0);
            regexes[0] = string.Concat("^", regexes[0], "$");
        }

        /// <summary>
        /// Check if a string matches the rules of the puzzle.
        /// </summary>
        /// <param name="input">The string to check.</param>
        /// <returns>True if it matches, false otherwise.</returns>
        public bool Match(string input)
            => Regex.IsMatch(input, regexes[0]);
    }
}

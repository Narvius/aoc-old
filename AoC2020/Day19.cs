using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day19 : ISolution
    {
        // Count how many strings match rule 0.
        public string PartOne(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var compiler = new RuleToRegexCompiler(chunks[0]);
            return chunks[1].Count(line => Regex.IsMatch(line, compiler.RuleZeroRegex)).ToString();
        }

        // Count how many strings match rule 0 if rules 8 and 11 were modified to loop with themselves.
        public string PartTwo(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var compiler = new RuleToRegexCompiler(chunks[0],
                loopRules: new Dictionary<int, string> { { 8, "42 8" }, { 11, "42 11 31" } },
                longestInputSize: chunks[1].Max(s => s.Length));
            return chunks[1].Count(line => Regex.IsMatch(line, compiler.RuleZeroRegex)).ToString();
        }
    }

    /// <summary>
    /// Converts rules from the puzzle input into regexes.
    /// </summary>
    public class RuleToRegexCompiler
    {
        private readonly string[] rules;
        private readonly Dictionary<int, string> loopRules;
        private readonly int longestInputSize;

        private readonly string[] regexes;

        public string RuleZeroRegex => regexes[0];

        /// <summary>
        /// Creates an object capable of matching strings against the provided puzzle rules.
        /// </summary>
        /// <param name="rawRules">The first chunk of the puzzle input that contains the rules.</param>
        /// <param name="loopRules">Contains all "loop rules". A loop rule is one that can recurse infinitely. Those need to be handled separately when generating regexes.</param>
        /// <param name="longestInputSize">The longest input size the regexes should be designed to handle. Only matters if <paramref name="loopRules"/> is true</param>
        public RuleToRegexCompiler(string[] rawRules, Dictionary<int, string> loopRules = null, int longestInputSize = 0)
        {
            // Convert the format of the rules from "[i] => n: rule" to "[n] => rule" for convenience.
            rules = new string[rawRules.Length];
            foreach (var rawRule in rawRules)
            {
                var items = rawRule.Split(": ");
                rules[int.Parse(items[0])] = items[1].Trim('"', ' ');
            }

            this.loopRules = loopRules ?? new Dictionary<int, string>();
            this.longestInputSize = longestInputSize;

            // Generate all regexes, and adorn the 0th rule with matches for the beginning and end of string,
            // so that too-long strings don't accidentally get counted.
            regexes = new string[rawRules.Length];
            regexes[0] = string.Concat("^", RegexFor(0).Replace(" ", ""), "$");
        }

        /// <summary>
        /// Get the regex for the given rule, generating it first if it's not cached.
        /// </summary>
        /// <param name="i">Rule number.</param>
        /// <returns>Regex corresponding to the given rule.</returns>
        private string RegexFor(int i)
        {
            // If already computed, return it.
            if (regexes[i] != null) return regexes[i];

            // Simple character-matching rules.
            if (rules[i] == "a" || rules[i] == "b") return regexes[i] = rules[i];

            var rule = rules[i];

            // If this is a loop rule, overwrite the basic rule with the looped one.
            if (loopRules.TryGetValue(i, out var loopRule))
            {
                var (pre, post) = ParseLoopRule(i, loopRule);
                var consumptionPerStep = string.Concat(pre, " ", post)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Sum(n => RuleInputConsumption(int.Parse(n)));

                rule = string.Concat(
                    "(?:",
                    string.Join('|', $"{pre} {post}".Unfold(s => $"{pre} {s} {post}").Take(longestInputSize / consumptionPerStep)),
                    ")");
            }

            // Expand the rule, handles both normal and loop rules.
            return regexes[i] = Regex.Replace(rule, @"\d+", match =>
            {
                var subRegex = RegexFor(int.Parse(match.Value));
                var hasAlternative = subRegex.Contains('|');

                // Wrap the regex in a non-capturing group if it has an alternative, for correct precedence.
                return hasAlternative
                    ? $"(?:{subRegex})"
                    : subRegex;
            });
        }

        /// <summary>
        /// Given a loop rule, returns the parts to the left and the right of the self-looping rule.
        /// </summary>
        /// <param name="i">Rule number.</param>
        /// <param name="loopRule">The contents of the rule containing the loop.</param>
        /// <returns>The parts to the left and right of the self-looping rule.</returns>
        private (string pre, string post) ParseLoopRule(int i, string loopRule)
        {
            var items = loopRule.Replace(i.ToString(), $". {i} .").Split('.');
            return (items[0].Trim(' '), items[2].Trim(' '));
        }

        /// <summary>
        /// Returns the amount of input characters consumed by the given rules. Not necessarily guaranteed
        /// to terminate.  May be wrong if alternatives in the rule have different results. Both of those
        /// warnings should not apply to components of loop rules from part 2 of the puzzle.
        /// </summary>
        /// <param name="i">Rule number.</param>
        /// <returns>The amount of input characters consumed by the given rule.</returns>
        private int RuleInputConsumption(int i)
        {
            if (rules[i] == "a" || rules[i] == "b") return 1;

            return rules[i].Split('|')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Sum(n => RuleInputConsumption(int.Parse(n)));
        }
    }
}

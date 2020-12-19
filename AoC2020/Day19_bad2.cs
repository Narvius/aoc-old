using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    // Second attempt.
    // I tried to... make an object model of a finite state automaton that would tell you if a string is acceptable.
    // Aborted once I realized that I was doing the same thing again just slightly differently--also the fact that
    // I technically had a finished solution that was just was too slow and memory-hungry.
    public class Day19 : ISolution
    {
        public string PartOne(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var automaton = new NondeterministicFiniteStateAutomaton(chunks[0]);
            return chunks[1].Count(automaton.Match).ToString();
        }

        public string PartTwo(string[] lines)
        {
            var chunks = lines.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();
            var automaton = new NondeterministicFiniteStateAutomaton(chunks[0], withLoopRules: true, longestInputSize: chunks[1].Max(s => s.Length));
            return chunks[1].Count(automaton.Match).ToString();
        }
    }

    public class NondeterministicFiniteStateAutomaton
    {
        NfsaNode node;

        public NondeterministicFiniteStateAutomaton(string[] rawRules, int longestInputSize = 0, bool withLoopRules = false)
        {
            // Convert the format of the rules from "[i] => n: rule" to "[n] => rule" for convenience.
            var rules = new string[rawRules.Length];
            foreach (var rawRule in rawRules)
            {
                var items = rawRule.Split(": ");
                rules[int.Parse(items[0])] = items[1].Trim('"', ' ');
            }

            // Construct all the subautomatons.
            var subautomatons = new NfsaNode[rules.Length];
            var empty = new List<NfsaNode>();

            // 8: 42 | 42 8
            // 11: 42 31 | 42 11 31

            // Either of those gets a new alternative.
            // This new alternative is looped.
            // How to maintain loops through concats?

            void BuildAutomaton(int i)
            {
                if (subautomatons[i] != null)
                    return;

                if (rules[i] == "a")
                    subautomatons[i] = new NfsaNode(new[] { new NfsaNode(empty, empty) }, empty);
                else if (rules[i] == "b")
                    subautomatons[i] = new NfsaNode(empty, new[] { new NfsaNode(empty, empty) });
                else if (withLoopRules && i == 8)
                {
                    // 8: add alternative 42 8
                    // Rule #42 is length 6 (checked empirically).
                    // So for the loop we can take one sixth of the longest input and replace the loop with all possible options:
                    // 8: 42 | 42 42 | 42 42 42 | 42 42 42 42 42 | ...
                    // etc up to the longest length
                    var step = GetById(42);

                    subautomatons[8] = step;
                    for (int n = 2; n < longestInputSize / 6; n++)
                        subautomatons[8] = Alternate(subautomatons[8], Concat(step, subautomatons[8]));
                }
                else if (withLoopRules && i == 11)
                {   
                    // 11: add alternative 42 11 31
                    // Rules #42 and #31 together are length 14 (checked empirically).
                    // So for the loop we can take one fourteenth of the longest input and replace the loop with all possible options:
                    // 11: 42 31 | 42 42 31 31 | 42 42 42 31 31 31 | ...
                    // etc up to the longest length
                    var start = GetById(42);
                    var end = GetById(31);
                    
                    subautomatons[11] = Concat(start, end);
                    for (int n = 2; n < longestInputSize / 14; n++)
                        subautomatons[11] = Alternate(subautomatons[11], Concat(start, Concat(subautomatons[11], end)));
                }
                else
                    subautomatons[i] = rules[i].Split('|')
                            .Select(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Select(GetById).Aggregate(Concat))
                            .Aggregate(Alternate);
            }

            NfsaNode GetById(int i)
            {
                BuildAutomaton(i);
                return subautomatons[i];
            }

            NfsaNode Concat(NfsaNode first, NfsaNode second)
            {
                if (first.Terminates)
                    return second;

                return new NfsaNode(first.A.Select(n => Concat(n, second)), first.B.Select(n => Concat(n, second)));
            }

            NfsaNode Alternate(NfsaNode first, NfsaNode second)
                => new NfsaNode(first.A.Concat(second.A), first.B.Concat(second.B));

            node = GetById(0);
        }

        public bool Match(string s)
        {
            var states = new[] { node };

            foreach (var c in s)
                states = states.SelectMany(state => state.Match(c)).ToArray();

            return states.Any(state => state.Terminates);
        }

        /// <summary>
        /// Node in a nondeterministic finite state automaton. Can only accept "a" and "b" as input.
        /// </summary>
        private class NfsaNode
        {
            public List<NfsaNode> A { get; }
            public List<NfsaNode> B { get; }

            public NfsaNode(IEnumerable<NfsaNode> a = null, IEnumerable<NfsaNode> b = null)
            {
                A = (a ?? Enumerable.Empty<NfsaNode>()).ToList();
                B = (b ?? Enumerable.Empty<NfsaNode>()).ToList();
            }

            public IEnumerable<NfsaNode> Match(char c)
                => c switch { 'a' => A, 'b' => B, _ => throw new Exception("invalid input char") };

            public bool Terminates => !A.Any() && !B.Any();
        }
    }
}

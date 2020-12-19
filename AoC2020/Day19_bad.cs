using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    // First attempt.
    // The concept was to create an object model of a state machine that would tell you if a string was acceptable or not.
    // Aborted after I saw the requirements for part 2.
    public class Day19 : ISolution
    {
        public string PartOne(string[] lines)
            => new MessageValidator(lines).CountValidMessages().ToString();

        public string PartTwo(string[] lines)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageValidator
    {
        private readonly RuleStateMachine evaluator;
        private readonly string[] messages;

        public MessageValidator(string[] input)
        {
            var chunks = input.ChunkBy(string.IsNullOrEmpty, true).Select(Enumerable.ToArray).ToArray();

            var rawRules = new string[chunks[0].Length];
            foreach (var rule in chunks[0])
            {
                var items = rule.Split(": ");
                rawRules[int.Parse(items[0])] = items[1].Trim('"', ' ');
            }
            evaluator = new RuleStateMachine(rawRules);

            messages = chunks[1];
        }

        public int CountValidMessages() => messages.Count(evaluator.Matches);
    }

    public class RuleStateMachine
    {
        private readonly INode rule;

        public RuleStateMachine(string[] content)
        {
            var compiledRules = new INode[content.Length];

            void ExpandRule(int i)
            {
                if (compiledRules[i] != null)
                    return;

                if (content[i] == "a" || content[i] == "b")
                    compiledRules[i] = new ConstantNode(content[i][0]);
                else
                    compiledRules[i] = BuildAlternative(content[i]);
            }

            INode BuildAlternative(string s)
            {
                var alternatives = s.Split('|');
                if (alternatives.Length == 1)
                    return BuildConcat(alternatives[0]);
                else
                    return new AlternativeNode(alternatives.Select(BuildConcat));
            }

            INode BuildConcat(string s)
            {
                var concats = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (concats.Length == 1)
                    return FromRuleNumber(int.Parse(concats[0]));
                else
                    return new ConcatNode(concats.Select(c => FromRuleNumber(int.Parse(c))));
            }

            INode FromRuleNumber(int i)
            {
                ExpandRule(i);
                return compiledRules[i];
            }

            rule = FromRuleNumber(0);
        }

        public bool Matches(string s)
        {
            var (matched, i) = rule.Match(s, 0);
            return matched && s.Length == i;
        }

        public interface INode
        {
            (bool matched, int new_index) Match(string s, int i);
        }

        public class ConcatNode : INode
        {
            public IEnumerable<INode> Items { get; }

            public ConcatNode(IEnumerable<INode> items)
                => Items = items.ToArray();

            public (bool, int) Match(string s, int i)
            {
                int start_i = i;
                bool matched;
                foreach (var item in Items)
                {
                    (matched, i) = item.Match(s, i);
                    if (!matched)
                        return (false, start_i);
                }
                return (true, i);
            }
        }

        public class AlternativeNode : INode
        {
            public IEnumerable<INode> Items { get; }

            public AlternativeNode(IEnumerable<INode> items)
                => Items = items.ToArray();

            public (bool, int) Match(string s, int i)
            {
                var results = Items.Select(n => n.Match(s, i)).ToArray();
                var result = results.FirstOrDefault(r => r.matched);

                return (result.matched, result.new_index);
            }
        }

        public class ConstantNode : INode
        {
            public char Constant { get; }

            public ConstantNode(char constant)
                => Constant = constant;

            public (bool, int) Match(string s, int i)
            {
                if (i <= s.Length && s[i] == Constant)
                    return (true, i + 1);
                return (false, i);
            }
        }
    }
}

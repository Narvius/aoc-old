using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AoC2020
{
    public class Day07 : ISolution
    {
        // Find the amount of bag that directly or indirectly contain "shiny gold".
        public string PartOne(string[] lines)
            => Bag.FromRulesFile(lines)["shiny gold"].PossibleContainerCount().ToString();

        // Find the total amount of bags contained within "shiny gold".
        public string PartTwo(string[] lines)
            => Bag.FromRulesFile(lines)["shiny gold"].ContainedBagCount().ToString();
    }

    /// <summary>
    /// Essentially a node in a directed, weighted graph.
    /// </summary>
    public class Bag : IEquatable<Bag>
    {
        private readonly string color;
        private readonly Dictionary<Bag, int> containedIn = new Dictionary<Bag, int>();
        private readonly Dictionary<Bag, int> contains = new Dictionary<Bag, int>();

        public Bag(string color)
        {
            this.color = color;
        }

        /// <summary>
        /// Parses the puzzle input into a useable graph.
        /// </summary>
        /// <param name="rules">The puzzle input.</param>
        /// <returns>A directed, weighted graph in the form of a dictionary containing each node keyed by their <seealso cref="color"/>.</returns>
        public static Dictionary<string, Bag> FromRulesFile(string[] rules)
        {
            const string sourceRegex = @"^\w+ \w+";
            const string targetRegex = @"(\d) (\w+ \w+)";

            var nodes = rules.Select(rule => Regex.Match(rule, sourceRegex).Value).ToDictionary(key => key, key => new Bag(key));
            foreach (var rule in rules)
            {
                var source = Regex.Match(rule, sourceRegex).Value;
                foreach (Match match in Regex.Matches(rule, targetRegex))
                {
                    var target = match.Groups[2].Value;
                    var amount = int.Parse(match.Groups[1].Value);
                    nodes[source].contains.Add(nodes[target], amount);
                    nodes[target].containedIn.Add(nodes[source], amount);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Finds the number of nodes that can contain this node, directly or indirectly.
        /// </summary>
        /// <returns>The amount of nodes that can contain this node, directly or indirectly.</returns>
        public int PossibleContainerCount()
        {
            var containers = new HashSet<Bag>();
            var toProcess = new Queue<Bag>(containedIn.Keys.AsEnumerable());

            while (toProcess.Count > 0)
            {
                var item = toProcess.Dequeue();
                containers.Add(item);
                foreach (var container in item.containedIn.Keys)
                    toProcess.Enqueue(container);
            }

            return containers.Count;
        }

        /// <summary>
        /// Calculates the total amount of bags that have to be included in this one, at every level.
        /// </summary>
        /// <returns>The number of bags contained within this one.</returns>
        public int ContainedBagCount()
            => contains.Sum(pair => pair.Value * (1 + pair.Key.ContainedBagCount()));

        // These allow bags to be used as dictionary keys without much overhead, by just using their color.
        public override bool Equals(object obj) => Equals(obj as Bag);
        public bool Equals(Bag other) => other != null && color == other.color;
        public override int GetHashCode() => color.GetHashCode();
    }
}
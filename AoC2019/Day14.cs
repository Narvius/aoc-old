using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2019
{
    public class Day14 : ISolution
    {
        // Get the total amount of ore used to make exactly one fuel.
        public string PartOne(string[] lines)
            => new ConversionGraph(lines).TotalOreUsedInOneFuel().ToString();

        // Get the amount of fuel that can be produced by efficiently using 1 trillion ore.
        public string PartTwo(string[] lines)
            => new ConversionGraph(lines).AmountOfFuelFromOre(1000000000000).ToString();
    }

    // A graph that describes the interdependencies of the various reagents that can be produced.
    public class ConversionGraph
    {
        // Each node represents a single possible reagent; including what it's called, how many are produced
        // per batch and what it's used in.
        public class Node
        {
            public string Label { get; }
            public int BatchSize { get; }
            public List<(Node node, int perBatch)> UsedIn { get; } = new List<(Node node, int perBatch)>();

            public Node(string label, int batchSize)
            {
                Label = label;
                BatchSize = batchSize;
            }

            // The total amount of _this_ reagent required to craft the requested amount of the named reagent.
            public long TotalPerCraft(string reagent, long amount = 1)
                => Label == reagent ? amount : UsedIn.Sum(p =>
                {
                    var requiredParents = p.node.TotalPerCraft(reagent, amount);
                    var parentBatches = requiredParents / p.node.BatchSize + (requiredParents % p.node.BatchSize > 0 ? 1 : 0);
                    return parentBatches * p.perBatch;
                });
        }

        private readonly Dictionary<string, Node> nodes = new Dictionary<string, Node>();

        public ConversionGraph(string[] lines)
        {
            var links = new List<(string from, int cost, string to)>();
            nodes.Add("ORE", new Node("ORE", 1));

            foreach (var line in lines)
            {
                var items = line.Split(" => ");
                var (name, batchSize) = ParseReagent(items[1]);
                nodes[name] = new Node(name, batchSize);
                foreach (var prereq in items[0].Split(", "))
                {
                    var (r, a) = ParseReagent(prereq);
                    links.Add((r, a, name));
                }
            }

            foreach (var (from, cost, to) in links)
                nodes[from].UsedIn.Add((nodes[to], cost));
        }

        public long TotalOreUsedInOneFuel() => nodes["ORE"].TotalPerCraft("FUEL");

        public long AmountOfFuelFromOre(long amount)
        {
            var node = nodes["ORE"];

            // Arbitrary first guess.
            int fuels = 100;
            long used = node.TotalPerCraft("FUEL", fuels);

            // Count up until we're just above the spending limit; but use much larger steps if we're far away.
            while (used < amount)
            {
                fuels += Math.Max(1, (int)Math.Floor(((double)amount - used) / ((double)used / fuels)));
                used = node.TotalPerCraft("FUEL", fuels);
            }

            // If the numbers align, we could overshoot by a couple fuels. Count down until we're back below the spending limit.
            while (used > amount)
            {
                fuels--;
                used = node.TotalPerCraft("FUEL", fuels);
            }

            return fuels;
        }

        private (string reagent, int amount) ParseReagent(string data)
        {
            var items = data.Split(' ');
            return (items[1].Trim(), int.Parse(items[0]));
        }
    }
}

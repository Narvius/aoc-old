using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day8 : ISolution
    {
        // The sum of all metadata in the tree.
        public string PartOne(string[] lines)
            => Traverse(Node.FromData(lines[0]), n => n.Children).Sum(n => n.Metadata.Sum()).ToString();
        
        // The value of the root node.
        public string PartTwo(string[] lines)
            => Node.FromData(lines[0]).GetValue().ToString();

        // Walks over any tree-like structure in a breadth-first manner.
        private IEnumerable<T> Traverse<T>(T root, Func<T, IEnumerable<T>> children)
        {
            var queue = new Queue<T>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                T item = queue.Dequeue();
                yield return item;
                foreach (var child in children(item))
                    queue.Enqueue(child);
            }
        }
    }

    public class Node
    {
        public readonly Node[] Children;
        public readonly int[] Metadata;

        // Parses the data format. For each node, recursively, it's:
        // [children count] [metadata count] [child data] [metadata]
        private Node(int[] data, ref int pointer)
        {
            Children = new Node[data[pointer++]];
            Metadata = new int[data[pointer++]];

            for (int i = 0; i < Children.Length; i++)
                Children[i] = new Node(data, ref pointer);

            for (int i = 0; i < Metadata.Length; i++)
                Metadata[i] = data[pointer++];
        }

        // Calculates the value of the node, based on the description in the challenge.
        public int GetValue()
        {
            if (Children.Length == 0)
                return Metadata.Sum();

            int sum = 0;
            foreach (var entry in Metadata.Select(m => m - 1))
                if (0 <= entry && entry < Children.Length)
                    sum += Children[entry].GetValue();
            return sum;
        }

        // Returns the tree described by the string.
        public static Node FromData(string data)
        {
            int i = 0;
            return new Node(data.Split(' ').Select(int.Parse).ToArray(), ref i);
        }
    }
}

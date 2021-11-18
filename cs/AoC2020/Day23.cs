using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day23 : ISolution
    {
        // For 9 cups, simulate 100 steps. Get the full order of cups.
        public string PartOne(string[] lines)
            => new Cups(lines[0]).StepTimes(100).After1Labels();

        // For one million cups, simulate ten million steps. Get a checksum of the cup order.
        public string PartTwo(string[] lines)
            => new Cups(lines[0], 1000000).StepTimes(10000000).After1Product().ToString();
    }

    /// <summary>
    /// Conceptually, a linked list with node values of type int. But because we use every integer from 1 to the number of cups, we can
    /// just use an array of integers as an implementation of the linked list. We re-label the cups with a number 1 lower, so their
    /// labelling is zero-based, and then "n" is the value of a cup, and "next[n]" is the value of the cup after it.
    /// </summary>
    public class Cups
    {
        private int[] next;
        private int current;

        public Cups(string input, int padTo = 0)
        {
            next = padTo > input.Length ? Enumerable.Range(1, padTo).ToArray() : new int[input.Length];
            current = input[0] - '1';

            foreach (var (p, n) in input.Zip(input.Skip(1).Append(input[0])))
                next[p - '1'] = n - '1';

            if (padTo > input.Length)
            {
                next[input[input.Length - 1] - '1'] = input.Length;
                next[next.Length - 1] = input[0] - '1';
            }
        }

        /// <summary>
        /// Performs a single step of the cup game.
        /// </summary>
        private void Step()
        {
            var (pa, pb, pc) = (next[current], next[next[current]], next[next[next[current]]]);
            var targetValue = current;
            do
            {
                targetValue = (targetValue + next.Length - 1) % next.Length;
            }
            while (targetValue == pa || targetValue == pb || targetValue == pc);

            var (preSplice, toMoveFirst, toMoveLast, postSplice) = (targetValue, next[current], next[next[next[current]]], next[targetValue]);

            next[current] = next[toMoveLast]; // Take out the 3 cups to move from the linked list.
            next[preSplice] = toMoveFirst;    // Append the three cups to the destination cup
            next[toMoveLast] = postSplice;    // Prepend the three cups to the cup after the destination cup

            current = next[current];
        }

        /// <summary>
        /// Performs <paramref name="n"/> steps of the cup game.
        /// </summary>
        /// <returns>The cups objects, in order to allow for a chained result calculation call.</returns>
        public Cups StepTimes(int n)
        {
            for (int i = 0; i < n; i++)
                Step();
            return this;
        }

        /// <summary>
        /// For the current cup constellation, gives the order of the cups.
        /// </summary>
        /// <returns>A concatenation of all cup labels going clockwise from (but excluding) the cup labelled 1.</returns>
        public string After1Labels()
            => string.Join("", 0.Unfold(c => ($"{c + 1}", next[c])).Skip(1).TakeWhile(s => s != "1"));

        /// <summary>
        /// For the current cup constellation, gives a checksum of the cup order.
        /// </summary>
        /// <returns>The product of the labels of the first two cups clockwise from the cup labelled 1.</returns>
        public long After1Product()
            => (long)(next[0] + 1) * (next[next[0]] + 1);
    }
}

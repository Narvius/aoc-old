using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdventOfCode2018
{
    public class Day9 : ISolution
    {
        // Result of the game.
        public string PartOne(string[] lines)
        {
            ExtractLimits(lines, out var players, out var maxMarble);
            var scores = new long[players];
            RunGame(scores, maxMarble);

            return scores.Max().ToString();
        }

        // The same but 100x bigger.
        public string PartTwo(string[] lines)
        {
            ExtractLimits(lines, out var players, out var maxMarble);
            var scores = new long[players];
            RunGame(scores, maxMarble * 100);

            return scores.Max().ToString();
        }

        // Runs the game according to the rules.
        private void RunGame(long[] scores, int maxMarble)
        {
            var list = new CircularLinkedList<int>() { 0 };

            int currentPlayer = 0;
            for (int marble = 1; marble <= maxMarble; marble++)
            {
                if ((marble % 23) == 0)
                {
                    list.Move(-7);
                    scores[currentPlayer] += marble + list.Remove();
                }
                else
                {
                    list.Move(1);
                    list.Add(marble);
                }
                currentPlayer = (currentPlayer + 1) % scores.Length;
            }
        }

        // Parse the input.
        private void ExtractLimits(string[] lines, out int players, out int maxMarble)
        {
            var data = lines[0].Split(';');
            players = int.Parse(new string(data[0].Where(char.IsDigit).ToArray()));
            maxMarble = int.Parse(new string(data[1].Where(char.IsDigit).ToArray()));
        }
    }

    // A thin and poorly-written wrapper around a linked list to make it circular.
    public class CircularLinkedList<T> : IEnumerable<T>
    {
        private LinkedList<T> data = new LinkedList<T>();
        private LinkedListNode<T> first;
        private LinkedListNode<T> last;
        private LinkedListNode<T> current;

        // Moves the "current" pointer. To the right if positive, to the left if negative.
        public void Move(int offset)
        {
            bool back = offset < 0;
            int d = Math.Abs(offset);

            if (back)
                for (int i = 0; i < d; i++)
                    if (current == first)
                        current = last;
                    else
                        current = current.Previous;
            else
                for (int i = 0; i < d; i++)
                    if (current == last)
                        current = first;
                    else
                        current = current.Next;
        }

        // Adds an item after the current element.
        public void Add(T item)
        {
            if (current == null)
                first = last = current = data.AddLast(item);
            else
            {
                var @new = data.AddAfter(current, item);
                if (current == last)
                    current = last = @new;
                else
                    current = @new;
            }
        }

        // Removes the current element from the list and returns it.
        public T Remove()
        {
            var toRemove = current;
            if (toRemove == last)
                last = toRemove.Previous;
            if (toRemove == first)
                first = toRemove.Next;
            Move(1);
            data.Remove(toRemove);
            return toRemove.Value;
        }

        public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    }
}

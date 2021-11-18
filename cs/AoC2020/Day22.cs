using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day22 : ISolution
    {
        // Run a game of Combat, get the winner's score.
        public string PartOne(string[] lines)
            => new Combat(lines).ScoreOfWinningDeck().ToString();

        // Run a game of Recursive Combat, get the winner's score.
        public string PartTwo(string[] lines)
            => new Combat(lines, recursive: true).ScoreOfWinningDeck().ToString();
    }

    /// <summary>
    /// Simulates the "card game" Combat, as well as the variant Recursive Combat.
    /// </summary>
    public class Combat
    {
        private readonly Queue<int> p1;
        private readonly Queue<int> p2;

        private bool recursive;
        private readonly HashSet<int> previousStateHashes = new HashSet<int>();

        /// <summary>
        /// Makes a hashcode of a relevant part of the current game state. Used when saving and detecting duplicate states.
        /// </summary>
        private int StateHash
            => HashCode.Combine(
                p1.ElementAtOrDefault(0), p1.ElementAtOrDefault(1), p1.ElementAtOrDefault(2), p1.ElementAtOrDefault(3),
                p2.ElementAtOrDefault(0), p2.ElementAtOrDefault(1), p2.ElementAtOrDefault(2), p2.ElementAtOrDefault(3));

        public Combat(string[] input, bool recursive = false)
        {
            var chunks = input.ChunkBy(string.IsNullOrEmpty, true).Select(chunk => chunk.ToArray()).ToArray();
            p1 = new Queue<int>(chunks[0].Skip(1).Select(int.Parse));
            p2 = new Queue<int>(chunks[1].Skip(1).Select(int.Parse));
            this.recursive = recursive;
        }

        private Combat(IEnumerable<int> p1, IEnumerable<int> p2)
        {
            this.p1 = new Queue<int>(p1);
            this.p2 = new Queue<int>(p2);
            recursive = true;
        }

        /// <summary>
        /// Performs one step of simulation in this game. The recursive variant may trigger a long chain
        /// of recursive sub-games.
        /// </summary>
        private void Step()
        {
            var (c1, c2) = (p1.Dequeue(), p2.Dequeue());
            var player2won = c2 > c1;
            if (recursive && c1 <= p1.Count && c2 <= p2.Count)
                player2won = new Combat(p1.Take(c1), p2.Take(c2)).Run();

            if (player2won) { p2.Enqueue(c2); p2.Enqueue(c1); }
            else { p1.Enqueue(c1); p1.Enqueue(c2); }
        }

        /// <summary>
        /// Keeps performing steps of the game until one player wins.
        /// </summary>
        /// <returns>False if player 1 won, true if player 2 won.</returns>
        private bool Run()
        {
            while (p1.Count > 0 && p2.Count > 0)
            {
                if (recursive)
                {
                    var hash = StateHash;
                    if (previousStateHashes.Contains(hash))
                        return false;
                    previousStateHashes.Add(hash);
                }

                Step();
            }
            return p2.Count > 0;
        }

        /// <summary>
        /// Finds the deck of the winner and calculates its score.
        /// </summary>
        /// <returns>Score of the winning deck.</returns>
        public int ScoreOfWinningDeck()
        {
            var deck = Run() ? p2 : p1;
            return deck.Select((c, i) => (deck.Count - i) * c).Sum();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day25 : ISolution
    {
        // Find the encryption key for the given pair of public keys.
        public string PartOne(string[] lines)
            => new Encryption().GetEncryptionKey(int.Parse(lines[0]), int.Parse(lines[1])).ToString();

        // Nothing! I'm done.
        public string PartTwo(string[] lines)
            => "Completed AoC2020";
    }

    public class Encryption
    {
        const int ModuloFactor = 20201227;

        /// <summary>
        /// Finds the encryption key (which is one of the public keys raised to the other key's secret loop size).
        /// </summary>
        public int GetEncryptionKey(int cardKey, int doorKey)
            => ModularExponentiation(doorKey, DiscreteLogarithm(7, cardKey, ModuloFactor), ModuloFactor);

        /// <summary>
        /// Finds the smallest number n that satisfies a^n == b (modulo m), using trial multiplication.
        /// </summary>
        private int DiscreteLogarithm(int a, int b, int m)
        {
            int i = 0;
            long k = 1;
            for (i = 0; k != b; i++)
                k = (k * a) % m;
            return i;
        }

        /// <summary>
        /// Calculates x^k (modulo m), using square-and-multiply.
        /// </summary>
        private int ModularExponentiation(int x, int k, int m)
        {
            long r = 1;
            var bits = (int)Math.Ceiling(Math.Log2(k));

            for (int i = bits - 1; i >= 0; i--)
            {
                r = (r * r) % m;
                if ((k & 1 << i) > 0)
                    r = (r * x) % m;
            }

            return (int)r;
        }
    }
}

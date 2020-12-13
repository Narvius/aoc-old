using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public class Day13 : ISolution
    {
        // Find the bus that's to come soonest after the given timestamp; calculate a checksum from that.
        public string PartOne(string[] lines)
        {
            int estimate = int.Parse(lines[0]);
            return (from item in lines[1].Split(',')
                    where item != "x"
                    let busLine = int.Parse(item)
                    let waitTime = busLine -  estimate % busLine
                    orderby waitTime ascending
                    select waitTime * busLine).First().ToString();
        }

        // Find the earliest timestamp at which the described sequence of buses starts.
        public string PartTwo(string[] lines)
        {
            // GENERAL CONCEPT:
            // Just trying candidates until I find one that works, but in a really constrained search space.
            // First observation: Buses that have a smaller offset than line number constute "true modulo constraints".
            // That means that (if "n" is the line number, "offset" is the offset, and "t" is the timestamp we seek):
            //   t % n == offset
            // We can combine all of those into one modulo constraint using the Chinese Remainder Theorem in order to
            // get a number to start searching at (the inverse of the  offset of the resulting combined constraint) and
            // a search step size (the "n" of the combined constraint).

            // Use the Chinese Remainder Theorem to combine two modulo constraints into one.
            // See: https://en.wikipedia.org/wiki/Chinese_remainder_theorem#Using_the_existence_construction
            (long n, long offset) CombineConstraints((long n, long offset) a, (long n, long offset) b)
            {
                var (gcd, x, y) = ExtendedEuclidian(a.n, b.n);
                return (a.n * b.n, TrueModulo(a.n * x * b.offset + b.n * y * a.offset, a.n * b.n));
            }

            var constraints = lines[1].Split(',')
                .Select((s, i) => (n: s == "x" ? -1 : long.Parse(s), offset: (long)i))
                .Where(c => c.n != -1);

            // Combine all constraints that are true modulo constraints into one modulo constraint using the Chinese Remainder Theorem.
            var moduloConstraint = constraints.Where(c => c.offset < c.n).Aggregate(CombineConstraints);
            constraints = constraints.Where(c => c.offset >= c.n).ToArray();

            // Check all the remaining constraints with brute force, using the one modulo constraint as a limit on the search space.
            for (long result = -moduloConstraint.offset; ; result += moduloConstraint.n)
                if (constraints.All(c => ((result + c.offset) % c.n) == 0))
                    return result.ToString();

            throw new Exception("input violates assumptions");
        }

        /// <summary>
        /// Find the only solution to 'n + km' that is in the interval (0, m).
        /// Different from just the '%' operator in that it works on negative numbers.
        /// </summary>
        /// <returns></returns>
        private long TrueModulo(long n, long m) => (n % m + m) % m;

        /// <summary>
        /// Calculates the greatest common divisor of two numbers, as well as a pair of Bezout coefficients for them.
        /// The Bezout coefficients are used in the Chinese Remainder Theorem to combine two modulo constraints into one.
        /// </summary>
        /// <remarks>
        /// Straightforward translation of the pseudocode from https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
        /// </remarks>
        /// <returns>A three-tuple containing the GCD, as well as Bezout coefficients for <paramref name="a"/> and <paramref name="b"/>.</returns>
        private (long gcd, long bezout_x, long bezout_y) ExtendedEuclidian(long a, long b)
        {
            var (old_r, r) = (a, b);
            var (old_s, s) = (1L, 0L);
            var (old_t, t) = (0L, 1L);

            while (r != 0)
            {
                var quotient = old_r / r;
                (old_r, r) = (r, old_r - quotient * r);
                (old_s, s) = (s, old_s - quotient * s);
                (old_t, t) = (t, old_t - quotient * t);
            }

            return (old_r, old_s, old_t);
        }
    }
}

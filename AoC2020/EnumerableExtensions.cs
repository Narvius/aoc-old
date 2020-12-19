using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoC2020
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Subdivides an enumeration into chunks of the given size. The last chunk might be shorter.
        /// </summary>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>The original enumeration, but subdivided into chunks.</returns>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> @this, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("must be greater than 0", nameof(chunkSize));

            var enumerator = @this.GetEnumerator();

            IEnumerable<T> BuildChunk()
            {
                yield return enumerator.Current;
                for (int i = 1; i < chunkSize && enumerator.MoveNext(); i++)
                    yield return enumerator.Current;
            }

            while (enumerator.MoveNext())
                yield return BuildChunk();
        }

        /// <summary>
        /// Subdivides an enumeration into chunks, with the final element of each chunk being an item that matches the given predicate.
        /// </summary>
        /// <remarks>The final chunk might not be terminated by an item matching the predicate.</remarks>
        /// <param name="predicate">Used to decide whether an item is the end of a chunk.</param>
        /// <param name="excludeMatchingItems">If true, the items matching <paramref name="predicate"/> will be filtered out. Defaults to false.</param>
        /// <returns>The original enumeration, but subdivided into chunks.</returns>
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> @this, Predicate<T> predicate, bool excludeMatchingItems = false)
        {
            var enumerator = @this.GetEnumerator();
            bool hadItem = true;
            bool emptyChunk = false;

            IEnumerable<T> BuildChunk()
            {
                emptyChunk = true;
                while (hadItem = enumerator.MoveNext())
                {
                    var matches = predicate(enumerator.Current);

                    if (!(excludeMatchingItems && matches))
                    {
                        yield return enumerator.Current;
                        emptyChunk = false;
                    }
                    if (matches)
                        yield break;
                }
            }

            while (hadItem)
            {
                var chunk = BuildChunk();
                if (!emptyChunk)
                    yield return chunk;
            }
        }

        /// <summary>
        /// Generates an infinite sequence by repeatedly applying the provided function to a state value and returning the result of it.
        /// </summary>
        /// <typeparam name="T">Type of the elements in the generated sequence.</typeparam>
        /// <typeparam name="TState">Type of the state passed into the generator function.</typeparam>
        /// <param name="this">The initial value to pass into the generator function.</param>
        /// <param name="f">The generator function.</param>
        /// <returns>An infinite sequence of return values of the generator function.</returns>
        public static IEnumerable<T> Unfold<T, TState>(this TState @this, Func<TState, (T result, TState newState)> f)
        {
            TState state = @this;
            T result;

            while (true)
            {
                (result, state) = f(state);
                yield return result;
            }
        }

        /// <summary>
        /// Generates an infinite sequence by applying the provided function to the previous element in the sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements produced.</typeparam>
        /// <param name="this">The initial element, also included in the result.</param>
        /// <param name="f">The generator function.</param>
        /// <returns>An infinite sequence of return values of the generator function.</returns>
        public static IEnumerable<T> Unfold<T>(this T @this, Func<T, T> f)
            => Unfold(@this, s => { var t = f(s); return (t, t); }).Prepend(@this);
    }
}
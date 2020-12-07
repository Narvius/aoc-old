using System;
using System.Collections.Generic;
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
    }
}
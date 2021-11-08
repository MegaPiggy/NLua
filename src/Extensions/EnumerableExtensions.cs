using System;
using System.Collections.Generic;
using System.Text;

namespace NLua.Extensions
{
    static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            yield return element;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            yield return element;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> addition)
        {
            using (var enumerator = addition.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> addition)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            using (var enumerator = addition.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }
    }
}

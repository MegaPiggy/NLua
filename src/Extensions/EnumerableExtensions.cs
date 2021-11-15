using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLua.Extensions
{
    static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            yield return element;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
            yield return element;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> addition)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (addition == null)
                throw new ArgumentNullException("addition");

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
            if (source == null)
                throw new ArgumentNullException("source");

            if (addition == null)
                throw new ArgumentNullException("addition");

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

        public static T MiddleOrDefault<T>(this IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            int count = items.Count();
            if (count == 0)
                return default(T);
            else if (count % 2 == 0)
                // count is even, return the element at the length divided by 2
                return NthOrDefault(items, count / 2);
            else
                // count is odd, return the middle element
                return NthOrDefault(items, (int)Math.Ceiling((double)count / 2D));
        }

        public static T MiddleOrDefault<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            var filteredItems = items.Where(item => predicate(item));
            int count = filteredItems.Count();
            if (count == 0)
                return default(T);
            else if (count % 2 == 0)
                // count is even, return the element at the length divided by 2
                return NthOrDefault(filteredItems, count / 2);
            else
                // count is odd, return the middle element
                return NthOrDefault(filteredItems, (int)Math.Ceiling((double)count / 2D));
        }

        public static T SecondOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 2);
        public static T ThirdOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 3);
        public static T FourthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 4);
        public static T FifthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 5);
        public static T SixthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 6);
        public static T SeventhOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 7);
        public static T EighthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 8);
        public static T NinthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 9);
        public static T TenthOrDefault<T>(this IEnumerable<T> items) => NthOrDefault(items, 10);

        public static T NthOrDefault<T>(this IEnumerable<T> items, int n)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (n <= 1)
                return items.FirstOrDefault();

            return items.Skip(n - 1).FirstOrDefault();
        }

        public static T NthOrDefault<T>(this IEnumerable<T> items, int n, Func<T, bool> predicate)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (predicate == null)
                throw new ArgumentNullException("predicate");

            if (n <= 1)
                return items.FirstOrDefault(predicate);

            return items.Where(item => predicate(item)).Skip(n - 1).FirstOrDefault();
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> kvps)
        {
            if (kvps == null)
                throw new ArgumentNullException("kvps");

            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            foreach (KeyValuePair<TKey, TValue> kvp in kvps)
                dict.Add(kvp.Key, kvp.Value);

            return dict;
        }
    }
}

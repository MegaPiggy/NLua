using System;
using System.Collections.Generic;
using System.Text;

namespace NLua
{
    public static class Luau
    {
        //public static LuaTable Create(long size, params object[] values)
        //{
        //}

        /// <summary>
        /// Within the given array-like table <paramref name="haystack"/>, find the first occurrence of value <paramref name="needle"/>, starting from index <paramref name="init"/> or the beginning if not provided.
        /// <br/>If the value is not found, <see langword="null"/> is returned.
        /// <br/><br/>
        /// A linear search algorithm is performed.
        /// </summary>
        /// <returns>the index of the found value or <see langword="null"/> if not found.</returns>
        public static long? Find(LuaTable haystack, object needle, long init = 1)
        {
            for (long i = init; true; ++i)
            {
                var e = haystack[i];
                if (e == null)
                    break;

                if (needle.Equals(e))
                    return i;
            }
            return null;
        }

        public static void Clear(LuaTable table) => table.Foreach((i, v) => table[i] = null);

        /// <returns>the number of elements in the table passed.</returns>
        public static int GetN(LuaTable table) => table.Length;
    }
}

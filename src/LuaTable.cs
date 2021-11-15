
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NLua.Extensions;

using LuaState = KeraLua.Lua;

namespace NLua
{
    /// <summary>
    /// A lua table
    /// </summary>
    public class LuaTable : LuaBase
    {
        public LuaTable(int reference, Lua interpreter): base(reference, interpreter)
        {
        }

        /// <summary>
        /// Indexer for string fields of the table
        /// </summary>
        public object this[string field] {
            get
            {
                Lua lua;
                if (!TryGet(out lua))
                    return null;
                return lua.GetObject(_Reference, field);
            }
            set
            {
                Lua lua;
                if (!TryGet(out lua))
                    return;
                lua.SetObject(_Reference, field, value);
            }
        }

        /// <summary>
        /// Indexer for numeric fields of the table
        /// </summary>
        public object this[object field] {
            get
            {
                Lua lua;
                if (!TryGet(out lua))
                    return null;

                return lua.GetObject(_Reference, field);
            }
            set
            {
                Lua lua;
                if (!TryGet(out lua))
                    return;

                lua.SetObject(_Reference, field, value);
            }
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            //Lua lua;
            //if (!TryGet(out lua))
            //    return null;

            //return lua.GetTableDict(this).GetEnumerator();
            return this.ToDictionary().GetEnumerator();
        }

        public ICollection Keys
        {
            get
            {
                //Lua lua;
                //if (!TryGet(out lua))
                //    return null;

                //return lua.GetTableDict(this).Keys;
                return this.ToDictionary().Keys;
            }
        }


        public ICollection Values
        {
            get
            {
                //Lua lua;
                //if (!TryGet(out lua))
                //    return new object[0];

                //return lua.GetTableDict(this).Values;
                return this.ToDictionary().Values;
            }
        }


        /// <summary>
        /// Gets an object at the string index of a table ignoring its metatable, if it exists
        /// </summary>
        internal object RawGet(string index)
        {
            Lua lua;
            if (!TryGet(out lua))
                return null;

            return lua.RawGetObject(_Reference, index);
        }

        /// <summary>
        /// Gets an object at the numerical index of a table ignoring its metatable, if it exists
        /// </summary>
        internal object RawGet(object index)
        {
            Lua lua;
            if (!TryGet(out lua))
                return null;

            return lua.RawGetObject(_Reference, index);
        }

        /// <summary>
        /// Sets an object at the string index of a table ignoring its metatable, if it exists
        /// </summary>
        internal void RawSet(string index, object value)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.RawSetObject(_Reference, index, value);
        }

        /// <summary>
        /// Sets an object at the numerical index of a table ignoring its metatable, if it exists
        /// </summary>
        internal void RawSet(object index, object value)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.RawSetObject(_Reference, index, value);
        }


        /// <summary>
        /// Pushes this table into the Lua stack
        /// </summary>
        /// <param name="luaState"></param>
        internal void Push(LuaState luaState)
        {
            luaState.GetRef(_Reference);
        }

        public override string ToString()
        {
            string bstring = base.ToString();
            return bstring == "nil" ? "table" : bstring;
        }

        /// <summary>Concatenates the elements from the given list.</summary>
        public string Concat()
        {
            Lua lua;
            if (!TryGet(out lua))
                return string.Empty;

            return (string)lua.GetFunction("table.concat").Call(this).FirstOrDefault();
        }

        /// <summary>Concatenates the elements from the given list.</summary>
        public string Concat(string sep)
        {
            Lua lua;
            if (!TryGet(out lua))
                return string.Empty;

            return (string)lua.GetFunction("table.concat").Call(this, sep).FirstOrDefault();
        }

        /// <summary>Concatenates the elements from the given list.</summary>
        public string Concat(int i, string sep = "")
        {
            Lua lua;
            if (!TryGet(out lua))
                return string.Empty;

            return (string)lua.GetFunction("table.concat").Call(this, sep, i).FirstOrDefault();
        }

        /// <summary>Concatenates the elements from the given list.</summary>
        public string Concat(int i, int j, string sep = "")
        {
            Lua lua;
            if (!TryGet(out lua))
                return string.Empty;

            return (string)lua.GetFunction("table.concat").Call(this, sep, i, j).FirstOrDefault();
        }

        /// <summary>
        /// Inserts element <paramref name="value"/> at the end of the table.
        /// </summary>
        public void Insert(object value)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.insert").Call(this, value);
        }

        /// <summary>
        /// Inserts element <paramref name="value"/> at position <paramref name="pos"/> in the table.
        /// </summary>
        public void Insert(int pos, object value)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.insert").Call(this, pos, value);
        }

        /// <summary>
        /// Moves elements at indexes <paramref name="f"/> through <paramref name="e"/> to index <paramref name="t"/>
        /// </summary>
        public void Move(int f, int e, int t)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.move").Call(this, f, e, t);
        }

        /// <summary>
        /// Moves elements at indexes <paramref name="f"/> through <paramref name="e"/> in source to index <paramref name="t"/> in <paramref name="dst"/>
        /// </summary>
        public void Move(int f, int e, int t, LuaTable dst)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.move").Call(this, f, e, t, dst);
        }

        /// <summary>
        /// Removes the the last element.
        /// </summary>
        /// <returns>the value of the removed element.</returns>
        public object Remove()
        {
            Lua lua;
            if (!TryGet(out lua))
                return null;

            return lua.GetFunction("table.remove").Call(this).FirstOrDefault();
        }

        /// <summary>
        /// Removes the element at position <paramref name="pos"/>.
        /// </summary>
        /// <returns>the value of the removed element.</returns>
        public object Remove(int pos)
        {
            Lua lua;
            if (!TryGet(out lua))
                return null;

            return lua.GetFunction("table.remove").Call(this, pos).FirstOrDefault();
        }

        /// <summary>
        /// Sorts the list elements in a given order.
        /// </summary>
        public void Sort()
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.sort").Call(this);
        }

        /// <summary>
        /// Sorts the list elements in a given order.
        /// </summary>
        public void Sort(LuaFunction comp)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.GetFunction("table.sort").Call(this, comp);
        }

        /// <returns>The elements from the given list.</returns>
        public object[] Unpack()
        {
            Lua lua;
            if (!TryGet(out lua))
                return Array.Empty<object>();

            return lua.GetFunction("table.unpack").Call(this);
        }

        /// <returns>The elements from the given list.</returns>
        public object[] Unpack(int i)
        {
            Lua lua;
            if (!TryGet(out lua))
                return Array.Empty<object>();

            return lua.GetFunction("table.unpack").Call(this, i);
        }

        /// <returns>The elements from the given list.</returns>
        public object[] Unpack(int i, int j)
        {
            Lua lua;
            if (!TryGet(out lua))
                return Array.Empty<object>();

            return lua.GetFunction("table.unpack").Call(this, i, j);
        }

        /// <summary>
        /// Sets the value for all keys within the given table to nil.<br/>
        /// This causes the # operator to return 0 for the given table.<br/>
        /// The allocated capacity of the table’s array portion is maintained, which allows for efficient re-use of the space.<br/>
        /// This function does not delete/destroy the table provided to it.<br/>
        /// This function is meant to be used specifically for tables that are to be re-used.
        /// </summary>
        public void Clear()
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.Table.Clear.Call(this);
        }

        /// <summary>
        /// Apply the function <paramref name="f"/> to the elements of the table passed. On each iteration the function <paramref name="f"/> is passed the key-value pair of that element in the table.<br/>
        /// If the function <paramref name="f"/> returns a non-<see langword="null"/> value the iteration loop terminates.
        /// </summary>
        public void Foreach(LuaFunction f)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.Table.Foreach.Call(this, f);
        }

        /// <summary>
        /// Apply the function <paramref name="f"/> to the elements of the table passed. On each iteration the function <paramref name="f"/> is passed the key-value pair of that element in the table.<br/>
        /// If the function <paramref name="f"/> returns a non-<see langword="null"/> value the iteration loop terminates.
        /// </summary>
        public void Foreach(Action<object, object> f)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.Table.Foreach.Call(this, lua.RegisterFunction(f));
        }

        /// <summary>
        /// Apply the function <paramref name="f"/> to the elements of the table passed. On each iteration the function <paramref name="f"/> is passed the index-value pair of that element in the table.<br/>
        /// If the function <paramref name="f"/> returns a non-<see langword="null"/> value the iteration loop terminates.<br/><br/>
        /// This is similar to <see cref="Foreach(LuaFunction)"/> except that index-value pairs are passed, not key-value pairs.
        /// </summary>
        public void Foreachi(LuaFunction f)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.Table.Foreachi.Call(this, f);
        }

        /// <summary>
        /// Apply the function <paramref name="f"/> to the elements of the table passed. On each iteration the function <paramref name="f"/> is passed the index-value pair of that element in the table.<br/>
        /// If the function <paramref name="f"/> returns a non-<see langword="null"/> value the iteration loop terminates.<br/><br/>
        /// This is similar to <see cref="Foreach(LuaFunction)"/> except that index-value pairs are passed, not key-value pairs.
        /// </summary>
        public void Foreachi(Action<object, object> f)
        {
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.Table.Foreachi.Call(this, lua.RegisterFunction(f));
        }

        public object Find(object needle, int init = 1)
        {
            LuaTable haystack = this;
            for (int i = init; true; ++i)
            {
                var e = haystack[i];
                if (e == null)
                    break;

                if (needle.Equals(e))
                    return i;
            }
            return null;
        }

        public int Length
        {
            get
            {
                Lua lua;
                if (!TryGet(out lua))
                    return 0;

                return lua.RawLen(this);
            }
        }

        public object[] ToArray() => this.ToList().ToArray();

        public List<object> ToList()
        {
            List<object> list = new List<object>();

            this.Foreach((i, v) => list.Add(v));

            return list;
        }

        public Dictionary<object, object> ToDictionary()
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            this.Foreach((i, v) => dict.Add(i, v));

            return dict;
        }
    }
}


using System;
using System.Collections;

using NLua.Extensions;

using LuaState = KeraLua.Lua;

namespace NLua
{
    /// <summary>
    /// A lua lightuserdata
    /// </summary>
    public class LuaLightUserData : LuaBase
    {
        public LuaLightUserData(int reference, Lua lua) : base(reference, lua)
        {
        }

        /// <summary>
        /// Pushes this light userdata into the Lua stack
        /// </summary>
        internal void Push(LuaState luaState)
        {
            luaState.GetRef(_Reference);
        }

        public override string ToString()
        {
            string bstring = base.ToString();
            return bstring == "nil" ? "lightuserdata" : bstring;
        }
    }
}

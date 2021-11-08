using System;
using System.Linq;

namespace NLua
{
    /// <summary>
    /// Base class to provide consistent disposal flow across lua objects. Uses code provided by Yves Duhoux and suggestions by Hans Schmeidenbacher and Qingrui Li 
    /// </summary>
    public abstract class LuaBase : IDisposable
    {
        private bool _disposed;
        /// <summary>
        /// Reference to the object
        /// </summary>
        protected readonly int _Reference;
        Lua _lua;

        /// <summary>
        /// Gets the lua interpreter if the state is not closed.
        /// </summary>
        /// <param name="lua">the got lua interpreter</param>
        /// <returns>successfulness of the get</returns>
        protected bool TryGet(out Lua lua)
        {
            if (_lua.State == null)
            {
                lua = null;
                return false;
            }

            lua = _lua;
            return true;
        }

        protected LuaBase(int reference, Lua lua)
        {
            _lua = lua;
            _Reference = reference;
        }

        ~LuaBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the lua reference
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void DisposeLuaReference(bool finalized)
        {
            if (_lua == null)
                return;
            Lua lua;
            if (!TryGet(out lua))
                return;

            lua.DisposeInternal(_Reference, finalized);
        }

        /// <summary>
        /// Disposes the lua reference
        /// </summary>
        public virtual void Dispose(bool disposeManagedResources)
        {
            if (_disposed)
                return;

            bool finalized = !disposeManagedResources;

            if (_Reference != 0)
            {
                DisposeLuaReference(finalized);
            }

            _lua = null;
            _disposed = true;
        }

        /// <summary>
        /// Compares the references of 2 <see cref="LuaBase"/> objects.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            var reference = o as LuaBase;
            if (reference == null)
                return false;

            Lua lua;
            if (!TryGet(out lua))
                return false;

            return lua.CompareRef(reference._Reference, _Reference);
        }

        /// <summary>
        /// Gets the reference to the object
        /// </summary>
        public override int GetHashCode()
        {
            return _Reference;
        }

        /// <summary>
        /// Object as a string
        /// </summary>
        public override string ToString()
        {
            Lua lua;
            if (!TryGet(out lua))
                return "nil";

            return (string)((lua.GetObjectFromPath("tostring") as LuaFunction).Call(this).FirstOrDefault());
        }
    }
}

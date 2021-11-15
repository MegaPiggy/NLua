using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using KeraLua;

using NLua.Event;
using NLua.Method;
using NLua.Exceptions;
using NLua.Extensions;

#if __IOS__ || __TVOS__ || __WATCHOS__
    using ObjCRuntime;
#endif

using LuaState = KeraLua.Lua;
using LuaNativeFunction = KeraLua.LuaFunction;
using System.Text;

namespace NLua
{
    public class Lua : IDisposable
    {
        #region lua debug functions
        /// <summary>
        /// Event that is raised when an exception occures during a hook call.
        /// </summary>
        public event EventHandler<HookExceptionEventArgs> HookException;
        /// <summary>
        /// Event when lua hook callback is called
        /// </summary>
        /// <remarks>
        /// Is only raised if SetDebugHook is called before.
        /// </remarks>
        public event EventHandler<DebugHookEventArgs> DebugHook;
        /// <summary>
        /// lua hook calback delegate
        /// </summary>
        private LuaHookFunction _hookCallback;
        #endregion
        #region Globals auto-complete
        private readonly List<string> _globals = new List<string>();
        private bool _globalsSorted;
        #endregion
        private LuaState _luaState;
        /// <summary>
        /// True while a script is being executed
        /// </summary>
        public bool IsExecuting => _executing;

        /// <summary>
        /// Lua state object
        /// </summary>
        public LuaState State => _luaState;

        private ObjectTranslator _translator;

        internal ObjectTranslator Translator => _translator;

        /// <summary>
        /// Used to ensure multiple .net threads all get serialized by this single lock for access to the lua stack/objects
        /// </summary>
        //private object luaLock = new object();
        private bool _StatePassed;
        private bool _executing;

        // The commented code bellow is the initLua, the code assigned here is minified for size/performance reasons.
        private const string InitLuanet = @"local a={}local rawget=rawget;local b=luanet.import_type;local c=luanet.load_assembly;luanet.error,luanet.type=error,type;function a:__index(d)local e=rawget(self,'.fqn')e=(e and e..'.'or'')..d;local f=rawget(luanet,d)or b(e)if f==nil then pcall(c,e)f={['.fqn']=e}setmetatable(f,a)end;rawset(self,d,f)return f end;function a:__call(...)error('No such type: '..rawget(self,'.fqn'),2)end;luanet['.fqn']=false;setmetatable(luanet,a)luanet.load_assembly('mscorlib')";
 //@"local metatable = {}
 //       local rawget = rawget
 //       local import_type = luanet.import_type
 //       local load_assembly = luanet.load_assembly
 //       luanet.error, luanet.type = error, type
 //       -- Lookup a .NET identifier component.
 //       function metatable:__index(key) -- key is e.g. 'Form'
 //           -- Get the fully-qualified name, e.g. 'System.Windows.Forms.Form'
 //           local fqn = rawget(self,'.fqn')
 //           fqn = ((fqn and fqn .. '.') or '') .. key

 //           -- Try to find either a luanet function or a CLR type
 //           local obj = rawget(luanet,key) or import_type(fqn)

 //           -- If key is neither a luanet function or a CLR type, then it is simply
 //           -- an identifier component.
 //           if obj == nil then
 //               -- It might be an assembly, so we load it too.
 //               pcall(load_assembly,fqn)
 //               obj = { ['.fqn'] = fqn }
 //               setmetatable(obj, metatable)
 //           end

 //           -- Cache this lookup
 //           rawset(self, key, obj)
 //           return obj
 //       end

 //       -- A non-type has been called; e.g. foo = System.Foo()
 //       function metatable:__call(...)
 //           error('No such type: ' .. rawget(self,'.fqn'), 2)
 //       end

 //       -- This is the root of the .NET namespace
 //       luanet['.fqn'] = false
 //       setmetatable(luanet, metatable)

 //       -- Preload the mscorlib assembly
 //       luanet.load_assembly('mscorlib')";

 private const string ClrPackage = @"if not luanet then require'luanet'end;local a,b=luanet.import_type,luanet.load_assembly;local c={__index=function(d,e)local f=rawget(d,e)if f==nil then f=a(d.packageName.."".""..e)if f==nil then f=a(e)end;d[e]=f end;return f end}function luanet.namespace(g)if type(g)=='table'then local h={}for i=1,#g do h[i]=luanet.namespace(g[i])end;return unpack(h)end;local j={packageName=g}setmetatable(j,c)return j end;local k,l;local function m()l={}k={__index=function(n,e)for i,d in ipairs(l)do local f=d[e]if f then _G[e]=f;return f end end end}setmetatable(_G,k)end;function CLRPackage(o,p)p=p or o;local q=pcall(b,o)return luanet.namespace(p)end;function import(o,p)if not k then m()end;if not p then local i=o:find('%.dll$')if i then p=o:sub(1,i-1)else p=o end end;local j=CLRPackage(o,p)table.insert(l,j)return j end;function luanet.make_array(r,s)local t=r[#s]for i,u in ipairs(s)do t:SetValue(u,i-1)end;return t end;function luanet.each(v)local w=v:GetEnumerator()return function()if w:MoveNext()then return w.Current end end end";
//@"---
//--- This lua module provides auto importing of .net classes into a named package.
//--- Makes for super easy use of LuaInterface glue
//---
//--- example:
//---   Threading = CLRPackage(""System"", ""System.Threading"")
//---   Threading.Thread.Sleep(100)
//---
//--- Extensions:
//--- import() is a version of CLRPackage() which puts the package into a list which is used by a global __index lookup,
//--- and thus works rather like C#'s using statement. It also recognizes the case where one is importing a local
//--- assembly, which must end with an explicit .dll extension.

//--- Alternatively, luanet.namespace can be used for convenience without polluting the global namespace:
//---   local sys,sysi = luanet.namespace {'System','System.IO'}
//--    sys.Console.WriteLine(""we are at {0}"",sysi.Directory.GetCurrentDirectory())


//-- LuaInterface hosted with stock Lua interpreter will need to explicitly require this...
//if not luanet then require 'luanet' end

//local import_type, load_assembly = luanet.import_type, luanet.load_assembly

//local mt = {
//    --- Lookup a previously unfound class and add it to our table
//    __index = function(package, classname)
//        local class = rawget(package, classname)
//        if class == nil then
//            class = import_type(package.packageName .. ""."" .. classname)
//            if class == nil then class = import_type(classname) end
//            package[classname] = class		-- keep what we found around, so it will be shared
//        end
//        return class
//    end
//}

//function luanet.namespace(ns)
//    if type(ns) == 'table' then
//        local res = {}
//        for i = 1,#ns do
//            res[i] = luanet.namespace(ns[i])
//        end
//        return unpack(res)
//    end
//    -- FIXME - table.packageName could instead be a private index (see Lua 13.4.4)
//    local t = { packageName = ns }
//    setmetatable(t,mt)
//    return t
//end

//local globalMT, packages

//local function set_global_mt()
//    packages = {}
//    globalMT = {
//        __index = function(T,classname)
//                for i,package in ipairs(packages) do
//                    local class = package[classname]
//                    if class then
//                        _G[classname] = class
//                        return class
//                    end
//                end
//        end
//    }
//    setmetatable(_G, globalMT)
//end

//--- Create a new Package class
//function CLRPackage(assemblyName, packageName)
//  -- a sensible default...
//  packageName = packageName or assemblyName
//  local ok = pcall(load_assembly,assemblyName)			-- Make sure our assembly is loaded
//  return luanet.namespace(packageName)
//end

//function import (assemblyName, packageName)
//    if not globalMT then
//        set_global_mt()
//    end
//    if not packageName then
//        local i = assemblyName:find('%.dll$')
//        if i then packageName = assemblyName:sub(1,i-1)
//        else packageName = assemblyName end
//    end
//    local t = CLRPackage(assemblyName,packageName)
//    table.insert(packages,t)
//    return t
//end


//function luanet.make_array (tp,tbl)
//    local arr = tp[#tbl]
//    for i,v in ipairs(tbl) do
//        arr:SetValue(v,i-1)
//    end
//    return arr
//end

//function luanet.each(o)
//   local e = o:GetEnumerator()
//   return function()
//      if e:MoveNext() then
//        return e.Current
//     end
//   end
//end
//";

        /// <summary>
        /// Whether to push to the debug traceback.
        /// </summary>
        public bool UseTraceback { get; set; } = false;

        /// <summary>
        /// The maximum number of recursive steps to take when adding global reference variables.  Defaults to 2.
        /// </summary>
        public int MaximumRecursion { get; set; } = 2;

        #region Globals auto-complete
        /// <summary>
        /// An alphabetically sorted list of all globals (objects, methods, etc.) externally added to this Lua instance
        /// </summary>
        /// <remarks>Members of globals are also listed. The formatting is optimized for text input auto-completion.</remarks>
        public IEnumerable<string> Globals {
            get
            {
                // Only sort list when necessary
                if (!_globalsSorted)
                {
                    _globals.Sort();
                    _globalsSorted = true;
                }

                return _globals;
            }
        }
        #endregion

        /// <summary>
        /// Get the thread object of this state.
        /// </summary>
        public LuaThread Thread
        {
            get
            {
                int oldTop = _luaState.GetTop();
                _luaState.PushThread();
                object returnValue = _translator.GetObject(_luaState, -1);

                _luaState.SetTop(oldTop);
                return (LuaThread)returnValue;
            }
        }

        /// <summary>
        /// Get the main thread object
        /// </summary>
        public LuaThread MainThread
        {
            get
            {
                LuaState mainThread = _luaState.MainThread;
                int oldTop = mainThread.GetTop();
                mainThread.PushThread();
                object returnValue = _translator.GetObject(mainThread, -1);

                mainThread.SetTop(oldTop);
                return (LuaThread)returnValue;
            }
        }

        /// <summary>
        /// Create a new lua state
        /// </summary>
        public Lua(int maxRecursion = 2)
        {
            _luaState = new LuaState();
            Init(maxRecursion);
            // We need to keep this in a managed reference so the delegate doesn't get garbage collected
            _luaState.AtPanic(PanicCallback);
        }

        /// <summary>
        /// CAUTION: <see cref="NLua.Lua"/> instances can't share the same lua state! 
        /// </summary>
        public Lua(LuaState luaState, int maxRecursion = 2)
        {
            luaState.PushString("NLua_Loaded");
            luaState.GetTable((int)LuaRegistry.Index);

            if (luaState.ToBoolean(-1))
            {
                luaState.SetTop(-2);
                throw new LuaException("There is already a NLua.Lua instance associated with this Lua state");
            }

            _luaState = luaState;
            _StatePassed = true;
            luaState.SetTop(-2);
            Init(maxRecursion);
        }

        void Init(int maxRecursion)
        {
            _luaState.PushString("NLua_Loaded");
            _luaState.PushBoolean(true);
            _luaState.SetTable((int)LuaRegistry.Index);
            if (_StatePassed == false)
            {
                _luaState.NewTable();
                _luaState.SetGlobal("luanet");
            }
            _luaState.PushGlobalTable();
            _luaState.GetGlobal("luanet");
            _luaState.PushString("getmetatable");
            _luaState.GetGlobal("getmetatable");
            _luaState.SetTable(-3);
            _luaState.PopGlobalTable();
            _translator = new ObjectTranslator(this, _luaState);

            ObjectTranslatorPool.Instance.Add(_luaState, _translator);

            _luaState.PopGlobalTable();
            _luaState.DoString(InitLuanet);

            MaximumRecursion = maxRecursion;

            Base = new BasePackage(this);
            Math = new MathPackage(this);
            UTF8 = new UTF8Package(this);
            Table = new TablePackage(this);
            Debug = new DebugPackage(this);
            String = new StringPackage(this);
            Package = new PackagePackage(this);
            Coroutine = new CoroutinePackage(this);
            InputOutput = new InputOutputPackage(this);
            OperatingSystem = new OperatingSystemPackage(this);
        }

        public BasePackage Base { get; private set; }
        public MathPackage Math { get; private set; }
        public UTF8Package UTF8 { get; private set; }
        public TablePackage Table { get; private set; }
        public DebugPackage Debug { get; private set; }
        public StringPackage String { get; private set; }
        public PackagePackage Package { get; private set; }
        public CoroutinePackage Coroutine { get; private set; }
        public InputOutputPackage InputOutput { get; private set; }
        public OperatingSystemPackage OperatingSystem { get; private set; }

        /// <summary>
        /// Destroys all objects in the given Lua state (calling the corresponding garbage-collection metamethods, if any) and frees all dynamic memory used by this state
        /// </summary>
        public void Close()
        {
            if (_StatePassed || _luaState == null)
                return;

            _luaState.Close();
            ObjectTranslatorPool.Instance.Remove(_luaState);
            _luaState = null;
        }

#if __IOS__ || __TVOS__ || __WATCHOS__
        [MonoPInvokeCallback(typeof(LuaNativeFunction))]
#endif
        static int PanicCallback(IntPtr state)
        {
            var luaState = LuaState.FromIntPtr(state);
            string reason = string.Format("Unprotected error in call to Lua API ({0})", luaState.ToString(-1, false));
            throw new LuaException(reason);
        }

        /// <summary>
        /// Assuming we have a Lua error string sitting on the stack, throw a C# exception out to the user's app
        /// </summary>
        /// <exception cref = "LuaScriptException">Thrown if the script caused an exception</exception>
        private void ThrowExceptionFromError(int oldTop)
        {
            object err = _translator.GetObject(_luaState, -1);
            _luaState.SetTop(oldTop);

            // A pre-wrapped exception - just rethrow it (stack trace of InnerException will be preserved)
            var luaEx = err as LuaScriptException;

            if (luaEx != null)
                throw luaEx;

            // A non-wrapped Lua error (best interpreted as a string) - wrap it and throw it
            if (err == null)
                err = "Unknown Lua Error";

            throw new LuaScriptException(err.ToString(), string.Empty);
        }

        /// <summary>
        /// Push a debug.traceback reference onto the stack, for a pcall function to use as error handler. (Remember to increment any top-of-stack markers!)
        /// </summary>
        private static int PushDebugTraceback(LuaState luaState, int argCount)
        {
            luaState.GetGlobal("debug");
            luaState.GetField(-1, "traceback");
            luaState.Remove(-2);
            int errIndex = -argCount - 2;
            luaState.Insert(errIndex);
            return errIndex;
        }

        /// <summary>
        /// <para>Return a debug.traceback() call result (a multi-line string, containing a full stack trace, including C calls.</para>
        /// <para>Note: it won't return anything unless the interpreter is in the middle of execution - that is, it only makes sense to call it from a method called from Lua, or during a coroutine yield.</para>
        /// </summary>
        public string GetDebugTraceback()
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetGlobal("debug"); // stack: debug
            _luaState.GetField(-1, "traceback"); // stack: debug,traceback
            _luaState.Remove(-2); // stack: traceback
            _luaState.PCall(0, -1, 0);
            return _translator.PopValues(_luaState, oldTop)[0] as string;
        }

        /// <summary>
        /// Convert C# exceptions into Lua errors
        /// </summary>
        /// <returns>num of things on stack</returns>
        /// <param name = "e">null for no pending exception</param>
        internal int SetPendingException(Exception e)
        {
            var caughtExcept = e;

            if (caughtExcept == null)
                return 0;

            _translator.ThrowError(_luaState, caughtExcept);
            return 1;
        }

        /// <summary>
        /// Load a Lua chunk.
        /// </summary>
        /// <param name = "chunk">Chunk to load</param>
        /// <param name = "chunkName">Name to associate with the chunk. Defaults to "chunk".</param>
        /// <returns>a LuaFunction to execute the chunk loaded (useful to see if the syntax of a file is ok)</returns>
        public LuaFunction LoadString(string chunk, string chunkName = "chunk")
        {
            int oldTop = _luaState.GetTop();
            _executing = true;

            try
            {
                if (_luaState.LoadString(chunk, chunkName) != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);
            }
            finally
            {
                _executing = false;
            }

            var result = _translator.GetFunction(_luaState, -1);
            _translator.PopValues(_luaState, oldTop);
            return result;
        }

        /// <summary>
        /// Load a Lua chunk.
        /// </summary>
        /// <param name = "chunk">Chunk to load</param>
        /// <param name = "chunkName">Name to associate with the chunk. Defaults to "chunk".</param>
        /// <returns>a LuaFunction to execute the chunk loaded (useful to see if the syntax of a file is ok)</returns>
        public LuaFunction LoadString(byte[] chunk, string chunkName = "chunk")
        {
            int oldTop = _luaState.GetTop();
            _executing = true;

            try
            {
                if (_luaState.LoadBuffer(chunk, chunkName) != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);
            }
            finally
            {
                _executing = false;
            }

            var result = _translator.GetFunction(_luaState, -1);
            _translator.PopValues(_luaState, oldTop);
            return result;
        }

        /// <summary>
        /// Load a Lua file.
        /// </summary>
        /// <param name = "fileName">File to load</param>
        /// <returns>a LuaFunction to execute the file loaded (useful to see if the syntax of a file is ok)</returns>
        public LuaFunction LoadFile(string fileName)
        {
            int oldTop = _luaState.GetTop();

            if (_luaState.LoadFile(fileName) != LuaStatus.OK)
                ThrowExceptionFromError(oldTop);

            var result = _translator.GetFunction(_luaState, -1);
            _translator.PopValues(_luaState, oldTop);
            return result;
        }

        /// <summary>
        /// Executes a Lua chunk.
        /// </summary>
        /// <param name = "chunk">Chunk to execute</param>
        /// <param name = "chunkName">Name to associate with the chunk. Defaults to "chunk".</param>
        /// <returns>all the chunk's return values in an array</returns>
        public object[] DoString(byte[] chunk, string chunkName = "chunk")
        {
            int oldTop = _luaState.GetTop();
            _executing = true;

            if (_luaState.LoadBuffer(chunk, chunkName) != LuaStatus.OK)
                ThrowExceptionFromError(oldTop);

            int errorFunctionIndex = 0;

            if (UseTraceback)
            {
                errorFunctionIndex = PushDebugTraceback(_luaState, 0);
                oldTop++;
            }

            try
            {
                if (_luaState.PCall(0, -1, errorFunctionIndex) != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);

                return _translator.PopValues(_luaState, oldTop);
            }
            finally
            {
                _executing = false;
            }
        }

        /// <summary>
        /// Executes a Lua chunk.
        /// </summary>
        /// <param name = "chunk">Chunk to execute</param>
        /// <param name = "chunkName">Name to associate with the chunk. Defaults to "chunk".</param>
        /// <returns>all the chunk's return values in an array</returns>
        public object[] DoString(string chunk, string chunkName = "chunk")
        {
            int oldTop = _luaState.GetTop();
            _executing = true;

            if (_luaState.LoadString(chunk, chunkName) != LuaStatus.OK)
                ThrowExceptionFromError(oldTop);

            int errorFunctionIndex = 0;

            if (UseTraceback)
            {
                errorFunctionIndex = PushDebugTraceback(_luaState, 0);
                oldTop++;
            }

            try
            {
                if (_luaState.PCall(0, -1, errorFunctionIndex) != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);

                return _translator.PopValues(_luaState, oldTop);
            }
            finally
            {
                _executing = false;
            }
        }

        /// <summary>
        /// Executes a Lua file.
        /// </summary>
        /// <returns>all the chunk's return values in an array</returns>
        public object[] DoFile(string fileName)
        {
            int oldTop = _luaState.GetTop();

            if (_luaState.LoadFile(fileName) != LuaStatus.OK)
                ThrowExceptionFromError(oldTop);

            _executing = true;

            int errorFunctionIndex = 0;
            if (UseTraceback)
            {
                errorFunctionIndex = PushDebugTraceback(_luaState, 0);
                oldTop++;
            }

            try
            {
                if (_luaState.PCall(0, -1, errorFunctionIndex) != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);

                 return _translator.PopValues(_luaState, oldTop);
            }
            finally
            {
                _executing = false;
            }
        }

        /// <summary>
        /// Gets an object global variable
        /// </summary>
        public object GetObjectFromPath(string fullPath)
        {
            int oldTop = _luaState.GetTop();
            string[] path = FullPathToArray(fullPath);
            _luaState.GetGlobal(path[0]);
            object returnValue = _translator.GetObject(_luaState, -1);

            if (path.Length > 1)
            {
                var dispose = returnValue as LuaBase;
                string[] remainingPath = new string[path.Length - 1];
                Array.Copy(path, 1, remainingPath, 0, path.Length - 1);
                returnValue = GetObject(remainingPath);
                dispose?.Dispose();
            }

            _luaState.SetTop(oldTop);
            return returnValue;
        }

        /// <summary>
        /// Sets an object to the global variable
        /// </summary>
        public void SetObjectToPath(string fullPath, object value)
        {
            int oldTop = _luaState.GetTop();
            string[] path = FullPathToArray(fullPath);

            if (path.Length == 1)
            {
                _translator.Push(_luaState, value);
                _luaState.SetGlobal(fullPath);
            }
            else
            {
                _luaState.GetGlobal(path[0]);
                string[] remainingPath = new string[path.Length - 1];
                Array.Copy(path, 1, remainingPath, 0, path.Length - 1);
                SetObject(remainingPath, value);
            }

            _luaState.SetTop(oldTop);

            // Globals auto-complete
            if (value == null)
            {
                // Remove now obsolete entries
                _globals.Remove(fullPath);
            }
            else
            {
                // Add new entries
                if (!_globals.Contains(fullPath))
                    RegisterGlobal(fullPath, value.GetType(), 0);
            }
        }

        /// <summary>
        /// Indexer for global variables from the LuaInterpreter
        /// <br/>
        /// Supports navigation of tables by using . operator
        /// </summary>
        public object this[string fullPath] {
            get
            {
                // Silently convert Lua integer to double for backward compatibility with index[] operator
                object obj = GetObjectFromPath(fullPath);
                if (obj is long l)
                    return (double)l;
                return obj;
            }
            set
            {
               SetObjectToPath(fullPath, value);
            }
        }

        #region Globals auto-complete
        /// <summary>
        /// Adds an entry to <see cref = "_globals"/> (recursivley handles 2 levels of members)
        /// </summary>
        /// <param name = "path">The index accessor path ot the entry</param>
        /// <param name = "type">The type of the entry</param>
        /// <param name = "recursionCounter">How deep have we gone with recursion?</param>
        private void RegisterGlobal(string path, Type type, int recursionCounter)
        {
            // If the type is a global method, list it directly
            if (type == typeof(LuaFunction))
            {
                // Format for easy method invocation
                _globals.Add(path + "(");
            }
            // If the type is a class or an interface and recursion hasn't been running too long, list the members
            else if ((type.IsClass || type.IsInterface) && type != typeof(string) && recursionCounter < MaximumRecursion)
            {
                #region Methods
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    string name = method.Name;
                    if (
                        // Check that the LuaHideAttribute and LuaGlobalAttribute were not applied
                        (!method.GetCustomAttributes(typeof(LuaHideAttribute), false).Any()) &&
                        (!method.GetCustomAttributes(typeof(LuaGlobalAttribute), false).Any()) &&
                        // Exclude some generic .NET methods that wouldn't be very usefull in Lua
                        name != "GetType" && name != "GetHashCode" && name != "Equals" &&
                        name != "ToString" && name != "Clone" && name != "Dispose" &&
                        name != "GetEnumerator" && name != "CopyTo" &&
                        !name.StartsWith("get_", StringComparison.Ordinal) &&
                        !name.StartsWith("set_", StringComparison.Ordinal) &&
                        !name.StartsWith("add_", StringComparison.Ordinal) &&
                        !name.StartsWith("remove_", StringComparison.Ordinal))
                    {
                        // Format for easy method invocation
                        string command = path + ":" + name + "(";

                        if (method.GetParameters().Length == 0)
                            command += ")";
                        _globals.Add(command);
                    }
                }
                #endregion

                #region Fields
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (
                        // Check that the LuaHideAttribute and LuaGlobalAttribute were not applied
                        (!field.GetCustomAttributes(typeof(LuaHideAttribute), false).Any()) &&
                        (!field.GetCustomAttributes(typeof(LuaGlobalAttribute), false).Any()))
                    {
                        // Go into recursion for members
                        RegisterGlobal(path + "." + field.Name, field.FieldType, recursionCounter + 1);
                    }
                }
                #endregion

                #region Properties
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (
                        // Check that the LuaHideAttribute and LuaGlobalAttribute were not applied
                        (!property.GetCustomAttributes(typeof(LuaHideAttribute), false).Any()) &&
                        (!property.GetCustomAttributes(typeof(LuaGlobalAttribute), false).Any())
                        // Exclude some generic .NET properties that wouldn't be very useful in Lua
                        && property.Name != "Item")
                    {
                        // Go into recursion for members
                        RegisterGlobal(path + "." + property.Name, property.PropertyType, recursionCounter + 1);
                    }
                }
                #endregion
            }
            else
                _globals.Add(path); // Otherwise simply add the element to the list

            // List will need to be sorted on next access
            _globalsSorted = false;
        }
        #endregion

        /// <summary>
        /// Navigates a table in the top of the stack
        /// </summary>
        /// <returns>the value of the specified field</returns>
        object GetObject(string[] remainingPath)
        {
            object returnValue = null;

            for (int i = 0; i < remainingPath.Length; i++)
            {
                _luaState.PushString(remainingPath[i]);
                _luaState.GetTable(-2);
                returnValue = _translator.GetObject(_luaState, -1);

                if (returnValue == null)
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// Gets a numeric global variable
        /// </summary>
        public LuaNumber GetNumber(string fullPath) => new LuaNumber(GetObjectFromPath(fullPath));

        /// <summary>
        /// Gets a <see cref="float"/> global variable
        /// </summary>
        public float GetFloat(string fullPath) => (float)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="double"/> global variable
        /// </summary>
        public double GetDouble(string fullPath) => (double)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="int"/> global variable
        /// </summary>
        public int GetInteger(string fullPath) => (int)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="long"/> global variable
        /// </summary>
        public long GetLong(string fullPath) => (long)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="char"/> global variable
        /// </summary>
        public char GetChar(string fullPath) => (char)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="bool"/> global variable
        /// </summary>
        public bool GetBoolean(string fullPath) => (bool)GetNumber(fullPath);

        /// <summary>
        /// Gets a <see cref="string"/> global variable
        /// </summary>
        public string GetString(string fullPath)
        {
            object obj = GetObjectFromPath(fullPath);
            if (obj == null)
                return string.Empty;

            return obj.ToString();
        }

        /// <summary>
        /// Gets a table global variable
        /// </summary>
        public LuaTable GetTable(string fullPath)
        {
            return (LuaTable)GetObjectFromPath(fullPath);
        }

        /// <summary>
        /// Gets a table global variable as an object implementing
        /// the interfaceType interface
        /// </summary>
        public object GetTable(Type interfaceType, string fullPath)
        {
            return CodeGeneration.Instance.GetClassInstance(interfaceType, GetTable(fullPath));
        }

        /// <summary>
        /// Gets a userdata global variable
        /// </summary>
        public LuaUserData GetUserData(string fullPath)
        {
            return (LuaUserData)GetObjectFromPath(fullPath);
        }

        /// <summary>
        /// Gets a lightuserdata global variable
        /// </summary>
        public LuaLightUserData GetLightUserData(string fullPath)
        {
            return (LuaLightUserData)GetObjectFromPath(fullPath);
        }

        /// <summary>
        /// Gets a file global variable
        /// </summary>
        public LuaFile GetFile(string fullPath)
        {
            return (LuaFile)GetObjectFromPath(fullPath);
        }

        /// <summary>
        /// Gets a thread global variable
        /// </summary>
        public LuaThread GetThread(string fullPath)
        {
            return (LuaThread)GetObjectFromPath(fullPath);
        }

        /// <summary>
        /// Gets a function global variable
        /// </summary>
        public LuaFunction GetFunction(string fullPath)
        {
            object obj = GetObjectFromPath(fullPath);
            var luaFunction = obj as LuaFunction;
            if (luaFunction != null)
                return luaFunction;
            
            luaFunction = new LuaFunction((LuaNativeFunction) obj, this);
            return luaFunction;
        }

        /// <summary>
        /// Gets an object global variable
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public T GetObject<T>(string fullPath, T customDefault = default(T))
        {
            object obj = GetObjectFromPath(fullPath);
            if (obj == null)
                return customDefault;

            return (T)obj;
        }

        /// <summary>
        /// Register a delegate type to be used to convert Lua functions to C# delegates (useful for iOS where there is no dynamic code generation)
        /// type delegateType
        /// </summary>
        public void RegisterLuaDelegateType(Type delegateType, Type luaDelegateType)
        {
            CodeGeneration.Instance.RegisterLuaDelegateType(delegateType, luaDelegateType);
        }

        /// <summary>
        /// Register a class type
        /// </summary>
        public void RegisterLuaClassType(Type klass, Type luaClass)
        {
            CodeGeneration.Instance.RegisterLuaClassType(klass, luaClass);
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// To access any .NET assembly to create objects, events etc inside Lua you need to ask NLua to use CLR as a Lua package.
        /// <br/>
        /// To do this just use this method and use the import function inside your Lua script to load the Assembly.
        /// </summary>
        public CLRPackage LoadCLRPackage()
        {
            _luaState.DoString(ClrPackage);
            return new CLRPackage(this);
        }

        /// <summary>
        /// Gets a function global variable as a delegate of
        /// type delegateType
        /// </summary>
        public Delegate GetFunction(Type delegateType, string fullPath)
        {
            return CodeGeneration.Instance.GetDelegate(delegateType, GetFunction(fullPath));
        }

        /// <summary>
        /// Calls the object as a function with the provided arguments, 
        /// returning the function's returned values inside an array
        /// </summary>
        internal object[] CallFunction(object function, object[] args)
        {
            return CallFunction(function, args, null);
        }

        /// <summary>
        /// Calls the object as a function with the provided arguments and
        /// casting returned values to the types in returnTypes before returning
        /// them in an array
        /// </summary>
        internal object[] CallFunction(object function, object[] args, Type[] returnTypes)
        {
            int nArgs = 0;
            int oldTop = _luaState.GetTop();

            if (!_luaState.CheckStack(args.Length + 6))
                throw new LuaException("Lua stack overflow");

            _translator.Push(_luaState, function);

            if (args.Length > 0)
            {
                nArgs = args.Length;

                for (int i = 0; i < args.Length; i++)
                    _translator.Push(_luaState, args[i]);
            }

            _executing = true;

            try
            {
                int errfunction = 0;
                if (UseTraceback)
                {
                    errfunction = PushDebugTraceback(_luaState, nArgs);
                    oldTop++;
                }

                LuaStatus error = _luaState.PCall(nArgs, -1, errfunction);
                if (error != LuaStatus.OK)
                    ThrowExceptionFromError(oldTop);
            }
            finally
            {
                _executing = false;
            }

            if (returnTypes != null)
                return _translator.PopValues(_luaState, oldTop, returnTypes);

            return _translator.PopValues(_luaState, oldTop);
        }

        /// <summary>
        /// Navigates a table to set the value of one of its indexes
        /// </summary>
        void SetObject(string[] remainingPath, object value)
        {
            for (int i = 0; i < remainingPath.Length - 1; i++)
            {
                _luaState.PushString(remainingPath[i]);
                _luaState.GetTable(-2);
            }

            _luaState.PushString(remainingPath[remainingPath.Length - 1]);
            _translator.Push(_luaState, value);
            _luaState.SetTable(-3);
        }

        /// <summary>
        /// Navigates a table to rawset the value of one of its indexes
        /// </summary>
        void RawSetObject(string[] remainingPath, object value)
        {
            for (int i = 0; i < remainingPath.Length - 1; i++)
            {
                _luaState.PushString(remainingPath[i]);
                _luaState.RawGet(-2);
            }

            _luaState.PushString(remainingPath[remainingPath.Length - 1]);
            _translator.Push(_luaState, value);
            _luaState.RawSet(-3);
        }

        string[] FullPathToArray(string fullPath)
        {
            return fullPath.SplitWithEscape('.', '\\').ToArray();
        }

        ///// <summary>
        ///// Creates a new function
        ///// </summary>
        //public LuaFunction NewFunction(byte[] chunk, string chunkName = "chunk") => (LuaFunction)DoString(chunk.Prepend(Encoding.ASCII.GetBytes("return function()\n")).Append(Encoding.ASCII.GetBytes("\nend")).ToArray(), chunkName).FirstOrDefault();

        ///// <summary>
        ///// Creates a new function
        ///// </summary>
        //public LuaFunction NewFunction(string chunk, string chunkName = "chunk") => (LuaFunction)DoString($"return function()\n{chunk}\nend", chunkName).FirstOrDefault();

        /// <summary>
        /// Creates a new table
        /// </summary>
        public LuaTable NewTable()
        {
            int oldTop = _luaState.GetTop();

            _luaState.NewTable();
            LuaTable table = (LuaTable)_translator.GetObject(_luaState, -1);

            _luaState.SetTop(oldTop);
            return table;
        }

        /// <summary>
        /// Creates a new table as a global variable or as a field inside an existing table
        /// </summary>
        public void NewTable(string fullPath)
        {
            string[] path = FullPathToArray(fullPath);
            int oldTop = _luaState.GetTop();

            if (path.Length == 1)
            {
                _luaState.NewTable();
                _luaState.SetGlobal(fullPath);
            }
            else
            {
                _luaState.GetGlobal(path[0]);

                for (int i = 1; i < path.Length - 1; i++)
                {
                    _luaState.PushString(path[i]);
                    _luaState.GetTable(-2);
                }

                _luaState.PushString(path[path.Length - 1]);
                _luaState.NewTable();
                _luaState.SetTable(-3);
            }

            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Convert table to dictionary
        /// </summary>
        public Dictionary<object, object> GetTableDict(LuaTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var dict = new Dictionary<object, object>();
            int oldTop = _luaState.GetTop();
            _translator.Push(_luaState, table);
            _luaState.PushNil();

            while (_luaState.Next(-2))
            {
                dict[_translator.GetObject(_luaState, -2)] = _translator.GetObject(_luaState, -1);
                _luaState.SetTop(-2);
            }

            _luaState.SetTop(oldTop);
            return dict;
        }

        #region lua debug functions
        /// <summary>
        /// Activates the debug hook
        /// </summary>
        /// <param name = "mask">Mask</param>
        /// <param name = "count">Count</param>
        /// <returns>see lua docs. -1 if hook is already set</returns>
        public int SetDebugHook(LuaHookMask mask, int count)
        {
            if (_hookCallback == null)
            {
                _hookCallback = DebugHookCallback;
                _luaState.SetHook(_hookCallback, mask, count);
            }

            return -1;
        }

        /// <summary>
        /// Removes the debug hook
        /// </summary>
        /// <returns>see lua docs</returns>
        public void RemoveDebugHook()
        {
            _hookCallback = null;
            _luaState.SetHook(null, LuaHookMask.Disabled, 0);
        }

        /// <summary>
        /// Gets the hook mask.
        /// </summary>
        /// <returns>hook mask</returns>
        public LuaHookMask GetHookMask()
        {
            return _luaState.HookMask;
        }

        /// <summary>
        /// Gets the hook count
        /// </summary>
        /// <returns>see lua docs</returns>
        public int GetHookCount()
        {
            return _luaState.HookCount;
        }


        /// <summary>
        /// Gets local (see lua docs)
        /// </summary>
        /// <param name = "luaDebug">lua debug structure</param>
        /// <param name = "n">see lua docs</param>
        /// <returns>see lua docs</returns>
        public string GetLocal(LuaDebug luaDebug, int n)
        {
            return _luaState.GetLocal(luaDebug, n);
        }

        /// <summary>
        /// Sets local (see lua docs)
        /// </summary>
        /// <param name = "luaDebug">lua debug structure</param>
        /// <param name = "n">see lua docs</param>
        /// <returns>see lua docs</returns>
        public string SetLocal(LuaDebug luaDebug, int n)
        {
            return _luaState.SetLocal(luaDebug, n);
        }

        /// <summary>
        /// Gets information about the interpreter runtime stack.
        /// </summary>
        public int GetStack(int level, ref LuaDebug ar)
        {
            return _luaState.GetStack(level, ref ar);
        }

        /// <summary>
        /// Gets information about a specific function or function invocation.
        /// </summary>
        /// <returns>This function returns <see langword="false"/> on error (for instance, an invalid option in what).</returns>
        public bool GetInfo(string what, ref LuaDebug ar)
        {
            return _luaState.GetInfo(what, ref ar);
        }

        /// <summary>
        /// Gets up value (see lua docs)
        /// </summary>
        /// <param name = "funcindex">see lua docs</param>
        /// <param name = "n">see lua docs</param>
        /// <returns>see lua docs</returns>
        public string GetUpValue(int funcindex, int n)
        {
            return _luaState.GetUpValue(funcindex, n);
        }

        /// <summary>
        /// Sets up value (see lua docs)
        /// </summary>
        /// <param name = "funcindex">see lua docs</param>
        /// <param name = "n">see lua docs</param>
        /// <returns>see lua docs</returns>
        public string SetUpValue(int funcindex, int n)
        {
            return _luaState.SetUpValue(funcindex, n);
        }

        /// <summary>
        /// Delegate that is called on lua hook callback
        /// </summary>
        /// <param name = "luaState">lua state</param>
        /// <param name = "luaDebug">Pointer to LuaDebug (lua_debug) structure</param>
        /// 
#if __IOS__ || __TVOS__ || __WATCHOS__
        [MonoPInvokeCallback(typeof(LuaHookFunction))]
#endif
        static void DebugHookCallback(IntPtr luaState, IntPtr luaDebug)
        {
            var state = LuaState.FromIntPtr(luaState);

            state.GetStack(0, luaDebug);

            if (!state.GetInfo("Snlu", luaDebug))
                return;

            var debug = LuaDebug.FromIntPtr(luaDebug);

            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(state);
            Lua lua = translator.Interpreter;
            lua.DebugHookCallbackInternal(debug);
        }

        private void DebugHookCallbackInternal(LuaDebug luaDebug)
        {
            try
            {
                var temp = DebugHook;

                if (temp != null)
                    temp(this, new DebugHookEventArgs(luaDebug));
            }
            catch (Exception ex)
            {
                OnHookException(new HookExceptionEventArgs(ex));
            }
        }

        private void OnHookException(HookExceptionEventArgs e)
        {
            var temp = HookException;
            if (temp != null)
                temp(this, e);
        }

        /// <summary>
        /// Pops a value from the lua stack.
        /// </summary>
        /// <returns>Returns the top value from the lua stack.</returns>
        public object Pop()
        {
            int top = _luaState.GetTop();
            return _translator.PopValues(_luaState, top - 1)[0];
        }

        /// <summary>
        /// Pushes a value onto the lua stack.
        /// </summary>
        /// <param name = "value">Value to push.</param>
        public void Push(object value)
        {
            _translator.Push(_luaState, value);
        }
        #endregion

        /// <summary>
        /// Lets go of a previously allocated reference to a table, function or userdata
        /// </summary>
        internal void DisposeInternal(int reference, bool finalized)
        {
            if (finalized && _translator != null)
            {
                _translator.AddFinalizedReference(reference);
                return;
            }

            if (_luaState != null && !finalized)
                _luaState.Unref(reference);
        }

        internal int RawLen(object value)
        {
            int top = _luaState.GetTop();

            _translator.Push(_luaState, value);
            int len = _luaState.RawLen(-1);

            _luaState.SetTop(top);
            return len;
        }

        /// <summary>
        /// Gets a index of the table corresponding to the provided reference using rawget (do not use metatables)
        /// </summary>
        internal object RawGetObject(int reference, string index)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            _luaState.PushString(index);
            _luaState.RawGet(-2);
            object obj = _translator.GetObject(_luaState, -1);
            _luaState.SetTop(oldTop);
            return obj;
        }

        /// <summary>
        /// Gets a index of the table corresponding to the provided reference using rawget (do not use metatables)
        /// </summary>
        internal object RawGetObject(int reference, object index)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            _translator.Push(_luaState, index);
            _luaState.RawGet(-2);
            object obj = _translator.GetObject(_luaState, -1);
            _luaState.SetTop(oldTop);
            return obj;
        }

        /// <summary>
        /// Sets a index of the table corresponding to the provided reference using rawset (do not use metatables)
        /// </summary>
        internal void RawSetObject(int reference, string index, object value)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            RawSetObject(FullPathToArray(index), value);
            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Sets a numeric index of the table corresponding to the provided reference using rawset (do not use metatables)
        /// </summary>
        internal void RawSetObject(int reference, object index, object value)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            _translator.Push(_luaState, index);
            _translator.Push(_luaState, value);
            _luaState.RawSet(-3);
            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Gets a index of the table or userdata corresponding the the provided reference
        /// </summary>
        internal object GetObject(int reference, string index)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            object returnValue = GetObject(FullPathToArray(index));
            _luaState.SetTop(oldTop);
            return returnValue;
        }

        /// <summary>
        /// Gets a numeric index of the table or userdata corresponding the the provided reference
        /// </summary>
        internal object GetObject(int reference, object index)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            _translator.Push(_luaState, index);
            _luaState.GetTable(-2);
            object returnValue = _translator.GetObject(_luaState, -1);
            _luaState.SetTop(oldTop);
            return returnValue;
        }

        /// <summary>
        /// Sets a index of the table or userdata corresponding the the provided reference to the provided value
        /// </summary>
        internal void SetObject(int reference, string index, object value)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            SetObject(FullPathToArray(index), value);
            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Sets a numeric field of the table or userdata corresponding the the provided reference to the provided value
        /// </summary>
        internal void SetObject(int reference, object index, object value)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            _translator.Push(_luaState, index);
            _translator.Push(_luaState, value);
            _luaState.SetTable(-3);
            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Gets the luaState from the thread
        /// </summary>
        internal LuaState GetThreadState(int reference)
        {
            int oldTop = _luaState.GetTop();
            _luaState.GetRef(reference);
            LuaState state = _luaState.ToThread(-1);
            _luaState.SetTop(oldTop);
            return state;
        }

        /// <summary>
        /// Exchange values between different threads of the same state.
        /// </summary>
        public void XMove(LuaState to, object val, int index = 1)
        {
            int oldTop = _luaState.GetTop();

            _translator.Push(_luaState, val);
            _luaState.XMove(to, index);

            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Exchange values between different threads of the same state.
        /// </summary>
        public void XMove(Lua to, object val, int index = 1)
        {
            int oldTop = _luaState.GetTop();

            _translator.Push(_luaState, val);
            _luaState.XMove(to._luaState, index);

            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Exchange values between different threads of the same state.
        /// </summary>
        public void XMove(LuaThread thread, object val, int index = 1)
        {
            int oldTop = _luaState.GetTop();

            _translator.Push(_luaState, val);
            _luaState.XMove(thread.State, index);

            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Creates a new empty thread
        /// </summary>
        public LuaState NewThread(out LuaThread thread)
        {
            int oldTop = _luaState.GetTop();

            LuaState state = _luaState.NewThread();
            thread = (LuaThread)_translator.GetObject(_luaState, -1);

            _luaState.SetTop(oldTop);
            return state;
        }

        /// <summary>
        /// Creates a new empty thread as a global variable or as a field inside an existing table
        /// </summary>
        public LuaState NewThread(string fullPath)
        {
            string[] path = FullPathToArray(fullPath);
            int oldTop = _luaState.GetTop();

            LuaState state;

            if (path.Length == 1)
            {
                state = _luaState.NewThread();
                _luaState.SetGlobal(fullPath);
            }
            else
            {
                _luaState.GetGlobal(path[0]);

                for (int i = 1; i < path.Length - 1; i++)
                {
                    _luaState.PushString(path[i]);
                    _luaState.GetTable(-2);
                }

                _luaState.PushString(path[path.Length - 1]);
                state = _luaState.NewThread();
                _luaState.SetTable(-3);
            }

            _luaState.SetTop(oldTop);
            return state;
        }

        /// <summary>
        /// Creates a new coroutine thread
        /// </summary>
        public LuaState NewThread(LuaFunction function, out LuaThread thread)
        {
            int oldTop = _luaState.GetTop();

            LuaState state = _luaState.NewThread();
            thread = (LuaThread)_translator.GetObject(_luaState, -1);

            _translator.Push(_luaState, function);
            _luaState.XMove(state, 1);

            _luaState.SetTop(oldTop);
            return state;
        }

        /// <summary>
        /// Creates a new coroutine thread as a global variable or as a field inside an existing table
        /// </summary>
        public void NewThread(string fullPath, LuaFunction function)
        {
            string[] path = FullPathToArray(fullPath);
            int oldTop = _luaState.GetTop();

            LuaState state;

            if (path.Length == 1)
            {
                state = _luaState.NewThread();
                _luaState.SetGlobal(fullPath);
            }
            else
            {
                _luaState.GetGlobal(path[0]);

                for (int i = 1; i < path.Length - 1; i++)
                {
                    _luaState.PushString(path[i]);
                    _luaState.GetTable(-2);
                }

                _luaState.PushString(path[path.Length - 1]);
                state = _luaState.NewThread();
                _luaState.SetTable(-3);
            }

            _translator.Push(_luaState, function);
            _luaState.XMove(state, 1);

            _luaState.SetTop(oldTop);
        }

        /// <summary>
        /// Creates an empty userdata
        /// </summary>
        /// <param name="needsMetatable">Whether the usedata should have a metatable</param>
        /// <returns>the created userdata</returns>
        public LuaUserData NewProxy(bool needsMetatable = false)
        {
            int oldTop = _luaState.GetTop();

            _luaState.NewIndexedUserData(0, 0);
            LuaUserData userData = (LuaUserData)_translator.GetObject(_luaState, -1);

            if (needsMetatable)
            {
                _luaState.NewTable();
                _luaState.SetMetaTable(-2);
            }

            _luaState.SetTop(oldTop);
            return userData;
        }

        public LuaTable Pack(params object[] args) => (LuaTable)GetFunction("table.pack").Call(args).FirstOrDefault();

        public LuaFunction CreateFunction(string content) => ((LuaFunction)DoString($"return function(...) {content} end").FirstOrDefault());
        public LuaFunction CreateFunction(string argNames, string content) => ((LuaFunction)DoString($"return function({argNames}) {content} end").FirstOrDefault());

        internal LuaFunction RegisterBaseFunction(string name, LuaFunction f)
        {
            int oldTop = _luaState.GetTop();

            _translator.Push(_luaState, f);
            _luaState.SetGlobal(name);

            _luaState.SetTop(oldTop);
            return f;
        }
        internal LuaFunction RegisterBaseFunction(string global, string name, LuaFunction f)
        {
            int oldTop = _luaState.GetTop();

            _luaState.GetGlobal(global);
            _luaState.PushString(name);
            _translator.Push(_luaState, f);
            _luaState.SetTable(-3);

            _luaState.SetTop(oldTop);
            return f;
        }

        internal LuaFunction CreateBaseFunction(string name, string content)
        {
            int oldTop = _luaState.GetTop();

            LuaFunction f = CreateFunction(content);

            _translator.Push(_luaState, f);
            _luaState.SetGlobal(name);

            _luaState.SetTop(oldTop);
            return f;
        }
        internal LuaFunction CreateBaseFunctionWithArgs(string name, string argNames, string content)
        {
            int oldTop = _luaState.GetTop();
            LuaFunction f = CreateFunction(argNames, content);

            _translator.Push(_luaState, f);
            _luaState.SetGlobal(name);

            _luaState.SetTop(oldTop);
            return f;
        }
        internal LuaFunction CreateBaseFunction(string global, string name, string content)
        {
            int oldTop = _luaState.GetTop();
            LuaFunction f = CreateFunction(content);

            _luaState.GetGlobal(global);
            _luaState.PushString(name);
            _translator.Push(_luaState, f);
            _luaState.SetTable(-3);

            _luaState.SetTop(oldTop);
            return f;
        }
        internal LuaFunction CreateBaseFunctionWithArgs(string global, string name, string argNames, string content)
        {
            int oldTop = _luaState.GetTop();
            LuaFunction f = CreateFunction(argNames, content);

            _luaState.GetGlobal(global);
            _luaState.PushString(name);
            _translator.Push(_luaState, f);
            _luaState.SetTable(-3);

            _luaState.SetTop(oldTop);
            return f;
        }

        public object[] DoFunction(string function) => GetFunction(function).Call();
        public object[] DoFunction(string function, params object[] args) => GetFunction(function).Call(args);

        /// <summary>
        /// Registers a static method as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, MethodBase function)
        {
            return RegisterFunction(path, null, function);
        }

        /// <summary>
        /// Registers a object's method as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, object target, MethodBase function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, target, new ProxyType(function.DeclaringType), function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }

        /// <summary>
        /// Registers a base Lua function
        /// <br/>
        /// The method may have any signature
        /// </summary>
        internal LuaFunction RegisterBaseFunction(string name, MethodBase function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, null, new ProxyType(function.DeclaringType), function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);

            _translator.Push(_luaState, value);
            _luaState.SetGlobal(name);

            LuaFunction f = GetFunction(name);
            _luaState.SetTop(oldTop);
            return f;
        }

        /// <summary>
        /// Registers a base Lua function
        /// <br/>
        /// The method may have any signature
        /// </summary>
        internal LuaFunction RegisterBaseFunction(string global, string name, MethodBase function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, null, new ProxyType(function.DeclaringType), function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);

            _luaState.GetGlobal(global);
            _luaState.PushString(name);
            _translator.Push(_luaState, value);
            _luaState.SetTable(-3);

            LuaFunction f = GetFunction($"{global}.{name}");
            _luaState.SetTop(oldTop);
            return f;
        }

        /// <summary>
        /// Registers a static method as a Lua function
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(MethodBase function)
        {
            return RegisterFunction(target: null, function: function);
        }

        /// <summary>
        /// Registers a object's method as a Lua function
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(object target, MethodBase function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, target, new ProxyType(function.DeclaringType), function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            LuaFunction f = null;
            if (value is LuaFunction lf)
                f = lf;
            else if (value is LuaNativeFunction nf)
                f = new LuaFunction(nf, this);

            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }


        /// <summary>
        /// Registers an action as a Lua function (global or table field)
        /// <br/>
        /// The method may have any signature
        /// </summary>
        public LuaFunction RegisterFunction(string path, Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object> function)
        {
            // We leave nothing on the stack when we are done
            int oldTop = _luaState.GetTop();
            var wrapper = new LuaMethodWrapper(_translator, function);

            _translator.Push(_luaState, new LuaNativeFunction(wrapper.InvokeFunction));

            object value = _translator.GetObject(_luaState, -1);
            SetObjectToPath(path, value);

            LuaFunction f = GetFunction(path);
            _luaState.SetTop(oldTop);
            return f;
        }

        /// <summary>
        /// Compares the two values referenced by <paramref name="ref1"/> and <paramref name="ref2"/> for equality
        /// </summary>
        internal bool CompareRef(int ref1, int ref2)
        {
            int top = _luaState.GetTop();
            _luaState.GetRef(ref1);
            _luaState.GetRef(ref2);
            bool equal = _luaState.AreEqual(-1, -2);
            _luaState.SetTop(top);
            return equal;
        }

        // ReSharper disable once InconsistentNaming
        internal void PushCSFunction(LuaNativeFunction function)
        {
            _translator.PushFunction(_luaState, function);
        }

        /// <summary>
        /// Default input file
        /// </summary>
        public LuaFile StdIn => GetFile("io.stdin");

        /// <summary>
        /// Default output file
        /// </summary>
        public LuaFile StdOut => GetFile("io.stdout");

        /// <summary>
        /// Default error file
        /// </summary>
        public LuaFile StdErr => GetFile("io.stderr");

        /// <summary>
        /// Opens a new temporary file.
        /// </summary>
        /// <returns>a new handle for the temporary file</returns>
        public LuaFile TemporaryFile
        {
            get
            {
                int oldTop = _luaState.GetTop();

                LuaFunction tmpfile = InputOutput["tmpfile"] as LuaFunction;
                object[] result = tmpfile.Call();
                LuaFile file = (LuaFile)result.FirstOrDefault();
                if (file == null)
                {
                    string message = (string)result.MiddleOrDefault();
                    LuaNumber code = new LuaNumber(result.LastOrDefault());

                    _luaState.SetTop(oldTop);
                    throw new LuaFileException(message, (int)code);
                }

                _luaState.SetTop(oldTop);
                return file;
            }
        }

        /// <summary>
        /// Open a file.
        /// </summary>
        /// <param name = "fileName">File to open</param>
        /// <param name = "mode">Mode for file</param>
        /// <param name = "binary">Is file binary?</param>
        /// <returns>a new handle for the file</returns>
        public LuaFile OpenFile(string fileName, FileMode mode = FileMode.Read, bool binary = false)
        {
            int oldTop = _luaState.GetTop();

            LuaFunction open = InputOutput["open"] as LuaFunction;
            object[] result = open.Call(fileName, binary ? (mode == FileMode.Read ? "rb" : (mode == FileMode.Append ? "ab" : "wb")) : (mode == FileMode.Read ? "r" : (mode == FileMode.Append ? "a" : "w")));
            LuaFile file = (LuaFile)result.FirstOrDefault();
            if (file == null)
            {
                string message = (string)result.MiddleOrDefault();
                LuaNumber code = new LuaNumber(result.LastOrDefault());

                _luaState.SetTop(oldTop);
                throw new LuaFileException(message, (int)code);
            }

            _luaState.SetTop(oldTop);
            return file;
        }

        #region IDisposable Members

        ~Lua()
        {
            Dispose();
        }
        public virtual void Dispose()
        {
            DisposePackages();

            if (_translator != null)
            {
                _translator.PendingEvents.Dispose();
                if (_translator.Tag != IntPtr.Zero)
                    Marshal.FreeHGlobal(_translator.Tag);
                _translator = null;
            }

            Close();
            GC.SuppressFinalize(this);
        }
        public virtual void DisposePackages()
        {
            Base?.Dispose();
            Math?.Dispose();
            UTF8?.Dispose();
            Table?.Dispose();
            String?.Dispose();
            Debug?.Dispose();
            Package?.Dispose();
            Coroutine?.Dispose();
            InputOutput?.Dispose();
            OperatingSystem?.Dispose();

            Base = null;
            Math = null;
            UTF8 = null;
            Table = null;
            String = null;
            Debug = null;
            Package = null;
            Coroutine = null;
            InputOutput = null;
            OperatingSystem = null;
        }
        #endregion

        public class Lib : IDisposable
        {
            protected Lua _interpreter;
            protected LuaTable _table;
            protected virtual string Name => "";
            private string Path => !string.IsNullOrWhiteSpace(Name) ? (Name + ".") : string.Empty;

            /// <summary>
            /// Gets the lua interpreter if the state is not closed.
            /// </summary>
            /// <param name="interpreter">the got lua interpreter</param>
            /// <returns>successfulness of the get</returns>
            protected bool TryGet(out Lua interpreter)
            {
                if (_interpreter.State == null)
                {
                    interpreter = null;
                    return false;
                }

                interpreter = _interpreter;
                return true;
            }

            /// <summary>
            /// Indexer for the package
            /// </summary>
            public object this[string field] => _table[field];

            protected string GetString(string path) => _interpreter.GetString(Path + path);
            protected LuaNumber GetNumber(string path) => _interpreter.GetNumber(Path + path);
            protected LuaTable GetTable(string path) => _interpreter.GetTable(Path + path);
            protected LuaFunction GetFunction(string path) => _interpreter.GetFunction(Path + path);
            protected LuaFile GetFile(string path) => _interpreter.GetFile(Path + path);
            
            public override int GetHashCode() => _table.GetHashCode();
            public override bool Equals(object obj)
            {
                if (obj is Lib lib)
                    return Path == lib.Path;
                else if (obj is LuaTable table)
                    return _table.GetHashCode() == table.GetHashCode();

                return base.Equals(obj);
            }
            public override string ToString() => Name;

            public virtual void Dispose()
            {
                if (_table != null)
                    _table.Dispose();

                GC.SuppressFinalize(this);
            }

            protected Lib(Lua interpreter)
            {
                _interpreter = interpreter;
                if (!string.IsNullOrWhiteSpace(Name))
                    _table = interpreter.GetTable(Name);
            }
        }

        public class BasePackage : Lib
        {
            private LuaFunction _assert;
            private LuaFunction _collectgarbage;
            private LuaFunction _dofile;
            private LuaFunction _error;
            private LuaFunction _getmetatable;
            private LuaTable _global;
            private LuaFunction _load;
            private LuaFunction _loadfile;
            //private LuaFunction _loadstring;
            private LuaFunction _next;
            private LuaFunction _ipairs;
            private LuaFunction _pairs;
            private LuaFunction _pairsByKeys;
            private LuaFunction _pcall;
            private LuaFunction _print;
            private LuaFunction _rawequal;
            private LuaFunction _rawget;
            private LuaFunction _rawset;
            private LuaFunction _rawlen;
            private LuaFunction _require;
            private LuaFunction _select;
            private LuaFunction _setmetatable;
            private LuaFunction _tick;
            private LuaFunction _tonumber;
            private LuaFunction _tostring;
            private LuaFunction _type;
            private LuaFunction _xpcall;
            public LuaFunction Assert => _assert;
            public LuaFunction CollectGarbage => _collectgarbage;
            public LuaFunction DoFile => _dofile;
            public LuaFunction Error => _error;
            public LuaTable _G => _global;
            public LuaFunction GetMetaTable => _getmetatable;
            public LuaFunction Load => _load;
            public LuaFunction LoadFile => _loadfile;
            //public LuaFunction LoadString => _loadstring;
            public LuaFunction Next => _next;
            public LuaFunction IPairs => _ipairs;
            public LuaFunction Pairs => _pairs;
            public LuaFunction PairsByKeys => _pairsByKeys;
            public LuaFunction ProtectedCall => _pcall;
            public LuaFunction Print => _print;
            public LuaFunction RawEqual => _rawequal;
            public LuaFunction RawGet => _rawget;
            public LuaFunction RawSet => _rawset;
            public LuaFunction RawLen => _rawlen;
            public LuaFunction Require => _require;
            public LuaFunction Select => _select;
            public LuaFunction SetMetaTable => _setmetatable;
            public LuaFunction Tick => _tick;
            public LuaFunction ToNumber => _tonumber;
            public new LuaFunction ToString => _tostring;
            public LuaFunction Type => _type;
            public LuaFunction XProtectedCall => _xpcall;

            public override void Dispose()
            {
                _assert?.Dispose();
                _collectgarbage?.Dispose();
                _dofile?.Dispose();
                _error?.Dispose();
                _getmetatable?.Dispose();
                _global?.Dispose();
                _load?.Dispose();
                _loadfile?.Dispose();
                //_loadstring?.Dispose();
                _next?.Dispose();
                _ipairs?.Dispose();
                _pairs?.Dispose();
                _pairsByKeys?.Dispose();
                _pcall?.Dispose();
                _print?.Dispose();
                _rawequal?.Dispose();
                _rawget?.Dispose();
                _rawset?.Dispose();
                _rawlen?.Dispose();
                _require?.Dispose();
                _select?.Dispose();
                _setmetatable?.Dispose();
                _tick?.Dispose();
                _tonumber?.Dispose();
                _tostring?.Dispose();
                _type?.Dispose();
                _xpcall?.Dispose();
                base.Dispose();
            }

            internal BasePackage(Lua interpreter) : base(interpreter)
            {
                _assert = interpreter.GetFunction("assert");
                _collectgarbage = interpreter.GetFunction("collectgarbage");
                _dofile = interpreter.GetFunction("dofile");
                _error = interpreter.GetFunction("error");
                _global = interpreter.GetTable("_G");
                _getmetatable = interpreter.GetFunction("getmetatable");
                _setmetatable = interpreter.GetFunction("setmetatable");
                _load = interpreter.GetFunction("load");
                _loadfile = interpreter.GetFunction("loadfile");
                //_loadstring = interpreter.GetFunction("loadstring");
                _next = interpreter.GetFunction("next");
                _ipairs = interpreter.GetFunction("ipairs");
                _pairs = interpreter.GetFunction("pairs");
                _pairsByKeys = interpreter.CreateBaseFunctionWithArgs("pairsByKeys", "t, f", @"local a = {}
                  for n in pairs(t) do table.insert(a, n) end
                  table.sort(a, f)
                  local i = 0
                  local iter = function()
                    i = i + 1
                    if a[i] == nil then return nil
                    else return a[i], t[a[i]]
                    end
                  end
                  return iter");
                _pcall = interpreter.GetFunction("pcall");
                _print = interpreter.GetFunction("print");
                _rawequal = interpreter.GetFunction("rawequal");
                _rawget = interpreter.GetFunction("rawget");
                _rawset = interpreter.GetFunction("rawset");
                _rawlen = interpreter.GetFunction("rawlen");
                _require = interpreter.GetFunction("require");
                _select = interpreter.GetFunction("select");
                _tick = interpreter.RegisterBaseFunction("tick", interpreter.GetFunction("os.time"));//interpreter.CreateBaseFunction("tick", "\nreturn os.time(os.date(\"*t\"))\n");
                _tonumber = interpreter.GetFunction("tonumber");
                _tostring = interpreter.GetFunction("tostring");
                _type = interpreter.GetFunction("type");
                _xpcall = interpreter.GetFunction("xpcall");
            }
        }

        public class InputOutputPackage : Lib
        {
            protected override string Name => "io";

            private LuaFunction _close;
            private LuaFunction _flush;
            private LuaFunction _input;
            private LuaFunction _lines;
            private LuaFunction _open;
            private LuaFunction _output;
            private LuaFunction _popen;
            private LuaFunction _read;
            private LuaFunction _tmpfile;
            private LuaFunction _type;
            private LuaFunction _write;

            public LuaFile StdIn => this.GetFile("stdin");

            public LuaFile StdOut => this.GetFile("stdout");

            public LuaFile StdErr => this.GetFile("stderr");
            public LuaFunction Close => _close;
            public LuaFunction Flush => _flush;
            public LuaFunction Input => _input;
            public LuaFunction Lines => _lines;
            public LuaFunction Open => _open;
            public LuaFunction Output => _output;
            public LuaFunction ProgramOpen => _popen;
            public LuaFunction Read => _read;
            public LuaFunction TemporaryFile => _tmpfile;
            public LuaFunction Type => _type;
            public LuaFunction Write => _write;

            public override void Dispose()
            {
                _close?.Dispose();
                _flush?.Dispose();
                _input?.Dispose();
                _lines?.Dispose();
                _open?.Dispose();
                _output?.Dispose();
                _popen?.Dispose();
                _read?.Dispose();
                _tmpfile?.Dispose();
                _type?.Dispose();
                _write?.Dispose();
                base.Dispose();
            }

            internal InputOutputPackage(Lua interpreter) : base(interpreter)
            {
                _close = this.GetFunction("close");
                _flush = this.GetFunction("flush");
                _input = this.GetFunction("input");
                _lines = this.GetFunction("lines");
                _open = this.GetFunction("open");
                _output = this.GetFunction("output");
                _popen = this.GetFunction("popen");
                _read = this.GetFunction("read");
                _tmpfile = this.GetFunction("tmpfile");
                _type = this.GetFunction("type");
                _write = this.GetFunction("write");
            }
        }

        public class OperatingSystemPackage : Lib
        {
            protected override string Name => "os";

            private LuaFunction _clock;
            private LuaFunction _date;
            private LuaFunction _difftime;
            private LuaFunction _execute;
            private LuaFunction _exit;
            private LuaFunction _getenv;
            private LuaFunction _remove;
            private LuaFunction _rename;
            private LuaFunction _setlocale;
            private LuaFunction _time;
            private LuaFunction _tmpname;
            public LuaFunction Clock => _clock;
            public LuaFunction Date => _date;
            public LuaFunction DifferenceTime => _difftime;
            public LuaFunction Execute => _execute;
            public LuaFunction Exit => _exit;
            public LuaFunction GetEnvironment => _getenv;
            public LuaFunction Remove => _remove;
            public LuaFunction Rename => _rename;
            public LuaFunction SetLocale => _setlocale;
            public LuaFunction Time => _time;
            public LuaFunction TemporaryFileName => _tmpname;

            public override void Dispose()
            {
                _clock?.Dispose();
                _date?.Dispose();
                _difftime?.Dispose();
                _execute?.Dispose();
                _exit?.Dispose();
                _getenv?.Dispose();
                _remove?.Dispose();
                _rename?.Dispose();
                _setlocale?.Dispose();
                _time?.Dispose();
                _tmpname?.Dispose();
                base.Dispose();
            }

            internal OperatingSystemPackage(Lua interpreter) : base(interpreter)
            {
                _clock = this.GetFunction("clock");
                _date = this.GetFunction("date");
                _difftime = this.GetFunction("difftime");
                _execute = this.GetFunction("execute");
                _exit = this.GetFunction("exit");
                _getenv = this.GetFunction("getenv");
                _remove = this.GetFunction("remove");
                _rename = this.GetFunction("rename");
                _setlocale = this.GetFunction("setlocale");
                _time = this.GetFunction("time");
                _tmpname = this.GetFunction("tmpname");
            }
        }

        public class StringPackage : Lib
        {
            protected override string Name => "string";

            private LuaFunction _byte;
            private LuaFunction _char;
            private LuaFunction _dump;
            private LuaFunction _find;
            private LuaFunction _format;
            private LuaFunction _gmatch;
            private LuaFunction _gsub;
            private LuaFunction _len;
            private LuaFunction _lower;
            private LuaFunction _match;
            private LuaFunction _pack;
            private LuaFunction _packsize;
            private LuaFunction _rep;
            private LuaFunction _reverse;
            private LuaFunction _sub;
            private LuaFunction _unpack;
            private LuaFunction _upper;
            public LuaFunction Byte => _byte;
            public LuaFunction Char => _char;
            public LuaFunction Dump => _dump;
            public LuaFunction Find => _find;
            public LuaFunction Format => _format;
            public LuaFunction GMatch => _gmatch;
            public LuaFunction GSub => _gsub;
            public LuaFunction Len => _len;
            public LuaFunction Lower => _lower;
            public LuaFunction Match => _match;
            public LuaFunction Pack => _pack;
            public LuaFunction PackSize => _packsize;
            public LuaFunction Rep => _rep;
            public LuaFunction Reverse => _reverse;
            public LuaFunction Sub => _sub;
            public LuaFunction Unpack => _unpack;
            public LuaFunction Upper => _upper;

            public override void Dispose()
            {
                _byte?.Dispose();
                _char?.Dispose();
                _dump?.Dispose();
                _find?.Dispose();
                _format?.Dispose();
                _gmatch?.Dispose();
                _gsub?.Dispose();
                _len?.Dispose();
                _lower?.Dispose();
                _match?.Dispose();
                _pack?.Dispose();
                _packsize?.Dispose();
                _rep?.Dispose();
                _reverse?.Dispose();
                _sub?.Dispose();
                _unpack?.Dispose();
                _upper?.Dispose();
                base.Dispose();
            }

            internal StringPackage(Lua interpreter) : base(interpreter)
            {
                _byte = this.GetFunction("byte");
                _char = this.GetFunction("char");
                _dump = this.GetFunction("dump");
                _find = this.GetFunction("find");
                _format = this.GetFunction("format");
                _gmatch = this.GetFunction("gmatch");
                _gsub = this.GetFunction("gsub");
                _len = this.GetFunction("len");
                _lower = this.GetFunction("lower");
                _match = this.GetFunction("match");
                _pack = this.GetFunction("pack");
                _packsize = this.GetFunction("packsize");
                _rep = this.GetFunction("rep");
                _reverse = this.GetFunction("reverse");
                _sub = this.GetFunction("sub");
                _unpack = this.GetFunction("unpack");
                _upper = this.GetFunction("upper");
            }
        }

        public class MathPackage : Lib
        {
            protected override string Name => "math";


            private LuaFunction _abs;
            private LuaFunction _acos;
            private LuaFunction _asin;
            private LuaFunction _atan;
            private LuaFunction _ceil;
            private LuaFunction _cos;
            private LuaFunction _deg;
            private LuaFunction _exp;
            private LuaFunction _floor;
            private LuaFunction _fmod;
            private LuaFunction _log;
            private LuaFunction _max;
            private LuaFunction _min;
            private LuaFunction _modf;
            private LuaFunction _rad;
            private LuaFunction _random;
            private LuaFunction _randomseed;
            private LuaFunction _round;
            private LuaFunction _sin;
            private LuaFunction _sqrt;
            private LuaFunction _tan;
            private LuaFunction _tointeger;
            private LuaFunction _type;
            private LuaFunction _ult;
            public LuaFunction Abs => _abs;
            public LuaFunction ArcCosine => _acos;
            public LuaFunction ArcSine => _asin;
            public LuaFunction ArcTangent => _atan;
            public LuaFunction Ceiling => _ceil;
            public LuaFunction Cosine => _cos;
            public LuaFunction Degrees => _deg;
            public LuaFunction Exponential => _exp;
            public LuaFunction Floor => _floor;
            public LuaFunction FractionModulus => _fmod;
            public LuaNumber Huge => GetNumber("huge");
            public LuaFunction Logarithm => _log;
            public LuaFunction Max => _max;
            public LuaNumber MaxInteger => GetNumber("maxinteger");
            public LuaFunction Min => _min;
            public LuaNumber MinInteger => GetNumber("mininteger");
            public LuaFunction ModulusFraction => _modf;
            public LuaNumber Pi => GetNumber("pi");
            public LuaFunction Radians => _rad;
            public LuaFunction Random => _random;
            public LuaFunction RandomSeed => _randomseed;
            public LuaFunction Round => _round;
            public LuaFunction Sine => _sin;
            public LuaFunction SquareRoot => _sqrt;
            public LuaFunction Tangent => _tan;
            public LuaFunction ToInteger => _tointeger;
            public LuaFunction Type => _type;
            public LuaFunction UnsignedLessThan => _ult;

            public override void Dispose()
            {
                _abs?.Dispose();
                _acos?.Dispose();
                _asin?.Dispose();
                _atan?.Dispose();
                _ceil?.Dispose();
                _cos?.Dispose();
                _deg?.Dispose();
                _exp?.Dispose();
                _floor?.Dispose();
                _fmod?.Dispose();
                _log?.Dispose();
                _max?.Dispose();
                _min?.Dispose();
                _modf?.Dispose();
                _rad?.Dispose();
                _random?.Dispose();
                _randomseed?.Dispose();
                _round?.Dispose();
                _sin?.Dispose();
                _sqrt?.Dispose();
                _tan?.Dispose();
                _tointeger?.Dispose();
                _type?.Dispose();
                _ult?.Dispose();
                base.Dispose();
            }

            internal MathPackage(Lua interpreter) : base(interpreter)
            {
                _abs = this.GetFunction("abs");
                _acos = this.GetFunction("acos");
                _asin = this.GetFunction("asin");
                _atan = this.GetFunction("atan");
                _ceil = this.GetFunction("ceil");
                _cos = this.GetFunction("cos");
                _deg = this.GetFunction("deg");
                _exp = this.GetFunction("exp");
                _floor = this.GetFunction("floor");
                _fmod = this.GetFunction("fmod");
                _log = this.GetFunction("log");
                _max = this.GetFunction("max");
                _min = this.GetFunction("min");
                _modf = this.GetFunction("modf");
                _rad = this.GetFunction("rad");
                _random = this.GetFunction("random");
                _randomseed = this.GetFunction("randomseed");
                _round = interpreter.CreateBaseFunctionWithArgs(Name, "round", "num, precision", "\nlocal m = 10 ^ (precision or 0)\nlocal r = math.floor(num * m + 0.5) / m\nreturn math.tointeger(r) or r\n");
                _sin = this.GetFunction("sin");
                _sqrt = this.GetFunction("sqrt");
                _tan = this.GetFunction("tan");
                _tointeger = this.GetFunction("tointeger");
                _type = this.GetFunction("type");
                _ult = this.GetFunction("ult");
            }
        }

        public class TablePackage : Lib
        {
            protected override string Name => "table";

            private LuaFunction _clear;
            private LuaFunction _concat;
            private LuaFunction _foreach;
            private LuaFunction _foreachi;
            private LuaFunction _find;
            private LuaFunction _getn;
            private LuaFunction _insert;
            private LuaFunction _invert;
            private LuaFunction _move;
            private LuaFunction _pack;
            private LuaFunction _remove;
            private LuaFunction _sort;
            private LuaFunction _unpack;
            public LuaFunction Clear => _clear;
            public LuaFunction Concat => _concat;
            public LuaFunction Foreach => _foreach;
            public LuaFunction Foreachi => _foreachi;
            public LuaFunction Find => _find;
            public LuaFunction GetN => _getn;
            public LuaFunction Insert => _insert;
            public LuaFunction Invert => _invert;
            public LuaFunction Move => _move;
            public LuaFunction Pack => _pack;
            public LuaFunction Remove => _remove;
            public LuaFunction Sort => _sort;
            public LuaFunction Unpack => _unpack;

            public override void Dispose()
            {
                _clear?.Dispose();
                _concat?.Dispose();
                _foreach?.Dispose();
                _foreachi?.Dispose();
                _find?.Dispose();
                _getn?.Dispose();
                _insert?.Dispose();
                _invert?.Dispose();
                _move?.Dispose();
                _pack?.Dispose();
                _remove?.Dispose();
                _sort?.Dispose();
                _unpack?.Dispose();
                base.Dispose();
            }

            internal TablePackage(Lua interpreter) : base(interpreter)
            {
                _clear = interpreter.CreateBaseFunctionWithArgs(Name, "clear", "t", "\nfor k in next, t do\n\trawset(t, k, nil)\nend\n");
                _concat = this.GetFunction("concat");
                _foreach = interpreter.CreateBaseFunctionWithArgs(Name, "foreach", "t, f", "\nfor k, v in pairs(t) do\nf(k,v)\nend\n");
                _foreachi = interpreter.CreateBaseFunctionWithArgs(Name, "foreachi", "t, f", "\nfor k, v in ipairs(t) do\nf(k,v)\nend\n");
                _find = interpreter.RegisterBaseFunction(Name, "find", typeof(Luau).GetMethod("Find"));
                _getn = interpreter.RegisterBaseFunction(Name, "getn", typeof(Luau).GetMethod("GetN"));
                _insert = this.GetFunction("insert");
                _invert = interpreter.CreateBaseFunctionWithArgs(Name, "invert", "t", "\nlocal s={}\nfor k, v in pairs(t) do\ns[v] = k\nend\nreturn s\n");
                _move = this.GetFunction("move");
                _pack = this.GetFunction("pack");
                _remove = this.GetFunction("remove");
                _sort = this.GetFunction("sort");
                _unpack = this.GetFunction("unpack");
            }
        }

        public class DebugPackage : Lib
        {
            protected override string Name => "debug";

            private LuaFunction _debug;
            private LuaFunction _gethook;
            private LuaFunction _getinfo;
            private LuaFunction _getlocal;
            private LuaFunction _getmetatable;
            private LuaFunction _getregistry;
            private LuaFunction _getupvalue;
            private LuaFunction _getuservalue;
            private LuaFunction _setcstacklimit;
            private LuaFunction _sethook;
            private LuaFunction _setlocal;
            private LuaFunction _setmetatable;
            private LuaFunction _setupvalue;
            private LuaFunction _setuservalue;
            private LuaFunction _traceback;
            private LuaFunction _upvalueid;
            private LuaFunction _upvaluejoin;
            public LuaFunction Debug => _debug;
            public LuaFunction GetHook => _gethook;
            public LuaFunction GetInfo => _getinfo;
            public LuaFunction GetLocal => _getlocal;
            public LuaFunction GetMetaTable => _getmetatable;
            public LuaFunction GetRegistry => _getregistry;
            public LuaFunction GetUpValue => _getupvalue;
            public LuaFunction GetUserValue => _getuservalue;
            public LuaFunction SetCStackLimit => _setcstacklimit;
            public LuaFunction SetHook => _sethook;
            public LuaFunction SetLocal => _setlocal;
            public LuaFunction SetMetaTable => _setmetatable;
            public LuaFunction SetUpValue => _setupvalue;
            public LuaFunction SetUserValue => _setuservalue;
            public LuaFunction Traceback => _traceback;
            public LuaFunction UpValueId => _upvalueid;
            public LuaFunction UpValueJoin => _upvaluejoin;

            public override void Dispose()
            {
                _debug?.Dispose();
                _gethook?.Dispose();
                _getinfo?.Dispose();
                _getlocal?.Dispose();
                _getmetatable?.Dispose();
                _getregistry?.Dispose();
                _getupvalue?.Dispose();
                _getuservalue?.Dispose();
                _setcstacklimit?.Dispose();
                _sethook?.Dispose();
                _setlocal?.Dispose();
                _setmetatable?.Dispose();
                _setupvalue?.Dispose();
                _setuservalue?.Dispose();
                _traceback?.Dispose();
                _upvalueid?.Dispose();
                _upvaluejoin?.Dispose();
                base.Dispose();
            }

            internal DebugPackage(Lua interpreter) : base(interpreter)
            {
                _debug = this.GetFunction("debug");
                _gethook = this.GetFunction("gethook");
                _getinfo = this.GetFunction("getinfo");
                _getlocal = this.GetFunction("getlocal");
                _getmetatable = this.GetFunction("getmetatable");
                _getregistry = this.GetFunction("getregistry");
                _getupvalue = this.GetFunction("getupvalue");
                _getuservalue = this.GetFunction("getuservalue");
                _setcstacklimit = this.GetFunction("setcstacklimit");
                _sethook = this.GetFunction("sethook");
                _setlocal = this.GetFunction("setlocal");
                _setmetatable = this.GetFunction("setmetatable");
                _setupvalue = this.GetFunction("setupvalue");
                _setuservalue = this.GetFunction("setuservalue");
                _traceback = this.GetFunction("traceback");
                _upvalueid = this.GetFunction("upvalueid");
                _upvaluejoin = this.GetFunction("upvaluejoin");
            }
        }

        public class CoroutinePackage : Lib
        {
            protected override string Name => "coroutine";

            private LuaFunction _close;
            private LuaFunction _create;
            private LuaFunction _isyieldable;
            private LuaFunction _resume;
            private LuaFunction _running;
            private LuaFunction _status;
            private LuaFunction _wrap;
            private LuaFunction _yield;
            public LuaFunction Close => _close;
            public LuaFunction Create => _create;
            public LuaFunction IsYieldable => _isyieldable;
            public LuaFunction Resume => _resume;
            public LuaFunction Running => _running;
            public LuaFunction Status => _status;
            public LuaFunction Wrap => _wrap;
            public LuaFunction Yield => _yield;

            public override void Dispose()
            {
                _close?.Dispose();
                _create?.Dispose();
                _isyieldable?.Dispose();
                _resume?.Dispose();
                _running?.Dispose();
                _status?.Dispose();
                _wrap?.Dispose();
                _yield?.Dispose();
                base.Dispose();
            }

            internal CoroutinePackage(Lua interpreter) : base(interpreter)
            {
                _close = this.GetFunction("close");
                _create = this.GetFunction("create");
                _isyieldable = this.GetFunction("isyieldable");
                _resume = this.GetFunction("resume");
                _running = this.GetFunction("running");
                _status = this.GetFunction("status");
                _wrap = this.GetFunction("wrap");
                _yield = this.GetFunction("yield");
            }
        }

        public class UTF8Package : Lib
        {
            protected override string Name => "utf8";

            private LuaFunction _char;
            private LuaFunction _codes;
            private LuaFunction _codepoint;
            private LuaFunction _len;
            private LuaFunction _offset;
            public LuaFunction Char => _char;
            public string CharPattern => this.GetString("charpattern");
            public LuaFunction Codes => _codes;
            public LuaFunction CodePoint => _codepoint;
            public LuaFunction Len => _len;
            public LuaFunction Offset => _offset;

            public override void Dispose()
            {
                _char?.Dispose();
                _codes?.Dispose();
                _codepoint?.Dispose();
                _len?.Dispose();
                _offset?.Dispose();
                base.Dispose();
            }

            internal UTF8Package(Lua interpreter) : base(interpreter)
            {
                _char = this.GetFunction("char");
                _codes = this.GetFunction("codes");
                _codepoint = this.GetFunction("codepoint");
                _len = this.GetFunction("len");
                _offset = this.GetFunction("offset");
            }
        }

        public class PackagePackage : Lib
        {
            protected override string Name => "package";

            private LuaTable _loaded;
            private LuaTable _loaders;
            private LuaFunction _loadlib;
            private LuaTable _preload;
            private LuaTable _searchers;
            private LuaFunction _searchpath;
            private LuaFunction _seeall;
            public string Config => GetString("config");
            public string Path => GetString("path");
            public string CPath => GetString("cpath");
            public LuaTable Loaded => _loaded;
            public LuaTable Loaders => _loaders;
            public LuaFunction LoadLib => _loadlib;
            public LuaTable PreLoad => _preload;
            public LuaTable Searchers => _searchers;
            public LuaFunction SearchPath => _searchpath;
            public LuaFunction SeeAll => _seeall;

            public override void Dispose()
            {
                _loadlib?.Dispose();
                _searchpath?.Dispose();
                _seeall?.Dispose();
                _searchers?.Dispose();
                _preload?.Dispose();
                _loaded?.Dispose();
                _loaders?.Dispose();
                base.Dispose();
            }

            internal PackagePackage(Lua interpreter) : base(interpreter)
            {
                _loadlib = this.GetFunction("loadlib");
                _searchpath = this.GetFunction("searchpath");
                _seeall = this.GetFunction("seeall");
                _searchers = this.GetTable("searchers");
                _preload = this.GetTable("preload");
                _loaded = this.GetTable("loaded");
                _loaders = this.GetTable("loaders");
            }
        }

        public class CLRPackage : Lib
        {
            protected override string Name => "luanet";

            private LuaFunction _import;
            public LuaFunction Import => _import;

            public override void Dispose()
            {
                _import?.Dispose();
                base.Dispose();
            }

            internal CLRPackage(Lua interpreter) : base(interpreter)
            {
                _import = interpreter.GetFunction("import");
            }
        }
    }
}

using System;

namespace NLua.Exceptions
{
	/// <summary>
	/// Exceptions thrown by the Lua runtime
	/// </summary>
	[Serializable]
	public class LuaException : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
		public LuaException (string message) : base(message)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaException"/> class with a specified error  message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public LuaException (string message, Exception innerException) : base(message, innerException)
		{
		}

	}
}

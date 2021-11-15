using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua.Extensions;

namespace NLua
{
	/// <summary>
	/// The available formats for reading files.
	/// </summary>
	public enum FileFormat
	{
		/// <summary>
		/// <para>"*n"</para>
		/// reads a numeral and returns it as a <see cref="double"/> or a <see cref="long"/>, following the lexical conventions of Lua. (The numeral may have leading spaces and a sign.) This format always reads the longest input sequence that is a valid prefix for a numeral; if that prefix does not form a valid numeral (e.g., an empty string, "0x", or "3.4e-"), it is discarded and the function returns <see langword="null"/>.
		/// </summary>
		Numeral,
		/// <summary>
		/// <para>"*a"</para>
		/// reads the whole file, starting at the current position. On end of file, it returns the empty string.
		/// </summary>
		WholeFile,
		/// <summary>
		/// <para>"*l"</para>
		/// reads the next line skipping the end of line, returning <see langword="null"/> on end of file.<para>should be used only for text files.</para>
		/// </summary>
		LineSkipEndOfLineCharacter,
		/// <summary>
		/// <para>"*L"</para>
		/// reads the next line keeping the end-of-line character (if present), returning <see langword="null"/> on end of file.<para>should be used only for text files.</para>
		/// </summary>
		LineKeepEndOfLineCharacter
	}

	/// <summary>
	/// File type
	/// </summary>
	public enum FileType
	{
		/// <summary>
		/// Not a file
		/// </summary>
		None,
		/// <summary>
		/// Open file
		/// </summary>
		Open,
		/// <summary>
		/// Closed file
		/// </summary>
		Closed
	}

	/// <summary>
	/// File access mode
	/// </summary>
	public enum FileMode
	{
		/// <summary>
		/// File is readable
		/// </summary>
		Read,
		/// <summary>
		/// File is writable
		/// </summary>
		Write,
		/// <summary>
		/// File is appendable
		/// </summary>
		Append
	}

	/// <summary>
	/// Buffering mode of a file
	/// </summary>
	public enum BufferingMode
	{
		/// <summary>
		/// no buffering
		/// </summary>
		No,
		/// <summary>
		/// full buffering
		/// </summary>
		Full,
		/// <summary>
		/// line buffering
		/// </summary>
		Line
	}

	/// <summary>
	/// Type of seek
	/// </summary>
	public enum Whence
	{
		/// <summary>
		/// position 0 (beginning of the file)
		/// </summary>
		Set,
		/// <summary>
		/// current position
		/// </summary>
		Cur,
		/// <summary>
		/// end of file
		/// </summary>
		End
	}

	public class LuaFile : LuaUserData
	{
		private IntPtr _luaStream;

		public LuaFile(int reference, Lua interpreter, IntPtr lstream) : base(reference, interpreter)
		{
			_luaStream = lstream;
		}

		/// <summary>
		/// File type
		/// </summary>
		public FileType Type
		{
			get
			{
				Lua lua;
				if (!TryGet(out lua))
					return FileType.None;

				string type = (string)lua.GetFunction("io.type").Call(this).FirstOrDefault();

				if (type == "file")
					return FileType.Open;
				else if (type == "closed file")
					return FileType.Closed;

				return FileType.None;
			}
		}

		internal LuaFunction GetFunction(string name) => this[name] as LuaFunction;

		internal object[] CallFunction(string name, params object[] args) => GetFunction(name).Call(args.Prepend(this).ToArray());

		/// <summary>
		/// Writes the value of each of its arguments. The arguments must be strings or numbers.
		/// </summary>
		public void Write(params object[] args) => CallFunction("write", args.Select(obj => (object)(obj != null ? obj.ToString() : string.Empty)).ToArray());

		/// <summary>
		/// Writes the value of each of its arguments. The arguments must be strings or numbers.
		/// </summary>
		public void WriteLine(params object[] args) => CallFunction("write", args.Select(obj => (object)(obj != null ? obj.ToString() : string.Empty)).Append("\n").ToArray());

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void Write(string value) => CallFunction("write", value);

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void WriteLine(string value) => CallFunction("write", value, "\n");

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void Write(long value) => CallFunction("write", value);

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void WriteLine(long value) => CallFunction("write", value, "\n");

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void Write(double value) => CallFunction("write", value);

		/// <summary>
		/// Writes the <paramref name="value"/>.
		/// </summary>
		public void WriteLine(double value) => CallFunction("write", value, "\n");

		/// <summary>
		/// Reads the whole file from the beginning.
		/// </summary>
		public string ReadAllLines()
		{
			Seek(Whence.Set, 0);
			return (string)Read(FileFormat.WholeFile);
		}

		/// <summary>
		/// Reads the whole file as a number, starting from the beginning.
		/// </summary>
		public LuaNumber ReadAllLinesAsNumber()
		{
			Seek(Whence.Set, 0);
			return (LuaNumber)Read(FileFormat.Numeral);
		}

		/// <summary>
		/// Reads the whole file, starting from the beginning plus the <paramref name="offset"/>.
		/// </summary>
		public string ReadAllLines(int offset)
		{
			Seek(Whence.Set, offset);
			return (string)Read(FileFormat.WholeFile);
		}

		/// <summary>
		/// Reads the whole file as a number, starting from the beginning plus the <paramref name="offset"/>.
		/// </summary>
		public LuaNumber ReadAllLinesAsNumber(int offset)
		{
			Seek(Whence.Set, offset);
			return (LuaNumber)Read(FileFormat.Numeral);
		}

		/// <summary>
		/// Reads the next line, starting at the current file position, returning <see langword="null"/> on end of file.
		/// </summary>
		/// <param name="keepEndOfLineCharacter">Whether to keep the end-of-line character (if present), or skip it.</param>
		public string ReadLine(bool keepEndOfLineCharacter = false)
		{
			if (keepEndOfLineCharacter)
				return (string)Read(FileFormat.LineKeepEndOfLineCharacter);
			return (string)Read(FileFormat.LineSkipEndOfLineCharacter);
		}

		/// <summary>
		/// Reads a string with up to this number of bytes, starting at the current file position, returning <see langword="null"/> on end of file. If number is zero, it reads nothing and returns an empty string, or nil on end of file.
		/// </summary>
		public string Read(long size) => (string)CallFunction("read", size).FirstOrDefault();

		/// <summary>
		/// Reads the file, according to the given format, which specify what to read, starting at the current file position.
		/// </summary>
		public object Read(FileFormat format = FileFormat.LineSkipEndOfLineCharacter)
		{
			string sformat;
			switch (format)
			{
				case FileFormat.Numeral:
					sformat = "*n";
					break;
				case FileFormat.WholeFile:
					sformat = "*a";
					break;
				case FileFormat.LineKeepEndOfLineCharacter:
					sformat = "*L";
					break;
				case FileFormat.LineSkipEndOfLineCharacter:
				default:
					sformat = "*l";
					break;
			}
			return Read(sformat);
		}

		/// <summary>
		/// Reads the file, according to the given format, which specify what to read, starting at the current file position.
		/// </summary>
		public object Read(string format) => CallFunction("read", format).FirstOrDefault();

		/// <summary>
		/// Gets the current file position.
		/// </summary>
		/// <returns>the final file position, measured in bytes from the beginning of the file.</returns>
		public long Seek() => new LuaNumber(CallFunction("seek").FirstOrDefault());

		/// <summary>
		/// Sets and gets the file position, measured from the beginning of the file, to the position of the <paramref name="whence"/>.
		/// </summary>
		/// <returns>the final file position, measured in bytes from the beginning of the file.</returns>
		public long Seek(Whence whence) => new LuaNumber(CallFunction("seek", whence.ToString().ToLowerInvariant()).FirstOrDefault());

		/// <summary>
		/// Sets and gets the file position, measured from the beginning of the file, to the position of the <paramref name="whence"/> with an <paramref name="offset"/>.
		/// </summary>
		/// <returns>the final file position, measured in bytes from the beginning of the file.</returns>
		public long Seek(Whence whence, int offset) => new LuaNumber(CallFunction("seek", whence.ToString().ToLowerInvariant(), offset).FirstOrDefault());

		///// <summary>
		///// Sets and gets the file position, measured from the beginning of the file, to the position given by <paramref name="offset"/>.
		///// </summary>
		///// <returns>the final file position, measured in bytes from the beginning of the file.</returns>
		//public long Seek(Whence whence = Whence.Cur, int offset = 0) => new LuaNumber(CallFunction("seek", whence.ToString().ToLowerInvariant(), offset).FirstOrDefault());

		/// <summary>
		/// Closes file
		/// </summary>
		public void Close() => CallFunction("close");

		/// <summary>
		/// Saves any written data
		/// </summary>
		public void Flush() => CallFunction("flush");

		/// <summary>
		/// Sets the buffering mode for a file.
		/// <br/><br/>
		/// The specific behavior of each mode is non portable; check the underlying ISO C function <b>setvbuf</b> in your platform for more details.
		/// </summary>
		/// <param name="mode">buffering mode</param>
		public void Setvbuf(BufferingMode mode) => CallFunction("setvbuf", mode.ToString().ToLowerInvariant());

		/// <summary>
		/// Sets the buffering mode for a file.
		/// <br/><br/>
		/// The specific behavior of each mode is non portable; check the underlying ISO C function <b>setvbuf</b> in your platform for more details.
		/// </summary>
		/// <param name="mode">buffering mode</param>
		/// <param name="size">a hint for the size of the buffer, in bytes.</param>
		public void Setvbuf(BufferingMode mode, int size) => CallFunction("setvbuf", mode.ToString().ToLowerInvariant(), size);

		/// <returns>an iterator function that, each time it is called, reads the file.</returns>
		public LuaFunction Lines() => CallFunction("lines").FirstOrDefault() as LuaFunction;

		/// <returns>an iterator function that, each time it is called, reads the file according to the given <paramref name="format"/>.</returns>
		public LuaFunction Lines(string format) => CallFunction("lines", format).FirstOrDefault() as LuaFunction;

		/// <summary>
		/// Sets this file as the default input file
		/// </summary>
		public void Input()
		{
			Lua lua;
			if (!TryGet(out lua))
				return;

			lua.GetFunction("io.input").Call(this);
		}

		/// <summary>
		/// Sets this file as the default output file
		/// </summary>
		public void Output()
		{
			Lua lua;
			if (!TryGet(out lua))
				return;

			lua.GetFunction("io.output").Call(this);
		}

		public override string ToString()
		{
			string bstring = base.ToString();
			return bstring == "userdata" ? "file" : bstring;
		}
	}

}

namespace NLua.Exceptions
{
	[Serializable]
	public class LuaFileException : LuaException
	{

		internal int _code;
		public int Code => _code;


		/// <summary>
		/// Initializes a new instance of the <see cref="LuaFileException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public LuaFileException(string message) : base(message)
		{
			_code = -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaFileException"/> class with a specified error message and code.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="code">The code for the error.</param>
		public LuaFileException(string message, int code) : base(message)
		{
			_code = code;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaFileException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public LuaFileException(string message, Exception innerException) : base(message, innerException)
		{
			_code = -1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaFileException"/> class with a specified error message, error code, and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="code">The code for the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public LuaFileException(string message, int code, Exception innerException) : base(message, innerException)
		{
			_code = code;
		}
	}
}

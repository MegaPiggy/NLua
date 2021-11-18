using System;

namespace NLua
{
    /// <summary>
    /// Type of number
    /// </summary>
    public enum NumericalType
    {
        /// <summary>
        /// <see cref="long"/>
        /// </summary>
        Integer,
        /// <summary>
        /// <see cref="double"/>
        /// </summary>
        Float,
        /// <summary>
        /// <see langword="null"/>
        /// </summary>
        Fail
    }

    /// <summary>
    /// A lua number
    /// </summary>
    public class LuaNumber : IComparable, IComparable<LuaNumber>, IConvertible, IEquatable<LuaNumber>, IComparable<double>, IEquatable<double>, IComparable<float>, IEquatable<float>, IComparable<decimal>, IEquatable<decimal>, IComparable<long>, IEquatable<long>, IComparable<int>, IEquatable<int>, IComparable<short>, IEquatable<short>, IComparable<byte>, IEquatable<byte>, IComparable<ulong>, IEquatable<ulong>, IComparable<uint>, IEquatable<uint>, IComparable<ushort>, IEquatable<ushort>, IComparable<sbyte>, IEquatable<sbyte>, IComparable<char>, IEquatable<char>, IComparable<bool>, IEquatable<bool>
    {
        private object number;
        public object InternalValue
        {
            get
            {
                if (number is string str)
                {
                    if (double.TryParse(str, out double result))
                        return result;
                }
                else if (number is bool torf)
                    return torf ? 1 : 0;
                return number;
            }
        }

        public LuaNumber(object num) => number = num;

        public override bool Equals(object obj)
        {
            if (obj is LuaNumber ln)
                return this == ln;
            return ((double)this).Equals(obj);
        }

        public override int GetHashCode() => number.GetHashCode();

        public override string ToString() => Convert.ToString(InternalValue);

        public Type GetInternalType() => number.GetType();

        public NumericalType NumericalType
        {
            get
            {
                if (number == null)
                    return NumericalType.Fail;
                return IsInteger() ? NumericalType.Integer : NumericalType.Float;
            }
        }

        public bool IsInteger()
        {
            if (this.number is double dpfpn)
                return (dpfpn % 1) == 0;
            else if (this.number is float single)
                return (single % 1) == 0;
            else if (this.number is decimal dec)
                return (dec % 1) == 0;
            return true;
        }

        public TypeCode GetTypeCode()
        {
            if (this.number is double)
                return TypeCode.Double;
            else if (this.number is long)
                return TypeCode.Int64;
            else if (this.number is int)
                return TypeCode.Int32;
            else if (this.number is float)
                return TypeCode.Single;
            else if (this.number is short)
                return TypeCode.Int16;
            else if (this.number is decimal)
                return TypeCode.Decimal;
            else if (this.number is uint)
                return TypeCode.UInt32;
            else if (this.number is ulong)
                return TypeCode.UInt64;
            else if (this.number is ushort)
                return TypeCode.UInt16;
            else if (this.number is byte)
                return TypeCode.Byte;
            else if (this.number is sbyte)
                return TypeCode.SByte;
            else if (this.number is bool)
                return TypeCode.Boolean;
            else if (this.number is string)
                return TypeCode.String;
            else if (this.number is DateTime)
                return TypeCode.DateTime;
            else if (this.number != null)
                return TypeCode.Object;
            return TypeCode.Empty;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToBoolean(dpfpn);
            else if (this.number is long int64)
                return Convert.ToBoolean(int64);
            else if (this.number is int int32)
                return Convert.ToBoolean(int32);
            else if (this.number is float single)
                return Convert.ToBoolean(single);
            else if (this.number is short int16)
                return Convert.ToBoolean(int16);
            else if (this.number is decimal dec)
                return Convert.ToBoolean(dec);
            else if (this.number is uint uint32)
                return Convert.ToBoolean(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToBoolean(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToBoolean(uint16);
            else if (this.number is byte b)
                return Convert.ToBoolean(b);
            else if (this.number is sbyte sb)
                return Convert.ToBoolean(sb);
            else if (this.number is char c)
                return c != 0;
            else if (this.number is bool torf)
                return torf;
            else if (this.number is string str)
            {
                if (bool.TryParse(str, out bool result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToBoolean(dt.Ticks);
            return false;
        }

        public char ToChar(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return (char)dpfpn;
            else if (this.number is long int64)
                return Convert.ToChar(int64);
            else if (this.number is int int32)
                return Convert.ToChar(int32);
            else if (this.number is float single)
                return (char)single;
            else if (this.number is short int16)
                return Convert.ToChar(int16);
            else if (this.number is decimal dec)
                return (char)dec;
            else if (this.number is uint uint32)
                return Convert.ToChar(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToChar(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToChar(uint16);
            else if (this.number is byte b)
                return Convert.ToChar(b);
            else if (this.number is sbyte sb)
                return Convert.ToChar(sb);
            else if (this.number is char c)
                return c;
            else if (this.number is bool torf)
                return torf ? (char)10003 : (char)10060;
            else if (this.number is string str)
                return Convert.ToChar(str);
            else if (this.number is DateTime dt)
                return Convert.ToChar(dt.Ticks);
            return char.MinValue;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToSByte(dpfpn);
            else if (this.number is long int64)
                return Convert.ToSByte(int64);
            else if (this.number is int int32)
                return Convert.ToSByte(int32);
            else if (this.number is float single)
                return Convert.ToSByte(single);
            else if (this.number is short int16)
                return Convert.ToSByte(int16);
            else if (this.number is decimal dec)
                return decimal.ToSByte(dec);
            else if (this.number is uint uint32)
                return Convert.ToSByte(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToSByte(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToSByte(uint16);
            else if (this.number is byte b)
                return Convert.ToSByte(b);
            else if (this.number is sbyte sb)
                return sb;
            else if (this.number is char c)
                return Convert.ToSByte(c);
            else if (this.number is bool torf)
                return torf ? (sbyte)1 : (sbyte)0;
            else if (this.number is string str)
            {
                if (sbyte.TryParse(str, out sbyte result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToSByte(dt.Ticks);
            return (sbyte)0;
        }

        public byte ToByte(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToByte(dpfpn);
            else if (this.number is long int64)
                return Convert.ToByte(int64);
            else if (this.number is int int32)
                return Convert.ToByte(int32);
            else if (this.number is float single)
                return Convert.ToByte(single);
            else if (this.number is short int16)
                return Convert.ToByte(int16);
            else if (this.number is decimal dec)
                return decimal.ToByte(dec);
            else if (this.number is uint uint32)
                return Convert.ToByte(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToByte(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToByte(uint16);
            else if (this.number is byte b)
                return b;
            else if (this.number is sbyte sb)
                return Convert.ToByte(sb);
            else if (this.number is char c)
                return Convert.ToByte(c);
            else if (this.number is bool torf)
                return torf ? (byte)1 : (byte)0;
            else if (this.number is string str)
            {
                if (byte.TryParse(str, out byte result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToByte(dt.Ticks);
            return (byte)0;
        }

        public short ToInt16(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToInt16(dpfpn);
            else if (this.number is long int64)
                return Convert.ToInt16(int64);
            else if (this.number is int int32)
                return Convert.ToInt16(int32);
            else if (this.number is float single)
                return Convert.ToInt16(single);
            else if (this.number is short int16)
                return int16;
            else if (this.number is decimal dec)
                return decimal.ToInt16(dec);
            else if (this.number is uint uint32)
                return Convert.ToInt16(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToInt16(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToInt16(uint16);
            else if (this.number is byte b)
                return Convert.ToInt16(b);
            else if (this.number is sbyte sb)
                return Convert.ToInt16(sb);
            else if (this.number is char c)
                return Convert.ToInt16(c);
            else if (this.number is bool torf)
                return torf ? (short)1 : (short)0;
            else if (this.number is string str)
            {
                if (short.TryParse(str, out short result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToInt16(dt.Ticks);
            return (short)0;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToUInt16(dpfpn);
            else if (this.number is long int64)
                return Convert.ToUInt16(int64);
            else if (this.number is int int32)
                return Convert.ToUInt16(int32);
            else if (this.number is float single)
                return Convert.ToUInt16(single);
            else if (this.number is short int16)
                return Convert.ToUInt16(int16);
            else if (this.number is decimal dec)
                return decimal.ToUInt16(dec);
            else if (this.number is uint uint32)
                return Convert.ToUInt16(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToUInt16(uint64);
            else if (this.number is ushort uint16)
                return uint16;
            else if (this.number is byte b)
                return Convert.ToUInt16(b);
            else if (this.number is sbyte sb)
                return Convert.ToUInt16(sb);
            else if (this.number is char c)
                return Convert.ToUInt16(c);
            else if (this.number is bool torf)
                return torf ? (ushort)1U : (ushort)0U;
            else if (this.number is string str)
            {
                if (ushort.TryParse(str, out ushort result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToUInt16(dt.Ticks);
            return (ushort)0U;
        }

        public int ToInt32(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToInt32(dpfpn);
            else if (this.number is long int64)
                return Convert.ToInt32(int64);
            else if (this.number is int int32)
                return int32;
            else if (this.number is float single)
                return Convert.ToInt32(single);
            else if (this.number is short int16)
                return Convert.ToInt32(int16);
            else if (this.number is decimal dec)
                return decimal.ToInt32(dec);
            else if (this.number is uint uint32)
                return Convert.ToInt32(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToInt32(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToInt32(uint16);
            else if (this.number is byte b)
                return Convert.ToInt32(b);
            else if (this.number is sbyte sb)
                return Convert.ToInt32(sb);
            else if (this.number is char c)
                return Convert.ToInt32(c);
            else if (this.number is bool torf)
                return torf ? 1 : 0;
            else if (this.number is string str)
            {
                if (int.TryParse(str, out int result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToInt32(dt.Ticks);
            return 0;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToUInt32(dpfpn);
            else if (this.number is long int64)
                return Convert.ToUInt32(int64);
            else if (this.number is int int32)
                return Convert.ToUInt32(int32);
            else if (this.number is float single)
                return Convert.ToUInt32(single);
            else if (this.number is short int16)
                return Convert.ToUInt32(int16);
            else if (this.number is decimal dec)
                return decimal.ToUInt32(dec);
            else if (this.number is uint uint32)
                return uint32;
            else if (this.number is ulong uint64)
                return Convert.ToUInt32(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToUInt32(uint16);
            else if (this.number is byte b)
                return Convert.ToUInt32(b);
            else if (this.number is sbyte sb)
                return Convert.ToUInt32(sb);
            else if (this.number is char c)
                return Convert.ToUInt32(c);
            else if (this.number is bool torf)
                return torf ? 1U : 0U;
            else if (this.number is string str)
            {
                if (uint.TryParse(str, out uint result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToUInt32(dt.Ticks);
            return 0U;
        }

        public long ToInt64(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToInt64(dpfpn);
            else if (this.number is long int64)
                return int64;
            else if (this.number is int int32)
                return Convert.ToInt64(int32);
            else if (this.number is float single)
                return Convert.ToInt64(single);
            else if (this.number is short int16)
                return Convert.ToInt64(int16);
            else if (this.number is decimal dec)
                return decimal.ToInt64(dec);
            else if (this.number is uint uint32)
                return Convert.ToInt64(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToInt64(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToInt64(uint16);
            else if (this.number is byte b)
                return Convert.ToInt64(b);
            else if (this.number is sbyte sb)
                return Convert.ToInt64(sb);
            else if (this.number is char c)
                return Convert.ToInt64(c);
            else if (this.number is bool torf)
                return torf ? 1L : 0L;
            else if (this.number is string str)
            {
                if (long.TryParse(str, out long result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return dt.Ticks;
            return 0L;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToUInt64(dpfpn);
            else if (this.number is long int64)
                return Convert.ToUInt64(int64);
            else if (this.number is int int32)
                return Convert.ToUInt64(int32);
            else if (this.number is float single)
                return Convert.ToUInt64(single);
            else if (this.number is short int16)
                return Convert.ToUInt64(int16);
            else if (this.number is decimal dec)
                return decimal.ToUInt64(dec);
            else if (this.number is uint uint32)
                return Convert.ToUInt64(uint32);
            else if (this.number is ulong uint64)
                return uint64;
            else if (this.number is ushort uint16)
                return Convert.ToUInt64(uint16);
            else if (this.number is byte b)
                return Convert.ToUInt64(b);
            else if (this.number is sbyte sb)
                return Convert.ToUInt64(sb);
            else if (this.number is char c)
                return Convert.ToUInt64(c);
            else if (this.number is bool torf)
                return torf ? 1UL : 0UL;
            else if (this.number is string str)
            {
                if (ulong.TryParse(str, out ulong result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToUInt64(dt.Ticks);
            return 0UL;
        }

        public float ToSingle(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToSingle(dpfpn);
            else if (this.number is long int64)
                return Convert.ToSingle(int64);
            else if (this.number is int int32)
                return Convert.ToSingle(int32);
            else if (this.number is float single)
                return single;
            else if (this.number is short int16)
                return Convert.ToSingle(int16);
            else if (this.number is decimal dec)
                return decimal.ToSingle(dec);
            else if (this.number is uint uint32)
                return Convert.ToSingle(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToSingle(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToSingle(uint16);
            else if (this.number is byte b)
                return Convert.ToSingle(b);
            else if (this.number is sbyte sb)
                return Convert.ToSingle(sb);
            else if (this.number is char c)
                return c;
            else if (this.number is bool torf)
                return torf ? 1f : 0f;
            else if (this.number is string str)
            {
                if (float.TryParse(str, out float result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToSingle(dt.Ticks);
            return 0f;
        }

        public double ToDouble(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return dpfpn;
            else if (this.number is long int64)
                return Convert.ToDouble(int64);
            else if (this.number is int int32)
                return Convert.ToDouble(int32);
            else if (this.number is float single)
                return Convert.ToDouble(single);
            else if (this.number is short int16)
                return Convert.ToDouble(int16);
            else if (this.number is decimal dec)
                return decimal.ToDouble(dec);
            else if (this.number is uint uint32)
                return Convert.ToDouble(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToDouble(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToDouble(uint16);
            else if (this.number is byte b)
                return Convert.ToDouble(b);
            else if (this.number is sbyte sb)
                return Convert.ToDouble(sb);
            else if (this.number is char c)
                return (double)(c);
            else if (this.number is bool torf)
                return torf ? 1d : 0d;
            else if (this.number is string str)
            {
                if (double.TryParse(str, out double result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToDouble(dt.Ticks);
            return 0d;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            if (this.number is double dpfpn)
                return Convert.ToDecimal(dpfpn);
            else if (this.number is long int64)
                return Convert.ToDecimal(int64);
            else if (this.number is int int32)
                return Convert.ToDecimal(int32);
            else if (this.number is float single)
                return Convert.ToDecimal(single);
            else if (this.number is short int16)
                return Convert.ToDecimal(int16);
            else if (this.number is decimal dec)
                return dec;
            else if (this.number is uint uint32)
                return Convert.ToDecimal(uint32);
            else if (this.number is ulong uint64)
                return Convert.ToDecimal(uint64);
            else if (this.number is ushort uint16)
                return Convert.ToDecimal(uint16);
            else if (this.number is byte b)
                return Convert.ToDecimal(b);
            else if (this.number is sbyte sb)
                return Convert.ToDecimal(sb);
            else if (this.number is char c)
                return c;
            else if (this.number is bool torf)
                return torf ? 1m : 0m;
            else if (this.number is string str)
            {
                if (decimal.TryParse(str, out decimal result))
                    return result;
            }
            else if (this.number is DateTime dt)
                return Convert.ToDecimal(dt.Ticks);
            return 0m;
        }

        public DateTime ToDateTime(IFormatProvider provider) => new DateTime(ToInt64(provider));

        public string ToString(IFormatProvider provider) => ToString();

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(double))
                return ToDouble(provider);
            else if (conversionType == typeof(long))
                return ToInt64(provider);
            else if (conversionType == typeof(int))
                return ToInt32(provider);
            else if (conversionType == typeof(float))
                return ToSingle(provider);
            else if (conversionType == typeof(short))
                return ToInt16(provider);
            else if (conversionType == typeof(decimal))
                return ToDecimal(provider);
            else if (conversionType == typeof(uint))
                return ToUInt32(provider);
            else if (conversionType == typeof(ulong))
                return ToUInt64(provider);
            else if (conversionType == typeof(ushort))
                return ToUInt16(provider);
            else if (conversionType == typeof(byte))
                return ToByte(provider);
            else if (conversionType == typeof(sbyte))
                return ToSByte(provider);
            else if (conversionType == typeof(bool))
                return ToBoolean(provider);
            else if (conversionType == typeof(char))
                return ToChar(provider);
            else if (conversionType == typeof(string))
                return ToString(provider);
            else if (conversionType == typeof(DateTime))
                return ToDateTime(provider);
            return null;
        }

        public bool Equals(LuaNumber other)
        {
            if (this.IsInteger() && other.IsInteger())
                return ((long)this) == ((long)other);
            else if (this.IsInteger() && !other.IsInteger())
                return ((long)this) == ((double)other);
            else if (!this.IsInteger() && other.IsInteger())
                return ((double)this) == ((long)other);
            else
                return ((double)this) == ((double)other);
        }

        public int CompareTo(LuaNumber other) => ((double)this).CompareTo((double)other);

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is LuaNumber ln)
                return this.CompareTo(ln);
            else if (obj is double d)
                return this.CompareTo(d);
            else if (obj is float f)
                return this.CompareTo(f);
            else if (obj is decimal dc)
                return this.CompareTo(dc);
            else if (obj is long l)
                return this.CompareTo(l);
            else if (obj is int i)
                return this.CompareTo(i);
            else if (obj is short s)
                return this.CompareTo(s);
            else if (obj is byte b)
                return this.CompareTo(b);
            else if (obj is ulong ul)
                return this.CompareTo(ul);
            else if (obj is uint ui)
                return this.CompareTo(ui);
            else if (obj is ushort us)
                return this.CompareTo(us);
            else if (obj is sbyte sb)
                return this.CompareTo(sb);
            else if (obj is char c)
                return this.CompareTo(c);
            else if (obj is bool torf)
                return this.CompareTo(torf);

            throw new ArgumentException("Object must be of a numerical type.");
        }

        public int CompareTo(double other) => ((double)this).CompareTo((double)other);

        public bool Equals(double other)
        {
            if (this.IsInteger())
                return other.Equals((long)this);
            else
                return other.Equals((double)this);
        }

        public int CompareTo(float other) => ((double)this).CompareTo((double)other);

        public bool Equals(float other)
        {
            if (this.IsInteger())
                return other.Equals((long)this);
            else
                return other.Equals((double)this);
        }

        public int CompareTo(decimal other) => ((double)this).CompareTo((double)other);

        public bool Equals(decimal other) => ((double)this) == (double)other;

        public int CompareTo(long other) => ((double)this).CompareTo((double)other);

        public bool Equals(long other) => ((double)this) == (double)other;

        public int CompareTo(int other) => ((double)this).CompareTo((double)other);

        public bool Equals(int other) => ((double)this) == (double)other;

        public int CompareTo(short other) => ((double)this).CompareTo((double)other);

        public bool Equals(short other) => ((double)this) == (double)other;

        public int CompareTo(byte other) => ((double)this).CompareTo((double)other);

        public bool Equals(byte other) => ((double)this) == (double)other;

        public int CompareTo(ulong other) => ((double)this).CompareTo((double)other);

        public bool Equals(ulong other) => ((double)this) == (double)other;

        public int CompareTo(uint other) => ((double)this).CompareTo((double)other);

        public bool Equals(uint other) => ((double)this) == (double)other;

        public int CompareTo(ushort other) => ((double)this).CompareTo((double)other);

        public bool Equals(ushort other) => ((double)this) == (double)other;

        public int CompareTo(sbyte other) => ((double)this).CompareTo((double)other);

        public bool Equals(sbyte other) => ((double)this) == (double)other;

        public int CompareTo(char other) => ((double)this).CompareTo((double)other);

        public bool Equals(char other) => ((double)this) == (double)other;

        public int CompareTo(bool other) => ((bool)this).CompareTo(other);

        public bool Equals(bool other) => ((bool)this) == other;

        public static LuaNumber operator +(LuaNumber ln) => ln.IsInteger() ? new LuaNumber(+(long)ln) : new LuaNumber(+(double)ln);
        public static LuaNumber operator -(LuaNumber ln) => ln.IsInteger() ? new LuaNumber(-(long)ln) : new LuaNumber(-(double)ln);

        public static bool operator ==(LuaNumber ln1, LuaNumber ln2) => (double)ln1 == (double)ln2;
        public static bool operator !=(LuaNumber ln1, LuaNumber ln2) => (double)ln1 != (double)ln2;

        public static bool operator <(LuaNumber ln1, LuaNumber ln2) => (double)ln1 < (double)ln2;
        public static bool operator >(LuaNumber ln1, LuaNumber ln2) => (double)ln1 > (double)ln2;
        public static bool operator <=(LuaNumber ln1, LuaNumber ln2) => (double)ln1 <= (double)ln2;
        public static bool operator >=(LuaNumber ln1, LuaNumber ln2) => (double)ln1 >= (double)ln2;

        public static LuaNumber operator +(LuaNumber ln1, LuaNumber ln2) => new LuaNumber((double)ln1 + (double)ln2);
        public static LuaNumber operator -(LuaNumber ln1, LuaNumber ln2) => new LuaNumber((double)ln1 - (double)ln2);
        public static LuaNumber operator *(LuaNumber ln1, LuaNumber ln2) => new LuaNumber((double)ln1 * (double)ln2);
        public static LuaNumber operator /(LuaNumber ln1, LuaNumber ln2) => new LuaNumber((double)ln1 / (double)ln2);
        public static LuaNumber operator %(LuaNumber ln1, LuaNumber ln2) => new LuaNumber((double)ln1 % (double)ln2);

        public static bool operator ==(LuaNumber ln, double d) => (double)ln == d;
        public static bool operator ==(double d, LuaNumber ln) => (double)ln == d;
        public static bool operator !=(LuaNumber ln, double d) => (double)ln != d;
        public static bool operator !=(double d, LuaNumber ln) => (double)ln != d;

        public static bool operator ==(LuaNumber ln, float f) => (double)ln == (double)f;
        public static bool operator ==(float f, LuaNumber ln) => (double)ln == (double)f;
        public static bool operator !=(LuaNumber ln, float f) => (double)ln != (double)f;
        public static bool operator !=(float f, LuaNumber ln) => (double)ln != (double)f;

        public static bool operator ==(LuaNumber ln, decimal dc) => (double)ln == (double)dc;
        public static bool operator ==(decimal dc, LuaNumber ln) => (double)ln == (double)dc;
        public static bool operator !=(LuaNumber ln, decimal dc) => (double)ln != (double)dc;
        public static bool operator !=(decimal dc, LuaNumber ln) => (double)ln != (double)dc;

        public static bool operator ==(LuaNumber ln, long l) => (double)ln == (double)l;
        public static bool operator ==(long l, LuaNumber ln) => (double)ln == (double)l;
        public static bool operator !=(LuaNumber ln, long l) => (double)ln != (double)l;
        public static bool operator !=(long l, LuaNumber ln) => (double)ln != (double)l;

        public static bool operator ==(LuaNumber ln, int i) => (double)ln == (double)i;
        public static bool operator ==(int i, LuaNumber ln) => (double)ln == (double)i;
        public static bool operator !=(LuaNumber ln, int i) => (double)ln != (double)i;
        public static bool operator !=(int i, LuaNumber ln) => (double)ln != (double)i;

        public static bool operator ==(LuaNumber ln, short s) => (double)ln == (double)s;
        public static bool operator ==(short s, LuaNumber ln) => (double)ln == (double)s;
        public static bool operator !=(LuaNumber ln, short s) => (double)ln != (double)s;
        public static bool operator !=(short s, LuaNumber ln) => (double)ln != (double)s;

        public static bool operator ==(LuaNumber ln, byte b) => (double)ln == (double)b;
        public static bool operator ==(byte b, LuaNumber ln) => (double)ln == (double)b;
        public static bool operator !=(LuaNumber ln, byte b) => (double)ln != (double)b;
        public static bool operator !=(byte b, LuaNumber ln) => (double)ln != (double)b;

        public static bool operator ==(LuaNumber ln, ulong ul) => (double)ln == (double)ul;
        public static bool operator ==(ulong ul, LuaNumber ln) => (double)ln == (double)ul;
        public static bool operator !=(LuaNumber ln, ulong ul) => (double)ln != (double)ul;
        public static bool operator !=(ulong ul, LuaNumber ln) => (double)ln != (double)ul;

        public static bool operator ==(LuaNumber ln, uint ui) => (double)ln == (double)ui;
        public static bool operator ==(uint ui, LuaNumber ln) => (double)ln == (double)ui;
        public static bool operator !=(LuaNumber ln, uint ui) => (double)ln != (double)ui;
        public static bool operator !=(uint ui, LuaNumber ln) => (double)ln != (double)ui;

        public static bool operator ==(LuaNumber ln, ushort us) => (double)ln == (double)us;
        public static bool operator ==(ushort us, LuaNumber ln) => (double)ln == (double)us;
        public static bool operator !=(LuaNumber ln, ushort us) => (double)ln != (double)us;
        public static bool operator !=(ushort us, LuaNumber ln) => (double)ln != (double)us;

        public static bool operator ==(LuaNumber ln, sbyte sb) => (double)ln == (double)sb;
        public static bool operator ==(sbyte sb, LuaNumber ln) => (double)ln == (double)sb;
        public static bool operator !=(LuaNumber ln, sbyte sb) => (double)ln != (double)sb;
        public static bool operator !=(sbyte sb, LuaNumber ln) => (double)ln != (double)sb;

        public static bool operator ==(LuaNumber ln, char c) => (double)ln == (double)c;
        public static bool operator ==(char c, LuaNumber ln) => (double)ln == (double)c;
        public static bool operator !=(LuaNumber ln, char c) => (double)ln != (double)c;
        public static bool operator !=(char c, LuaNumber ln) => (double)ln != (double)c;

        public static bool operator ==(LuaNumber ln, bool torf) => (bool)ln == torf;
        public static bool operator ==(bool torf, LuaNumber ln) => (bool)ln == torf;
        public static bool operator !=(LuaNumber ln, bool torf) => (bool)ln == torf;
        public static bool operator !=(bool torf, LuaNumber ln) => (bool)ln == torf;

        public static bool operator <(LuaNumber ln, double d) => (double)ln < d;
        public static bool operator >(LuaNumber ln, double d) => (double)ln > d;
        public static bool operator <=(LuaNumber ln, double d) => (double)ln <= d;
        public static bool operator >=(LuaNumber ln, double d) => (double)ln >= d;
        public static bool operator <(double d, LuaNumber ln) => d < (double)ln;
        public static bool operator >(double d, LuaNumber ln) => d > (double)ln;
        public static bool operator <=(double d, LuaNumber ln) => d <= (double)ln;
        public static bool operator >=(double d, LuaNumber ln) => d >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, double d) => new LuaNumber((double)ln + d);
        public static LuaNumber operator -(LuaNumber ln, double d) => new LuaNumber((double)ln - d);
        public static LuaNumber operator *(LuaNumber ln, double d) => new LuaNumber((double)ln * d);
        public static LuaNumber operator /(LuaNumber ln, double d) => new LuaNumber((double)ln / d);
        public static LuaNumber operator %(LuaNumber ln, double d) => new LuaNumber((double)ln % d);
        public static LuaNumber operator +(double d, LuaNumber ln) => new LuaNumber(d + (double)ln);
        public static LuaNumber operator -(double d, LuaNumber ln) => new LuaNumber(d - (double)ln);
        public static LuaNumber operator *(double d, LuaNumber ln) => new LuaNumber(d * (double)ln);
        public static LuaNumber operator /(double d, LuaNumber ln) => new LuaNumber(d / (double)ln);
        public static LuaNumber operator %(double d, LuaNumber ln) => new LuaNumber(d % (double)ln);

        public static bool operator <(LuaNumber ln, float f) => (double)ln < (double)f;
        public static bool operator >(LuaNumber ln, float f) => (double)ln > (double)f;
        public static bool operator <=(LuaNumber ln, float f) => (double)ln <= (double)f;
        public static bool operator >=(LuaNumber ln, float f) => (double)ln >= (double)f;
        public static bool operator <(float f, LuaNumber ln) => (double)f < (double)ln;
        public static bool operator >(float f, LuaNumber ln) => (double)f > (double)ln;
        public static bool operator <=(float f, LuaNumber ln) => (double)f <= (double)ln;
        public static bool operator >=(float f, LuaNumber ln) => (double)f >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, float f) => new LuaNumber((double)ln + (double)f);
        public static LuaNumber operator -(LuaNumber ln, float f) => new LuaNumber((double)ln - (double)f);
        public static LuaNumber operator *(LuaNumber ln, float f) => new LuaNumber((double)ln * (double)f);
        public static LuaNumber operator /(LuaNumber ln, float f) => new LuaNumber((double)ln / (double)f);
        public static LuaNumber operator %(LuaNumber ln, float f) => new LuaNumber((double)ln % (double)f);
        public static LuaNumber operator +(float f, LuaNumber ln) => new LuaNumber((double)f + (double)ln);
        public static LuaNumber operator -(float f, LuaNumber ln) => new LuaNumber((double)f - (double)ln);
        public static LuaNumber operator *(float f, LuaNumber ln) => new LuaNumber((double)f * (double)ln);
        public static LuaNumber operator /(float f, LuaNumber ln) => new LuaNumber((double)f / (double)ln);
        public static LuaNumber operator %(float f, LuaNumber ln) => new LuaNumber((double)f % (double)ln);

        public static bool operator <(LuaNumber ln, decimal dc) => (double)ln < (double)dc;
        public static bool operator >(LuaNumber ln, decimal dc) => (double)ln > (double)dc;
        public static bool operator <=(LuaNumber ln, decimal dc) => (double)ln <= (double)dc;
        public static bool operator >=(LuaNumber ln, decimal dc) => (double)ln >= (double)dc;
        public static bool operator <(decimal dc, LuaNumber ln) => (double)dc < (double)ln;
        public static bool operator >(decimal dc, LuaNumber ln) => (double)dc > (double)ln;
        public static bool operator <=(decimal dc, LuaNumber ln) => (double)dc <= (double)ln;
        public static bool operator >=(decimal dc, LuaNumber ln) => (double)dc >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, decimal dc) => new LuaNumber((double)ln + (double)dc);
        public static LuaNumber operator -(LuaNumber ln, decimal dc) => new LuaNumber((double)ln - (double)dc);
        public static LuaNumber operator *(LuaNumber ln, decimal dc) => new LuaNumber((double)ln * (double)dc);
        public static LuaNumber operator /(LuaNumber ln, decimal dc) => new LuaNumber((double)ln / (double)dc);
        public static LuaNumber operator %(LuaNumber ln, decimal dc) => new LuaNumber((double)ln % (double)dc);
        public static LuaNumber operator +(decimal dc, LuaNumber ln) => new LuaNumber((double)dc + (double)ln);
        public static LuaNumber operator -(decimal dc, LuaNumber ln) => new LuaNumber((double)dc - (double)ln);
        public static LuaNumber operator *(decimal dc, LuaNumber ln) => new LuaNumber((double)dc * (double)ln);
        public static LuaNumber operator /(decimal dc, LuaNumber ln) => new LuaNumber((double)dc / (double)ln);
        public static LuaNumber operator %(decimal dc, LuaNumber ln) => new LuaNumber((double)dc % (double)ln);

        public static bool operator <(LuaNumber ln, long l) => (double)ln < (double)l;
        public static bool operator >(LuaNumber ln, long l) => (double)ln > (double)l;
        public static bool operator <=(LuaNumber ln, long l) => (double)ln <= (double)l;
        public static bool operator >=(LuaNumber ln, long l) => (double)ln >= (double)l;
        public static bool operator <(long l, LuaNumber ln) => (double)l < (double)ln;
        public static bool operator >(long l, LuaNumber ln) => (double)l > (double)ln;
        public static bool operator <=(long l, LuaNumber ln) => (double)l <= (double)ln;
        public static bool operator >=(long l, LuaNumber ln) => (double)l >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, long l) => new LuaNumber((double)ln + (double)l);
        public static LuaNumber operator -(LuaNumber ln, long l) => new LuaNumber((double)ln - (double)l);
        public static LuaNumber operator *(LuaNumber ln, long l) => new LuaNumber((double)ln * (double)l);
        public static LuaNumber operator /(LuaNumber ln, long l) => new LuaNumber((double)ln / (double)l);
        public static LuaNumber operator %(LuaNumber ln, long l) => new LuaNumber((double)ln % (double)l);
        public static LuaNumber operator +(long l, LuaNumber ln) => new LuaNumber((double)l + (double)ln);
        public static LuaNumber operator -(long l, LuaNumber ln) => new LuaNumber((double)l - (double)ln);
        public static LuaNumber operator *(long l, LuaNumber ln) => new LuaNumber((double)l * (double)ln);
        public static LuaNumber operator /(long l, LuaNumber ln) => new LuaNumber((double)l / (double)ln);
        public static LuaNumber operator %(long l, LuaNumber ln) => new LuaNumber((double)l % (double)ln);

        public static bool operator <(LuaNumber ln, int i) => (double)ln < (double)i;
        public static bool operator >(LuaNumber ln, int i) => (double)ln > (double)i;
        public static bool operator <=(LuaNumber ln, int i) => (double)ln <= (double)i;
        public static bool operator >=(LuaNumber ln, int i) => (double)ln >= (double)i;
        public static bool operator <(int i, LuaNumber ln) => (double)i < (double)ln;
        public static bool operator >(int i, LuaNumber ln) => (double)i > (double)ln;
        public static bool operator <=(int i, LuaNumber ln) => (double)i <= (double)ln;
        public static bool operator >=(int i, LuaNumber ln) => (double)i >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, int i) => new LuaNumber((double)ln + (double)i);
        public static LuaNumber operator -(LuaNumber ln, int i) => new LuaNumber((double)ln - (double)i);
        public static LuaNumber operator *(LuaNumber ln, int i) => new LuaNumber((double)ln * (double)i);
        public static LuaNumber operator /(LuaNumber ln, int i) => new LuaNumber((double)ln / (double)i);
        public static LuaNumber operator %(LuaNumber ln, int i) => new LuaNumber((double)ln % (double)i);
        public static LuaNumber operator +(int i, LuaNumber ln) => new LuaNumber((double)i + (double)ln);
        public static LuaNumber operator -(int i, LuaNumber ln) => new LuaNumber((double)i - (double)ln);
        public static LuaNumber operator *(int i, LuaNumber ln) => new LuaNumber((double)i * (double)ln);
        public static LuaNumber operator /(int i, LuaNumber ln) => new LuaNumber((double)i / (double)ln);
        public static LuaNumber operator %(int i, LuaNumber ln) => new LuaNumber((double)i % (double)ln);

        public static bool operator <(LuaNumber ln, short s) => (double)ln < (double)s;
        public static bool operator >(LuaNumber ln, short s) => (double)ln > (double)s;
        public static bool operator <=(LuaNumber ln, short s) => (double)ln <= (double)s;
        public static bool operator >=(LuaNumber ln, short s) => (double)ln >= (double)s;
        public static bool operator <(short s, LuaNumber ln) => (double)s < (double)ln;
        public static bool operator >(short s, LuaNumber ln) => (double)s > (double)ln;
        public static bool operator <=(short s, LuaNumber ln) => (double)s <= (double)ln;
        public static bool operator >=(short s, LuaNumber ln) => (double)s >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, short s) => new LuaNumber((double)ln + (double)s);
        public static LuaNumber operator -(LuaNumber ln, short s) => new LuaNumber((double)ln - (double)s);
        public static LuaNumber operator *(LuaNumber ln, short s) => new LuaNumber((double)ln * (double)s);
        public static LuaNumber operator /(LuaNumber ln, short s) => new LuaNumber((double)ln / (double)s);
        public static LuaNumber operator %(LuaNumber ln, short s) => new LuaNumber((double)ln % (double)s);
        public static LuaNumber operator +(short s, LuaNumber ln) => new LuaNumber((double)s + (double)ln);
        public static LuaNumber operator -(short s, LuaNumber ln) => new LuaNumber((double)s - (double)ln);
        public static LuaNumber operator *(short s, LuaNumber ln) => new LuaNumber((double)s * (double)ln);
        public static LuaNumber operator /(short s, LuaNumber ln) => new LuaNumber((double)s / (double)ln);
        public static LuaNumber operator %(short s, LuaNumber ln) => new LuaNumber((double)s % (double)ln);

        public static bool operator <(LuaNumber ln, byte b) => (double)ln < (double)b;
        public static bool operator >(LuaNumber ln, byte b) => (double)ln > (double)b;
        public static bool operator <=(LuaNumber ln, byte b) => (double)ln <= (double)b;
        public static bool operator >=(LuaNumber ln, byte b) => (double)ln >= (double)b;
        public static bool operator <(byte b, LuaNumber ln) => (double)b < (double)ln;
        public static bool operator >(byte b, LuaNumber ln) => (double)b > (double)ln;
        public static bool operator <=(byte b, LuaNumber ln) => (double)b <= (double)ln;
        public static bool operator >=(byte b, LuaNumber ln) => (double)b >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, byte b) => new LuaNumber((double)ln + (double)b);
        public static LuaNumber operator -(LuaNumber ln, byte b) => new LuaNumber((double)ln - (double)b);
        public static LuaNumber operator *(LuaNumber ln, byte b) => new LuaNumber((double)ln * (double)b);
        public static LuaNumber operator /(LuaNumber ln, byte b) => new LuaNumber((double)ln / (double)b);
        public static LuaNumber operator %(LuaNumber ln, byte b) => new LuaNumber((double)ln % (double)b);
        public static LuaNumber operator +(byte b, LuaNumber ln) => new LuaNumber((double)b + (double)ln);
        public static LuaNumber operator -(byte b, LuaNumber ln) => new LuaNumber((double)b - (double)ln);
        public static LuaNumber operator *(byte b, LuaNumber ln) => new LuaNumber((double)b * (double)ln);
        public static LuaNumber operator /(byte b, LuaNumber ln) => new LuaNumber((double)b / (double)ln);
        public static LuaNumber operator %(byte b, LuaNumber ln) => new LuaNumber((double)b % (double)ln);

        public static bool operator <(LuaNumber ln, ulong ul) => (double)ln < (double)ul;
        public static bool operator >(LuaNumber ln, ulong ul) => (double)ln > (double)ul;
        public static bool operator <=(LuaNumber ln, ulong ul) => (double)ln <= (double)ul;
        public static bool operator >=(LuaNumber ln, ulong ul) => (double)ln >= (double)ul;
        public static bool operator <(ulong ul, LuaNumber ln) => (double)ul < (double)ln;
        public static bool operator >(ulong ul, LuaNumber ln) => (double)ul > (double)ln;
        public static bool operator <=(ulong ul, LuaNumber ln) => (double)ul <= (double)ln;
        public static bool operator >=(ulong ul, LuaNumber ln) => (double)ul >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, ulong ul) => new LuaNumber((double)ln + (double)ul);
        public static LuaNumber operator -(LuaNumber ln, ulong ul) => new LuaNumber((double)ln - (double)ul);
        public static LuaNumber operator *(LuaNumber ln, ulong ul) => new LuaNumber((double)ln * (double)ul);
        public static LuaNumber operator /(LuaNumber ln, ulong ul) => new LuaNumber((double)ln / (double)ul);
        public static LuaNumber operator %(LuaNumber ln, ulong ul) => new LuaNumber((double)ln % (double)ul);
        public static LuaNumber operator +(ulong ul, LuaNumber ln) => new LuaNumber((double)ul + (double)ln);
        public static LuaNumber operator -(ulong ul, LuaNumber ln) => new LuaNumber((double)ul - (double)ln);
        public static LuaNumber operator *(ulong ul, LuaNumber ln) => new LuaNumber((double)ul * (double)ln);
        public static LuaNumber operator /(ulong ul, LuaNumber ln) => new LuaNumber((double)ul / (double)ln);
        public static LuaNumber operator %(ulong ul, LuaNumber ln) => new LuaNumber((double)ul % (double)ln);

        public static bool operator <(LuaNumber ln, uint ui) => (double)ln < (double)ui;
        public static bool operator >(LuaNumber ln, uint ui) => (double)ln > (double)ui;
        public static bool operator <=(LuaNumber ln, uint ui) => (double)ln <= (double)ui;
        public static bool operator >=(LuaNumber ln, uint ui) => (double)ln >= (double)ui;
        public static bool operator <(uint ui, LuaNumber ln) => (double)ui < (double)ln;
        public static bool operator >(uint ui, LuaNumber ln) => (double)ui > (double)ln;
        public static bool operator <=(uint ui, LuaNumber ln) => (double)ui <= (double)ln;
        public static bool operator >=(uint ui, LuaNumber ln) => (double)ui >= (double)ln;

        public static bool operator <(LuaNumber ln, ushort us) => (double)ln < (double)us;
        public static bool operator >(LuaNumber ln, ushort us) => (double)ln > (double)us;
        public static bool operator <=(LuaNumber ln, ushort us) => (double)ln <= (double)us;
        public static bool operator >=(LuaNumber ln, ushort us) => (double)ln >= (double)us;
        public static bool operator <(ushort us, LuaNumber ln) => (double)us < (double)ln;
        public static bool operator >(ushort us, LuaNumber ln) => (double)us > (double)ln;
        public static bool operator <=(ushort us, LuaNumber ln) => (double)us <= (double)ln;
        public static bool operator >=(ushort us, LuaNumber ln) => (double)us >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, ushort us) => new LuaNumber((double)ln + (double)us);
        public static LuaNumber operator -(LuaNumber ln, ushort us) => new LuaNumber((double)ln - (double)us);
        public static LuaNumber operator *(LuaNumber ln, ushort us) => new LuaNumber((double)ln * (double)us);
        public static LuaNumber operator /(LuaNumber ln, ushort us) => new LuaNumber((double)ln / (double)us);
        public static LuaNumber operator %(LuaNumber ln, ushort us) => new LuaNumber((double)ln % (double)us);
        public static LuaNumber operator +(ushort us, LuaNumber ln) => new LuaNumber((double)us + (double)ln);
        public static LuaNumber operator -(ushort us, LuaNumber ln) => new LuaNumber((double)us - (double)ln);
        public static LuaNumber operator *(ushort us, LuaNumber ln) => new LuaNumber((double)us * (double)ln);
        public static LuaNumber operator /(ushort us, LuaNumber ln) => new LuaNumber((double)us / (double)ln);
        public static LuaNumber operator %(ushort us, LuaNumber ln) => new LuaNumber((double)us % (double)ln);

        public static bool operator <(LuaNumber ln, sbyte sb) => (double)ln < (double)sb;
        public static bool operator >(LuaNumber ln, sbyte sb) => (double)ln > (double)sb;
        public static bool operator <=(LuaNumber ln, sbyte sb) => (double)ln <= (double)sb;
        public static bool operator >=(LuaNumber ln, sbyte sb) => (double)ln >= (double)sb;
        public static bool operator <(sbyte sb, LuaNumber ln) => (double)sb < (double)ln;
        public static bool operator >(sbyte sb, LuaNumber ln) => (double)sb > (double)ln;
        public static bool operator <=(sbyte sb, LuaNumber ln) => (double)sb <= (double)ln;
        public static bool operator >=(sbyte sb, LuaNumber ln) => (double)sb >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, sbyte sb) => new LuaNumber((double)ln + (double)sb);
        public static LuaNumber operator -(LuaNumber ln, sbyte sb) => new LuaNumber((double)ln - (double)sb);
        public static LuaNumber operator *(LuaNumber ln, sbyte sb) => new LuaNumber((double)ln * (double)sb);
        public static LuaNumber operator /(LuaNumber ln, sbyte sb) => new LuaNumber((double)ln / (double)sb);
        public static LuaNumber operator %(LuaNumber ln, sbyte sb) => new LuaNumber((double)ln % (double)sb);
        public static LuaNumber operator +(sbyte sb, LuaNumber ln) => new LuaNumber((double)sb + (double)ln);
        public static LuaNumber operator -(sbyte sb, LuaNumber ln) => new LuaNumber((double)sb - (double)ln);
        public static LuaNumber operator *(sbyte sb, LuaNumber ln) => new LuaNumber((double)sb * (double)ln);
        public static LuaNumber operator /(sbyte sb, LuaNumber ln) => new LuaNumber((double)sb / (double)ln);
        public static LuaNumber operator %(sbyte sb, LuaNumber ln) => new LuaNumber((double)sb % (double)ln);

        public static bool operator <(LuaNumber ln, char c) => (double)ln < (double)c;
        public static bool operator >(LuaNumber ln, char c) => (double)ln > (double)c;
        public static bool operator <=(LuaNumber ln, char c) => (double)ln <= (double)c;
        public static bool operator >=(LuaNumber ln, char c) => (double)ln >= (double)c;
        public static bool operator <(char c, LuaNumber ln) => (double)c < (double)ln;
        public static bool operator >(char c, LuaNumber ln) => (double)c > (double)ln;
        public static bool operator <=(char c, LuaNumber ln) => (double)c <= (double)ln;
        public static bool operator >=(char c, LuaNumber ln) => (double)c >= (double)ln;

        public static LuaNumber operator +(LuaNumber ln, char c) => new LuaNumber((double)ln + (double)c);
        public static LuaNumber operator -(LuaNumber ln, char c) => new LuaNumber((double)ln - (double)c);
        public static LuaNumber operator *(LuaNumber ln, char c) => new LuaNumber((double)ln * (double)c);
        public static LuaNumber operator /(LuaNumber ln, char c) => new LuaNumber((double)ln / (double)c);
        public static LuaNumber operator %(LuaNumber ln, char c) => new LuaNumber((double)ln % (double)c);
        public static LuaNumber operator +(char c, LuaNumber ln) => new LuaNumber((double)c + (double)ln);
        public static LuaNumber operator -(char c, LuaNumber ln) => new LuaNumber((double)c - (double)ln);
        public static LuaNumber operator *(char c, LuaNumber ln) => new LuaNumber((double)c * (double)ln);
        public static LuaNumber operator /(char c, LuaNumber ln) => new LuaNumber((double)c / (double)ln);
        public static LuaNumber operator %(char c, LuaNumber ln) => new LuaNumber((double)c % (double)ln);


        public static implicit operator LuaNumber(double dpfpn) => new LuaNumber(dpfpn);

        public static implicit operator LuaNumber(long int64) => new LuaNumber(int64);

        public static implicit operator LuaNumber(int int32) => new LuaNumber(int32);

        public static implicit operator LuaNumber(float single) => new LuaNumber(single);

        public static implicit operator LuaNumber(short int16) => new LuaNumber(int16);

        public static implicit operator LuaNumber(decimal dec) => new LuaNumber(dec);

        public static implicit operator LuaNumber(ulong uint64) => new LuaNumber(uint64);

        public static implicit operator LuaNumber(uint uint32) => new LuaNumber(uint32);

        public static implicit operator LuaNumber(ushort uint16) => new LuaNumber(uint16);

        public static implicit operator LuaNumber(byte b) => new LuaNumber(b);

        public static implicit operator LuaNumber(sbyte sb) => new LuaNumber(sb);

        public static implicit operator LuaNumber(char c) => new LuaNumber(c);

        public static explicit operator LuaNumber(bool torf) => new LuaNumber(torf);

        public static explicit operator LuaNumber(string str) => new LuaNumber(str);

        public static explicit operator LuaNumber(DateTime dt) => new LuaNumber(dt);

        public static explicit operator double(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return dpfpn;
            else if (ln.number is long int64)
                return Convert.ToDouble(int64);
            else if (ln.number is int int32)
                return Convert.ToDouble(int32);
            else if (ln.number is float single)
                return Convert.ToDouble(single);
            else if (ln.number is short int16)
                return Convert.ToDouble(int16);
            else if (ln.number is decimal dec)
                return decimal.ToDouble(dec);
            else if (ln.number is uint uint32)
                return Convert.ToDouble(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToDouble(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToDouble(uint16);
            else if (ln.number is byte b)
                return Convert.ToDouble(b);
            else if (ln.number is sbyte sb)
                return Convert.ToDouble(sb);
            else if (ln.number is char c)
                return c;
            else if (ln.number is bool torf)
                return torf ? 1d : 0d;
            else if (ln.number is string str)
            {
                if (double.TryParse(str, out double result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToDouble(dt.Ticks);
            return 0d;
        }

        public static explicit operator float(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToSingle(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToSingle(int64);
            else if (ln.number is int int32)
                return Convert.ToSingle(int32);
            else if (ln.number is float single)
                return single;
            else if (ln.number is short int16)
                return Convert.ToSingle(int16);
            else if (ln.number is decimal dec)
                return decimal.ToSingle(dec);
            else if (ln.number is uint uint32)
                return Convert.ToSingle(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToSingle(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToSingle(uint16);
            else if (ln.number is byte b)
                return Convert.ToSingle(b);
            else if (ln.number is sbyte sb)
                return Convert.ToSingle(sb);
            else if (ln.number is char c)
                return c;
            else if (ln.number is bool torf)
                return torf ? 1f : 0f;
            else if (ln.number is string str)
            {
                if (float.TryParse(str, out float result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToSingle(dt.Ticks);
            return 0f;
        }

        public static explicit operator int(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToInt32(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToInt32(int64);
            else if (ln.number is int int32)
                return int32;
            else if (ln.number is float single)
                return Convert.ToInt32(single);
            else if (ln.number is short int16)
                return Convert.ToInt32(int16);
            else if (ln.number is decimal dec)
                return decimal.ToInt32(dec);
            else if (ln.number is uint uint32)
                return Convert.ToInt32(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToInt32(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToInt32(uint16);
            else if (ln.number is byte b)
                return Convert.ToInt32(b);
            else if (ln.number is sbyte sb)
                return Convert.ToInt32(sb);
            else if (ln.number is char c)
                return Convert.ToInt32(c);
            else if (ln.number is bool torf)
                return torf ? 1 : 0;
            else if (ln.number is string str)
            {
                if (int.TryParse(str, out int result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToInt32(dt.Ticks);
            return 0;
        }

        public static explicit operator long(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToInt64(dpfpn);
            else if (ln.number is long int64)
                return int64;
            else if (ln.number is int int32)
                return Convert.ToInt64(int32);
            else if (ln.number is float single)
                return Convert.ToInt64(single);
            else if (ln.number is short int16)
                return Convert.ToInt64(int16);
            else if (ln.number is decimal dec)
                return decimal.ToInt64(dec);
            else if (ln.number is uint uint32)
                return Convert.ToInt64(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToInt64(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToInt64(uint16);
            else if (ln.number is byte b)
                return Convert.ToInt64(b);
            else if (ln.number is sbyte sb)
                return Convert.ToInt64(sb);
            else if (ln.number is char c)
                return Convert.ToInt64(c);
            else if (ln.number is bool torf)
                return torf ? 1L : 0L;
            else if (ln.number is string str)
            {
                if (long.TryParse(str, out long result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return dt.Ticks;
            return 0L;
        }

        public static explicit operator short(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToInt16(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToInt16(int64);
            else if (ln.number is int int32)
                return Convert.ToInt16(int32);
            else if (ln.number is float single)
                return Convert.ToInt16(single);
            else if (ln.number is short int16)
                return int16;
            else if (ln.number is decimal dec)
                return decimal.ToInt16(dec);
            else if (ln.number is uint uint32)
                return Convert.ToInt16(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToInt16(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToInt16(uint16);
            else if (ln.number is byte b)
                return Convert.ToInt16(b);
            else if (ln.number is sbyte sb)
                return Convert.ToInt16(sb);
            else if (ln.number is char c)
                return Convert.ToInt16(c);
            else if (ln.number is bool torf)
                return torf ? (short)1 : (short)0;
            else if (ln.number is string str)
            {
                if (short.TryParse(str, out short result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToInt16(dt.Ticks);
            return (short)0;
        }

        public static explicit operator decimal(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToDecimal(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToDecimal(int64);
            else if (ln.number is int int32)
                return Convert.ToDecimal(int32);
            else if (ln.number is float single)
                return Convert.ToDecimal(single);
            else if (ln.number is short int16)
                return Convert.ToDecimal(int16);
            else if (ln.number is decimal dec)
                return dec;
            else if (ln.number is uint uint32)
                return Convert.ToDecimal(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToDecimal(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToDecimal(uint16);
            else if (ln.number is byte b)
                return Convert.ToDecimal(b);
            else if (ln.number is sbyte sb)
                return Convert.ToDecimal(sb);
            else if (ln.number is char c)
                return c;
            else if (ln.number is bool torf)
                return torf ? 1m : 0m;
            else if (ln.number is string str)
            {
                if (decimal.TryParse(str, out decimal result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToDecimal(dt.Ticks);
            return 0m;
        }

        public static explicit operator uint(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToUInt32(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToUInt32(int64);
            else if (ln.number is int int32)
                return Convert.ToUInt32(int32);
            else if (ln.number is float single)
                return Convert.ToUInt32(single);
            else if (ln.number is short int16)
                return Convert.ToUInt32(int16);
            else if (ln.number is decimal dec)
                return decimal.ToUInt32(dec);
            else if (ln.number is uint uint32)
                return uint32;
            else if (ln.number is ulong uint64)
                return Convert.ToUInt32(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToUInt32(uint16);
            else if (ln.number is byte b)
                return Convert.ToUInt32(b);
            else if (ln.number is sbyte sb)
                return Convert.ToUInt32(sb);
            else if (ln.number is char c)
                return Convert.ToUInt32(c);
            else if (ln.number is bool torf)
                return torf ? 1U : 0U;
            else if (ln.number is string str)
            {
                if (uint.TryParse(str, out uint result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToUInt32(dt.Ticks);
            return 0U;
        }

        public static explicit operator ulong(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToUInt64(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToUInt64(int64);
            else if (ln.number is int int32)
                return Convert.ToUInt64(int32);
            else if (ln.number is float single)
                return Convert.ToUInt64(single);
            else if (ln.number is short int16)
                return Convert.ToUInt64(int16);
            else if (ln.number is decimal dec)
                return decimal.ToUInt64(dec);
            else if (ln.number is uint uint32)
                return Convert.ToUInt64(uint32);
            else if (ln.number is ulong uint64)
                return uint64;
            else if (ln.number is ushort uint16)
                return Convert.ToUInt64(uint16);
            else if (ln.number is byte b)
                return Convert.ToUInt64(b);
            else if (ln.number is sbyte sb)
                return Convert.ToUInt64(sb);
            else if (ln.number is char c)
                return Convert.ToUInt64(c);
            else if (ln.number is bool torf)
                return torf ? 1UL : 0UL;
            else if (ln.number is string str)
            {
                if (ulong.TryParse(str, out ulong result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToUInt64(dt.Ticks);
            return 0UL;
        }

        public static explicit operator ushort(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToUInt16(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToUInt16(int64);
            else if (ln.number is int int32)
                return Convert.ToUInt16(int32);
            else if (ln.number is float single)
                return Convert.ToUInt16(single);
            else if (ln.number is short int16)
                return Convert.ToUInt16(int16);
            else if (ln.number is decimal dec)
                return decimal.ToUInt16(dec);
            else if (ln.number is uint uint32)
                return Convert.ToUInt16(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToUInt16(uint64);
            else if (ln.number is ushort uint16)
                return uint16;
            else if (ln.number is byte b)
                return Convert.ToUInt16(b);
            else if (ln.number is sbyte sb)
                return Convert.ToUInt16(sb);
            else if (ln.number is char c)
                return Convert.ToUInt16(c);
            else if (ln.number is bool torf)
                return torf ? (ushort)1U : (ushort)0U;
            else if (ln.number is string str)
            {
                if (ushort.TryParse(str, out ushort result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToUInt16(dt.Ticks);
            return (ushort)0U;
        }

        public static explicit operator byte(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToByte(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToByte(int64);
            else if (ln.number is int int32)
                return Convert.ToByte(int32);
            else if (ln.number is float single)
                return Convert.ToByte(single);
            else if (ln.number is short int16)
                return Convert.ToByte(int16);
            else if (ln.number is decimal dec)
                return decimal.ToByte(dec);
            else if (ln.number is uint uint32)
                return Convert.ToByte(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToByte(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToByte(uint16);
            else if (ln.number is byte b)
                return b;
            else if (ln.number is sbyte sb)
                return Convert.ToByte(sb);
            else if (ln.number is char c)
                return Convert.ToByte(c);
            else if (ln.number is bool torf)
                return torf ? (byte)1 : (byte)0;
            else if (ln.number is string str)
            {
                if (byte.TryParse(str, out byte result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToByte(dt.Ticks);
            return (byte)0;
        }

        public static explicit operator sbyte(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToSByte(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToSByte(int64);
            else if (ln.number is int int32)
                return Convert.ToSByte(int32);
            else if (ln.number is float single)
                return Convert.ToSByte(single);
            else if (ln.number is short int16)
                return Convert.ToSByte(int16);
            else if (ln.number is decimal dec)
                return decimal.ToSByte(dec);
            else if (ln.number is uint uint32)
                return Convert.ToSByte(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToSByte(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToSByte(uint16);
            else if (ln.number is byte b)
                return Convert.ToSByte(b);
            else if (ln.number is sbyte sb)
                return sb;
            else if (ln.number is char c)
                return Convert.ToSByte(c);
            else if (ln.number is bool torf)
                return torf ? (sbyte)1 : (sbyte)0;
            else if (ln.number is string str)
            {
                if (sbyte.TryParse(str, out sbyte result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToSByte(dt.Ticks);
            return (sbyte)0;
        }

        public static explicit operator bool(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return Convert.ToBoolean(dpfpn);
            else if (ln.number is long int64)
                return Convert.ToBoolean(int64);
            else if (ln.number is int int32)
                return Convert.ToBoolean(int32);
            else if (ln.number is float single)
                return Convert.ToBoolean(single);
            else if (ln.number is short int16)
                return Convert.ToBoolean(int16);
            else if (ln.number is decimal dec)
                return Convert.ToBoolean(dec);
            else if (ln.number is uint uint32)
                return Convert.ToBoolean(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToBoolean(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToBoolean(uint16);
            else if (ln.number is byte b)
                return Convert.ToBoolean(b);
            else if (ln.number is sbyte sb)
                return Convert.ToBoolean(sb);
            else if (ln.number is char c)
                return c != 0;
            else if (ln.number is bool torf)
                return torf;
            else if (ln.number is string str)
            {
                if (bool.TryParse(str, out bool result))
                    return result;
            }
            else if (ln.number is DateTime dt)
                return Convert.ToBoolean(dt.Ticks);
            return false;
        }

        public static explicit operator char(LuaNumber ln)
        {
            if (ln.number is double dpfpn)
                return (char)dpfpn;
            else if (ln.number is long int64)
                return Convert.ToChar(int64);
            else if (ln.number is int int32)
                return Convert.ToChar(int32);
            else if (ln.number is float single)
                return (char)single;
            else if (ln.number is short int16)
                return Convert.ToChar(int16);
            else if (ln.number is decimal dec)
                return (char)dec;
            else if (ln.number is uint uint32)
                return Convert.ToChar(uint32);
            else if (ln.number is ulong uint64)
                return Convert.ToChar(uint64);
            else if (ln.number is ushort uint16)
                return Convert.ToChar(uint16);
            else if (ln.number is byte b)
                return Convert.ToChar(b);
            else if (ln.number is sbyte sb)
                return Convert.ToChar(sb);
            else if (ln.number is char c)
                return c;
            else if (ln.number is bool torf)
                return torf ? (char)10003 : (char)10060;
            else if (ln.number is string str)
                return Convert.ToChar(str);
            else if (ln.number is DateTime dt)
                return Convert.ToChar(dt.Ticks);
            return char.MinValue;
        }

        public static explicit operator DateTime(LuaNumber ln) => new DateTime((long)ln);
    }
}

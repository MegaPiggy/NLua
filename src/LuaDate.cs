using System;
using System.Collections.Generic;
using System.Linq;

namespace NLua
{
    public class LuaDate
    {
        public static implicit operator DateTime(LuaDate date) => new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        public static implicit operator LuaDate(DateTime dateTime) => new LuaDate(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.DayOfWeek, dateTime.DayOfYear);
        public LuaTable ToTable(Lua interpreter)
        {
            LuaTable table = interpreter.NewTable();
            table["year"] = date.Year;
            table["month"] = date.Month;
            table["day"] = date.Day;
            table["hour"] = date.Hour;
            table["min"] = date.Minute;
            table["sec"] = date.Second;
            if (date.IsDaylightSavingsTime.HasValue)
                table["isdst"] = date.IsDaylightSavingsTime.Value;
            table["wday"] = ((int)date.DayOfWeek) + 1;
            table["yday"] = date.DayOfYear;
            return table;
        }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; } = 12;
        public int Minute { get; set; } = 0;
        public int Second { get; set; } = 0;
        public DayOfWeek DayOfWeek { get; set; }
        public int DayOfYear { get; set; }
        public bool? IsDaylightSavingsTime { get; set; } = null;

        public LuaDate(LuaTable table)
        {
            Dictionary<object, object> dict = table.ToDictionary();
            Year = Convert.ToInt32(dict.GetOrDefault("year", 0));
            Month = Convert.ToInt32(dict.GetOrDefault("month", 0));
            Day = Convert.ToInt32(dict.GetOrDefault("day", 0));
            Hour = Convert.ToInt32(dict.GetOrDefault("hour", 12));
            Minute = Convert.ToInt32(dict.GetOrDefault("min", 0));
            Second = Convert.ToInt32(dict.GetOrDefault("sec", 0));
            var isdst = dict.GetOrDefault("isdst", null);
            IsDaylightSavingsTime = isdst != null ? Convert.ToBoolean(isdst) : null;
            DayOfWeek = (DayOfWeek)(Convert.ToInt32(dict.GetOrDefault("wday", ((int)((DateTime)this).DayOfWeek) + 1)) - 1);//((DateTime)this).DayOfWeek
            DayOfYear = Convert.ToInt32(dict.GetOrDefault("yday", ((DateTime)this).DayOfYear));//((DateTime)this).DayOfYear;
        }

        public LuaDate(int year, int month, int day, int hour = 12, int min = 0, int sec = 0, bool? isdst = null)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = min;
            Second = sec;
            IsDaylightSavingsTime = isdst;
            DayOfWeek = ((DateTime)this).DayOfWeek;
            DayOfYear = ((DateTime)this).DayOfYear;
        }

        public LuaDate(int year, int month, int day, int hour, int min, int sec, int wday, int yday, bool? isdst = null)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = min;
            Second = sec;
            DayOfWeek = (DayOfWeek)(wday - 1);
            DayOfYear = yday;
            IsDaylightSavingsTime = isdst;
        }

        public LuaDate(int year, int month, int day, int hour, int min, int sec, DayOfWeek wday, int yday, bool? isdst = null)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = min;
            Second = sec;
            DayOfWeek = wday;
            DayOfYear = yday;
            IsDaylightSavingsTime = isdst;
        }
    }
}

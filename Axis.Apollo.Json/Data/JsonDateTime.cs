using System;

namespace Axis.Apollo.Json.Data
{
    /// <summary>
    /// This Object ALWAYS represents LOCAL time. To get UTC time, subtract the UTC offset if it exists. If utc offset is 0, this time represents utc time.
    /// </summary>
    public class JsonDateTime
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
        public int second { get; set; }
        public int millisecond { get; set; }

        /// <summary>
        /// minute-offset from utc. E.g West Central africa will have a value of 60. New York will have a value of -300, i.e -60*5.
        /// </summary>
        public int utcOffset { get; set; }

        public bool IsMinValue()
        => year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0 && millisecond == 0;

        #region init
        public DateTime ToDateTime(DateTimeKind kind = DateTimeKind.Local)
        {
            if (IsMinValue()) return DateTime.MinValue;
            else if (kind == DateTimeKind.Local) return new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Local);
            else return new DateTime(year, month, hour, minute, second, millisecond, DateTimeKind.Utc) - TimeSpan.FromMinutes(utcOffset);
        }

        public JsonDateTime()
        { }

        public JsonDateTime(DateTime dateTime)
        {
            year = dateTime.Year;
            month = dateTime.Month;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
            millisecond = dateTime.Millisecond;
        }
        public JsonDateTime(long totalTicks) : this(new DateTime(totalTicks))
        { }
        #endregion
    }
}

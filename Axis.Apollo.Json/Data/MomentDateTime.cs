using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Apollo.Json.Data
{
    public class MomentDateTime
    {
        public int year { get; set; }

        /// <summary>
        /// Note that month is a zero-offset value, thus month 0 is january, month 1 is february, etc...
        /// </summary>
        public int month { get; set; }
        public int day { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
        public int second { get; set; }
        public int millisecond { get; set; }

        #region init
        public DateTime ToDateTime(DateTimeKind kind = DateTimeKind.Local) => new DateTime(year, month + 1, day, hour, minute, second, millisecond, kind);

        public MomentDateTime()
        { }

        public MomentDateTime(DateTime dateTime)
        {
            year = dateTime.Year;
            month = dateTime.Month - 1;
            day = dateTime.Day;
            hour = dateTime.Hour;
            minute = dateTime.Minute;
            second = dateTime.Second;
            millisecond = dateTime.Millisecond;
        }
        public MomentDateTime(long totalTicks) : this(new DateTime(totalTicks))
        { }
        #endregion
    }
}

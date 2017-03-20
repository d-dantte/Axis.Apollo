using System;

namespace Axis.Apollo.Json.Data
{

    public class JsonTimeSpan
    {
        public int days { get; set; }
        public int hours { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }
        public int milliSeconds { get; set; }

        public bool IsMinValue()
        => days == 0 && hours == 0 && minutes == 0 && seconds == 0 && milliSeconds == 0;

        #region init
        public TimeSpan TimeSpan()
        => IsMinValue() ? System.TimeSpan.MinValue:
           new TimeSpan(days, hours, minutes, seconds, milliSeconds);

        public JsonTimeSpan()
        { }

        public JsonTimeSpan(TimeSpan ts)
        {
            days = ts.Days;
            hours = ts.Hours;
            minutes = ts.Minutes;
            seconds = ts.Seconds;
            milliSeconds = ts.Milliseconds;
        }
        #endregion
    }
}

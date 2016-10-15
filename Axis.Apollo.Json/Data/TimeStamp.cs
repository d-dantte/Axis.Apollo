using System;

namespace Axis.Apollo.Json.Data
{

    public class TimeSpanObject
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int MilliSeconds { get; set; }

        #region init
        public TimeSpan TimeSpan() => new TimeSpan(Days, Hours, Minutes, Seconds, MilliSeconds);

        public TimeSpanObject()
        { }

        public TimeSpanObject(TimeSpan ts)
        {
            Days = ts.Days;
            Hours = ts.Hours;
            Minutes = ts.Minutes;
            Seconds = ts.Seconds;
            MilliSeconds = ts.Milliseconds;
        }
        #endregion
    }
}

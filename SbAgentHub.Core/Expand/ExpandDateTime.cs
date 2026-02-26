namespace SbAgentHub.Core.Expand
{
    public static class ExpandDateTime
    {
        /// <summary>
        /// 时间转换Unix时间戳
        /// </summary>
        public static int GetTimestamp(this DateTime datetime)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (int)(datetime - startTime).TotalSeconds;
        }

        /// <summary>
        /// 时间转换毫秒时间戳
        /// </summary>
        public static long GetTimestampMillisecond(this DateTime datetime)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return (datetime.Ticks - startTime.Ticks) / 10000;
        }

        /// <summary>
        /// 时间转换Unix时间戳
        /// </summary>
        public static DateTime GetDatetime(this long timestamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timestamp * 10000000;
            TimeSpan toNow = new TimeSpan(lTime);


            if (timestamp > 1700000000000)
            {
                lTime = timestamp / 1000;

                toNow = TimeSpan.FromSeconds(lTime);
            }



            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 时间转换Unix时间戳
        /// </summary>
        public static DateTime GetDatetime(this int timestamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timestamp * 10000000;
            TimeSpan toNow = new TimeSpan(lTime);

            return dtStart.Add(toNow);
        }
    }
}
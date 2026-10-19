namespace Sb.Apps.AgentHub.Core.Expand
{
    /// <summary>
    /// DateTime 扩展方法
    /// </summary>
    public static class ExpandDateTime
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 时间转换Unix时间戳（秒）
        /// </summary>
        /// <param name="datetime">要转换的时间</param>
        /// <returns>Unix 时间戳（秒）</returns>
        public static int GetTimestamp(this DateTime datetime)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(UnixEpoch, TimeZoneInfo.Local);
            return (int)(datetime - startTime).TotalSeconds;
        }

        /// <summary>
        /// 时间转换毫秒时间戳
        /// </summary>
        /// <param name="datetime">要转换的时间</param>
        /// <returns>Unix 时间戳（毫秒）</returns>
        public static long GetTimestampMillisecond(this DateTime datetime)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(UnixEpoch, TimeZoneInfo.Local);
            return (datetime.Ticks - startTime.Ticks) / 10000;
        }

        /// <summary>
        /// Unix时间戳转换为本地时间
        /// </summary>
        /// <param name="timestamp">Unix 时间戳（秒或毫秒）</param>
        /// <returns>本地时间</returns>
        public static DateTime GetDatetime(this long timestamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(UnixEpoch, TimeZoneInfo.Local);
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
        /// Unix时间戳（秒）转换为本地时间
        /// </summary>
        /// <param name="timestamp">Unix 时间戳（秒）</param>
        /// <returns>本地时间</returns>
        public static DateTime GetDatetime(this int timestamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(UnixEpoch, TimeZoneInfo.Local);
            long lTime = timestamp * 10000000;
            TimeSpan toNow = new TimeSpan(lTime);

            return dtStart.Add(toNow);
        }
    }
}

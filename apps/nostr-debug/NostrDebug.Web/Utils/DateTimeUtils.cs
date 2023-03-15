namespace NostrDebug.Web.Utils
{
    public static class DateTimeUtils
    {
        public static DateTime UtcNowOnlyHours()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        }
    }
}

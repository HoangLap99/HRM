#nullable disable
namespace HRM.Api.Services;

public class Common
{
    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        => DateTimeHelper.UnixTimeStampToDateTime(unixTimeStamp);

    public static double DateTimeToUnixTimeStamp(DateTime time)
        => DateTimeHelper.DateTimeToUnixTimeStamp(time);

    public static DateTime getDateTimeWithoutSeconds(DateTime time)
        => new(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);

    public static DateTime ToDateTime(string input)
        => DateTimeHelper.ToDateTime(input);
}

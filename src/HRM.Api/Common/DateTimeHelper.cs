﻿using System.Globalization;

namespace HRM.Api.Common;

/// <summary>
/// Tiện ích xử lý ngày giờ và chuyển đổi Unix Timestamp thay thế các hàm static cũ trong dungchung/DateTimeHelper.
/// </summary>
public static class DateTimeHelper
{
    public static readonly DateTime MinDate = new(1790, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MIN_DATE = MinDate;
    public static readonly DateTime MinSqlDate = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MaxDate = new(3000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static readonly DateTime MAX_DATE = MaxDate;
    public static DateTime DATE_NOW => DateTime.Now;
    public static DateTime DATE_NOW_LOCAL => DateTime.UtcNow;

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
    }

    public static DateTime? UnixTimeStampToDateTime(double? unixTimeStamp)
    {
        if (unixTimeStamp is null or 0)
        {
            return null;
        }

        return UnixTimeStampToDateTime(unixTimeStamp.Value);
    }

    public static double DateTimeToUnixTimeStamp(DateTime? time)
    {
        if (time is null)
        {
            return 0;
        }

        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        return (time.Value - epochStart).TotalSeconds;
    }

    public static DateTime ToDateTime(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return UnixTimeStampToDateTime(0);
        }

        if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        if (DateTime.TryParse(input, new CultureInfo("vi-VN"), DateTimeStyles.None, out result))
        {
            return result;
        }

        return UnixTimeStampToDateTime(0);
    }

    public static DateTime ClampSqlDate(DateTime value)
        => value < MinSqlDate ? MinSqlDate : value;

    public static DateTime GetFirstDayOfMonth(DateTime date) => new(date.Year, date.Month, 1);

    public static DateTime GetLastDayOfMonth(DateTime date) => GetFirstDayOfMonth(date).AddMonths(1).AddDays(-1);

    public static int GetNumberWorkDayOfMonth(DateTime date)
    {
        var lastDay = GetLastDayOfMonth(date);
        var sundayCount = 0;

        for (int day = 1; day <= lastDay.Day; day++)
        {
            var current = new DateTime(lastDay.Year, lastDay.Month, day);
            if (current.DayOfWeek == DayOfWeek.Sunday)
            {
                sundayCount++;
            }
        }

        return lastDay.Day - sundayCount;
    }

    public static DateTime GetDayOfWeek(DateTime date, DayOfWeek targetDayOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - targetDayOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }

    public static DateTime GetFirstDayOfYear(DateTime date) => new(date.Year, 1, 1);

    public static DateTime GetLastDayOfYear(DateTime date) => GetFirstDayOfYear(date).AddYears(1).AddDays(-1);

    public static int GetAge(DateTime birthDate) => DateTime.Now.Year - birthDate.Year;
}


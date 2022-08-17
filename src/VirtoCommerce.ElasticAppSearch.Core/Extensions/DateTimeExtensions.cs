using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetPreviousMillisecond(this DateTime dateTime)
    {
        return dateTime - new TimeSpan(0, 0, 0, 0, 1);
    }

    public static DateTime GetNextMillisecond(this DateTime dateTime)
    {
        return dateTime + new TimeSpan(0, 0, 0, 0, 1);
    }
}

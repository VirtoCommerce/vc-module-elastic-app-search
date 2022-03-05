using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetPreviousSecond(this DateTime dateTime)
    {
        return dateTime - new TimeSpan(0, 0, 0, 1);
    }

    public static DateTime GetNextSecond(this DateTime dateTime)
    {
        return dateTime + new TimeSpan(0, 0, 0, 1);
    }
}

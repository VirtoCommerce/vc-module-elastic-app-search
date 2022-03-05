using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime PreviousSecond(this DateTime dateTime)
    {
        return dateTime - new TimeSpan(0, 0, 0, 1);
    }

    public static DateTime NextSecond(this DateTime dateTime)
    {
        return dateTime + new TimeSpan(0, 0, 0, 1);
    }
}

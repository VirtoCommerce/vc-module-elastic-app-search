using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DoubleExtensions
{
    public static double GetNearestLower(this double value)
    {
        return Math.BitDecrement(value);
    }

    public static double GetNearestHigher(this double value)
    {
        return Math.BitIncrement(value);
    }
}

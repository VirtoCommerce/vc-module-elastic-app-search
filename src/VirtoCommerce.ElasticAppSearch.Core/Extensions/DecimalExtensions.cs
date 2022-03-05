using System;
using System.Linq;
using System.Numerics;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DecimalExtensions
{
    private const int IntegerPartsCount = 3;
    private const byte MaxScale = 28;

    public static decimal NearestLower(this decimal value)
    {
        return Nearest(value, addition => value - addition);
    }

    public static decimal NearestHigher(this decimal value)
    {
        return Nearest(value, addition => value + addition);
    }

    private static decimal Nearest(decimal value, Func<decimal, decimal> valueConverter)
    {
        var parts = decimal.GetBits(value);

        var integerParts = parts[..IntegerPartsCount].Select(part => unchecked((uint)part)).ToArray();
        var integer = new BigInteger(integerParts.SelectMany(BitConverter.GetBytes).ToArray(), true);

        var additionScale = (byte)(MaxScale - (int)Math.Truncate(BigInteger.Log10(integer)));
        var addition = new decimal(1, 0, 0, false, additionScale);

        value = valueConverter(addition);

        return value;
    }
}

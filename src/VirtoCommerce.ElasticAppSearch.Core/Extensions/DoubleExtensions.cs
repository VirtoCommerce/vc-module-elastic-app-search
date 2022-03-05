using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DoubleExtensions
{
    private const ulong SignMask = 0x8000_0000_0000_0000;
    private const int SignShift = 63;

    private const ulong ExponentMask = 0x7FF0_0000_0000_0000;
    private const int ExponentShift = 52;

    private const ulong SignificandMask = 0x000F_FFFF_FFFF_FFFF;

    public static double GetNearestLower(this double value)
    {
        if (value == double.MinValue)
        {
            throw new ArgumentException("Can't create double lower than minimum value", nameof(value));
        }

        return GetNearest(value, (sign, significand) =>
        {
            if (significand == 0)
            {
                sign = true;
            }

            significand = sign ? significand + 1 : significand - 1;

            return (sign, significand);
        });
    }

    public static double GetNearestHigher(this double value)
    {
        if (value == double.MaxValue)
        {
            throw new ArgumentException("Can't create double higher than maximum value", nameof(value));
        }

        return GetNearest(value, (sign, significand) =>
        {
            significand = sign ? significand - 1 : significand + 1;
            
            if (significand == 0)
            {
                sign = false;
            }

            return (sign, significand);
        });
    }

    private static double GetNearest(double value, Func<bool, ulong,(bool, ulong)> significandConverter)
    {
        var bits = BitConverter.DoubleToUInt64Bits(value);
        var significand = GetSignificand(bits);
        var exponent = GetExponent(bits);
        var sign = GetSign(bits);
        (sign, significand) = significandConverter(sign, significand);
        bits = ((sign ? 1ul : 0ul) << SignShift) + ((ulong)exponent << ExponentShift) + significand;
        return BitConverter.UInt64BitsToDouble(bits);
    }

    private static bool GetSign(ulong bits)
    {
        return (bits & SignMask) >> SignShift != 0;
    }

    private static int GetExponent(ulong bits)
    {
        return (int)((bits & ExponentMask) >> ExponentShift);
    }

    private static ulong GetSignificand(ulong bits)
    {
        return bits & SignificandMask;
    }
}

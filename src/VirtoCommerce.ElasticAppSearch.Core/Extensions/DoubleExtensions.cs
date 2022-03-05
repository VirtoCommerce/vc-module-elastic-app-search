using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class DoubleExtensions
{
    private const ulong SignMask = 0x8000_0000_0000_0000;
    private const int SignShift = 63;

    private const ulong ExponentMask = 0x7FF0_0000_0000_0000;
    private const int ExponentShift = 52;

    private const ulong SignificandMask = 0x000F_FFFF_FFFF_FFFF;

    public static double NearestLower(this double value)
    {
        if (value == double.MinValue)
        {
            throw new ArgumentException("Can't create double lower than minimum value", nameof(value));
        }

        return Nearest(value, (sign, significand) =>
        {
            if (significand == 0)
            {
                sign = true;
            }

            significand = sign ? significand + 1 : significand - 1;

            return (sign, significand);
        });
    }

    public static double NearestHigher(this double value)
    {
        if (value == double.MaxValue)
        {
            throw new ArgumentException("Can't create double higher than maximum value", nameof(value));
        }

        return Nearest(value, (sign, significand) =>
        {
            significand = sign ? significand - 1 : significand + 1;
            
            if (significand == 0)
            {
                sign = false;
            }

            return (sign, significand);
        });
    }

    private static double Nearest(double value, Func<bool, ulong,(bool, ulong)> significandConverter)
    {
        var bits = BitConverter.DoubleToUInt64Bits(value);
        var significand = ExtractSignificand(bits);
        var exponent = ExtractExponent(bits);
        var sign = ExtractSign(bits);
        (sign, significand) = significandConverter(sign, significand);
        bits = ((sign ? 1ul : 0ul) << SignShift) + ((ulong)exponent << ExponentShift) + significand;
        return BitConverter.UInt64BitsToDouble(bits);
    }

    private static bool ExtractSign(ulong bits)
    {
        return (bits & SignMask) >> SignShift != 0;
    }

    private static int ExtractExponent(ulong bits)
    {
        return (int)((bits & ExponentMask) >> ExponentShift);
    }

    private static ulong ExtractSignificand(ulong bits)
    {
        return bits & SignificandMask;
    }
}

using NickvisionMoney.Shared.Helpers;
using System.Globalization;
using Xunit;

namespace NickvisionMoney.Shared.Tests;

public class CurrencyHelperTests
{
    private static decimal[] SampleAmounts = { 0M, 109M, 100M, 10920M, 0.002M, 1.2M, 12.00000004M, 1.0234567890M };

    public static IEnumerable<object[]> GetSampleDataWithRealCultures()
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures).AsEnumerable();
        foreach (var culture in cultures)
        {
            foreach (var number in SampleAmounts)
            {
                yield return new object[] { culture, number };
            }
        }
    }

    public static IEnumerable<object[]> GetSampleDataWithCustomCultures()
    {
        var culture1 = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        var culture2 = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        var culture3 = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        var culture4 = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        // Decimal digits
        culture1.NumberFormat.CurrencyDecimalDigits = 1;
        culture2.NumberFormat.CurrencyDecimalDigits = 2;
        culture3.NumberFormat.CurrencyDecimalDigits = 3;
        culture4.NumberFormat.CurrencyDecimalDigits = 99;
        // Decimal separator
        culture1.NumberFormat.CurrencyDecimalSeparator = ".";
        culture2.NumberFormat.CurrencyDecimalSeparator = "/";
        culture3.NumberFormat.CurrencyDecimalSeparator = "'";
        culture4.NumberFormat.CurrencyDecimalSeparator = "*";
        // Group separator
        culture1.NumberFormat.CurrencyGroupSeparator = ",";
        culture2.NumberFormat.CurrencyGroupSeparator = "-";
        culture3.NumberFormat.CurrencyGroupSeparator = ".";
        culture4.NumberFormat.CurrencyGroupSeparator = " ";
        // Currency symbol
        culture1.NumberFormat.CurrencySymbol = "\ud83e\ude99";
        culture2.NumberFormat.CurrencySymbol = ":O";
        culture3.NumberFormat.CurrencySymbol = ":D";
        culture4.NumberFormat.CurrencySymbol = ":(";
        // Currency positive pattern
        culture1.NumberFormat.CurrencyPositivePattern = 0;
        culture2.NumberFormat.CurrencyPositivePattern = 1;
        culture3.NumberFormat.CurrencyPositivePattern = 2;
        culture4.NumberFormat.CurrencyPositivePattern = 3;
        foreach (var amount in SampleAmounts)
        {
            yield return new object[] { culture1, amount };
            yield return new object[] { culture2, amount };
            yield return new object[] { culture3, amount };
            yield return new object[] { culture4, amount };
        }
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithRealCultures))]
    public void ToAmountString_RealCulturesShouldWorkByDefault(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        var result = amount.ToAmountString(culture, false);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithCustomCultures))]
    public void ToAmountString_CustomCulturesShouldWorkByDefault(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        RemoveSymbol(ref expected, culture);
        FormatUnlimitedDecimals(ref expected, culture);
        AddSymbol(ref expected, culture);
        var result = amount.ToAmountString(culture, false);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithRealCultures))]
    public void ToAmountString_RealCulturesShouldWorkWithNativeDigits(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        expected = expected
            .Replace("0", culture.NumberFormat.NativeDigits[0])
            .Replace("1", culture.NumberFormat.NativeDigits[1])
            .Replace("2", culture.NumberFormat.NativeDigits[2])
            .Replace("3", culture.NumberFormat.NativeDigits[3])
            .Replace("4", culture.NumberFormat.NativeDigits[4])
            .Replace("5", culture.NumberFormat.NativeDigits[5])
            .Replace("6", culture.NumberFormat.NativeDigits[6])
            .Replace("7", culture.NumberFormat.NativeDigits[7])
            .Replace("8", culture.NumberFormat.NativeDigits[8])
            .Replace("9", culture.NumberFormat.NativeDigits[9]);
        var result = amount.ToAmountString(culture, true);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithRealCultures))]
    public void ToAmountString_RealCulturesShouldWorkWithoutCurrencySymbol(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        expected = expected.Replace(culture.NumberFormat.CurrencySymbol, "").Trim();
        var result = amount.ToAmountString(culture, false, false);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithCustomCultures))]
    public void ToAmountString_CustomCulturesShouldWorkWithoutCurrencySymbol(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        RemoveSymbol(ref expected, culture);
        FormatUnlimitedDecimals(ref expected, culture);
        var result = amount.ToAmountString(culture, false, false);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetSampleDataWithRealCultures))]
    public void ToAmountString_AllCulturesShouldWorkWithOverwriteDecimal(CultureInfo culture, decimal amount)
    {
        //Arrange
        var expected = amount.ToString("C6", culture).Trim();
        RemoveSymbol(ref expected, culture);
        if (culture.Name is "kea-CV" or "pt-CV" && expected.EndsWith('0'))
        {
            expected = expected.TrimEnd('0');
            if (expected.EndsWith(culture.NumberFormat.CurrencyDecimalSeparator))
                expected = $"{expected}00";
            else if (expected.Substring(expected.IndexOf(culture.NumberFormat.CurrencyDecimalSeparator) + 1).Length == 1)
                expected += "0";
        }
        else
        {
            FormatUnlimitedDecimals(ref expected, culture, true);
        }
        AddSymbol(ref expected, culture);
        //Act
        var result = amount.ToAmountString(culture, false, overwriteDecimal: true);
        //Assert
        Assert.Equal(expected, result);
    }

    private static void RemoveSymbol(ref string amount, CultureInfo culture)
    {
        amount = amount.Replace(culture.NumberFormat.CurrencySymbol, "").Trim();
    }

    private static void FormatUnlimitedDecimals(ref string number, CultureInfo culture, bool force = false)
    {
        if (culture.NumberFormat.CurrencyDecimalDigits != 99 && !force)
            return;

        number = number.TrimEnd('0');
        if (number.EndsWith(culture.NumberFormat.CurrencyDecimalSeparator))
            number = number.Replace(culture.NumberFormat.CurrencyDecimalSeparator, "");
    }

    private static void AddSymbol(ref string amount, CultureInfo culture)
    {
        var formatString = culture.NumberFormat.CurrencyPositivePattern switch
        {
            0 => $"{culture.NumberFormat.CurrencySymbol}{{0}}",
            1 => $"{{0}}{culture.NumberFormat.CurrencySymbol}",
            2 => $"{culture.NumberFormat.CurrencySymbol} {{0}}",
            3 => $"{{0}} {culture.NumberFormat.CurrencySymbol}",
            _ => $"{culture.NumberFormat.CurrencySymbol}{{0}}"
        };
        amount = string.Format(formatString, amount);
    }
}
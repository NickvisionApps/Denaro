using System.Globalization;
using NickvisionMoney.Shared.Helpers;
using Xunit;

namespace NickvisionMoney.Shared.Tests;

public class CurrencyHelperTests
{
    public static decimal[] SampleAmounts = { 0M, 109M, 100M, 10920M, 0.002M, 1.2M, 12.00000004M, 1.0234567890M };

    public static IEnumerable<object[]> GetSampleData()
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
    
    [Theory]
    [MemberData(nameof(GetSampleData))]
    public void ToAmountString_AllCulturesShouldWorkByDefault(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        var result = amount.ToAmountString(culture, false);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [MemberData(nameof(GetSampleData))]
    public void ToAmountString_AllCulturesShouldWorkWithNativeDigits(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        expected =  expected
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
    [MemberData(nameof(GetSampleData))]
    public void ToAmountString_AllCulturesShouldWorkWithoutCurrencySymbol(CultureInfo culture, decimal amount)
    {
        var expected = amount.ToString("C", culture);
        if(culture.NumberFormat.CurrencyDecimalSeparator != culture.NumberFormat.CurrencySymbol)
        { 
            expected = expected.Remove(expected.IndexOf(culture.NumberFormat.CurrencySymbol), culture.NumberFormat.CurrencySymbol.Length).Trim();
        }
        var result = amount.ToAmountString(culture, false, false);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [MemberData(nameof(GetSampleData))]
    public void ToAmountString_AllCulturesShouldWorkWithOverwriteDecimal(CultureInfo culture, decimal amount)
    {
        //Arrange
        var expected = amount.ToString("C6", culture).Replace("\u200b", "").Trim();
        if (culture.Name is "kea-CV" or "pt-CV")
        {
            expected = expected.TrimEnd('0');
            if (expected.EndsWith(culture.NumberFormat.CurrencySymbol))
                expected += "00"; // Make the currency symbol be in the center of the string
        }
        else
        {
            var symbolAtEnd = expected.EndsWith(culture.NumberFormat.CurrencySymbol);
            if(symbolAtEnd)
                expected = expected.Replace(culture.NumberFormat.CurrencySymbol, "").Trim();
            expected = expected.TrimEnd('0');
            if (expected.EndsWith(culture.NumberFormat.CurrencyDecimalSeparator))
            {
                expected = expected.Remove(expected.LastIndexOf(culture.NumberFormat.CurrencyDecimalSeparator));
            }
            if (symbolAtEnd)
                expected += (culture.NumberFormat.CurrencyPositivePattern == 3 ? " " : "") + culture.NumberFormat.CurrencySymbol;
        }
        //Act
        var result = amount.ToAmountString(culture, false, overwriteDecimal: true);
        //Assert
        Assert.Equal(expected, result);
    }
}
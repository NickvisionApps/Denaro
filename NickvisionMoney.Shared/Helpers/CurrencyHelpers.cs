using System;
using System.Globalization;

namespace NickvisionMoney.Shared.Helpers;

/// <summary>
/// Helper methods for working with currency
/// </summary>
public static class CurrencyHelpers
{
    /// <summary>
    /// Converts amount to currency string (non-negative)
    /// <summary>
    /// <param name="amount">Amount decimal value</param>
    /// <param name="culture">Culture used for formatting</param>
    /// <param name="showCurrencySymbol">Whether to add currency symbol</param>
    /// <returns>Formatted amount string</returns>
    public static string ToAmountString(this decimal amount, CultureInfo culture, bool showCurrencySymbol = true)
    {
        var result = Math.Abs(amount).ToString("C", culture);
        result = result.Remove(result.IndexOf(culture.NumberFormat.CurrencySymbol), culture.NumberFormat.CurrencySymbol.Length).Trim();
        if (culture.NumberFormat.CurrencyDecimalDigits == 99)
        {
            result = result.TrimEnd('0');
            if (result.EndsWith(culture.NumberFormat.CurrencyDecimalSeparator))
            {
                result = result.Remove(result.LastIndexOf(culture.NumberFormat.CurrencyDecimalSeparator));
            }
        }
        if (showCurrencySymbol)
        {
            var formatString = culture.NumberFormat.CurrencyPositivePattern switch
            {
                0 => $"{culture.NumberFormat.CurrencySymbol}{{0}}",
                1 => $"{{0}}{culture.NumberFormat.CurrencySymbol}",
                2 => $"{culture.NumberFormat.CurrencySymbol} {{0}}",
                3 => $"{{0}} {culture.NumberFormat.CurrencySymbol}"
            };
            result = string.Format(formatString, result);
        }
        return result;
    }
}
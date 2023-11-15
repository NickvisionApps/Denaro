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
    /// <param name="useNativeDigits">Whether to convert Latin digits to native digits</param>
    /// <param name="showCurrencySymbol">Whether to add currency symbol</param>
    /// <param name="overwriteDecimal">Whether to keep more digits in decimal part to increase precision</param>
    /// <returns>Formatted amount string</returns>
    public static string ToAmountString(this decimal amount, CultureInfo culture, bool useNativeDigits, bool showCurrencySymbol = true, bool overwriteDecimal = false)
    {
        var result = Math.Abs(amount).ToString(overwriteDecimal ? "C6" : culture.Name is "kea-CV" or "pt-CV" ? "C2" : "C", culture);
        result = result.Replace(culture.NumberFormat.CurrencySymbol, "").Trim();
        if (culture.NumberFormat.CurrencyDecimalDigits == 99 || overwriteDecimal)
        {
            result = result.TrimEnd('0');
            if (result.EndsWith(culture.NumberFormat.CurrencyDecimalSeparator))
            {
                result = culture.Name is "kea-CV" or "pt-CV" ? $"{result}00" : result.Replace(culture.NumberFormat.CurrencyDecimalSeparator, "");
            }
            else if (result.Substring(result.IndexOf(culture.NumberFormat.CurrencyDecimalSeparator) + 1).Length == 1 && culture.Name is "kea-CV" or "pt-CV")
            {
                result = $"{result}0";
            }
        }
        if (showCurrencySymbol)
        {
            var formatString = culture.NumberFormat.CurrencyPositivePattern switch
            {
                0 => $"{culture.NumberFormat.CurrencySymbol}{{0}}",
                1 => $"{{0}}{culture.NumberFormat.CurrencySymbol}",
                2 => $"{culture.NumberFormat.CurrencySymbol} {{0}}",
                3 => $"{{0}} {culture.NumberFormat.CurrencySymbol}",
                _ => $"{culture.NumberFormat.CurrencySymbol}{{0}}"
            };
            result = string.Format(formatString, result);
        }
        if (useNativeDigits && "0" != culture.NumberFormat.NativeDigits[0])
        {
            result = result.Replace("0", culture.NumberFormat.NativeDigits[0])
                           .Replace("1", culture.NumberFormat.NativeDigits[1])
                           .Replace("2", culture.NumberFormat.NativeDigits[2])
                           .Replace("3", culture.NumberFormat.NativeDigits[3])
                           .Replace("4", culture.NumberFormat.NativeDigits[4])
                           .Replace("5", culture.NumberFormat.NativeDigits[5])
                           .Replace("6", culture.NumberFormat.NativeDigits[6])
                           .Replace("7", culture.NumberFormat.NativeDigits[7])
                           .Replace("8", culture.NumberFormat.NativeDigits[8])
                           .Replace("9", culture.NumberFormat.NativeDigits[9]);
        }
        return result;
    }

    /// <summary>
    /// Replaces native digits in a string with Latin digits
    /// </summary>
    /// <param name="amountString">The amount string</param>
    /// <param name="culture">Culture used for formatting</param>
    /// <returns>A new string with native digits replaced with Latin digits</returns>
    public static string ReplaceNativeDigits(this string amountString, CultureInfo culture)
    {
        var result = amountString;
        foreach (var digit in culture.NumberFormat.NativeDigits)
        {
            result = result.Replace(digit, Array.FindIndex(culture.NumberFormat.NativeDigits, c => c == digit).ToString());
        }
        return result;
    }
}
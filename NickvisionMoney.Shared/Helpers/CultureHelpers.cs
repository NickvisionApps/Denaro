using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.Shared.Helpers;

/// <summary>
/// Helpers for working with culture
/// </summary>
public static class CultureHelpers
{
    /// <summary>
    /// A culture to use for date strings
    /// </summary>
    public static CultureInfo DateCulture { get; }
    /// <summary>
    /// The system reported currency string (Ex: "$ (USD)")
    /// </summary>
    public static string ReportedCurrencyString { get; }

    /// <summary>
    /// Constructs CultureHelpers
    /// </summary>
    static CultureHelpers()
    {
        //Date Culture
        var lcTime = Environment.GetEnvironmentVariable("LC_TIME");
        if (lcTime != null && lcTime.Contains(".UTF-8"))
        {
            lcTime = lcTime.Remove(lcTime.IndexOf(".UTF-8"), 6);
        }
        else if (lcTime != null && lcTime.Contains(".utf8"))
        {
            lcTime = lcTime.Remove(lcTime.IndexOf(".utf8"), 5);
        }
        if (lcTime != null && lcTime.Contains('_'))
        {
            lcTime = lcTime.Replace('_', '-');
        }
        DateCulture = new CultureInfo(!string.IsNullOrWhiteSpace(lcTime) ? lcTime : CultureInfo.CurrentCulture.Name, true);
        //Reported Currency String
        var lcMonetary = Environment.GetEnvironmentVariable("LC_MONETARY");
        if (lcMonetary != null && lcMonetary.Contains(".UTF-8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".UTF-8"), 6);
        }
        else if (lcMonetary != null && lcMonetary.Contains(".utf8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".utf8"), 5);
        }
        if (lcMonetary != null && lcMonetary.Contains('_'))
        {
            lcMonetary = lcMonetary.Replace('_', '-');
        }
        if (lcMonetary != null && lcMonetary.Contains('@'))
        {
            lcMonetary = lcMonetary.Replace('@', '-');
        }
        var culture = new CultureInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
        var region = new RegionInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
        ReportedCurrencyString = $"{culture.NumberFormat.CurrencySymbol} ({region.ISOCurrencySymbol})";
    }

    /// <summary>
    /// Gets a culture to use for number strings based on AccountMetadata
    /// </summary>
    /// <param name="metadata">AccountMetadata</param>
    /// <returns>CultureInfo</returns>
    public static CultureInfo GetNumberCulture(AccountMetadata metadata)
    {
        var lcMonetary = Environment.GetEnvironmentVariable("LC_MONETARY");
        if (lcMonetary != null && lcMonetary.Contains(".UTF-8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".UTF-8"), 6);
        }
        else if (lcMonetary != null && lcMonetary.Contains(".utf8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".utf8"), 5);
        }
        if (lcMonetary != null && lcMonetary.Contains('_'))
        {
            lcMonetary = lcMonetary.Replace('_', '-');
        }
        if (lcMonetary != null && lcMonetary.Contains('@'))
        {
            lcMonetary = lcMonetary.Replace('@', '-');
        }
        var culture = new CultureInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
        var region = new RegionInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
        if (metadata.UseCustomCurrency)
        {
            culture.NumberFormat.CurrencySymbol = string.IsNullOrWhiteSpace(metadata.CustomCurrencySymbol) ? culture.NumberFormat.CurrencySymbol : metadata.CustomCurrencySymbol;
            culture.NumberFormat.NaNSymbol = string.IsNullOrWhiteSpace(metadata.CustomCurrencyCode) ? region.ISOCurrencySymbol : metadata.CustomCurrencyCode;
            culture.NumberFormat.CurrencyPositivePattern = metadata.CustomCurrencyAmountStyle ?? culture.NumberFormat.CurrencyPositivePattern;
            culture.NumberFormat.CurrencyDecimalSeparator = string.IsNullOrEmpty(metadata.CustomCurrencyDecimalSeparator) ? culture.NumberFormat.CurrencyDecimalSeparator : metadata.CustomCurrencyDecimalSeparator;
            culture.NumberFormat.NumberDecimalSeparator = string.IsNullOrEmpty(metadata.CustomCurrencyDecimalSeparator) ? culture.NumberFormat.NumberDecimalSeparator : metadata.CustomCurrencyDecimalSeparator;
            culture.NumberFormat.CurrencyGroupSeparator = string.IsNullOrEmpty(metadata.CustomCurrencyGroupSeparator) ? culture.NumberFormat.CurrencyGroupSeparator : metadata.CustomCurrencyGroupSeparator;
            culture.NumberFormat.NumberGroupSeparator = string.IsNullOrEmpty(metadata.CustomCurrencyGroupSeparator) ? culture.NumberFormat.NumberGroupSeparator : metadata.CustomCurrencyGroupSeparator;
            culture.NumberFormat.CurrencyDecimalDigits = metadata.CustomCurrencyDecimalDigits ?? culture.NumberFormat.CurrencyDecimalDigits;
            culture.NumberFormat.NumberDecimalDigits = metadata.CustomCurrencyDecimalDigits ?? culture.NumberFormat.CurrencyDecimalDigits;
        }
        else
        {
            culture.NumberFormat.NaNSymbol = region.ISOCurrencySymbol;
        }
        return culture;
    }
}
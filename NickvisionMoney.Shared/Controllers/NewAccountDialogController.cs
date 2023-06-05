using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when account currency is validated
/// </summary>
[Flags]
public enum CurrencyCheckStatus
{
    Valid = 1,
    EmptyCurrencySymbol = 2,
    InvalidCurrencySymbol = 4,
    EmptyCurrencyCode = 8,
    EmptyDecimalSeparator = 16,
    SameSeparators = 32,
    SameSymbolAndDecimalSeparator = 64,
    SameSymbolAndGroupSeparator = 128,
}

/// <summary>
/// A controller for a NewAccountDialog
/// </summary>
public class NewAccountDialogController
{
    /// <summary>
    /// The password of the new account
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// The folder to save the new account
    /// </summary>
    public string Folder { get; set; }
    /// <summary>
    /// The metadata represented by the controller
    /// </summary>
    public AccountMetadata Metadata { get; init; }
    
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    
    /// <summary>
    /// Constructs a NewAccountDialogController
    /// </summary>
    public NewAccountDialogController()
    {
        Password = null;
        Folder = "";
        Metadata = new AccountMetadata("", AccountType.Checking);
    }
    
    /// <summary>
    /// The system reported currency string (Ex: "$ (USD)")
    /// </summary>
    public string ReportedCurrencyString
    {
        get
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
            var culture = new CultureInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
            var region = new RegionInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
            return $"{culture.NumberFormat.CurrencySymbol} ({region.ISOCurrencySymbol})";
        }
    }
    
    /// <summary>
    /// Updates the Metadata object
    /// </summary>
    /// <param name="useCustom">Whether or not to use a custom currency</param>
    /// <param name="customSymbol">The new custom currency symbol</param>
    /// <param name="customCode">The new custom currency code</param>
    /// <param name="customDecimalSeparator">The new custom decimal separator</param>
    /// <param name="customGroupSeparator">The new custom group separator</param>
    /// <param name="customDecimalDigits">The new custom decimal digits number</param>
    /// <returns>CurrencyCheckStatus</returns>
    public CurrencyCheckStatus UpdateCurrency(bool useCustom, string? customSymbol, string? customCode, string? customDecimalSeparator, string? customGroupSeparator, uint customDecimalDigits)
    {
        CurrencyCheckStatus result = 0;
        if (useCustom && string.IsNullOrEmpty(customSymbol))
        {
            result |= CurrencyCheckStatus.EmptyCurrencySymbol;
        }
        decimal symbolAsNumber;
        if (useCustom && !string.IsNullOrEmpty(customSymbol) && Decimal.TryParse(customSymbol, out symbolAsNumber))
        {
            result |= CurrencyCheckStatus.InvalidCurrencySymbol;
        }
        if (useCustom && string.IsNullOrEmpty(customCode))
        {
            result |= CurrencyCheckStatus.EmptyCurrencyCode;
        }
        if (useCustom && string.IsNullOrEmpty(customDecimalSeparator))
        {
            result |= CurrencyCheckStatus.EmptyDecimalSeparator;
        }
        if (useCustom && !string.IsNullOrEmpty(customDecimalSeparator) && customDecimalSeparator == customGroupSeparator)
        {
            result |= CurrencyCheckStatus.SameSeparators;
        }
        if (useCustom && !string.IsNullOrEmpty(customDecimalSeparator) && customSymbol!.Contains(customDecimalSeparator))
        {
            result |= CurrencyCheckStatus.SameSymbolAndDecimalSeparator;
        }
        if (useCustom && !string.IsNullOrEmpty(customGroupSeparator) && customSymbol!.Contains(customGroupSeparator))
        {
            result |= CurrencyCheckStatus.SameSymbolAndGroupSeparator;
        }
        if (result != 0)
        {
            return result;
        }
        if (customSymbol != null && customSymbol.Length > 3)
        {
            customSymbol = customSymbol.Substring(0, 3);
        }
        if (customCode != null && customCode.Length > 3)
        {
            customCode = customCode.Substring(0, 3);
        }
        Metadata.UseCustomCurrency = useCustom;
        if (Metadata.UseCustomCurrency)
        {
            Metadata.CustomCurrencySymbol = customSymbol;
            Metadata.CustomCurrencyCode = customCode?.ToUpper();
            Metadata.CustomCurrencyDecimalSeparator = customDecimalSeparator;
            Metadata.CustomCurrencyGroupSeparator = customGroupSeparator;
            Metadata.CustomCurrencyDecimalDigits = (int?)customDecimalDigits;
        }
        else
        {
            Metadata.CustomCurrencySymbol = null;
            Metadata.CustomCurrencyCode = null;
            Metadata.CustomCurrencyDecimalSeparator = null;
            Metadata.CustomCurrencyGroupSeparator = null;
            Metadata.CustomCurrencyDecimalDigits = null;
        }
        return CurrencyCheckStatus.Valid;
    }
}
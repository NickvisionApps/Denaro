using Nickvision.Aura;
using NickvisionMoney.Shared.Models;
using System;
using System.Globalization;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when account metadata is validated
/// </summary>
[Flags]
public enum AccountMetadataCheckStatus
{
    Valid = 1,
    EmptyName = 2,
    EmptyCurrencySymbol = 4,
    InvalidCurrencySymbol = 8,
    EmptyCurrencyCode = 16,
    EmptyDecimalSeparator = 32,
    SameSeparators = 64,
    SameSymbolAndDecimalSeparator = 128,
    SameSymbolAndGroupSeparator = 256,
    NonMatchingPasswords = 512
}

/// <summary>
/// A controller for an AccountSettingsDialog
/// </summary>
public class AccountSettingsDialogController
{
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The metadata represented by the controller
    /// </summary>
    public AccountMetadata Metadata { get; init; }
    /// <summary>
    /// Whether or not the account is encrypted
    /// </summary>
    public bool IsEncrypted { get; init; }
    /// <summary>
    /// The new password for the account, if available
    /// </summary>
    public string? NewPassword { get; private set; }
    
    /// <summary>
    /// Strings to show for a custom currency's amount styles if available
    /// </summary>
    public string[] CustomCurrencyAmountStyleStrings => Metadata.CustomCurrencySymbol == null ? Array.Empty<string>() : new string[] { $"{Metadata.CustomCurrencySymbol}100", $"100{Metadata.CustomCurrencySymbol}", $"{Metadata.CustomCurrencySymbol} 100", $"100 {Metadata.CustomCurrencySymbol}" };

    /// <summary>
    /// Creates an AccountSettingsDialogController
    /// </summary>
    /// <param name="metadata">The AccountMetadata object represented by the controller</param>
    /// <param name="isEncrypted">Whether or not the account is encrypted</param>
    internal AccountSettingsDialogController(AccountMetadata metadata, bool isEncrypted)
    {
        Metadata = (AccountMetadata)metadata.Clone();
        IsEncrypted = isEncrypted;
        NewPassword = null;
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
    /// <param name="name">The new name of the account</param>
    /// <param name="type">The new type of the account</param>
    /// <param name="useCustom">Whether or not to use a custom currency</param>
    /// <param name="customSymbol">The new custom currency symbol</param>
    /// <param name="customCode">The new custom currency code</param>
    /// <param name="customAmountStyle">The new custom currency amount style</param>
    /// <param name="customDecimalSeparator">The new custom decimal separator</param>
    /// <param name="customGroupSeparator">The new custom group separator</param>
    /// <param name="customDecimalDigits">The new custom decimal digits number</param>
    /// <param name="defaultTransactionType">The new default transaction type</param>
    /// <param name="transactionReminder">The new reminder threshold for transactions</param>
    /// <param name="newPassword">The new password</param>
    /// <param name="confirmPassword">The new password confirmed</param>
    /// <returns>AccountMetadataCheckStatus</returns>
    public AccountMetadataCheckStatus UpdateMetadata(string name, AccountType type, bool useCustom, string? customSymbol, string? customCode, int? customAmountStyle, string? customDecimalSeparator, string? customGroupSeparator, int? customDecimalDigits, TransactionType defaultTransactionType, RemindersThreshold transactionReminder, string newPassword, string confirmPassword)
    {
        AccountMetadataCheckStatus result = 0;
        if (string.IsNullOrEmpty(name))
        {
            result |= AccountMetadataCheckStatus.EmptyName;
        }
        if (useCustom && string.IsNullOrEmpty(customSymbol))
        {
            result |= AccountMetadataCheckStatus.EmptyCurrencySymbol;
        }
        if (useCustom && !string.IsNullOrEmpty(customSymbol) && Decimal.TryParse(customSymbol, out _))
        {
            result |= AccountMetadataCheckStatus.InvalidCurrencySymbol;
        }
        if (useCustom && string.IsNullOrEmpty(customCode))
        {
            result |= AccountMetadataCheckStatus.EmptyCurrencyCode;
        }
        if (useCustom && string.IsNullOrEmpty(customDecimalSeparator))
        {
            result |= AccountMetadataCheckStatus.EmptyDecimalSeparator;
        }
        if (useCustom && !string.IsNullOrEmpty(customDecimalSeparator) && customDecimalSeparator == customGroupSeparator)
        {
            result |= AccountMetadataCheckStatus.SameSeparators;
        }
        if (useCustom && !string.IsNullOrEmpty(customDecimalSeparator) && customSymbol!.Contains(customDecimalSeparator))
        {
            result |= AccountMetadataCheckStatus.SameSymbolAndDecimalSeparator;
        }
        if (useCustom && !string.IsNullOrEmpty(customGroupSeparator) && customSymbol!.Contains(customGroupSeparator))
        {
            result |= AccountMetadataCheckStatus.SameSymbolAndGroupSeparator;
        }
        if (newPassword != confirmPassword)
        {
            result |= AccountMetadataCheckStatus.NonMatchingPasswords;
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
        Metadata.Name = name;
        Metadata.AccountType = type;
        Metadata.UseCustomCurrency = useCustom;
        if (Metadata.UseCustomCurrency)
        {
            Metadata.CustomCurrencySymbol = customSymbol;
            Metadata.CustomCurrencyCode = customCode?.ToUpper();
            Metadata.CustomCurrencyAmountStyle = customAmountStyle;
            Metadata.CustomCurrencyDecimalSeparator = customDecimalSeparator;
            Metadata.CustomCurrencyGroupSeparator = customGroupSeparator;
            Metadata.CustomCurrencyDecimalDigits = customDecimalDigits;
        }
        else
        {
            Metadata.CustomCurrencySymbol = null;
            Metadata.CustomCurrencyCode = null;
            Metadata.CustomCurrencyAmountStyle = null;
            Metadata.CustomCurrencyDecimalSeparator = null;
            Metadata.CustomCurrencyGroupSeparator = null;
            Metadata.CustomCurrencyDecimalDigits = null;
        }
        Metadata.DefaultTransactionType = defaultTransactionType;
        Metadata.TransactionRemindersThreshold = transactionReminder;
        NewPassword = string.IsNullOrEmpty(newPassword) ? (NewPassword == "" ? "" : null) : newPassword;
        return AccountMetadataCheckStatus.Valid;
    }

    /// <summary>
    /// Sets the password to be removed from the account
    /// </summary>
    public void SetRemovePassword() => NewPassword = "";
}

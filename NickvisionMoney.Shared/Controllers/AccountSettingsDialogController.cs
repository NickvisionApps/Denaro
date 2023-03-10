using NickvisionMoney.Shared.Helpers;
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
    EmptyCurrencyCode = 8,
    EmptyDecimalSeparator = 16,
    SameSeparators = 32,
    NonMatchingPasswords = 64
}

/// <summary>
/// A controller for an AccountSettingsDialog
/// </summary>
public class AccountSettingsDialogController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The metadata represented by the controller
    /// </summary>
    public AccountMetadata Metadata { get; init; }
    /// <summary>
    /// Whether or not the dialog should be used for necessary account setup
    /// </summary>
    public bool NeedsSetup { get; init; }
    /// <summary>
    /// Whether or not the account is encrypted
    /// </summary>
    public bool IsEncrypted { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// The new password for the account, if available
    /// </summary>
    public string? NewPassword { get; private set; }

    /// <summary>
    /// Creates an AccountSettingsDialogController
    /// </summary>
    /// <param name="metadata">The AccountMetadata object represented by the controller</param>
    /// <param name="needsSetup">Whether or not the dialog should be used for necessary account setup</param>
    /// <param name="isEncrypted">Whether or not the account is encrypted</param>
    /// <param name="localizer">The Localizer of the app</param>
    internal AccountSettingsDialogController(AccountMetadata metadata, bool needsSetup, bool isEncrypted, Localizer localizer)
    {
        Localizer = localizer;
        Metadata = (AccountMetadata)metadata.Clone();
        NeedsSetup = needsSetup;
        IsEncrypted = isEncrypted;
        Accepted = false;
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
    /// Gets a color for an account type
    /// </summary>
    /// <param name="accountType">The account type</param>
    /// <returns>The rgb color for the account type</returns>
    public string GetColorForAccountType(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Checking => Configuration.Current.AccountCheckingColor,
            AccountType.Savings => Configuration.Current.AccountSavingsColor,
            AccountType.Business => Configuration.Current.AccountBusinessColor,
            _ => Configuration.Current.AccountSavingsColor
        };
    }

    /// <summary>
    /// Updates the Metadata object
    /// </summary>
    /// <param name="name">The new name of the account</param>
    /// <param name="type">The new type of the account</param>
    /// <param name="useCustom">Whether or not to use a custom currency</param>
    /// <param name="customSymbol">The new custom currency symbol</param>
    /// <param name="customCode">The new custom currency code</param>
    /// <param name="customDecimalSeparator">The new custom decimal separator</param>
    /// <param name="customGroupSeparator">The new custom group separator</param>
    /// <param name="customDecimalDigits">The new custom decimal digits number</param>
    /// <param name="defaultTransactionType">The new default transaction type</param>
    /// <param name="newPassword">The new password</param>
    /// <param name="confirmPassword">The new password confirmed</param>
    /// <returns></returns>
    public AccountMetadataCheckStatus UpdateMetadata(string name, AccountType type, bool useCustom, string? customSymbol, string? customCode, string? customDecimalSeparator, string? customGroupSeparator, uint customDecimalDigits, TransactionType defaultTransactionType, string newPassword, string confirmPassword)
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
        Metadata.DefaultTransactionType = defaultTransactionType;
        NewPassword = string.IsNullOrEmpty(newPassword) ? (NewPassword == "" ? "" : null) : newPassword;
        return AccountMetadataCheckStatus.Valid;
    }

    /// <summary>
    /// Sets the password to be removed from the account
    /// </summary>
    public void SetRemovePassword() => NewPassword = "";
}

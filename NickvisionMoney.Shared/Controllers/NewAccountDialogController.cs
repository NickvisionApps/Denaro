using Nickvision.Aura;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when the account name is validated
/// </summary>
[Flags]
public enum NameCheckStatus
{
    Valid = 1,
    AlreadyOpen = 2,
    Exists = 4
}

/// <summary>
/// Statuses for when the account currency is validated
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
    private List<string> _openAccountPaths;

    /// <summary>
    /// The metadata represented by the controller
    /// </summary>
    public AccountMetadata Metadata { get; init; }
    /// <summary>
    /// The password of the new account
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// The folder to save the new account
    /// </summary>
    public string Folder { get; set; }
    /// <summary>
    /// Whether or not to overwrite existing accounts
    /// </summary>
    public bool OverwriteExisting { get; set; }
    /// <summary>
    /// A file to use to import data from
    /// </summary>
    public string ImportFile { get; set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The path of the new account
    /// </summary>
    public string Path => $"{Folder}{System.IO.Path.DirectorySeparatorChar}{Metadata.Name}.nmoney";
    /// <summary>
    /// Strings to show for a custom currency's amount styles if available
    /// </summary>
    public string[] CustomCurrencyAmountStyleStrings => Metadata.CustomCurrencySymbol == null ? Array.Empty<string>() : new string[] { $"{Metadata.CustomCurrencySymbol}100", $"100{Metadata.CustomCurrencySymbol}", $"{Metadata.CustomCurrencySymbol} 100", $"100 {Metadata.CustomCurrencySymbol}" };

    /// <summary>
    /// Constructs a NewAccountDialogController
    /// </summary>
    /// <param name="openAccountPaths">The list of open account paths</param>
    public NewAccountDialogController(IEnumerable<string> openAccountPaths)
    {
        _openAccountPaths = openAccountPaths.ToList();
        Metadata = new AccountMetadata("", AccountType.Checking);
        Password = null;
        Folder = "";
        OverwriteExisting = false;
        ImportFile = "";
    }

    /// <summary>
    /// Updates the account name
    /// </summary>
    /// <param name="name">The new name</param>
    /// <returns>NameCheckStatus</returns>
    public NameCheckStatus UpdateName(string name)
    {
        var tempPath = $"{Folder}{System.IO.Path.DirectorySeparatorChar}{name}.nmoney";
        if (_openAccountPaths.Contains(tempPath))
        {
            return NameCheckStatus.AlreadyOpen;
        }
        else if (File.Exists(tempPath) && !OverwriteExisting)
        {
            return NameCheckStatus.Exists;
        }
        Metadata.Name = name;
        return NameCheckStatus.Valid;
    }

    /// <summary>
    /// Updates the Metadata object
    /// </summary>
    /// <param name="useCustom">Whether or not to use a custom currency</param>
    /// <param name="customSymbol">The new custom currency symbol</param>
    /// <param name="customCode">The new custom currency code</param>
    /// <param name="customAmountStyle">The new custom currency amount style</param>
    /// <param name="customDecimalSeparator">The new custom decimal separator</param>
    /// <param name="customGroupSeparator">The new custom group separator</param>
    /// <param name="customDecimalDigits">The new custom decimal digits number</param>
    /// <returns>CurrencyCheckStatus</returns>
    public CurrencyCheckStatus UpdateCurrency(bool useCustom, string? customSymbol, string? customCode, int? customAmountStyle, string? customDecimalSeparator, string? customGroupSeparator, int? customDecimalDigits)
    {
        CurrencyCheckStatus result = 0;
        if (useCustom && string.IsNullOrWhiteSpace(customSymbol))
        {
            result |= CurrencyCheckStatus.EmptyCurrencySymbol;
        }
        if (useCustom && !string.IsNullOrWhiteSpace(customSymbol) && Decimal.TryParse(customSymbol, out _))
        {
            result |= CurrencyCheckStatus.InvalidCurrencySymbol;
        }
        if (useCustom && string.IsNullOrWhiteSpace(customCode))
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
        return CurrencyCheckStatus.Valid;
    }
}
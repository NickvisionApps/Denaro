using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System.Globalization;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when account metadata is validated
/// </summary>
public enum AccountMetadataCheckStatus
{
    Valid = 0,
    EmptyName
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
    /// Whether or not the dialog should be used for first time account setup
    /// </summary>
    public bool IsFirstTimeSetup { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }

    /// <summary>
    /// The system reported currency string (Ex: "$ (USD)")
    /// </summary>
    public string ReportedCurrencyString => $"{CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol} ({RegionInfo.CurrentRegion.ISOCurrencySymbol})";

    /// <summary>
    /// Creates an AccountSettingsDialogController
    /// </summary>
    /// <param name="metadata">The AccountMetadata object represented by the controller</param>
    /// <param name="isFirstTimeSetup">Whether or not the dialog should be used for first time account setup</param>
    /// <param name="localizer">The Localizer of the app</param>
    public AccountSettingsDialogController(AccountMetadata metadata, bool isFirstTimeSetup, Localizer localizer)
    {
        Localizer = localizer;
        Metadata = metadata;
        IsFirstTimeSetup = isFirstTimeSetup;
        Accepted = false;
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
            AccountType.Checking => "rgb(53,132,228)",
            AccountType.Savings => "rgb(38,162,105)",
            AccountType.Business => "rgb(99,69,44)",
            _ => "rgb(53,132,228)"
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
    /// <param name="defaultTransactionType">The new default transaction type</param>
    /// <returns></returns>
    public AccountMetadataCheckStatus UpdateMetadata(string name, AccountType type, bool useCustom, string? customSymbol, string? customCode, TransactionType defaultTransactionType)
    {
        if(string.IsNullOrEmpty(name))
        {
            return AccountMetadataCheckStatus.EmptyName;
        }
        Metadata.Name = name;
        Metadata.AccountType = type;
        Metadata.UseCustomCurrency = useCustom;
        Metadata.CustomCurrencySymbol = customSymbol;
        Metadata.CustomCurrencyCode = customCode;
        Metadata.DefaultTransactionType = defaultTransactionType;
        return AccountMetadataCheckStatus.Valid;
    }
}

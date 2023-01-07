using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;

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
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }

    /// <summary>
    /// Creates an AccountSettingsDialogController
    /// </summary>
    /// <param name="metadata">The AccountMetadata object represented by the controller</param>
    /// <param name="localizer">The Localizer of the app</param>
    public AccountSettingsDialogController(AccountMetadata metadata, Localizer localizer)
    {
        Localizer = localizer;
        Metadata = metadata;
        Accepted = false;
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
    /// <param name="showGroupsList">Whether or not to show the groups section on the account view</param>
    /// <returns></returns>
    public AccountMetadataCheckStatus UpdateMetadata(string name, AccountType type, bool useCustom, string? customSymbol, string? customCode, TransactionType defaultTransactionType, bool showGroupsList)
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
        Metadata.ShowGroupsList = showGroupsList;
        return AccountMetadataCheckStatus.Valid;
    }
}

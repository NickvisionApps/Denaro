using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NickvisionMoney.Shared.Controllers;


/// <summary>
/// Statuses for when a transfer is validated
/// </summary>
[Flags]
public enum TransferCheckStatus
{
    Valid = 1,
    InvalidDestPath = 2,
    InvalidAmount = 4,
    InvalidConversionRate = 8
}

/// <summary>
/// A controller for a TransferDialog
/// </summary>
public class TransferDialogController
{
    private readonly decimal _sourceAmount;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transfer represented by the controller
    /// </summary>
    public Transfer Transfer { get; init; }
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<RecentAccount> RecentAccounts { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString { get; init; }
    /// <summary>
    /// The currency code of the destination account, if available
    /// </summary>
    public string? DestinationCurrencyCode { get; private set; }

    /// <summary>
    /// The currency code of the source account
    /// </summary>
    public string SourceCurrencyCode => CultureForNumberString.NumberFormat.NaNSymbol;

    /// <summary>
    /// Constructs a TransferDialogController
    /// </summary>
    /// <param name="transfer">The Transfer model</param>
    /// <param name="sourceAmount">The amount of the source account</param>
    /// <param name="recentAccounts">The recent accounts of the app</param>
    /// <param name="culture">The CultureInfo to use for the amount string</param>
    /// <param name="localizer">The Localizer for the app</param>
    internal TransferDialogController(Transfer transfer, decimal sourceAmount, List<RecentAccount> recentAccounts, CultureInfo culture, Localizer localizer)
    {
        _sourceAmount = sourceAmount;
        Localizer = localizer;
        Transfer = transfer;
        RecentAccounts = new List<RecentAccount>();
        foreach (var account in recentAccounts)
        {
            if (account.Path != Transfer.SourceAccountPath)
            {
                RecentAccounts.Add(account);
            }
        }
        Accepted = false;
        CultureForNumberString = culture;
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
    /// Updates the Transfer object
    /// </summary>
    /// <param name="destPath">The new path of the destination account</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="conversionRateString">The conversion rate string for different currencies</param>
    /// <returns>TransferCheckStatus</returns>
    public TransferCheckStatus UpdateTransfer(string destPath, string amountString, string? conversionRateString)
    {
        TransferCheckStatus result = 0;
        var amount = 0m;
        var conversionRate = 0m;
        if(string.IsNullOrEmpty(destPath) || !Path.Exists(destPath) || Path.GetExtension(destPath) != ".nmoney" || Transfer.SourceAccountPath == destPath)
        {
            result |= TransferCheckStatus.InvalidDestPath;
        }
        try
        {
            amount = decimal.Parse(amountString, NumberStyles.Currency, CultureForNumberString);
        }
        catch
        {
            result |= TransferCheckStatus.InvalidAmount;
        }
        var lcMonetary = Environment.GetEnvironmentVariable("LC_MONETARY");
        if (lcMonetary != null && lcMonetary.Contains(".UTF-8"))
        {
            lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".UTF-8"), 6);
        }
        if (lcMonetary != null && lcMonetary.Contains('_'))
        {
            lcMonetary = lcMonetary.Replace('_', '-');
        }
        var region = new RegionInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
        var destMetadata = AccountMetadata.LoadFromAccountFile(destPath)!;
        DestinationCurrencyCode = destMetadata.UseCustomCurrency ? destMetadata.CustomCurrencyCode : region.ISOCurrencySymbol;
        if(SourceCurrencyCode != DestinationCurrencyCode)
        {
            try
            {
                conversionRate = decimal.Parse(amountString, NumberStyles.Number, CultureForNumberString);
            }
            catch
            {
                result |= TransferCheckStatus.InvalidConversionRate;
            }
        }
        else
        {
            conversionRate = 1.0m;
        }
        if (amount <= 0 || amount > _sourceAmount)
        {
            result |= TransferCheckStatus.InvalidAmount;
        }
        if(result != 0)
        {
            return result;
        }
        Transfer.DestinationAccountPath = destPath;
        Transfer.DestinationAccountName = AccountMetadata.LoadFromAccountFile(destPath)!.Name;
        Transfer.SourceAmount = amount;
        Transfer.ConversionRate = conversionRate;
        return TransferCheckStatus.Valid;
    }
}

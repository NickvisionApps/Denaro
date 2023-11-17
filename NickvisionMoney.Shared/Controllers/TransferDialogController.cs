using Nickvision.Aura;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transfer is validated
/// </summary>
[Flags]
public enum TransferCheckStatus
{
    Valid = 1,
    InvalidDestPath = 2,
    DestAccountRequiresPassword = 4,
    DestAccountPasswordInvalid = 8,
    InvalidAmount = 16,
    InvalidConversionRate = 32
}

/// <summary>
/// A controller for a TransferDialog
/// </summary>
public class TransferDialogController
{
    private readonly decimal _sourceAmount;
    private string? _previousDestPath;
    private AccountMetadata? _previousDestMetadata;

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The transfer represented by the controller
    /// </summary>
    public Transfer Transfer { get; init; }
    /// <summary>
    /// The list of recent accounts
    /// </summary>
    public List<RecentAccount> RecentAccounts { get; init; }
    /// <summary>
    /// The CultureInfo to use when displaying a source number string
    /// </summary>
    public CultureInfo CultureForSourceNumberString { get; init; }
    /// <summary>
    /// The CultureInfo to use when displaying a destination number string
    /// </summary>
    public CultureInfo? CultureForDestNumberString { get; private set; }

    /// <summary>
    /// The currency code of the source account
    /// </summary>
    public string SourceCurrencyCode => CultureForSourceNumberString.NumberFormat.NaNSymbol;
    /// <summary>
    /// The currency code of the destination account, if available
    /// </summary>
    public string? DestinationCurrencyCode => CultureForDestNumberString == null ? null : CultureForDestNumberString.NumberFormat.NaNSymbol;
    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Configuration.Current.UseNativeDigits;
    /// <summary>
    /// Decimal Separator Inserting
    /// <summary>
    public InsertSeparator InsertSeparator => Configuration.Current.InsertSeparator;

    /// <summary>
    /// Constructs a TransferDialogController
    /// </summary>
    /// <param name="transfer">The Transfer model</param>
    /// <param name="sourceAmount">The amount of the source account</param>
    /// <param name="recentAccounts">The recent accounts of the app</param>
    /// <param name="culture">The CultureInfo to use for the amount string</param>
    internal TransferDialogController(Transfer transfer, decimal sourceAmount, List<RecentAccount> recentAccounts, CultureInfo culture)
    {
        _sourceAmount = sourceAmount;
        _previousDestPath = null;
        _previousDestMetadata = null;
        Transfer = transfer;
        RecentAccounts = new List<RecentAccount>();
        foreach (var account in recentAccounts)
        {
            if (account.Path != Transfer.SourceAccountPath)
            {
                RecentAccounts.Add(account);
            }
        }
        CultureForSourceNumberString = culture;
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
    /// Gets the conversion rate from the source currency to the destination currency using the internet
    /// </summary>
    /// <returns>(string Source, string Destination)</returns>
    public async Task<(string Source, string Destination)> GetConversionRateOnlineAsync()
    {
        if (string.IsNullOrWhiteSpace(DestinationCurrencyCode))
        {
            return ("", "");
        }
        var rates = await CurrencyConversionService.GetConversionRatesAsync(SourceCurrencyCode);
        if (rates != null && rates.ContainsKey(DestinationCurrencyCode))
        {
            return (rates[SourceCurrencyCode].ToAmountString(CultureForSourceNumberString, UseNativeDigits, false), rates[DestinationCurrencyCode].ToAmountString(CultureForDestNumberString!, UseNativeDigits, false, true));
        }
        return ("", "");
    }

    /// <summary>
    /// Updates the Transfer object
    /// </summary>
    /// <param name="destPath">The new path of the destination account</param>
    /// <param name="destPassword">The password for the destination account (if needed)</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="sourceConversionAmountString">The source currency conversion amount</param>
    /// <param name="destConversionAmountString">The destination currency conversion amount</param>
    /// <returns>TransferCheckStatus</returns>
    public TransferCheckStatus UpdateTransfer(string destPath, string? destPassword, string amountString, string sourceConversionAmountString, string destConversionAmountString)
    {
        TransferCheckStatus result = 0;
        var amount = 0m;
        var conversionRate = 0m;
        if (string.IsNullOrWhiteSpace(destPath) || !Path.Exists(destPath) || Path.GetExtension(destPath).ToLower() != ".nmoney" || Transfer.SourceAccountPath == destPath)
        {
            result |= TransferCheckStatus.InvalidDestPath;
        }
        else
        {
            if (new Account(destPath).IsEncrypted && string.IsNullOrEmpty(destPassword))
            {
                result |= TransferCheckStatus.DestAccountRequiresPassword;
            }
            else
            {
                if (_previousDestPath != destPath)
                {
                    _previousDestPath = destPath;
                    _previousDestMetadata = null;
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
                if (lcMonetary != null && lcMonetary.Contains('@'))
                {
                    lcMonetary = lcMonetary.Replace('@', '-');
                }
                CultureForDestNumberString = new CultureInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
                var destRegion = new RegionInfo(!string.IsNullOrWhiteSpace(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
                if (_previousDestMetadata == null)
                {
                    _previousDestMetadata = AccountMetadata.LoadFromAccountFile(destPath, destPassword)!;
                }
                if (_previousDestMetadata == null)
                {
                    result |= TransferCheckStatus.DestAccountPasswordInvalid;
                }
                else
                {
                    Transfer.DestinationAccountPassword = destPassword;
                    if (_previousDestMetadata.UseCustomCurrency)
                    {
                        CultureForDestNumberString.NumberFormat.CurrencySymbol = _previousDestMetadata.CustomCurrencySymbol ?? CultureForDestNumberString.NumberFormat.CurrencySymbol;
                        CultureForDestNumberString.NumberFormat.NaNSymbol = _previousDestMetadata.CustomCurrencyCode ?? destRegion.ISOCurrencySymbol;
                    }
                    else
                    {
                        CultureForDestNumberString.NumberFormat.NaNSymbol = destRegion.ISOCurrencySymbol;
                    }
                    if (SourceCurrencyCode != DestinationCurrencyCode)
                    {
                        try
                        {
                            conversionRate = decimal.Parse(sourceConversionAmountString.ReplaceNativeDigits(CultureForSourceNumberString), NumberStyles.Number, CultureForSourceNumberString) / decimal.Parse(destConversionAmountString.ReplaceNativeDigits(CultureForSourceNumberString), NumberStyles.Number, CultureForSourceNumberString);
                            if (conversionRate == 0)
                            {
                                throw new ArgumentException();
                            }
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
                }
            }
        }
        try
        {
            amount = decimal.Parse(amountString.ReplaceNativeDigits(CultureForSourceNumberString), NumberStyles.Currency, CultureForSourceNumberString);
        }
        catch
        {
            result |= TransferCheckStatus.InvalidAmount;
        }
        if (amount <= 0 || amount > _sourceAmount)
        {
            result |= TransferCheckStatus.InvalidAmount;
        }
        if (result != 0)
        {
            return result;
        }
        Transfer.DestinationAccountPath = destPath;
        Transfer.DestinationAccountName = _previousDestMetadata!.Name;
        Transfer.SourceAmount = amount;
        Transfer.ConversionRate = conversionRate;
        return TransferCheckStatus.Valid;
    }
}
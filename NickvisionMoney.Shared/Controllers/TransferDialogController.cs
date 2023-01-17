using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
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
    InvalidAmount = 4
}

/// <summary>
/// A controller for a TransferDialog
/// </summary>
public class TransferDialogController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transfer represented by the controller
    /// </summary>
    public Transfer Transfer { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString { get; init; }

    /// <summary>
    /// Constructs a TransferDialogController
    /// </summary>
    /// <param name="transfer">The Transfer model</param>
    /// <param name="culture">The CultureInfo to use for the amount string</param>
    /// <param name="localizer">The Localizer for the app</param>
    internal TransferDialogController(Transfer transfer, CultureInfo culture, Localizer localizer)
    {
        Localizer = localizer;
        Transfer = transfer;
        Accepted = false;
        CultureForNumberString = culture;
    }

    /// <summary>
    /// Updates the Transfer object
    /// </summary>
    /// <param name="destPath">The new path of the destination account</param>
    /// <param name="amountString">The new amount string</param>
    /// <returns>TransferCheckStatus</returns>
    public TransferCheckStatus UpdateTransfer(string destPath, string amountString)
    {
        TransferCheckStatus result = 0;
        var amount = 0m;
        if(string.IsNullOrEmpty(destPath) || !Path.Exists(destPath) || Transfer.SourceAccountPath == destPath)
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
        if (amount <= 0)
        {
            result |= TransferCheckStatus.InvalidAmount;
        }
        if(result != 0)
        {
            return result;
        }
        Transfer.DestinationAccountPath = destPath;
        Transfer.Amount = amount;
        return TransferCheckStatus.Valid;
    }
}

using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transaction is validated
/// </summary>
public enum TransactionCheckStatus
{
    Valid = 0,
    EmptyDescription,
    InvalidAmount
}

/// <summary>
/// A controller for a TransactionDialog
/// </summary>
public class TransactionDialogController
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
    /// <summary>
    /// The groups in the account
    /// </summary>
    public Dictionary<uint, string> Groups { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// A default color for the transaction
    /// </summary>
    public string TransactionDefaultColor { get; init; }

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="transaction">The Transaction object represented by the controller</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="localizer">The Localizer of the app</param>
    public TransactionDialogController(Transaction transaction, Dictionary<uint, string> groups, string transactionDefaultColor, Localizer localizer)
    {
        Localizer = localizer;
        Transaction = transaction;
        Groups = groups;
        Accepted = false;
        TransactionDefaultColor = transactionDefaultColor;
    }

    /// <summary>
    /// Updates the Transaction object
    /// </summary>
    /// <param name="date">The new DateOnly object</param>
    /// <param name="description">The new description</param>
    /// <param name="type">The new TransactionType</param>
    /// <param name="repeat">The new TransactionRepeatInterval</param>
    /// <param name="groupName">The new Group name</param>
    /// <param name="rgba">The new rgba string</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="receipt">The new receipt image</param>
    /// <returns>TransactionCheckStatus</returns>
    public TransactionCheckStatus UpdateTransaction(DateOnly date, string description, TransactionType type, TransactionRepeatInterval repeat, string groupName, string rgba, string amountString, Image? receipt)
    {
        var amount = 0m;
        if(string.IsNullOrEmpty(description))
        {
            return TransactionCheckStatus.EmptyDescription;
        }
        try
        {
            amount = decimal.Parse(amountString, NumberStyles.Currency);
        }
        catch
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        if (amount <= 0)
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        Transaction.RepeatInterval = repeat;
        Transaction.Amount = amount;
        Transaction.GroupId = groupName == "Ungrouped" ? -1 : (int)Groups.FirstOrDefault(x => x.Value == groupName).Key;
        Transaction.RGBA = rgba;
        Transaction.Receipt = receipt;
        return TransactionCheckStatus.Valid;
    }
}

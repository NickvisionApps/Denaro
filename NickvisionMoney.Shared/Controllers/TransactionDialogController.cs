using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System;
using System.Collections.Generic;

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
    private Dictionary<uint, Group> _groups;

    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
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
    public TransactionDialogController(Transaction transaction, Dictionary<uint, Group> groups, string transactionDefaultColor, Localizer localizer)
    {
        _groups = groups;
        Localizer = localizer;
        Transaction = transaction;
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
    /// <param name="group">The new Group object</param>
    /// <param name="rgba">The new rgba string</param>
    /// <param name="amount">The new amount</param>
    /// <returns>TransactionCheckStatus</returns>
    public TransactionCheckStatus UpdateTransaction(DateOnly date, string description, TransactionType type, TransactionRepeatInterval repeat, Group group, string rgba, decimal amount)
    {
        if(string.IsNullOrEmpty(description))
        {
            return TransactionCheckStatus.EmptyDescription;
        }
        if(amount <= 0)
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        Transaction.RepeatInterval = repeat;
        Transaction.Amount = amount;
        Transaction.GroupId = group.Id == 0 ? -1 : (int)group.Id;
        Transaction.RGBA = rgba;
        return TransactionCheckStatus.Valid;
    }
}

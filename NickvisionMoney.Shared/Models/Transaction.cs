using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// Types of a transaction
/// </summary>
public enum TransactionType
{
    Income = 0,
    Expense
}

/// <summary>
/// Repeat intervals for a transaction
/// </summary>
public enum TransactionRepeatInterval
{
    Never = 0,
    Daily,
    Weekly,
    Biweekly = 7,
    Monthly = 3,
    Quarterly,
    Yearly,
    Biyearly
}

/// <summary>
/// A model of a transaction
/// </summary>
public class Transaction : ICloneable, IComparable<Transaction>, IDisposable, IEquatable<Transaction>
{
    private bool _disposed;
    private int _groupId;

    /// <summary>
    /// The Id of the transaction
    /// </summary>
    public uint Id { get; init; }
    /// <summary>
    /// The date of the transaction
    /// </summary>
    public DateOnly Date { get; set; }
    /// <summary>
    /// The description of the transaction
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The type of the transaction
    /// </summary>
    public TransactionType Type { get; set; }
    /// <summary>
    /// The repeat inerval of the transaction
    /// </summary>
    public TransactionRepeatInterval RepeatInterval { get; set; }
    /// <summary>
    /// The amount of the transaction
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// The RGBA color of the transaction
    /// </summary>
    public string RGBA { get; set; }
    /// <summary>
    /// Whether to use group color for transaction
    /// </summary>
    public bool UseGroupColor { get; set; }
    /// <summary>
    /// The receipt image for the transaction
    /// </summary>
    public Image? Receipt { get; set; }
    /// <summary>
    /// The id of the transaction to repeat from (or -1 for non repeat transaction, 0 for original repeat transaction)
    /// </summary>
    public int RepeatFrom { get; set; }
    /// <summary>
    /// The date of when to end the repeat sequence
    /// </summary>
    public DateOnly? RepeatEndDate { get; set; }
    /// <summary>
    /// A tags list for the Transaction
    /// </summary>
    public List<string> Tags { get; set; }
    /// <summary>
    /// The notes for the transaction
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// Constructs a Transaction
    /// </summary>
    /// <param name="id">The id of the transaction</param>
    public Transaction(uint id = 0)
    {
        _disposed = false;
        Id = id;
        Date = DateOnly.FromDateTime(DateTime.Today);
        Description = "";
        Type = TransactionType.Income;
        RepeatInterval = TransactionRepeatInterval.Never;
        Amount = 0m;
        GroupId = -1;
        RGBA = "rgb(0,0,0)";
        UseGroupColor = true;
        Receipt = null;
        RepeatFrom = -1;
        RepeatEndDate = null;
        Tags = new List<string>();
        Notes = "";
    }

    /// <summary>
    /// Finalizes the Transaction
    /// </summary>
    ~Transaction() => Dispose(false);

    /// <summary>
    /// The group id of the transaction
    /// </summary>
    public int GroupId
    {
        get => _groupId;

        set
        {
            if (value <= 0)
            {
                value = -1;
            }
            _groupId = value;
        }
    }

    /// <summary>
    /// Frees resources used by the Transaction object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the Transaction object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Receipt?.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Clones the transaction
    /// </summary>
    /// <returns>A new transaction</returns>
    public object Clone()
    {
        return new Transaction(Id)
        {
            Date = Date,
            Description = Description,
            Type = Type,
            RepeatInterval = RepeatInterval,
            Amount = Amount,
            GroupId = GroupId,
            RGBA = RGBA,
            UseGroupColor = UseGroupColor,
            Receipt = Receipt?.Clone((x) => { }) ?? null,
            RepeatFrom = RepeatFrom,
            RepeatEndDate = RepeatEndDate,
            Tags = Tags,
            Notes = Notes
        };
    }

    /// <summary>
    /// Compares this with other
    /// </summary>
    /// <param name="other">The Transaction object to compare to</param>
    /// <returns>-1 if this is less than other. 0 if this is equal to other. 1 if this is greater than other</returns>
    /// <exception cref="NullReferenceException">Thrown if other is null</exception>
    public int CompareTo(Transaction? other)
    {
        if (other == null)
        {
            throw new NullReferenceException();
        }
        if (this < other)
        {
            return -1;
        }
        if (this == other)
        {
            return 0;
        }
        return 1;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this Transaction
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Transaction toCompare)
        {
            return Id == toCompare.Id;
        }
        return false;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this Transaction
    /// </summary>
    /// <param name="obj">The Transaction? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(Transaction? obj) => Equals((object?)obj);

    /// <summary>
    /// Gets a hash code for the object
    /// </summary>
    /// <returns>The hash code for the object</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares two Transaction objects by ==
    /// </summary>
    /// <param name="a">The first Transaction object</param>
    /// <param name="b">The second Transaction object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(Transaction? a, Transaction? b) => a?.Id == b?.Id;

    /// <summary>
    /// Compares two Transaction objects by !=
    /// </summary>
    /// <param name="a">The first Transaction object</param>
    /// <param name="b">The second Transaction object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(Transaction? a, Transaction? b) => a?.Id != b?.Id;

    /// <summary>
    /// Compares two Transaction objects by >
    /// </summary>
    /// <param name="a">The first Transaction object</param>
    /// <param name="b">The second Transaction object</param>
    /// <returns>True if a > b, else false</returns>
    public static bool operator <(Transaction? a, Transaction? b) => a?.Id < b?.Id;

    /// <summary>
    /// Compares two Transaction objects by <
    /// </summary>
    /// <param name="a">The first Transaction object</param>
    /// <param name="b">The second Transaction object</param>
    /// <returns>True if a < b, else false</returns>
    public static bool operator >(Transaction? a, Transaction? b) => a?.Id > b?.Id;
}

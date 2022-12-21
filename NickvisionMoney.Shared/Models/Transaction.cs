using System;

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
    Monthly,
    Quarterly,
    Yearly,
    Biyearly
}

/// <summary>
/// A model of a transaction
/// </summary>
public class Transaction : IComparable<Transaction>
{
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
    /// Constructs a Transaction
    /// </summary>
    /// <param name="id">The id of the transaction</param>
    public Transaction(uint id = 0)
    {
        Id = id;
        Date = DateOnly.FromDateTime(DateTime.Today);
        Description = "";
        Type = TransactionType.Income;
        RepeatInterval = TransactionRepeatInterval.Never;
        Amount = 0m;
        GroupId = -1;
        RGBA = "";
    }

    /// <summary>
    /// The group id of the transaction
    /// </summary>
    public int GroupId
    {
        get => _groupId;

        set
        {
            if(value <= 0)
            {
                value = -1;
            }
            _groupId = value;
        }
    }

    /// <summary>
    /// Gets whether or not an object is equal to this Transaction
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if(obj is Transaction toCompare)
        {
            return Id == toCompare.Id;
        }
        return false;
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
        else if (this == other)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

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

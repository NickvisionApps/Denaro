using System;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of a transaction
/// </summary>
public class Group : ICloneable, IComparable<Group>, IEquatable<Group>
{
    private TransactionFilter _transactionFilter;
    /// <summary>
    /// The id of the group
    /// </summary>
    public uint Id { get; init; }
    /// <summary>
    /// The name of the group
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The description of the group
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// The balance of the group
    /// </summary>
    public decimal Balance { get; private set; } = 0m;
    /// <summary>
    /// The RGBA color of the group
    /// </summary>
    public string RGBA { get; set; }

    private Dictionary<uint, Transaction> Transactions { get; set; }

    /// <summary>
    /// Constructs a group
    /// </summary>
    /// <param name="id">The id of the group</param>
    public Group(uint id, TransactionFilter filter)
    {
        Id = id;
        _transactionFilter = filter;
        Name = "";
        Description = "";
        Balance = 0m;
        RGBA = "rgb(0,0,0)";
    }

    public void AddTransaction(Transaction transaction)
    {
        Transactions.Add(transaction.Id, transaction);
        UpdateBalance();
    }

    public void RemoveTransaction(uint id)
    {
        Transactions.Remove(id);
        UpdateBalance();
    }

    private void UpdateBalance()
    {
        var transactions = Transactions.Values.ToList();
        if (_transactionFilter == null)
        {
            Balance = transactions
                .Sum((t) => (t.Type == TransactionType.Income ? 1 : -1) * t.Amount);
        }
        else
        {
            _transactionFilter.Filter(transactions)
                .Sum((t) => (t.Type == TransactionType.Income ? 1 : -1) * t.Amount);
        }
    }

    /// <summary>
    /// Clones the group
    /// </summary>
    /// <returns>A new Group</returns>
    public object Clone()
    {
        return new Group(Id)
        {
            Name = Name,
            Description = Description,
            Balance = Balance,
            RGBA = RGBA
        };
    }

    /// <summary>
    /// Compares this with other
    /// </summary>
    /// <param name="other">The Group object to compare to</param>
    /// <returns>-1 if this is less than other. 0 if this is equal to other. 1 if this is greater than other</returns>
    /// <exception cref="NullReferenceException">Thrown if other is null</exception>
    public int CompareTo(Group? other)
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
    /// Gets whether or not an object is equal to this Group
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if equals, else false</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Group toCompare)
        {
            return Id == toCompare.Id;
        }
        return false;
    }

    /// <summary>
    /// Gets whether or not an object is equal to this Group
    /// </summary>
    /// <param name="obj">The Group? object to compare</param>
    /// <returns>True if equals, else false</returns>
    public bool Equals(Group? obj) => Equals((object?)obj);

    /// <summary>
    /// Gets a hash code for the object
    /// </summary>
    /// <returns>The hash code for the object</returns>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Compares two Group objects by ==
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a == b, else false</returns>
    public static bool operator ==(Group? a, Group? b) => a?.Id == b?.Id;

    /// <summary>
    /// Compares two Group objects by !=
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a != b, else false</returns>
    public static bool operator !=(Group? a, Group? b) => a?.Id != b?.Id;

    /// <summary>
    /// Compares two Group objects by >
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a > b, else false</returns>
    public static bool operator <(Group? a, Group? b) => a?.Name.CompareTo(b?.Name) == -1;

    /// <summary>
    /// Compares two Group objects by <
    /// </summary>
    /// <param name="a">The first Group object</param>
    /// <param name="b">The second Group object</param>
    /// <returns>True if a < b, else false</returns>
    public static bool operator >(Group? a, Group? b) => a?.Name.CompareTo(b?.Name) == 1;
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of an account
/// </summary>
public class Account : IDisposable
{
    private SqliteConnection _database { get; set; }

    /// <summary>
    /// The path of the account
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// A map of groups in the account
    /// </summary>
    public Dictionary<uint, Group> Groups { get; init; }
    /// <summary>
    /// A map of transactions in the account
    /// </summary>
    public Dictionary<uint, Transaction> Transactions { get; init; }

    /// <summary>
    /// Constructs an Account
    /// </summary>
    /// <param name="path">The path of the account</param>
    public Account(string path)
    {
        Path = path;
        Groups = new Dictionary<uint, Group>();
        Transactions = new Dictionary<uint, Transaction>();
        //Open Database
        _database = new SqliteConnection(Path);
        _database.Open();
        //Setup Tables
        var cmdTableGroups = _database.CreateCommand();
        cmdTableGroups.CommandText = "CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT)";
        cmdTableGroups.ExecuteNonQuery();
        var cmdTableTransactions = _database.CreateCommand();
        cmdTableTransactions.CommandText = "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT)";
        cmdTableTransactions.ExecuteNonQuery();
        try
        {
            var cmdTableTransactionsUpdate1 = _database.CreateCommand();
            cmdTableTransactionsUpdate1.CommandText = "ALTER TABLE transactions ADD COLUMN gid INTEGER";
            cmdTableTransactionsUpdate1.ExecuteNonQuery();
        }
        catch { }
        try
        {
            var cmdTableTransactionsUpdate2 = _database.CreateCommand();
            cmdTableTransactionsUpdate2.CommandText = "ALTER TABLE transactions ADD COLUMN rgba TEXT";
            cmdTableTransactionsUpdate2.ExecuteNonQuery();
        }
        catch { }
        //Get Groups
        var cmdQueryGroups = _database.CreateCommand();
        cmdQueryGroups.CommandText = "SELECT g.*, CAST(COALESCE(SUM(IIF(t.type=1, -t.amount, t.amount)), 0) AS TEXT) FROM groups g LEFT JOIN transactions t ON t.gid = g.id GROUP BY g.id;";
        using var readQueryGroups = cmdQueryGroups.ExecuteReader();
        while(readQueryGroups.Read())
        {
            var group = new Group((uint)readQueryGroups.GetInt32(0))
            {
                Name = readQueryGroups.GetString(1),
                Description = readQueryGroups.GetString(2),
                Balance = readQueryGroups.GetDecimal(3)
            };
            Groups.Add(group.Id, group);
        }
        //Get Transactions
        var cmdQueryTransactions = _database.CreateCommand();
        cmdQueryTransactions.CommandText = "SELECT * FROM transactions";
        using var readQueryTransactions = cmdQueryTransactions.ExecuteReader();
        while(readQueryTransactions.Read())
        {
            var transaction = new Transaction((uint)readQueryTransactions.GetInt32(0))
            {
                Date = DateOnly.Parse(readQueryTransactions.GetString(1)),
                Description = readQueryTransactions.GetString(2),
                Type = (TransactionType)readQueryTransactions.GetInt32(3),
                RepeatInterval = (TransactionRepeatInterval)readQueryTransactions.GetInt32(4),
                Amount = readQueryTransactions.GetDecimal(5),
                GroupId = readQueryTransactions.GetInt32(6)
            };
            var rgba = readQueryTransactions.GetString(7);
            if (!string.IsNullOrEmpty(rgba))
            {
                transaction.RGBA = rgba;
            }
            Transactions.Add(transaction.Id, transaction);
        }
        //Repeat Needed Transactions
        var transactions = Transactions.Values.ToList();
        foreach(var transaction in transactions) 
        { 
            if(transaction.RepeatInterval != TransactionRepeatInterval.Never)
            {
                var repeatNeeded = false;
                if(transaction.RepeatInterval == TransactionRepeatInterval.Daily)
                {
                    if(DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddDays(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if(transaction.RepeatInterval == TransactionRepeatInterval.Weekly)
                {
                    if (DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddDays(7))
                    {
                        repeatNeeded = true;
                    }
                }
                else if(transaction.RepeatInterval == TransactionRepeatInterval.Monthly)
                {
                    if (DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddMonths(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if(transaction.RepeatInterval == TransactionRepeatInterval.Quarterly)
                {
                    if (DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddMonths(4))
                    {
                        repeatNeeded = true;
                    }
                }
                else if(transaction.RepeatInterval == TransactionRepeatInterval.Yearly)
                {
                    if (DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddYears(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Biyearly)
                {
                    if (DateOnly.FromDateTime(DateTime.Now) >= transaction.Date.AddYears(2))
                    {
                        repeatNeeded = true;
                    }
                }
                if(repeatNeeded)
                {
                    var newTransaction = new Transaction(NextAvailableTransactionId)
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        Description = transaction.Description,
                        Type= transaction.Type,
                        RepeatInterval = transaction.RepeatInterval,
                        Amount = transaction.Amount,
                        GroupId = transaction.GroupId,
                        RGBA = transaction.RGBA,
                    };
                    AddTransaction(newTransaction);
                    transaction.RepeatInterval = TransactionRepeatInterval.Never;
                    UpdateTransaction(transaction);
                }
            }
        }
    }

    /// <summary>
    /// The next available group id
    /// </summary>
    public uint NextAvailableGroupId
    {
        get
        {
            if(Groups.Count == 0)
            {
                return 1;
            }
            return Groups.Last().Value.Id + 1;
        }
    }

    /// <summary>
    /// The next available transaction id
    /// </summary>
    public uint NextAvailableTransactionId
    {
        get
        {
            if (Transactions.Count == 0)
            {
                return 1;
            }
            return Transactions.Last().Value.Id + 1;
        }
    }

    /// <summary>
    /// The income of the account
    /// </summary>
    public decimal Income
    {
        get
        {
            var income = 0m;
            foreach(var pair in Transactions)
            {
                if(pair.Value.Type == TransactionType.Income)
                {
                    income += pair.Value.Amount;
                }
            }
            return income;
        }
    }

    /// <summary>
    /// The expense of the account
    /// </summary>
    public decimal Expense
    {
        get
        {
            var expense = 0m;
            foreach (var pair in Transactions)
            {
                if (pair.Value.Type == TransactionType.Expense)
                {
                    expense += pair.Value.Amount;
                }
            }
            return expense;
        }
    }

    /// <summary>
    /// The total of the account
    /// </summary>
    public decimal Total
    {
        get
        {
            var total = 0m;
            foreach (var pair in Transactions)
            {
                if (pair.Value.Type == TransactionType.Income)
                {
                    total += pair.Value.Amount;
                }
                else if(pair.Value.Type == TransactionType.Expense)
                {
                    total -= pair.Value.Amount;
                }
            }
            return total;
        }
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    public void Dispose() => _database.Dispose();

    public Group? GetGroupById(uint id)
    {
        return null;
    }

    public bool AddGroup(Group group)
    {
        return false;
    }

    public bool UpdateGroup(Group group)
    {
        return false;
    }

    public bool DeleteGroup(uint id)
    {
        return false;
    }

    public Transaction? GetTransactionById(uint id)
    {
        return null;
    }

    public bool AddTransaction(Transaction transaction)
    {
        return false;
    }

    public bool UpdateTransaction(Transaction transaction)
    {
        return false;
    }

    public bool DeleteTransaction(uint id)
    {
        return false;
    }

    public int ImportFromFile(string path)
    {
        return 0;
    }

    public bool ExportToCSV(string path)
    {
        return false;
    }

    private int ImportFromCSV(string path)
    {
        return 0;
    }

    private int ImportFromOFX(string path)
    {
        return 0;
    }

    private int ImportFromQIF(string path)
    {
        return 0;
    }

    private void UpdateGroupAmounts()
    {

    }
}

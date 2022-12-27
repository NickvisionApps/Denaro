using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of an account
/// </summary>
public class Account : IDisposable
{
    private SqliteConnection _database;

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
        _database = new SqliteConnection(new SqliteConnectionStringBuilder()
        {
            DataSource = Path,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ConnectionString);
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
                Date = DateOnly.Parse(readQueryTransactions.GetString(1), new CultureInfo("en-US", false)),
                Description = readQueryTransactions.GetString(2),
                Type = (TransactionType)readQueryTransactions.GetInt32(3),
                RepeatInterval = (TransactionRepeatInterval)readQueryTransactions.GetInt32(4),
                Amount = readQueryTransactions.GetDecimal(5),
                GroupId = readQueryTransactions.GetInt32(6),
                RGBA = readQueryTransactions.GetString(7)
            };
            Transactions.Add(transaction.Id, transaction);
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

    /// <summary>
    /// Checks if repeat transactions are needed and creates them if so
    /// </summary>
    /// <returns>True if at least one transaction was created, else false</returns>
    public async Task<bool> RunRepeatTransactionsAsync()
    {
        var transactionsAdded = false;
        var transactions = Transactions.Values.ToList();
        foreach (var transaction in transactions)
        {
            if (transaction.RepeatInterval != TransactionRepeatInterval.Never)
            {
                var repeatNeeded = false;
                if (transaction.RepeatInterval == TransactionRepeatInterval.Daily)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddDays(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Weekly)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddDays(7))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Monthly)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddMonths(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Quarterly)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddMonths(4))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Yearly)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddYears(1))
                    {
                        repeatNeeded = true;
                    }
                }
                else if (transaction.RepeatInterval == TransactionRepeatInterval.Biyearly)
                {
                    if (DateOnly.FromDateTime(DateTime.Today) >= transaction.Date.AddYears(2))
                    {
                        repeatNeeded = true;
                    }
                }
                if (repeatNeeded)
                {
                    var newTransaction = new Transaction(NextAvailableTransactionId)
                    {
                        Date = DateOnly.FromDateTime(DateTime.Today),
                        Description = transaction.Description,
                        Type = transaction.Type,
                        RepeatInterval = transaction.RepeatInterval,
                        Amount = transaction.Amount,
                        GroupId = transaction.GroupId,
                        RGBA = transaction.RGBA,
                    };
                    await AddTransactionAsync(newTransaction);
                    transaction.RepeatInterval = TransactionRepeatInterval.Never;
                    await UpdateTransactionAsync(transaction);
                    transactionsAdded = true;
                }
            }
        }
        return transactionsAdded;
    }

    /// <summary>
    /// Adds a group to the account
    /// </summary>
    /// <param name="group">The group to add</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> AddGroupAsync(Group group)
    {
        var cmdAddGroup = _database.CreateCommand();
        cmdAddGroup.CommandText = "INSERT INTO groups (id, name, description) VALUES ($id, $name, $description)";
        cmdAddGroup.Parameters.AddWithValue("$id", group.Id);
        cmdAddGroup.Parameters.AddWithValue("$name", group.Name);
        cmdAddGroup.Parameters.AddWithValue("$description", group.Description);
        if(await cmdAddGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups.Add(group.Id, group);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates a group in the account
    /// </summary>
    /// <param name="group">The group to update</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> UpdateGroupAsync(Group group)
    {
        var cmdUpdateGroup = _database.CreateCommand();
        cmdUpdateGroup.CommandText = "UPDATE groups SET name = $name, description = $description WHERE id = $id";
        cmdUpdateGroup.Parameters.AddWithValue("$name", group.Name);
        cmdUpdateGroup.Parameters.AddWithValue("$description", group.Description);
        cmdUpdateGroup.Parameters.AddWithValue("$id", group.Id);
        if(await cmdUpdateGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups[group.Id] = group;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Deletes a group from the account
    /// </summary>
    /// <param name="id">The id of the group to delete</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> DeleteGroupAsync(uint id)
    {
        var cmdDeleteGroup = _database.CreateCommand();
        cmdDeleteGroup.CommandText = "DELETE FROM groups WHERE id = $id";
        cmdDeleteGroup.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups.Remove(id);
            foreach (var pair in Transactions)
            {
                if (pair.Value.GroupId == id)
                {
                    pair.Value.GroupId = -1;
                    await UpdateTransactionAsync(pair.Value);
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a transaction to the account
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> AddTransactionAsync(Transaction transaction)
    {
        var cmdAddTransaction = _database.CreateCommand();
        cmdAddTransaction.CommandText = "INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba) VALUES ($id, $date, $description, $type, $repeat, $amount, $gid, $rgba)";
        cmdAddTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdAddTransaction.Parameters.AddWithValue("$date", transaction.Date.ToString("d", new CultureInfo("en-US")));
        cmdAddTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdAddTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdAddTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdAddTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdAddTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdAddTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        if (await cmdAddTransaction.ExecuteNonQueryAsync() > 0)
        {
            Transactions.Add(transaction.Id, transaction);
            if(transaction.GroupId != -1)
            {
                await UpdateGroupAmountsAsync();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates a transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> UpdateTransactionAsync(Transaction transaction)
    {
        var cmdUpdateTransaction = _database.CreateCommand();
        cmdUpdateTransaction.CommandText = "UPDATE transactions SET date = $date, description = $description, type = $type, repeat = $repeat, amount = $amount, gid = $gid, rgba = $rgba WHERE id = $id";
        cmdUpdateTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdUpdateTransaction.Parameters.AddWithValue("$date", transaction.Date.ToShortDateString());
        cmdUpdateTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdUpdateTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdUpdateTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdUpdateTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdUpdateTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdUpdateTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        if (await cmdUpdateTransaction.ExecuteNonQueryAsync() > 0)
        {
            Transactions[transaction.Id] = transaction;
            await UpdateGroupAmountsAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// The transaction to delete from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> DeleteTransactionAsync(uint id)
    {
        var cmdDeleteTransaction = _database.CreateCommand();
        cmdDeleteTransaction.CommandText = "DELETE FROM transactions WHERE id = $id";
        cmdDeleteTransaction.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteTransaction.ExecuteNonQueryAsync() > 0)
        {
            var updateGroups = Transactions[id].GroupId != -1;
            Transactions.Remove(id);
            if(updateGroups)
            {
                await UpdateGroupAmountsAsync();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Creates an expense transaction for the transfer
    /// </summary>
    /// <param name="transfer">The transfer to send</param>
    /// <param name="description">The description for the new transaction</param>
    public async Task SendTransferAsync(Transfer transfer, string description)
    {
        var transaction = new Transaction(NextAvailableTransactionId)
        {
            Description = description,
            Type = TransactionType.Expense,
            Amount = transfer.Amount,
            RGBA = Configuration.Current.TransactionDefaultColor
        };
        await AddTransactionAsync(transaction);
    }

    /// <summary>
    /// Creates an income transaction for the transfer
    /// </summary>
    /// <param name="transfer"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public async Task ReceiveTransferAsync(Transfer transfer, string description)
    {
        var transaction = new Transaction(NextAvailableTransactionId)
        {
            Description = description,
            Type = TransactionType.Income,
            Amount = transfer.Amount,
            RGBA = Configuration.Current.TransactionDefaultColor
        };
        await AddTransactionAsync(transaction);
    }

    /// <summary>
    /// Imports transactions from a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The number of transactions imported. -1 for error</returns>
    public async Task<int> ImportFromFileAsync(string path)
    {
        if(!System.IO.Path.Exists(path))
        {
            return -1;
        }
        if(System.IO.Path.GetExtension(path) == ".csv")
        {
            return await ImportFromCSVAsync(path);
        }
        else if(System.IO.Path.GetExtension(path) == ".ofc")
        {
            return await ImportFromOFXAsync(path);
        }
        else if(System.IO.Path.GetExtension(path) == ".qif")
        {
            return await ImportFromQIFAsync(path);
        }
        return -1;
    }

    /// <summary>
    /// Exports the account to a CSV file
    /// </summary>
    /// <param name="path">The path to the CSV file</param>
    /// <returns>True if successful, else false</returns>
    public bool ExportToCSV(string path)
    {
        string result = "";
        result += "ID;Date;Description;Type;RepeatInterval;Amount;RGBA;Group;GroupName;GroupDescription\n";
        foreach (var pair in Transactions)
        {
            result += $"{pair.Value.Id};{pair.Value.Date.ToShortDateString()};{pair.Value.Description};{(int)pair.Value.Type};{(int)pair.Value.RepeatInterval};{pair.Value.Amount};{pair.Value.RGBA};{pair.Value.GroupId};";
            if (pair.Value.GroupId != -1)
            {
                result += $"{Groups[(uint)pair.Value.GroupId].Name};{Groups[(uint)pair.Value.GroupId].Description}\n";
            }
            else
            {
                result += ";\n";
            }
        }
        try
        {
            File.WriteAllText(path, result);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Imports transactions from a CSV file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The number of transactions imported. -1 for error</returns>
    private async Task<int> ImportFromCSVAsync(string path)
    {
        var imported = 0;
        var lines = default(string[]);
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch
        {
            return -1;
        }
        foreach (var line in lines)
        {
            var fields = line.Split(';');
            if (fields.Length != 10)
            {
                continue;
            }
            //Get Id
            var id = 0u;
            try
            {
                id = uint.Parse(fields[0]);
            }
            catch
            {
                continue;
            }
            if (Transactions.ContainsKey(id))
            {
                continue;
            }
            //Get Date
            var date = default(DateOnly);
            try
            {
                date = DateOnly.Parse(fields[1]);
            }
            catch
            {
                continue;
            }
            //Get Description
            var description = fields[2];
            //Get Type
            var type = TransactionType.Income;
            try
            {
                type = (TransactionType)int.Parse(fields[3]);
            }
            catch
            {
                continue;
            }
            //Get Repeat Interval
            var repeat = TransactionRepeatInterval.Never;
            try
            {
                repeat = (TransactionRepeatInterval)int.Parse(fields[4]);
            }
            catch
            {
                continue;
            }
            //Get Amount
            var amount = 0m;
            try
            {
                amount = decimal.Parse(fields[5]);
            }
            catch
            {
                continue;
            }
            //Get RGBA
            var rgba = fields[6];
            //Get Group Id
            var gid = 0;
            try
            {
                gid = int.Parse(fields[7]);
            }
            catch
            {
                continue;
            }
            //Get Group Name
            var groupName = fields[8];
            //Get Group Description
            var groupDescription = fields[9];
            //Create Group If Needed
            if (gid != -1 && !Groups.ContainsKey((uint)gid))
            {
                var group = new Group((uint)gid)
                {
                    Name = groupName,
                    Description = groupDescription
                };
                await AddGroupAsync(group);
            }
            //Add Transaction
            var transaction = new Transaction(id)
            {
                Date = date,
                Description = description,
                Type = type,
                RepeatInterval = repeat,
                Amount = amount,
                GroupId = gid,
                RGBA = rgba
            };
            await AddTransactionAsync(transaction);
            imported++;
        }
        return imported;
    }

    /// <summary>
    /// Imports transactions from an OFX file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The number of transactions imported. -1 for error</returns>
    private async Task<int> ImportFromOFXAsync(string path)
    {
        var lines = default(string[]);
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch
        {
            return -1;
        }
        var imported = 0;
        var nextId = NextAvailableTransactionId;
        var xmlTagRegex = new Regex("^<([^/>]+|/STMTTRN)>(?:[+-])?([^<]+)");
        var transaction = default(Transaction);
        var skipTransaction = false;
        foreach (var line in lines)
        {
            var match = xmlTagRegex.Match(line);
            if (match.Success)
            {
                var tag = match.Groups[1].Value;
                var content = match.Groups[2].Value;
                if (content.Contains('\r'))
                {
                    content = content.Remove(content.IndexOf('\r'));
                }
                //Add Previous Transaction
                if (tag == "/STMTTRN" && transaction != null)
                {
                    if (!skipTransaction)
                    {
                        await AddTransactionAsync(transaction);
                        imported++;
                    }
                    skipTransaction = false;
                    transaction = null;
                }
                //New Transaction
                if (tag == "STMTTRN")
                {
                    transaction = new Transaction(nextId);
                    nextId++;
                }
                if (!skipTransaction)
                {
                    //Type
                    if (tag == "TRNTYPE")
                    {
                        if (content == "CREDIT")
                        {
                            transaction!.Type = TransactionType.Income;
                        }
                        else if (content == "DEBIT")
                        {
                            transaction!.Type = TransactionType.Expense;
                        }
                        else
                        {
                            //Unknown Entry
                            skipTransaction = true;
                            continue;
                        }
                    }
                    //Name
                    if (tag == "MEMO")
                    {
                        transaction!.Description = content;
                    }
                    //Amount
                    if (tag == "TRNAMT")
                    {
                        try
                        {
                            transaction!.Amount = decimal.Parse(content, NumberStyles.Currency);
                        }
                        catch
                        {
                            skipTransaction = true;
                            continue;
                        }
                    }
                    //Date
                    if (tag == "DTPOSTED")
                    {
                        try
                        {
                            transaction!.Date = DateOnly.Parse(content);
                        }
                        catch
                        {
                            skipTransaction = true;
                            continue;
                        }
                    }
                }
            }
        }
        return imported;
    }

    /// <summary>
    /// Imports transactions from a QIF file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The number of transactions imported. -1 for error</returns>
    private async Task<int> ImportFromQIFAsync(string path)
    {
        var lines = default(string[]);
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch
        {
            return -1;
        }
        var imported = 0;
        var nextId = NextAvailableTransactionId;
        var transaction = new Transaction(nextId);
        var skipTransaction = false;
        foreach(var l in lines)
        {
            var line = l;
            if(line.Contains('\r'))
            {
                line = line.Remove(line.IndexOf('\r'));
            }
            if(line.Length == 0)
            {
                continue;
            }
            //Add Previous Transaction
            if (line[0] == '^')
            {
                if(!skipTransaction)
                {
                    await AddTransactionAsync(transaction);
                    imported++;
                }
                skipTransaction = false;
                nextId++;
                transaction = new Transaction(nextId);
            }
            //Date
            if (line[0] == 'D')
            {
                try
                {
                    var year = int.Parse(line.Substring(7, 4));
                    var month = int.Parse(line.Substring(4, 2));
                    var day = int.Parse(line.Substring(1, 2));
                    transaction.Date = new DateOnly(year, month, day);
                }
                catch
                {
                    skipTransaction = true;
                    continue;
                }
            }
            //Amount and Type
            if (line[0] == 'T')
            {
                try
                {
                    transaction.Type = line[1] == '-' ? TransactionType.Expense : TransactionType.Income;
                    transaction.Amount = decimal.Parse(line.Substring(2), NumberStyles.Currency);
                }
                catch
                {
                    skipTransaction = true;
                    continue;
                }
            }
            //Description
            if (line[0] == 'P')
            {
                transaction.Description = line.Substring(1);
            }
        }
        return imported;
    }

    /// <summary>
    /// Updates the amount of each Group object in the account
    /// </summary>
    private async Task UpdateGroupAmountsAsync()
    {
        var cmdQueryGroupBalance = _database.CreateCommand();
        cmdQueryGroupBalance.CommandText = "SELECT g.id, CAST(COALESCE(SUM(IIF(t.type=1, -t.amount, t.amount)), 0) AS TEXT) FROM transactions t RIGHT JOIN groups g on g.id = t.gid GROUP BY g.id;";
        using var readQueryGroupBalance = await cmdQueryGroupBalance.ExecuteReaderAsync();
        while(await readQueryGroupBalance.ReadAsync())
        {
            Groups[(uint)readQueryGroupBalance.GetInt32(0)].Balance = readQueryGroupBalance.GetDecimal(1);
        }
    }
}

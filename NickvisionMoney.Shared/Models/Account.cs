using Hazzik.Qif;
using Microsoft.Data.Sqlite;
using NickvisionMoney.Shared.Helpers;
using OfxSharp;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of an account
/// </summary>
public class Account : IDisposable
{
    private bool _loggedIn;
    private bool _disposed;
    private SqliteConnection? _database;
    private bool? _isEncrypted;

    /// <summary>
    /// The path of the account
    /// </summary>
    public string Path { get; init; }
    /// <summary>
    /// The metadata of the account
    /// </summary>
    public AccountMetadata Metadata { get; init; }
    /// <summary>
    /// A map of groups in the account
    /// </summary>
    public Dictionary<uint, Group> Groups { get; init; }
    /// <summary>
    /// A map of transactions in the account
    /// </summary>
    public Dictionary<uint, Transaction> Transactions { get; init; }
    /// <summary>
    /// Whether or not an account needs to be setup
    /// </summary>
    public bool NeedsAccountSetup { get; private set; }

    /// <summary>
    /// The next available group id
    /// </summary>
    public uint NextAvailableGroupId { get; private set; }
    /// <summary>
    /// The next available transaction id
    /// </summary>
    public uint NextAvailableTransactionId { get; private set; }
    /// <summary>
    /// The income amount of the account for today
    /// </summary>
    public decimal TodayIncome { get; private set; }
    /// <summary>
    /// The expense amount of the account for today
    /// </summary>
    public decimal TodayExpense { get; private set; }

    /// <summary>
    /// The total amount of the account for today
    /// </summary>
    public decimal TodayTotal => TodayIncome - TodayExpense;

    /// <summary>
    /// Constructs an Account
    /// </summary>
    /// <param name="path">The path of the account</param>
    public Account(string path)
    {
        _loggedIn = false;
        _disposed = false;
        _isEncrypted = null;
        Path = path;
        Metadata = new AccountMetadata(System.IO.Path.GetFileNameWithoutExtension(Path), AccountType.Checking);
        Groups = new Dictionary<uint, Group>();
        Transactions = new Dictionary<uint, Transaction>();
        NeedsAccountSetup = true;
        NextAvailableGroupId = 0;
        NextAvailableTransactionId = 0;
        TodayIncome = 0;
        TodayExpense = 0;
    }

    /// <summary>
    /// Whether or not the account is encrypted (requiring a password)
    /// </summary>
    public bool IsEncrypted
    {
        get
        {
            if (_isEncrypted == null)
            {
                if (!File.Exists(Path))
                {
                    _isEncrypted = false;
                }
                else
                {
                    var tempConnectionString = new SqliteConnectionStringBuilder()
                    {
                        DataSource = Path,
                        Mode = SqliteOpenMode.ReadOnly,
                        Pooling = false
                    };
                    using var tempDatabase = new SqliteConnection(tempConnectionString.ConnectionString);
                    tempDatabase.Open();
                    try
                    {
                        using var tempCmd = tempDatabase.CreateCommand();
                        tempCmd.CommandText = "PRAGMA schema_version";
                        tempCmd.ExecuteScalar();
                        _isEncrypted = false;
                    }
                    catch
                    {
                        _isEncrypted = true;
                    }
                    finally
                    {
                        tempDatabase.Close();
                    }
                }
            }
            return _isEncrypted.Value;
        }
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            foreach (var pair in Transactions)
            {
                pair.Value.Dispose();
            }
            if (_database != null)
            {
                FreeMemory();
                _database.Close();
                _database.Dispose();
                _database = null;
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Logins into an account
    /// </summary>
    /// <param name="password">The password of the account, if needed</param>
    /// <returns>True if logged in, else false</returns>
    public bool Login(string? password)
    {
        if (!_loggedIn)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = Path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false
            };
            //Set Password
            if (IsEncrypted)
            {
                if (string.IsNullOrEmpty(password))
                {
                    _loggedIn = false;
                    return false;
                }
                else
                {
                    connectionStringBuilder.Password = password;
                }
            }
            _database = new SqliteConnection(connectionStringBuilder.ConnectionString);
            try
            {
                _database.Open();
                _loggedIn = true;
            }
            catch
            {
                _database.Close();
                _database.Dispose();
                _database = null;
                _loggedIn = false;
            }
        }
        return _loggedIn;
    }

    /// <summary>
    /// Sets the password of the account. Specifying a null/empty string will remove the password and decrypt the database
    /// </summary>
    /// <param name="password">The password to set</param>
    /// <returns>True if successful, else false</returns>
    public bool SetPassword(string password)
    {
        //Remove password if empty (decrypts)
        if (string.IsNullOrEmpty(password))
        {
            //Create Temp Decrypted Database
            var tempPath = $"{Path}.decrypt";
            using var command = _database.CreateCommand();
            command.CommandText = $"ATTACH DATABASE '{tempPath}' AS plaintext KEY ''";
            command.ExecuteNonQuery();
            command.CommandText = $"SELECT sqlcipher_export('plaintext')";
            command.ExecuteNonQuery();
            command.CommandText = $"DETACH DATABASE plaintext";
            command.ExecuteNonQuery();
            //Remove Old Encrypted Database
            _database.Close();
            _database.Dispose();
            _database = null;
            File.Delete(Path);
            File.Move(tempPath, Path, true);
            //Open New Decrypted Database
            _database = new SqliteConnection(new SqliteConnectionStringBuilder()
            {
                DataSource = Path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false
            }.ConnectionString);
            _database.Open();
            _isEncrypted = false;
        }
        using var cmdQuote = _database.CreateCommand();
        cmdQuote.CommandText = "SELECT quote($password)";
        cmdQuote.Parameters.AddWithValue("$password", password);
        var quotedPassword = (string)cmdQuote.ExecuteScalar()!;
        //Change password
        if (IsEncrypted)
        {
            using var command = _database.CreateCommand();
            command.CommandText = $"PRAGMA rekey = {quotedPassword}";
            command.ExecuteNonQuery();
            _database.Close();
            _database.ConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
                Password = password
            }.ConnectionString;
            _database.Open();
            _isEncrypted = true;
        }
        //Sets new password (encrypts for first time)
        else
        {
            //Create Temp Encrypted Database
            var tempPath = $"{Path}.ecrypt";
            using var command = _database.CreateCommand();
            command.CommandText = $"ATTACH DATABASE '{tempPath}' AS encrypted KEY {quotedPassword}";
            command.ExecuteNonQuery();
            command.CommandText = $"SELECT sqlcipher_export('encrypted')";
            command.ExecuteNonQuery();
            command.CommandText = $"DETACH DATABASE encrypted";
            command.ExecuteNonQuery();
            //Remove Old Unencrypted Database
            _database.Close();
            _database.Dispose();
            _database = null;
            File.Delete(Path);
            File.Move(tempPath, Path, true);
            //Open New Encrypted Database
            _database = new SqliteConnection(new SqliteConnectionStringBuilder()
            {
                DataSource = Path,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
                Password = password
            }.ConnectionString);
            _database.Open();
            _isEncrypted = true;
        }
        return true;
    }

    /// <summary>
    /// Loads an account
    /// </summary>
    /// <returns>True if loaded, else false</returns>
    public async Task<bool> LoadAsync()
    {
        if (!_loggedIn)
        {
            return false;
        }
        //Setup Metadata Table
        using var cmdTableMetadata = _database.CreateCommand();
        cmdTableMetadata.CommandText = "CREATE TABLE IF NOT EXISTS metadata (id INTEGER PRIMARY KEY, name TEXT, type INTEGER, useCustomCurrency INTEGER, customSymbol TEXT, customCode TEXT, defaultTransactionType INTEGER, showGroupsList INTEGER, sortFirstToLast INTEGER, sortTransactionsBy INTEGER, customDecimalSeparator TEXT, customGroupSeparator TEXT, customDecimalDigits INTEGER)";
        cmdTableMetadata.ExecuteNonQuery();
        try
        {
            using var cmdTableMetadataUpdate1 = _database.CreateCommand();
            cmdTableMetadataUpdate1.CommandText = "ALTER TABLE metadata ADD COLUMN sortTransactionsBy INTEGER";
            cmdTableMetadataUpdate1.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableMetadataUpdate2 = _database.CreateCommand();
            cmdTableMetadataUpdate2.CommandText = "ALTER TABLE metadata ADD COLUMN customDecimalSeparator TEXT";
            cmdTableMetadataUpdate2.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableMetadataUpdate3 = _database.CreateCommand();
            cmdTableMetadataUpdate3.CommandText = "ALTER TABLE metadata ADD COLUMN customGroupSeparator TEXT";
            cmdTableMetadataUpdate3.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableMetadataUpdate4 = _database.CreateCommand();
            cmdTableMetadataUpdate4.CommandText = "ALTER TABLE metadata ADD COLUMN customDecimalDigits INTEGER";
            cmdTableMetadataUpdate4.ExecuteNonQuery();
        }
        catch { }
        //Setup Groups Table
        using var cmdTableGroups = _database.CreateCommand();
        cmdTableGroups.CommandText = "CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT)";
        cmdTableGroups.ExecuteNonQuery();
        //Setup Transactions Table
        using var cmdTableTransactions = _database.CreateCommand();
        cmdTableTransactions.CommandText = "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT)";
        cmdTableTransactions.ExecuteNonQuery();
        try
        {
            using var cmdTableTransactionsUpdate1 = _database.CreateCommand();
            cmdTableTransactionsUpdate1.CommandText = "ALTER TABLE transactions ADD COLUMN gid INTEGER";
            cmdTableTransactionsUpdate1.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate2 = _database.CreateCommand();
            cmdTableTransactionsUpdate2.CommandText = "ALTER TABLE transactions ADD COLUMN rgba TEXT";
            cmdTableTransactionsUpdate2.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate3 = _database.CreateCommand();
            cmdTableTransactionsUpdate3.CommandText = "ALTER TABLE transactions ADD COLUMN receipt TEXT";
            cmdTableTransactionsUpdate3.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate4 = _database.CreateCommand();
            cmdTableTransactionsUpdate4.CommandText = "ALTER TABLE transactions ADD COLUMN repeatFrom INTEGER";
            cmdTableTransactionsUpdate4.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate5 = _database.CreateCommand();
            cmdTableTransactionsUpdate5.CommandText = "ALTER TABLE transactions ADD COLUMN repeatEndDate TEXT";
            cmdTableTransactionsUpdate5.ExecuteNonQuery();
        }
        catch { }
        //Get Metadata
        using var cmdQueryMetadata = _database.CreateCommand();
        cmdQueryMetadata.CommandText = "SELECT * FROM metadata where id = 0";
        using var readQueryMetadata = cmdQueryMetadata.ExecuteReader();
        if (readQueryMetadata.HasRows)
        {
            readQueryMetadata.Read();
            Metadata.Name = readQueryMetadata.GetString(1);
            Metadata.AccountType = (AccountType)readQueryMetadata.GetInt32(2);
            Metadata.UseCustomCurrency = readQueryMetadata.GetBoolean(3);
            Metadata.CustomCurrencySymbol = string.IsNullOrEmpty(readQueryMetadata.GetString(4)) ? null : readQueryMetadata.GetString(4);
            Metadata.CustomCurrencyCode = string.IsNullOrEmpty(readQueryMetadata.GetString(5)) ? null : readQueryMetadata.GetString(5);
            Metadata.DefaultTransactionType = (TransactionType)readQueryMetadata.GetInt32(6);
            Metadata.ShowGroupsList = readQueryMetadata.GetBoolean(7);
            Metadata.SortFirstToLast = readQueryMetadata.GetBoolean(8);
            Metadata.SortTransactionsBy = readQueryMetadata.IsDBNull(9) ? SortBy.Id : (SortBy)readQueryMetadata.GetInt32(9);
            Metadata.CustomCurrencyDecimalSeparator = readQueryMetadata.IsDBNull(10) ? null : readQueryMetadata.GetString(10);
            Metadata.CustomCurrencyGroupSeparator = readQueryMetadata.IsDBNull(11) ? null : readQueryMetadata.GetString(11);
            Metadata.CustomCurrencyDecimalDigits = readQueryMetadata.IsDBNull(12) ? null : readQueryMetadata.GetInt32(12);
            NeedsAccountSetup = Metadata.UseCustomCurrency && (string.IsNullOrEmpty(Metadata.CustomCurrencySymbol) || string.IsNullOrEmpty(Metadata.CustomCurrencyCode) || string.IsNullOrEmpty(Metadata.CustomCurrencyDecimalSeparator) || string.IsNullOrEmpty(Metadata.CustomCurrencyGroupSeparator) || Metadata.CustomCurrencyDecimalDigits == null);
        }
        else
        {
            using var cmdAddMetadata = _database.CreateCommand();
            cmdAddMetadata.CommandText = "INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits) VALUES (0, $name, $type, $useCustomCurrency, $customSymbol, $customCode, $defaultTransactionType, $showGroupsList, $sortFirstToLast, $sortTransactionsBy, $customDecimalSeparator, $customGroupSeparator, $customDecimalDigits)";
            cmdAddMetadata.Parameters.AddWithValue("$name", Metadata.Name);
            cmdAddMetadata.Parameters.AddWithValue("$type", (int)Metadata.AccountType);
            cmdAddMetadata.Parameters.AddWithValue("$useCustomCurrency", Metadata.UseCustomCurrency);
            cmdAddMetadata.Parameters.AddWithValue("$customSymbol", Metadata.CustomCurrencySymbol ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$customCode", Metadata.CustomCurrencyCode ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$defaultTransactionType", (int)Metadata.DefaultTransactionType);
            cmdAddMetadata.Parameters.AddWithValue("$showGroupsList", Metadata.ShowGroupsList);
            cmdAddMetadata.Parameters.AddWithValue("$sortFirstToLast", Metadata.SortFirstToLast);
            cmdAddMetadata.Parameters.AddWithValue("$sortTransactionsBy", (int)Metadata.SortTransactionsBy);
            cmdAddMetadata.Parameters.AddWithValue("$customDecimalSeparator", Metadata.CustomCurrencyDecimalSeparator ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$customGroupSeparator", Metadata.CustomCurrencyGroupSeparator ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$customDecimalDigits", Metadata.CustomCurrencyDecimalDigits ?? 2);
            cmdAddMetadata.ExecuteNonQuery();
        }
        //Get Groups
        using var localizer = new Localizer();
        Groups.Add(0, new Group(0)
        {
            Name = localizer["Ungrouped"],
            Description = localizer["UngroupedDescription"],
            Balance = 0u
        });
        using var cmdQueryGroups = _database.CreateCommand();
        cmdQueryGroups.CommandText = "SELECT * FROM groups";
        using var readQueryGroups = cmdQueryGroups.ExecuteReader();
        while (readQueryGroups.Read())
        {
            if (readQueryGroups.IsDBNull(0))
            {
                continue;
            }
            var group = new Group((uint)readQueryGroups.GetInt32(0))
            {
                Name = readQueryGroups.IsDBNull(1) ? "" : readQueryGroups.GetString(1),
                Description = readQueryGroups.IsDBNull(2) ? "" : readQueryGroups.GetString(2),
                Balance = 0m
            };
            Groups.Add(group.Id, group);
            if (group.Id > NextAvailableGroupId)
            {
                NextAvailableGroupId = group.Id;
            }
        }
        NextAvailableGroupId++;
        //Get Transactions
        using var cmdQueryTransactions = _database.CreateCommand();
        cmdQueryTransactions.CommandText = "SELECT * FROM transactions";
        using var readQueryTransactions = cmdQueryTransactions.ExecuteReader();
        while (readQueryTransactions.Read())
        {
            if (readQueryTransactions.IsDBNull(0))
            {
                continue;
            }
            var transaction = new Transaction((uint)readQueryTransactions.GetInt32(0))
            {
                Date = readQueryTransactions.IsDBNull(1) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(readQueryTransactions.GetString(1), new CultureInfo("en-US", false)),
                Description = readQueryTransactions.IsDBNull(2) ? "" : readQueryTransactions.GetString(2),
                Type = readQueryTransactions.IsDBNull(3) ? TransactionType.Income : (TransactionType)readQueryTransactions.GetInt32(3),
                RepeatInterval = readQueryTransactions.IsDBNull(4) ? TransactionRepeatInterval.Never : (TransactionRepeatInterval)readQueryTransactions.GetInt32(4),
                Amount = readQueryTransactions.IsDBNull(5) ? 0m : readQueryTransactions.GetDecimal(5),
                GroupId = readQueryTransactions.IsDBNull(6) ? -1 : readQueryTransactions.GetInt32(6),
                RGBA = readQueryTransactions.IsDBNull(7) ? "" : readQueryTransactions.GetString(7),
                RepeatFrom = readQueryTransactions.IsDBNull(9) ? -1 : readQueryTransactions.GetInt32(9),
                RepeatEndDate = readQueryTransactions.IsDBNull(10) ? null : (string.IsNullOrEmpty(readQueryTransactions.GetString(10)) ? null : DateOnly.Parse(readQueryTransactions.GetString(10), new CultureInfo("en-US", false)))
            };
            var receiptString = readQueryTransactions.IsDBNull(8) ? "" : readQueryTransactions.GetString(8);
            if (!string.IsNullOrEmpty(receiptString))
            {
                transaction.Receipt = Image.Load(Convert.FromBase64String(receiptString), new JpegDecoder());
            }
            Transactions.Add(transaction.Id, transaction);
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                Groups[groupId].Balance += (transaction.Type == TransactionType.Income ? 1 : -1) * transaction.Amount;
                if (transaction.Type == TransactionType.Income)
                {
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    TodayExpense += transaction.Amount;
                }
            }
            if (transaction.Id > NextAvailableTransactionId)
            {
                NextAvailableTransactionId = transaction.Id;
            }
        }
        NextAvailableTransactionId++;
        //Repeats
        await SyncRepeatTransactionsAsync();
        //Cleanup
        FreeMemory();
        return true;
    }

    /// <summary>
    /// Gets the income amount for the date range
    /// </summary>
    /// <param name="endDate">The end date</param>
    /// <param name="startDate">The start date</param>
    /// <returns>The income amount for the date range</returns>
    public decimal GetIncome(DateOnly endDate, DateOnly? startDate = null)
    {
        var income = 0m;
        foreach (var pair in Transactions)
        {
            if (startDate != null)
            {
                if (pair.Value.Date < startDate)
                {
                    continue;
                }
            }
            if (pair.Value.Type == TransactionType.Income && pair.Value.Date <= endDate)
            {
                income += pair.Value.Amount;
            }
        }
        return income;
    }

    /// <summary>
    /// Gets the expense amount for the date range
    /// </summary>
    /// <param name="endDate">The end date</param>
    /// <param name="startDate">The start date</param>
    /// <returns>The expense amount for the date range</returns>
    public decimal GetExpense(DateOnly endDate, DateOnly? startDate = null)
    {
        var expense = 0m;
        foreach (var pair in Transactions)
        {
            if (startDate != null)
            {
                if (pair.Value.Date < startDate)
                {
                    continue;
                }
            }
            if (pair.Value.Type == TransactionType.Expense && pair.Value.Date <= endDate)
            {
                expense += pair.Value.Amount;
            }
        }
        return expense;
    }

    /// <summary>
    /// Gets the total amount for the date range
    /// </summary>
    /// <param name="endDate">The end date</param>
    /// <param name="startDate">The start date</param>
    /// <returns>The total amount for the date range</returns>
    public decimal GetTotal(DateOnly endDate, DateOnly? startDate = null)
    {
        var total = 0m;
        foreach (var pair in Transactions)
        {
            if (startDate != null)
            {
                if (pair.Value.Date < startDate)
                {
                    continue;
                }
            }
            if (pair.Value.Date <= endDate)
            {
                if (pair.Value.Type == TransactionType.Income)
                {
                    total += pair.Value.Amount;
                }
                else
                {
                    total -= pair.Value.Amount;
                }
            }
        }
        return total;
    }

    /// <summary>
    /// Updates the metadata of the account
    /// </summary>
    /// <param name="metadata">The new metadata</param>
    /// <returns>True if successful, else false</returns>
    public bool UpdateMetadata(AccountMetadata metadata)
    {
        using var cmdUpdateMetadata = _database.CreateCommand();
        cmdUpdateMetadata.CommandText = "UPDATE metadata SET name = $name, type = $type, useCustomCurrency = $useCustomCurrency, customSymbol = $customSymbol, customCode = $customCode, defaultTransactionType = $defaultTransactionType, showGroupsList = $showGroupsList, sortFirstToLast = $sortFirstToLast, sortTransactionsBy = $sortTransactionsBy, customDecimalSeparator = $customDecimalSeparator, customGroupSeparator = $customGroupSeparator, customDecimalDigits = $customDecimalDigits WHERE id = 0";
        cmdUpdateMetadata.Parameters.AddWithValue("$name", metadata.Name);
        cmdUpdateMetadata.Parameters.AddWithValue("$type", (int)metadata.AccountType);
        cmdUpdateMetadata.Parameters.AddWithValue("$useCustomCurrency", metadata.UseCustomCurrency);
        cmdUpdateMetadata.Parameters.AddWithValue("$customSymbol", metadata.CustomCurrencySymbol ?? "");
        cmdUpdateMetadata.Parameters.AddWithValue("$customCode", metadata.CustomCurrencyCode ?? "");
        cmdUpdateMetadata.Parameters.AddWithValue("$defaultTransactionType", (int)metadata.DefaultTransactionType);
        cmdUpdateMetadata.Parameters.AddWithValue("$showGroupsList", metadata.ShowGroupsList);
        cmdUpdateMetadata.Parameters.AddWithValue("$sortFirstToLast", metadata.SortFirstToLast);
        cmdUpdateMetadata.Parameters.AddWithValue("$sortTransactionsBy", (int)metadata.SortTransactionsBy);
        cmdUpdateMetadata.Parameters.AddWithValue("$customDecimalSeparator", metadata.CustomCurrencyDecimalSeparator ?? "");
        cmdUpdateMetadata.Parameters.AddWithValue("$customGroupSeparator", metadata.CustomCurrencyGroupSeparator ?? "");
        cmdUpdateMetadata.Parameters.AddWithValue("$customDecimalDigits", metadata.CustomCurrencyDecimalDigits ?? 2);
        if (cmdUpdateMetadata.ExecuteNonQuery() > 0)
        {
            Metadata.Name = metadata.Name;
            Metadata.AccountType = metadata.AccountType;
            Metadata.UseCustomCurrency = metadata.UseCustomCurrency;
            Metadata.CustomCurrencySymbol = metadata.CustomCurrencySymbol;
            Metadata.CustomCurrencyCode = metadata.CustomCurrencyCode;
            Metadata.DefaultTransactionType = metadata.DefaultTransactionType;
            Metadata.ShowGroupsList = metadata.ShowGroupsList;
            Metadata.SortFirstToLast = metadata.SortFirstToLast;
            Metadata.SortTransactionsBy = metadata.SortTransactionsBy;
            Metadata.CustomCurrencyDecimalSeparator = metadata.CustomCurrencyDecimalSeparator;
            Metadata.CustomCurrencyGroupSeparator = metadata.CustomCurrencyGroupSeparator;
            Metadata.CustomCurrencyDecimalDigits = metadata.CustomCurrencyDecimalDigits;
            NeedsAccountSetup = Metadata.UseCustomCurrency && (string.IsNullOrEmpty(Metadata.CustomCurrencySymbol) || string.IsNullOrEmpty(Metadata.CustomCurrencyCode) || string.IsNullOrEmpty(Metadata.CustomCurrencyDecimalSeparator) || string.IsNullOrEmpty(Metadata.CustomCurrencyGroupSeparator) || Metadata.CustomCurrencyDecimalDigits == null);
            FreeMemory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Syncs repeat transactions in the account
    /// </summary>
    /// <returns>True if transactions were modified, else false</returns>
    public async Task<bool> SyncRepeatTransactionsAsync()
    {
        var transactionsModified = false;
        var transactions = Transactions.Values.ToList();
        var i = 0;
        foreach (var transaction in transactions)
        {
            if (transaction.RepeatFrom == 0)
            {
                var dates = new List<DateOnly>();
                var endDate = (transaction.RepeatEndDate ?? DateOnly.FromDateTime(DateTime.Now)) < DateOnly.FromDateTime(DateTime.Now) ? transaction.RepeatEndDate : DateOnly.FromDateTime(DateTime.Now);
                for (var date = transaction.Date; date <= endDate; date = date.AddDays(0))
                {
                    if (date != transaction.Date)
                    {
                        dates.Add(date);
                    }
                    if (transaction.RepeatInterval == TransactionRepeatInterval.Daily)
                    {
                        date = date.AddDays(1);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Weekly)
                    {
                        date = date.AddDays(7);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Biweekly)
                    {
                        date = date.AddDays(14);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Monthly)
                    {
                        date = date.AddMonths(1);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Quarterly)
                    {
                        date = date.AddMonths(3);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Yearly)
                    {
                        date = date.AddYears(1);
                    }
                    else if (transaction.RepeatInterval == TransactionRepeatInterval.Biyearly)
                    {
                        date = date.AddYears(2);
                    }
                }
                for (var j = i; j < transactions.Count; j++)
                {
                    if (transactions[j].RepeatFrom == transaction.Id)
                    {
                        dates.Remove(transactions[j].Date);
                    }
                }
                foreach (var date in dates)
                {
                    var newTransaction = new Transaction(NextAvailableTransactionId)
                    {
                        Date = date,
                        Description = transaction.Description,
                        Type = transaction.Type,
                        RepeatInterval = transaction.RepeatInterval,
                        Amount = transaction.Amount,
                        GroupId = transaction.GroupId,
                        RGBA = transaction.RGBA,
                        Receipt = transaction.Receipt,
                        RepeatFrom = (int)transaction.Id,
                        RepeatEndDate = transaction.RepeatEndDate
                    };
                    await AddTransactionAsync(newTransaction);
                    transactionsModified = true;
                }
            }
            else if (transaction.RepeatFrom > 0)
            {
                if (Transactions[(uint)transaction.RepeatFrom].RepeatEndDate < transaction.Date)
                {
                    await DeleteTransactionAsync(transaction.Id);
                    transactionsModified = true;
                }
            }
            i++;
        }
        return transactionsModified;
    }

    /// <summary>
    /// Adds a group to the account
    /// </summary>
    /// <param name="group">The group to add</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> AddGroupAsync(Group group)
    {
        using var cmdAddGroup = _database.CreateCommand();
        cmdAddGroup.CommandText = "INSERT INTO groups (id, name, description) VALUES ($id, $name, $description)";
        cmdAddGroup.Parameters.AddWithValue("$id", group.Id);
        cmdAddGroup.Parameters.AddWithValue("$name", group.Name);
        cmdAddGroup.Parameters.AddWithValue("$description", group.Description);
        if (await cmdAddGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups.Add(group.Id, group);
            NextAvailableGroupId++;
            FreeMemory();
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
        using var cmdUpdateGroup = _database.CreateCommand();
        cmdUpdateGroup.CommandText = "UPDATE groups SET name = $name, description = $description WHERE id = $id";
        cmdUpdateGroup.Parameters.AddWithValue("$name", group.Name);
        cmdUpdateGroup.Parameters.AddWithValue("$description", group.Description);
        cmdUpdateGroup.Parameters.AddWithValue("$id", group.Id);
        if (await cmdUpdateGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups[group.Id] = group;
            FreeMemory();
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
        using var cmdDeleteGroup = _database.CreateCommand();
        cmdDeleteGroup.CommandText = "DELETE FROM groups WHERE id = $id";
        cmdDeleteGroup.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups.Remove(id);
            if (id + 1 == NextAvailableGroupId)
            {
                NextAvailableGroupId--;
            }
            foreach (var pair in Transactions)
            {
                if (pair.Value.GroupId == id)
                {
                    pair.Value.GroupId = -1;
                    await UpdateTransactionAsync(pair.Value);
                }
            }
            FreeMemory();
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
        using var cmdAddTransaction = _database.CreateCommand();
        cmdAddTransaction.CommandText = "INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba, receipt, repeatFrom, repeatEndDate) VALUES ($id, $date, $description, $type, $repeat, $amount, $gid, $rgba, $receipt, $repeatFrom, $repeatEndDate)";
        cmdAddTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdAddTransaction.Parameters.AddWithValue("$date", transaction.Date.ToString("d", new CultureInfo("en-US")));
        cmdAddTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdAddTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdAddTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdAddTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdAddTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdAddTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        if (transaction.Receipt != null)
        {
            using var memoryStream = new MemoryStream();
            await transaction.Receipt.SaveAsync(memoryStream, new JpegEncoder());
            cmdAddTransaction.Parameters.AddWithValue("$receipt", Convert.ToBase64String(memoryStream.ToArray()));
        }
        else
        {
            cmdAddTransaction.Parameters.AddWithValue("$receipt", "");
        }
        cmdAddTransaction.Parameters.AddWithValue("$repeatFrom", transaction.RepeatFrom);
        cmdAddTransaction.Parameters.AddWithValue("$repeatEndDate", transaction.RepeatEndDate != null ? transaction.RepeatEndDate.Value.ToString("d", new CultureInfo("en-US")) : "");
        if (await cmdAddTransaction.ExecuteNonQueryAsync() > 0)
        {
            Transactions.Add(transaction.Id, transaction);
            NextAvailableTransactionId++;
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                Groups[groupId].Balance += (transaction.Type == TransactionType.Income ? 1 : -1) * transaction.Amount;
                if (transaction.Type == TransactionType.Income)
                {
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    TodayExpense += transaction.Amount;
                }
            }
            if (transaction.RepeatInterval != TransactionRepeatInterval.Never && transaction.RepeatFrom == 0)
            {
                await SyncRepeatTransactionsAsync();
            }
            FreeMemory();
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
        using var cmdUpdateTransaction = _database.CreateCommand();
        cmdUpdateTransaction.CommandText = "UPDATE transactions SET date = $date, description = $description, type = $type, repeat = $repeat, amount = $amount, gid = $gid, rgba = $rgba, receipt = $receipt, repeatFrom = $repeatFrom, repeatEndDate = $repeatEndDate WHERE id = $id";
        cmdUpdateTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdUpdateTransaction.Parameters.AddWithValue("$date", transaction.Date.ToString("d", new CultureInfo("en-US")));
        cmdUpdateTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdUpdateTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdUpdateTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdUpdateTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdUpdateTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdUpdateTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        if (transaction.Receipt != null)
        {
            using var memoryStream = new MemoryStream();
            await transaction.Receipt.SaveAsync(memoryStream, new JpegEncoder());
            cmdUpdateTransaction.Parameters.AddWithValue("$receipt", Convert.ToBase64String(memoryStream.ToArray()));
        }
        else
        {
            cmdUpdateTransaction.Parameters.AddWithValue("$receipt", "");
        }
        cmdUpdateTransaction.Parameters.AddWithValue("$repeatFrom", transaction.RepeatFrom);
        cmdUpdateTransaction.Parameters.AddWithValue("$repeatEndDate", transaction.RepeatEndDate != null ? transaction.RepeatEndDate.Value.ToString("d", new CultureInfo("en-US")) : "");
        if (await cmdUpdateTransaction.ExecuteNonQueryAsync() > 0)
        {
            var oldTransaction = Transactions[transaction.Id];
            if (oldTransaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = oldTransaction.GroupId == -1 ? 0u : (uint)oldTransaction.GroupId;
                Groups[groupId].Balance -= (oldTransaction.Type == TransactionType.Income ? 1 : -1) * oldTransaction.Amount;
                if (oldTransaction.Type == TransactionType.Income)
                {
                    TodayIncome -= oldTransaction.Amount;
                }
                else
                {
                    TodayExpense -= oldTransaction.Amount;
                }
            }
            Transactions[transaction.Id].Dispose();
            Transactions[transaction.Id] = transaction;
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                Groups[groupId].Balance += (transaction.Type == TransactionType.Income ? 1 : -1) * transaction.Amount;
                if (transaction.Type == TransactionType.Income)
                {
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    TodayExpense += transaction.Amount;
                }
            }
            if (transaction.RepeatFrom == 0)
            {
                await SyncRepeatTransactionsAsync();
            }
            FreeMemory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Updates a source transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <param name="updateGenerated">Whether or not to update generated transactions associated with the source</param>
    public async Task UpdateSourceTransactionAsync(Transaction transaction, bool updateGenerated)
    {
        var transactions = Transactions.Values.ToList();
        if (updateGenerated)
        {
            foreach (var t in transactions)
            {
                if (t.RepeatFrom == (int)transaction.Id)
                {
                    var tt = (Transaction)t.Clone();
                    tt.Description = transaction.Description;
                    tt.Type = transaction.Type;
                    tt.Amount = transaction.Amount;
                    tt.GroupId = transaction.GroupId;
                    tt.RGBA = transaction.RGBA;
                    tt.Receipt = transaction.Receipt;
                    tt.RepeatEndDate = transaction.RepeatEndDate;
                    await UpdateTransactionAsync(tt);
                }
            }
            await UpdateTransactionAsync(transaction);
        }
        else
        {
            foreach (var t in transactions)
            {
                if (t.RepeatFrom == (int)transaction.Id)
                {
                    var tt = (Transaction)t.Clone();
                    tt.RepeatInterval = TransactionRepeatInterval.Never;
                    tt.RepeatFrom = -1;
                    tt.RepeatEndDate = null;
                    await UpdateTransactionAsync(tt);
                }
            }
            await UpdateTransactionAsync(transaction);
        }
    }

    /// <summary>
    /// The transaction to delete from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> DeleteTransactionAsync(uint id)
    {
        using var cmdDeleteTransaction = _database.CreateCommand();
        cmdDeleteTransaction.CommandText = "DELETE FROM transactions WHERE id = $id";
        cmdDeleteTransaction.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteTransaction.ExecuteNonQueryAsync() > 0)
        {
            var transaction = Transactions[id];
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                Groups[groupId].Balance -= (transaction.Type == TransactionType.Income ? 1 : -1) * transaction.Amount;
                if (transaction.Type == TransactionType.Income)
                {
                    TodayIncome -= transaction.Amount;
                }
                else
                {
                    TodayExpense -= transaction.Amount;
                }
            }
            Transactions[id].Dispose();
            Transactions.Remove(id);
            if (id + 1 == NextAvailableTransactionId)
            {
                NextAvailableTransactionId--;
            }
            FreeMemory();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a source transaction from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <param name="deleteGenerated">Whether or not to delete generated transactions associated with the source</param>
    public async Task DeleteSourceTransactionAsync(uint id, bool deleteGenerated)
    {
        var transactions = Transactions.Values.ToList();
        if (deleteGenerated)
        {
            await DeleteTransactionAsync(id);
            foreach (var transaction in transactions)
            {
                if (transaction.RepeatFrom == (int)id)
                {
                    await DeleteTransactionAsync(transaction.Id);
                }
            }
        }
        else
        {
            await DeleteTransactionAsync(id);
            foreach (var transaction in transactions)
            {
                if (transaction.RepeatFrom == (int)id)
                {
                    var t = (Transaction)transaction.Clone();
                    t.RepeatInterval = TransactionRepeatInterval.Never;
                    t.RepeatFrom = -1;
                    t.RepeatEndDate = null;
                    await UpdateTransactionAsync(t);
                }
            }
        }
    }

    /// <summary>
    /// Removes generated repeat transactions from the account
    /// </summary>
    /// <param name="id">The id of the source transaction</param>
    public async Task DeleteGeneratedTransactionsAsync(uint id)
    {
        var transactions = Transactions.Values.ToList();
        foreach (var transaction in transactions)
        {
            if (transaction.RepeatFrom == (int)id)
            {
                await DeleteTransactionAsync(transaction.Id);
            }
        }
    }

    /// <summary>
    /// Creates an expense transaction for the transfer
    /// </summary>
    /// <param name="transfer">The transfer to send</param>
    /// <param name="description">The description for the new transaction</param>
    /// <returns>The new transaction created</returns>
    public async Task<Transaction> SendTransferAsync(Transfer transfer, string description)
    {
        var transaction = new Transaction(NextAvailableTransactionId)
        {
            Description = description,
            Type = TransactionType.Expense,
            Amount = transfer.SourceAmount,
            RGBA = Configuration.Current.TransferDefaultColor
        };
        await AddTransactionAsync(transaction);
        return transaction;
    }

    /// <summary>
    /// Creates an income transaction for the transfer
    /// </summary>
    /// <param name="transfer"></param>
    /// <param name="description"></param>
    /// <returns>The new transaction created</returns>
    public async Task<Transaction> ReceiveTransferAsync(Transfer transfer, string description)
    {
        var transaction = new Transaction(NextAvailableTransactionId)
        {
            Description = description,
            Type = TransactionType.Income,
            Amount = transfer.DestinationAmount,
            RGBA = Configuration.Current.TransferDefaultColor
        };
        await AddTransactionAsync(transaction);
        return transaction;
    }

    /// <summary>
    /// Imports transactions from a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="rgba">The rgba for imported transactions</param>
    /// <returns>The list of Ids of newly imported transactions</returns>
    public async Task<List<uint>> ImportFromFileAsync(string path, string rgba)
    {
        var ids = new List<uint>();
        if (!System.IO.Path.Exists(path))
        {
            return ids;
        }
        var extension = System.IO.Path.GetExtension(path).ToLower();
        if (extension == ".csv")
        {
            return await ImportFromCSVAsync(path);
        }
        else if (extension == ".ofx")
        {
            return await ImportFromOFXAsync(path, rgba);
        }
        else if (extension == ".qif")
        {
            return await ImportFromQIFAsync(path, rgba);
        }
        return ids;
    }

    /// <summary>
    /// Exports the account to a file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>True if successful, else false</returns>
    public bool ExportToFile(string path)
    {
        var extension = System.IO.Path.GetExtension(path).ToLower();
        if (extension == ".csv")
        {
            return ExportToCSV(path);
        }
        else if (extension == ".pdf")
        {
            return ExportToPDF(path);
        }
        return false;
    }

    /// <summary>
    /// Imports transactions from a CSV file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The list of Ids of newly imported transactions</returns>
    private async Task<List<uint>> ImportFromCSVAsync(string path)
    {
        var ids = new List<uint>();
        string[]? lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch
        {
            return ids;
        }
        foreach (var line in lines)
        {
            var fields = line.Split(';');
            if (fields.Length != 12)
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
                date = DateOnly.Parse(fields[1], new CultureInfo("en-US"));
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
            //Get Repeat From
            var repeatFrom = 0;
            try
            {
                repeatFrom = int.Parse(fields[5]);
            }
            catch
            {
                continue;
            }
            //Get Repeat End Date
            var repeatEndDate = default(DateOnly?);
            try
            {
                repeatEndDate = DateOnly.Parse(fields[6]);
            }
            catch { }
            //Get Amount
            var amount = 0m;
            try
            {
                amount = decimal.Parse(fields[7], NumberStyles.Currency, new CultureInfo("en-US"));
            }
            catch
            {
                continue;
            }
            amount = Math.Abs(amount);
            //Get RGBA
            var rgba = fields[8];
            //Get Group Id
            var gid = 0;
            try
            {
                gid = int.Parse(fields[9]);
            }
            catch
            {
                continue;
            }
            //Get Group Name
            var groupName = fields[10];
            //Get Group Description
            var groupDescription = fields[11];
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
                RGBA = rgba,
                RepeatFrom = repeatFrom,
                RepeatEndDate = repeatEndDate
            };
            await AddTransactionAsync(transaction);
            ids.Add(transaction.Id);
            if (transaction.RepeatInterval != TransactionRepeatInterval.Never)
            {
                foreach (var pair in Transactions)
                {
                    if (pair.Value.RepeatFrom == transaction.Id)
                    {
                        ids.Add(pair.Value.Id);
                    }
                }
            }
        }
        return ids;
    }

    /// <summary>
    /// Imports transactions from an OFX file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The list of Ids of newly imported transactions</returns>
    private async Task<List<uint>> ImportFromOFXAsync(string path, string rgba)
    {
        var ids = new List<uint>();
        var localizer = new Localizer();
        var ofx = new OFXDocumentParser().Import(new FileStream(path, FileMode.Open));
        //Transactions
        foreach (var transaction in ofx.Transactions)
        {
            if (transaction.Amount != 0)
            {
                ids.Add(NextAvailableTransactionId);
                await AddTransactionAsync(new Transaction(NextAvailableTransactionId)
                {
                    Description = string.IsNullOrEmpty(transaction.Name) ? (string.IsNullOrEmpty(transaction.Memo) ? localizer["NotAvailable"] : transaction.Memo) : transaction.Name,
                    Date = DateOnly.FromDateTime(transaction.Date),
                    Type = transaction.Amount > 0 ? TransactionType.Income : TransactionType.Expense,
                    Amount = Math.Abs(transaction.Amount),
                    RGBA = rgba
                });
            }
        }
        return ids;
    }

    /// <summary>
    /// Imports transactions from a QIF file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <returns>The list of Ids of newly imported transactions</returns>
    private async Task<List<uint>> ImportFromQIFAsync(string path, string rgba)
    {
        var ids = new List<uint>();
        QifDocument? qif = null;
        try
        {
            qif = QifDocument.Load(File.OpenRead(path));
        }
        catch
        {
            return ids;
        }
        //Groups
        foreach (var group in qif.CategoryListTransactions)
        {
            if (Groups.Values.FirstOrDefault(x => x.Name == group.CategoryName) == null)
            {
                await AddGroupAsync(new Group(NextAvailableGroupId)
                {
                    Name = group.CategoryName,
                    Description = group.Description
                });
            }
        }
        //Transactions
        foreach (var transaction in qif.BankTransactions.Concat(qif.CashTransactions).Concat(qif.CreditCardTransactions))
        {
            if (transaction.Amount != 0)
            {
                ids.Add(NextAvailableTransactionId);
                var localizer = new Localizer();
                var group = Groups.Values.FirstOrDefault(x => x.Name == transaction.Category);
                await AddTransactionAsync(new Transaction(NextAvailableTransactionId)
                {
                    Description = string.IsNullOrEmpty(transaction.Memo) ? localizer["NotAvailable"] : transaction.Memo,
                    Date = DateOnly.FromDateTime(transaction.Date),
                    Type = transaction.Amount > 0 ? TransactionType.Income : TransactionType.Expense,
                    Amount = Math.Abs(transaction.Amount),
                    GroupId = group == null ? -1 : (int)group.Id,
                    RGBA = rgba
                });
            }
        }
        return ids;
    }

    /// <summary>
    /// Exports the account to a CSV file
    /// </summary>
    /// <param name="path">The path to the CSV file</param>
    /// <returns>True if successful, else false</returns>
    private bool ExportToCSV(string path)
    {
        string result = "";
        result += "ID;Date (en_US Format);Description;Type;RepeatInterval;RepeatFrom (-1=None,0=Original,Other=Id Of Source);RepeatEndDate (en_US Format);Amount (en_US Format);RGBA;Group(Id Starts At 1);GroupName;GroupDescription\n";
        foreach (var pair in Transactions)
        {
            result += $"{pair.Value.Id};{pair.Value.Date.ToString("d", new CultureInfo("en-US"))};{pair.Value.Description};{(int)pair.Value.Type};{(int)pair.Value.RepeatInterval};{pair.Value.RepeatFrom};{(pair.Value.RepeatEndDate != null ? pair.Value.RepeatEndDate.Value.ToString("d", new CultureInfo("en-US")) : "")};{pair.Value.Amount};{pair.Value.RGBA};{pair.Value.GroupId};";
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
    /// Exports the account to a PDF file
    /// </summary>
    /// <param name="path">The path to the PDF file</param>
    /// <returns>True if successful, else false</returns>
    private bool ExportToPDF(string path)
    {
        try
        {
            using var localizer = new Localizer();
            //Amount Culture
            var lcMonetary = Environment.GetEnvironmentVariable("LC_MONETARY");
            if (lcMonetary != null && lcMonetary.Contains(".UTF-8"))
            {
                lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".UTF-8"), 6);
            }
            else if (lcMonetary != null && lcMonetary.Contains(".utf8"))
            {
                lcMonetary = lcMonetary.Remove(lcMonetary.IndexOf(".utf8"), 5);
            }
            if (lcMonetary != null && lcMonetary.Contains('_'))
            {
                lcMonetary = lcMonetary.Replace('_', '-');
            }
            var cultureAmount = new CultureInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name, true);
            var regionAmount = new RegionInfo(!string.IsNullOrEmpty(lcMonetary) ? lcMonetary : CultureInfo.CurrentCulture.Name);
            if (Metadata.UseCustomCurrency)
            {
                cultureAmount.NumberFormat.CurrencySymbol = Metadata.CustomCurrencySymbol ?? cultureAmount.NumberFormat.CurrencySymbol;
                cultureAmount.NumberFormat.NaNSymbol = Metadata.CustomCurrencyCode ?? regionAmount.ISOCurrencySymbol;
                cultureAmount.NumberFormat.CurrencyDecimalSeparator = Metadata.CustomCurrencyDecimalSeparator ?? cultureAmount.NumberFormat.CurrencyDecimalSeparator;
                cultureAmount.NumberFormat.CurrencyGroupSeparator = Metadata.CustomCurrencyGroupSeparator ?? cultureAmount.NumberFormat.CurrencyGroupSeparator;
                cultureAmount.NumberFormat.CurrencyDecimalDigits = Metadata.CustomCurrencyDecimalDigits ?? cultureAmount.NumberFormat.CurrencyDecimalDigits;
            }
            //Date Culture
            var lcTime = Environment.GetEnvironmentVariable("LC_TIME");
            if (lcTime != null && lcTime.Contains(".UTF-8"))
            {
                lcTime = lcTime.Remove(lcTime.IndexOf(".UTF-8"), 6);
            }
            else if (lcTime != null && lcTime.Contains(".utf8"))
            {
                lcTime = lcTime.Remove(lcTime.IndexOf(".utf8"), 5);
            }
            if (lcTime != null && lcTime.Contains('_'))
            {
                lcTime = lcTime.Replace('_', '-');
            }
            var cultureDate = new CultureInfo(!string.IsNullOrEmpty(lcTime) ? lcTime : CultureInfo.CurrentCulture.Name, true);
            using var appiconStream = Assembly.GetCallingAssembly().GetManifestResourceStream("NickvisionMoney.Shared.Resources.org.nickvision.money-symbolic.png")!;
            using var interRegularFontStream = Assembly.GetCallingAssembly().GetManifestResourceStream("NickvisionMoney.Shared.Resources.Inter-Regular.otf")!;
            using var interSemiBoldFontStream = Assembly.GetCallingAssembly().GetManifestResourceStream("NickvisionMoney.Shared.Resources.Inter-SemiBold.otf")!;
            using var notoEmojiFontStream = Assembly.GetCallingAssembly().GetManifestResourceStream("NickvisionMoney.Shared.Resources.NotoEmoji-VariableFont_wght.ttf")!;
            FontManager.RegisterFont(interRegularFontStream);
            FontManager.RegisterFont(interSemiBoldFontStream);
            FontManager.RegisterFont(notoEmojiFontStream);
            Document.Create(container =>
            {
                //Page 1
                container.Page(page =>
                {
                    //Settings
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(TextStyle.Default.FontFamily("Inter").FontSize(12).Fallback(x => x.FontFamily("Noto Emoji")));
                    //Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem(2).Text(Metadata.Name).SemiBold().FontSize(16).Fallback(x => x.FontFamily("Noto Emoji").FontSize(16));
                        row.RelativeItem(1).AlignRight().Width(32, Unit.Point).Height(32, Unit.Point).Image(appiconStream, ImageScaling.FitArea);
                    });
                    //Content
                    page.Content().PaddingVertical(0.4f, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);
                        //Generated Date
                        col.Item().Text(string.Format(localizer["Generated", "PDF"], DateTime.Now.ToString("g", cultureDate)));
                        //Overview
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //Type, Amount
                                x.RelativeColumn();
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(localizer["Overview"]);
                            //Data
                            var maxDate = DateOnly.FromDateTime(DateTime.Today);
                            foreach (var pair in Transactions)
                            {
                                if (pair.Value.Date > maxDate)
                                {
                                    maxDate = pair.Value.Date;
                                }
                            }
                            tbl.Cell().Text(localizer["Total"]);
                            var total = GetTotal(maxDate);
                            tbl.Cell().AlignRight().Text($"{(total < 0 ? "-  " : "+  ")}{Math.Abs(total).ToString("C", cultureAmount)}");
                            tbl.Cell().Background(Colors.Grey.Lighten3).Text(localizer["Income"]);
                            tbl.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text(GetIncome(maxDate).ToString("C", cultureAmount));
                            tbl.Cell().Text(localizer["Expense"]);
                            tbl.Cell().AlignRight().Text(GetExpense(maxDate).ToString("C", cultureAmount));
                        });
                        //Metadata
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //Type, UseCustomCurrency, CustomSymbol, CustomCode
                                x.RelativeColumn();
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(localizer["AccountSettings"]);
                            tbl.Cell().Text(localizer["AccountType", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["Currency", "PDF"]).SemiBold();
                            //Data
                            tbl.Cell().Background(Colors.Grey.Lighten3).Text(Metadata.AccountType switch
                            {
                                AccountType.Checking => localizer["AccountType", "Checking"],
                                AccountType.Savings => localizer["AccountType", "Savings"],
                                AccountType.Business => localizer["AccountType", "Business"],
                                _ => ""
                            });
                            if (Metadata.UseCustomCurrency)
                            {
                                tbl.Cell().Background(Colors.Grey.Lighten3).Text($"{Metadata.CustomCurrencySymbol} ({Metadata.CustomCurrencyCode})");
                            }
                            else
                            {
                                tbl.Cell().Background(Colors.Grey.Lighten3).Text($"{cultureAmount.NumberFormat.CurrencySymbol} ({regionAmount.ISOCurrencySymbol})");
                            }
                        });
                        //Groups
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //Name, Description, Balance
                                x.RelativeColumn();
                                x.RelativeColumn();
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten1).Text(localizer["Groups"]);
                            tbl.Cell().Text(localizer["Name", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["Description", "Field"]).SemiBold();
                            tbl.Cell().AlignRight().Text(localizer["Amount", "Field"]).SemiBold();
                            //Data
                            var i = 0;
                            foreach (var pair in Groups)
                            {
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Name);
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Description);
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).AlignRight().Text($"{(pair.Value.Balance < 0 ? "-  " : "+  ")}{Math.Abs(pair.Value.Balance).ToString("C", cultureAmount)}");
                                i++;
                            }
                        });
                        //Transactions
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //ID, Date, Description, Type, GroupName, RepeatInterval, Amount
                                x.ConstantColumn(30);
                                x.ConstantColumn(80);
                                x.RelativeColumn();
                                x.ConstantColumn(70);
                                x.RelativeColumn();
                                x.ConstantColumn(100);
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(7).Background(Colors.Grey.Lighten1).Text(localizer["Transactions"]);
                            tbl.Cell().Text(localizer["Id", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["Date", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["Description", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["TransactionType", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["GroupName", "PDF"]).SemiBold();
                            tbl.Cell().Text(localizer["TransactionRepeatInterval", "Field"]).SemiBold();
                            tbl.Cell().AlignRight().Text(localizer["Amount", "Field"]).SemiBold();
                            //Data
                            foreach (var pair in Transactions)
                            {
                                var hex = "#32"; //120
                                var rgba = pair.Value.RGBA;
                                if (rgba.StartsWith("#"))
                                {
                                    rgba = rgba.Remove(0, 1);
                                    if (rgba.Length == 8)
                                    {
                                        rgba = rgba.Remove(rgba.Length - 2);
                                    }
                                    hex += rgba;
                                }
                                else
                                {
                                    rgba = rgba.Remove(0, rgba.StartsWith("rgb(") ? 4 : 5);
                                    rgba = rgba.Remove(rgba.Length - 1);
                                    var fields = rgba.Split(',');
                                    hex += byte.Parse(fields[0]).ToString("X2");
                                    hex += byte.Parse(fields[1]).ToString("X2");
                                    hex += byte.Parse(fields[2]).ToString("X2");
                                }
                                tbl.Cell().Background(hex).Text(pair.Value.Id.ToString());
                                tbl.Cell().Background(hex).Text(pair.Value.Date.ToString("d", cultureDate));
                                tbl.Cell().Background(hex).Text(pair.Value.Description);
                                tbl.Cell().Background(hex).Text(pair.Value.Type switch
                                {
                                    TransactionType.Income => localizer["Income"],
                                    TransactionType.Expense => localizer["Expense"],
                                    _ => ""
                                });
                                tbl.Cell().Background(hex).Text(pair.Value.GroupId == -1 ? localizer["Ungrouped"] : Groups[(uint)pair.Value.GroupId].Name);
                                tbl.Cell().Background(hex).Text(pair.Value.RepeatInterval switch
                                {
                                    TransactionRepeatInterval.Never => localizer["RepeatInterval", "Never"],
                                    TransactionRepeatInterval.Daily => localizer["RepeatInterval", "Daily"],
                                    TransactionRepeatInterval.Weekly => localizer["RepeatInterval", "Weekly"],
                                    TransactionRepeatInterval.Biweekly => localizer["RepeatInterval", "Biweekly"],
                                    TransactionRepeatInterval.Monthly => localizer["RepeatInterval", "Monthly"],
                                    TransactionRepeatInterval.Quarterly => localizer["RepeatInterval", "Quarterly"],
                                    TransactionRepeatInterval.Yearly => localizer["RepeatInterval", "Yearly"],
                                    TransactionRepeatInterval.Biyearly => localizer["RepeatInterval", "Biyearly"],
                                    _ => ""
                                });
                                tbl.Cell().Background(hex).AlignRight().Text($"{(pair.Value.Type == TransactionType.Income ? "+  " : "-  ")}{Math.Abs(pair.Value.Amount).ToString("C", cultureAmount)}");
                            }
                        });
                        //Receipts
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //ID, Receipt
                                x.ConstantColumn(30);
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(localizer["Receipts", "PDF"]);
                            tbl.Cell().Text(localizer["Id", "Field"]).SemiBold();
                            tbl.Cell().Text(localizer["Receipt", "Field"]).SemiBold();
                            //Data
                            var i = 0;
                            foreach (var pair in Transactions)
                            {
                                if (pair.Value.Receipt != null)
                                {
                                    using var memoryStream = new MemoryStream();
                                    pair.Value.Receipt.Save(memoryStream, new JpegEncoder());
                                    tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Id.ToString());
                                    tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).MaxWidth(300).MaxHeight(300).Image(memoryStream.ToArray());
                                    i++;
                                }
                            }
                        });
                    });
                    //Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem(2).Text(localizer["NickvisionMoneyAccount"]).FontColor(Colors.Grey.Medium);
                        row.RelativeItem(1).Text(x =>
                        {
                            var pageString = localizer["PageNumber", "PDF"];
                            if (pageString.EndsWith("{0}"))
                            {
                                x.Span(pageString.Remove(pageString.IndexOf("{0}"), 3)).FontColor(Colors.Grey.Medium);
                            }
                            x.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                            if (pageString.StartsWith("{0}"))
                            {
                                x.Span(pageString.Remove(pageString.IndexOf("{0}"), 3)).FontColor(Colors.Grey.Medium);
                            }
                            x.AlignRight();
                        });
                    });
                });
            }).GeneratePdf(path);
        }
        catch
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Frees up memory used by the database
    /// </summary>
    private void FreeMemory()
    {
        using var cmdClean = _database.CreateCommand();
        cmdClean.CommandText = "PRAGMA shrink_memory";
        cmdClean.ExecuteNonQuery();
    }
}

using Hazzik.Qif;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.Data.Sqlite;
using NickvisionMoney.Shared.Helpers;
using OfxSharp;
using PdfSharpCore.Pdf.IO;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp.Formats.Jpeg;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.Shared.Models;

public enum ExportMode
{
    All,
    CurrentView
}

public enum GraphType
{
    IncomeExpensePie,
    IncomeExpensePerGroup,
    IncomeExpenseOverTime,
    IncomeByGroup,
    ExpenseByGroup,
}

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
    /// A list of tags in the account
    /// </summary>
    public List<string> Tags { get; init; }
    /// <summary>
    /// A map of transactions in the account
    /// </summary>
    public Dictionary<uint, Transaction> Transactions { get; init; }
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
    /// The list of upcoming transaction reminders
    /// </summary>
    public List<(string Title, string Subtitle)> TransactionReminders { get; private set; }

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
        Tags = new List<string>() { _("Untagged") };
        Transactions = new Dictionary<uint, Transaction>();
        NextAvailableGroupId = 1;
        NextAvailableTransactionId = 1;
        TodayIncome = 0;
        TodayExpense = 0;
        TransactionReminders = new List<(string Title, string Subtitle)>();
    }

    /// <summary>
    /// Finalizes the Account
    /// </summary>
    ~Account() => Dispose(false);

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
    /// The password of the account. Specifying a null/empty string will remove the password and decrypt the database
    /// </summary>
    public string Password
    {
        set
        {
            //Remove Password If Empty (Decrypts)
            if (string.IsNullOrEmpty(value))
            {
                //Create Temp Decrypted Database
                var tempPath = $"{Path}.decrypt";
                using var command = _database!.CreateCommand();
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
            using var cmdQuote = _database!.CreateCommand();
            cmdQuote.CommandText = "SELECT quote($password)";
            cmdQuote.Parameters.AddWithValue("$password", value);
            var quotedPassword = (string)cmdQuote.ExecuteScalar()!;
            //Change Password
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
                    Password = value
                }.ConnectionString;
                _database.Open();
                _isEncrypted = true;
            }
            //Sets New Password (Encrypts For First Time)
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
                    Password = value
                }.ConnectionString);
                _database.Open();
                _isEncrypted = true;
            }
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
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                _database.Close();
                _database.Dispose();
                _database = null;
                _loggedIn = false;
            }
        }
        return _loggedIn;
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
        using var cmdTableMetadata = _database!.CreateCommand();
        cmdTableMetadata.CommandText = "CREATE TABLE IF NOT EXISTS metadata (id INTEGER PRIMARY KEY, name TEXT, type INTEGER, useCustomCurrency INTEGER, customSymbol TEXT, customCode TEXT, defaultTransactionType INTEGER, showGroupsList INTEGER, sortFirstToLast INTEGER, sortTransactionsBy INTEGER, customDecimalSeparator TEXT, customGroupSeparator TEXT, customDecimalDigits INTEGER, showTagsList INTEGER, transactionRemindersThreshold INTEGER, customAmountStyle INTEGER)";
        cmdTableMetadata.ExecuteNonQuery();
        AccountMetadata.UpdateMetadataDatabaseTable(_database);
        //Setup Groups Table
        using var cmdTableGroups = _database.CreateCommand();
        cmdTableGroups.CommandText = "CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT, rgba TEXT)";
        try
        {
            using var cmdTableGroupsUpdate1 = _database.CreateCommand();
            cmdTableGroupsUpdate1.CommandText = "ALTER TABLE groups ADD COLUMN rgba TEXT";
            cmdTableGroupsUpdate1.ExecuteNonQuery();
        }
        catch { }
        cmdTableGroups.ExecuteNonQuery();
        //Setup Transactions Table
        using var cmdTableTransactions = _database.CreateCommand();
        cmdTableTransactions.CommandText = "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT, useGroupColor INTEGER, notes TEXT, tags TEXT)";
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
        try
        {
            using var cmdTableTransactionsUpdate6 = _database.CreateCommand();
            cmdTableTransactionsUpdate6.CommandText = "ALTER TABLE transactions ADD COLUMN useGroupColor INTEGER";
            cmdTableTransactionsUpdate6.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate7 = _database.CreateCommand();
            cmdTableTransactionsUpdate7.CommandText = "ALTER TABLE transactions ADD COLUMN notes TEXT";
            cmdTableTransactionsUpdate7.ExecuteNonQuery();
        }
        catch { }
        try
        {
            using var cmdTableTransactionsUpdate8 = _database.CreateCommand();
            cmdTableTransactionsUpdate8.CommandText = "ALTER TABLE transactions ADD COLUMN tags TEXT";
            cmdTableTransactionsUpdate8.ExecuteNonQuery();
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
            Metadata.CustomCurrencySymbol = string.IsNullOrWhiteSpace(readQueryMetadata.GetString(4)) ? null : readQueryMetadata.GetString(4);
            Metadata.CustomCurrencyCode = string.IsNullOrWhiteSpace(readQueryMetadata.GetString(5)) ? null : readQueryMetadata.GetString(5);
            Metadata.DefaultTransactionType = (TransactionType)readQueryMetadata.GetInt32(6);
            Metadata.ShowGroupsList = readQueryMetadata.GetBoolean(7);
            Metadata.SortFirstToLast = readQueryMetadata.GetBoolean(8);
            Metadata.SortTransactionsBy = readQueryMetadata.IsDBNull(9) ? SortBy.Id : (SortBy)readQueryMetadata.GetInt32(9);
            Metadata.CustomCurrencyDecimalSeparator = readQueryMetadata.IsDBNull(10) ? null : readQueryMetadata.GetString(10);
            Metadata.CustomCurrencyGroupSeparator = readQueryMetadata.IsDBNull(11) ? null : (readQueryMetadata.GetString(11) == "empty" ? "" : readQueryMetadata.GetString(11));
            Metadata.CustomCurrencyDecimalDigits = readQueryMetadata.IsDBNull(12) ? null : readQueryMetadata.GetInt32(12);
            Metadata.ShowTagsList = readQueryMetadata.IsDBNull(13) ? true : readQueryMetadata.GetBoolean(13);
            Metadata.TransactionRemindersThreshold = readQueryMetadata.IsDBNull(14) ? RemindersThreshold.OneDayBefore : (RemindersThreshold)readQueryMetadata.GetInt32(14);
            Metadata.CustomCurrencyAmountStyle = readQueryMetadata.IsDBNull(15) ? null : readQueryMetadata.GetInt32(15);
        }
        else
        {
            using var cmdAddMetadata = _database.CreateCommand();
            cmdAddMetadata.CommandText = "INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits, showTagsList, transactionRemindersThreshold, customAmountStyle) VALUES (0, $name, $type, $useCustomCurrency, $customSymbol, $customCode, $defaultTransactionType, $showGroupsList, $sortFirstToLast, $sortTransactionsBy, $customDecimalSeparator, $customGroupSeparator, $customDecimalDigits, $showTagsList, $transactionRemindersThreshold, $customAmountStyle)";
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
            cmdAddMetadata.Parameters.AddWithValue("$customGroupSeparator", string.IsNullOrEmpty(Metadata.CustomCurrencyGroupSeparator) ? "empty" : Metadata.CustomCurrencyGroupSeparator);
            cmdAddMetadata.Parameters.AddWithValue("$customDecimalDigits", Metadata.CustomCurrencyDecimalDigits ?? 2);
            cmdAddMetadata.Parameters.AddWithValue("$showTagsList", Metadata.ShowGroupsList);
            cmdAddMetadata.Parameters.AddWithValue("$transactionRemindersThreshold", (int)Metadata.TransactionRemindersThreshold);
            cmdAddMetadata.Parameters.AddWithValue("$customAmountStyle", Metadata.CustomCurrencyAmountStyle ?? 0);
            cmdAddMetadata.ExecuteNonQuery();
        }
        //Get Groups
        Groups.Add(0, new Group(0)
        {
            Name = _("Ungrouped"),
            Description = _("Transactions without a group"),
            RGBA = ""
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
                RGBA = readQueryGroups.IsDBNull(3) ? "" : readQueryGroups.GetString(3)
            };
            Groups.Add(group.Id, group);
            if (group.Id >= NextAvailableGroupId)
            {
                NextAvailableGroupId = group.Id + 1;
            }
        }
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
                UseGroupColor = readQueryTransactions.IsDBNull(11) ? false : readQueryTransactions.GetBoolean(11),
                RepeatFrom = readQueryTransactions.IsDBNull(9) ? -1 : readQueryTransactions.GetInt32(9),
                RepeatEndDate = readQueryTransactions.IsDBNull(10) ? null : (string.IsNullOrWhiteSpace(readQueryTransactions.GetString(10)) ? null : DateOnly.Parse(readQueryTransactions.GetString(10), new CultureInfo("en-US", false))),
                Notes = readQueryTransactions.IsDBNull(12) ? "" : readQueryTransactions.GetString(12),
                Tags = readQueryTransactions.IsDBNull(13) ? new List<string>() : (string.IsNullOrWhiteSpace(readQueryTransactions.GetString(13)) ? new List<string>() : readQueryTransactions.GetString(13).Split(',').ToList())
            };
            Tags.AddRange(transaction.Tags.Where(t => !Tags.Contains(t)));
            var receiptString = readQueryTransactions.IsDBNull(8) ? "" : readQueryTransactions.GetString(8);
            if (!string.IsNullOrWhiteSpace(receiptString))
            {
                transaction.Receipt = SixLabors.ImageSharp.Image.Load(Convert.FromBase64String(receiptString));
            }
            Transactions.Add(transaction.Id, transaction);
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                if (transaction.Type == TransactionType.Income)
                {
                    Groups[groupId].Income += transaction.Amount;
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    Groups[groupId].Expense += transaction.Amount;
                    TodayExpense += transaction.Amount;
                }
            }
            if (transaction.Id >= NextAvailableTransactionId)
            {
                NextAvailableTransactionId = transaction.Id + 1;
            }
        }
        //Repeats
        await SyncRepeatTransactionsAsync();
        return true;
    }

    /// <summary>
    /// Gets the total income amount for the transactions given
    /// </summary>
    /// <param name="transactionIds">The ids of transactions to consider</param>
    /// <returns>The income amount</returns>
    public decimal GetIncome(IEnumerable<uint>? transactionIds = null) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Where(transaction => transaction.Type == TransactionType.Income)
        .Sum(transaction => transaction.Amount);

    /// <summary>
    /// Gets the total expense amount for transactions given
    /// </summary>
    /// <param name="transactionIds">The ids of transactions to consider</param>
    /// <returns>The total expense amount</returns>
    public decimal GetExpense(IEnumerable<uint>? transactionIds = null) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Where(transaction => transaction.Type == TransactionType.Expense)
        .Sum(transaction => transaction.Amount);

    /// <summary>
    /// Gets the balance amount left after income and expense for the transactions given
    /// </summary>
    /// <param name="transactionIds">The ids of transactions to consider</param>
    /// <returns>The balance amount after the transactions</returns>
    public decimal GetTotal(IEnumerable<uint>? transactionIds = null) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Sum(transaction => transaction.Type == TransactionType.Income ? transaction.Amount : (-1 * transaction.Amount));

    /// <summary>
    /// Gets the total income for a group
    /// </summary>
    /// <param name="group">The group to consider</param>
    /// <param name="transactionIds">The ids of the transactions to consider</param>
    /// <returns>The total income amount</returns>
    public decimal GetGroupIncome(Group group, IEnumerable<uint>? transactionIds) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Where(transaction => transaction.GroupId == group.Id || (transaction.GroupId == -1 && group.Id == 0))
        .Where(transaction => transaction.Type == TransactionType.Income)
        .Sum(transaction => transaction.Amount);

    /// <summary>
    /// Gets the total expense for a group
    /// </summary>
    /// <param name="group">The group to consider</param>
    /// <param name="transactionIds">The ids of the transactions to consider</param>
    /// <returns>The total expense amount</returns>
    public decimal GetGroupExpense(Group group, IEnumerable<uint>? transactionIds = null) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Where(transaction => transaction.GroupId == group.Id || (transaction.GroupId == -1 && group.Id == 0))
        .Where(transaction => transaction.Type == TransactionType.Expense)
        .Sum(transaction => transaction.Amount);

    /// <summary>
    /// Gets the balance amount left after income and expense for a group
    /// </summary>
    /// <param name="group">The group to consider</param>
    /// <param name="transactionIds">The ids of the transactions to consider</param>
    /// <returns>The balance amount for the group</returns>
    public decimal GetGroupTotal(Group group, IEnumerable<uint>? transactionIds = null) =>
        (transactionIds ?? Transactions.Keys)
        .Select(id => Transactions[id])
        .Where(transaction => transaction.GroupId == group.Id || (transaction.GroupId == -1 && group.Id == 0))
        .Sum(transaction => transaction.Type == TransactionType.Income ? transaction.Amount : (-1 * transaction.Amount));

    /// <summary>
    /// Updates the metadata of the account
    /// </summary>
    /// <param name="metadata">The new metadata</param>
    /// <returns>True if successful, else false</returns>
    public bool UpdateMetadata(AccountMetadata metadata)
    {
        using var cmdUpdateMetadata = _database!.CreateCommand();
        cmdUpdateMetadata.CommandText = "UPDATE metadata SET name = $name, type = $type, useCustomCurrency = $useCustomCurrency, customSymbol = $customSymbol, customCode = $customCode, defaultTransactionType = $defaultTransactionType, showGroupsList = $showGroupsList, sortFirstToLast = $sortFirstToLast, sortTransactionsBy = $sortTransactionsBy, customDecimalSeparator = $customDecimalSeparator, customGroupSeparator = $customGroupSeparator, customDecimalDigits = $customDecimalDigits, showTagsList = $showTagsList, transactionRemindersThreshold = $transactionRemindersThreshold, customAmountStyle = $customAmountStyle WHERE id = 0";
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
        cmdUpdateMetadata.Parameters.AddWithValue("$customGroupSeparator", string.IsNullOrEmpty(metadata.CustomCurrencyGroupSeparator) ? "empty" : metadata.CustomCurrencyGroupSeparator);
        cmdUpdateMetadata.Parameters.AddWithValue("$customDecimalDigits", metadata.CustomCurrencyDecimalDigits ?? 2);
        cmdUpdateMetadata.Parameters.AddWithValue("$showTagsList", metadata.ShowTagsList);
        cmdUpdateMetadata.Parameters.AddWithValue("$transactionRemindersThreshold", (int)metadata.TransactionRemindersThreshold);
        cmdUpdateMetadata.Parameters.AddWithValue("$customAmountStyle", metadata.CustomCurrencyAmountStyle ?? 0);
        if (cmdUpdateMetadata.ExecuteNonQuery() > 0)
        {
            var needsRemindersUpdate = Metadata.TransactionRemindersThreshold != metadata.TransactionRemindersThreshold;
            Metadata.Name = metadata.Name;
            Metadata.AccountType = metadata.AccountType;
            Metadata.UseCustomCurrency = metadata.UseCustomCurrency;
            Metadata.CustomCurrencySymbol = metadata.CustomCurrencySymbol;
            Metadata.CustomCurrencyCode = metadata.CustomCurrencyCode;
            Metadata.CustomCurrencyAmountStyle = metadata.CustomCurrencyAmountStyle;
            Metadata.DefaultTransactionType = metadata.DefaultTransactionType;
            Metadata.TransactionRemindersThreshold = metadata.TransactionRemindersThreshold;
            Metadata.ShowGroupsList = metadata.ShowGroupsList;
            Metadata.ShowTagsList = metadata.ShowTagsList;
            Metadata.SortFirstToLast = metadata.SortFirstToLast;
            Metadata.SortTransactionsBy = metadata.SortTransactionsBy;
            Metadata.CustomCurrencyDecimalSeparator = metadata.CustomCurrencyDecimalSeparator;
            Metadata.CustomCurrencyGroupSeparator = metadata.CustomCurrencyGroupSeparator;
            Metadata.CustomCurrencyDecimalDigits = metadata.CustomCurrencyDecimalDigits;
            if (needsRemindersUpdate)
            {
                CalculateTransactionReminders();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a group to the account
    /// </summary>
    /// <param name="group">The group to add</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> AddGroupAsync(Group group)
    {
        using var cmdAddGroup = _database!.CreateCommand();
        cmdAddGroup.CommandText = "INSERT INTO groups (id, name, description, rgba) VALUES ($id, $name, $description, $rgba)";
        cmdAddGroup.Parameters.AddWithValue("$id", group.Id);
        cmdAddGroup.Parameters.AddWithValue("$name", group.Name);
        cmdAddGroup.Parameters.AddWithValue("$description", group.Description);
        cmdAddGroup.Parameters.AddWithValue("$rgba", group.RGBA);
        if (await cmdAddGroup.ExecuteNonQueryAsync() > 0)
        {
            Groups.Add(group.Id, group);
            if (group.Id >= NextAvailableGroupId)
            {
                NextAvailableGroupId = group.Id + 1;
            }
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
        using var cmdUpdateGroup = _database!.CreateCommand();
        cmdUpdateGroup.CommandText = "UPDATE groups SET name = $name, description = $description, rgba = $rgba WHERE id = $id";
        cmdUpdateGroup.Parameters.AddWithValue("$name", group.Name);
        cmdUpdateGroup.Parameters.AddWithValue("$description", group.Description);
        cmdUpdateGroup.Parameters.AddWithValue("$rgba", group.RGBA);
        cmdUpdateGroup.Parameters.AddWithValue("$id", group.Id);
        if (await cmdUpdateGroup.ExecuteNonQueryAsync() > 0)
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
    /// <returns>(Result, BelongingTransactions)</returns>
    public async Task<(bool Result, List<uint> BelongingTransactions)> DeleteGroupAsync(uint id)
    {
        using var cmdDeleteGroup = _database!.CreateCommand();
        cmdDeleteGroup.CommandText = "DELETE FROM groups WHERE id = $id";
        cmdDeleteGroup.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteGroup.ExecuteNonQueryAsync() > 0)
        {
            var belongingTransactions = new List<uint>();
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
                    if (pair.Value.UseGroupColor)
                    {
                        pair.Value.UseGroupColor = false;
                        belongingTransactions.Add(pair.Key);
                    }
                    await UpdateTransactionAsync(pair.Value);
                }
            }
            return (true, belongingTransactions);
        }
        return (false, new List<uint>());
    }

    /// <summary>
    /// Adds a transaction to the account
    /// </summary>
    /// <param name="transaction">The transaction to add</param>
    /// <returns>(bool Successful, List NewTags)</returns>
    public async Task<(bool Successful, List<string> NewTags)> AddTransactionAsync(Transaction transaction)
    {
        using var cmdAddTransaction = _database!.CreateCommand();
        cmdAddTransaction.CommandText = "INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba, receipt, repeatFrom, repeatEndDate, useGroupColor, notes, tags) VALUES ($id, $date, $description, $type, $repeat, $amount, $gid, $rgba, $receipt, $repeatFrom, $repeatEndDate, $useGroupColor, $notes, $tags)";
        cmdAddTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdAddTransaction.Parameters.AddWithValue("$date", transaction.Date.ToString("d", new CultureInfo("en-US")));
        cmdAddTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdAddTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdAddTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdAddTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdAddTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdAddTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        cmdAddTransaction.Parameters.AddWithValue("$useGroupColor", transaction.UseGroupColor);
        cmdAddTransaction.Parameters.AddWithValue("$notes", transaction.Notes);
        cmdAddTransaction.Parameters.AddWithValue("$tags", string.Join(',', transaction.Tags));
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
            if (transaction.Id >= NextAvailableTransactionId)
            {
                NextAvailableTransactionId = transaction.Id + 1;
            }
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                if (transaction.Type == TransactionType.Income)
                {
                    Groups[groupId].Income += transaction.Amount;
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    Groups[groupId].Expense += transaction.Amount;
                    TodayExpense += transaction.Amount;
                }
            }
            var newTags = new List<string>();
            Tags.AddRange(transaction.Tags.Where(t =>
            {
                if (!Tags.Contains(t))
                {
                    newTags.Add(t);
                    return true;
                }
                return false;
            }));
            if (transaction.RepeatInterval != TransactionRepeatInterval.Never && transaction.RepeatFrom == 0)
            {
                await SyncRepeatTransactionsAsync();
            }
            else if (transaction.Date > DateOnly.FromDateTime(DateTime.Today))
            {
                CalculateTransactionReminders();
            }
            BackupAccountToCSV();
            return (true, newTags);
        }
        return (false, new List<string>());
    }

    /// <summary>
    /// Updates a transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <returns>(bool Successful, List NewTags)</returns>
    public async Task<(bool Successful, List<string> NewTags)> UpdateTransactionAsync(Transaction transaction)
    {
        using var cmdUpdateTransaction = _database!.CreateCommand();
        cmdUpdateTransaction.CommandText = "UPDATE transactions SET date = $date, description = $description, type = $type, repeat = $repeat, amount = $amount, gid = $gid, rgba = $rgba, receipt = $receipt, repeatFrom = $repeatFrom, repeatEndDate = $repeatEndDate, useGroupColor = $useGroupColor, notes = $notes, tags = $tags WHERE id = $id";
        cmdUpdateTransaction.Parameters.AddWithValue("$id", transaction.Id);
        cmdUpdateTransaction.Parameters.AddWithValue("$date", transaction.Date.ToString("d", new CultureInfo("en-US")));
        cmdUpdateTransaction.Parameters.AddWithValue("$description", transaction.Description);
        cmdUpdateTransaction.Parameters.AddWithValue("$type", (int)transaction.Type);
        cmdUpdateTransaction.Parameters.AddWithValue("$repeat", (int)transaction.RepeatInterval);
        cmdUpdateTransaction.Parameters.AddWithValue("$amount", transaction.Amount);
        cmdUpdateTransaction.Parameters.AddWithValue("$gid", transaction.GroupId);
        cmdUpdateTransaction.Parameters.AddWithValue("$rgba", transaction.RGBA);
        cmdUpdateTransaction.Parameters.AddWithValue("$useGroupColor", transaction.UseGroupColor);
        cmdUpdateTransaction.Parameters.AddWithValue("$notes", transaction.Notes);
        cmdUpdateTransaction.Parameters.AddWithValue("$tags", string.Join(',', transaction.Tags));
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
                if (oldTransaction.Type == TransactionType.Income)
                {
                    Groups[groupId].Income -= transaction.Amount;
                    TodayIncome -= oldTransaction.Amount;
                }
                else
                {
                    Groups[groupId].Expense -= transaction.Amount;
                    TodayExpense -= oldTransaction.Amount;
                }
            }
            Transactions[transaction.Id].Dispose();
            Transactions[transaction.Id] = transaction;
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                if (transaction.Type == TransactionType.Income)
                {
                    Groups[groupId].Income += transaction.Amount;
                    TodayIncome += transaction.Amount;
                }
                else
                {
                    Groups[groupId].Expense += transaction.Amount;
                    TodayExpense += transaction.Amount;
                }
            }
            var newTags = new List<string>();
            Tags.AddRange(transaction.Tags.Where(t =>
            {
                if (!Tags.Contains(t))
                {
                    newTags.Add(t);
                    return true;
                }
                return false;
            }));
            if (transaction.RepeatFrom == 0)
            {
                await SyncRepeatTransactionsAsync();
            }
            else if (transaction.Date > DateOnly.FromDateTime(DateTime.Today))
            {
                CalculateTransactionReminders();
            }
            BackupAccountToCSV();
            return (true, newTags);
        }
        return (false, new List<string>());
    }

    /// <summary>
    /// Updates a source transaction in the account
    /// </summary>
    /// <param name="transaction">The transaction to update</param>
    /// <param name="updateGenerated">Whether or not to update generated transactions associated with the source</param>
    /// <returns>(bool Successful, List NewTags)</returns>
    public async Task<(bool Successful, List<string> NewTags)> UpdateSourceTransactionAsync(Transaction transaction, bool updateGenerated)
    {
        var transactions = Transactions.Values.ToList();
        var success = true;
        var newTags = new List<string>();
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
                    tt.UseGroupColor = transaction.UseGroupColor;
                    tt.Receipt = transaction.Receipt;
                    tt.RepeatEndDate = transaction.RepeatEndDate;
                    tt.Notes = transaction.Notes;
                    tt.Tags = transaction.Tags;
                    var r = await UpdateTransactionAsync(tt);
                    success = success && r.Successful;
                    newTags = newTags.Union(r.NewTags).ToList();
                }
            }
            var res = await UpdateTransactionAsync(transaction);
            success = success && res.Successful;
            newTags.AddRange(res.NewTags.Where(t => !newTags.Contains(t)));
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
                    var r = await UpdateTransactionAsync(tt);
                    success = success && r.Successful;
                    newTags = newTags.Union(r.NewTags).ToList();
                }
            }
            var res = await UpdateTransactionAsync(transaction);
            success = success && res.Successful;
            newTags.AddRange(res.NewTags.Where(t => !newTags.Contains(t)));
        }
        return (success, newTags);
    }

    /// <summary>
    /// The transaction to delete from the account
    /// </summary>
    /// <param name="id">The id of the transaction to delete</param>
    /// <returns>True if successful, else false</returns>
    public async Task<bool> DeleteTransactionAsync(uint id)
    {
        using var cmdDeleteTransaction = _database!.CreateCommand();
        cmdDeleteTransaction.CommandText = "DELETE FROM transactions WHERE id = $id";
        cmdDeleteTransaction.Parameters.AddWithValue("$id", id);
        if (await cmdDeleteTransaction.ExecuteNonQueryAsync() > 0)
        {
            var transaction = Transactions[id];
            if (transaction.Date <= DateOnly.FromDateTime(DateTime.Now))
            {
                var groupId = transaction.GroupId == -1 ? 0u : (uint)transaction.GroupId;
                if (transaction.Type == TransactionType.Income)
                {
                    Groups[groupId].Income -= transaction.Amount;
                    TodayIncome -= transaction.Amount;
                }
                else
                {
                    Groups[groupId].Expense -= transaction.Amount;
                    TodayExpense -= transaction.Amount;
                }
            }
            else
            {
                CalculateTransactionReminders();
            }
            Transactions[id].Dispose();
            Transactions.Remove(id);
            if (id + 1 == NextAvailableTransactionId)
            {
                if(Transactions.Count == 0)
                {
                    NextAvailableTransactionId = 1;
                }
                else
                {
                    NextAvailableTransactionId = Transactions.Max(x => x.Key) + 1;
                }
            }
            BackupAccountToCSV();
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
        CalculateTransactionReminders();
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
        CalculateTransactionReminders();
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
                for (var date = transaction.Date; date <= endDate; date = date.AddDays(0)) //calculate needed repeat transaction dates up until today
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
                for (var j = i; j < transactions.Count; j++) //remove dates of existing repeat transactions
                {
                    if (transactions[j].RepeatFrom == transaction.Id)
                    {
                        dates.Remove(transactions[j].Date);
                    }
                }
                foreach (var date in dates) //create missing repeat transactions
                {
                    var res = (await AddTransactionAsync(transaction.Repeat(NextAvailableTransactionId, date))).Successful;
                    transactionsModified = transactionsModified || res;
                }
            }
            else if (transaction.RepeatFrom > 0) //delete repeat transactions if the date from the original transaction was changed to a smaller date
            {
                if (Transactions[(uint)transaction.RepeatFrom].RepeatEndDate < transaction.Date)
                {
                    var res = await DeleteTransactionAsync(transaction.Id);
                    transactionsModified = transactionsModified || res;
                }
            }
            i++;
        }
        CalculateTransactionReminders();
        return transactionsModified;
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
    /// <param name="defaultTransactionRGBA">The default color for a transaction</param>
    /// <param name="defaultGroupRGBA">The default color for a group</param>
    /// <returns>ImportResult</returns>
    public async Task<ImportResult> ImportFromFileAsync(string path, string defaultTransactionRGBA, string defaultGroupRGBA)
    {
        if (!System.IO.Path.Exists(path))
        {
            Console.Error.WriteLine($"File not found: {path}");
            return ImportResult.Empty;
        }
        var extension = System.IO.Path.GetExtension(path).ToLower();
        if (extension == ".csv")
        {
            return await ImportFromCSVAsync(path, defaultTransactionRGBA, defaultGroupRGBA);
        }
        else if (extension == ".ofx")
        {
            return await ImportFromOFXAsync(path, defaultTransactionRGBA);
        }
        else if (extension == ".qif")
        {
            return await ImportFromQIFAsync(path, defaultTransactionRGBA, defaultGroupRGBA);
        }
        Console.Error.WriteLine($"Unsupported file extension: {extension}");
        return ImportResult.Empty;
    }

    /// <summary>
    /// Imports transactions from a CSV file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="defaultTransactionRGBA">The default color for a transaction</param>
    /// <param name="defaultGroupRGBA">The default color for a group</param>
    /// <returns>ImportResult</returns>
    private async Task<ImportResult> ImportFromCSVAsync(string path, string defaultTransactionRGBA, string defaultGroupRGBA)
    {
        string[]? lines;
        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return ImportResult.Empty;
        }
        var importResult = new ImportResult();
        foreach (var line in lines)
        {
            var fields = line.Split(';');
            if (fields.Length != 15)
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
            if (string.IsNullOrWhiteSpace(rgba))
            {
                rgba = defaultTransactionRGBA;
            }
            //Get UseGroupColor
            var useGroupColor = false;
            try
            {
                useGroupColor = Convert.ToBoolean(int.Parse(fields[9]));
            }
            catch
            {
                continue;
            }
            //Get Group Id
            var gid = 0;
            try
            {
                gid = int.Parse(fields[10]);
            }
            catch
            {
                continue;
            }
            //Get Group Name
            var groupName = fields[11];
            //Get Group Description
            var groupDescription = fields[12];
            //Get Group RGBA
            var groupRGBA = fields[13];
            //Create Group If Needed
            if (gid != -1 && !Groups.ContainsKey((uint)gid))
            {
                var group = new Group((uint)gid)
                {
                    Name = groupName,
                    Description = groupDescription,
                    RGBA = string.IsNullOrWhiteSpace(groupRGBA) ? defaultGroupRGBA : groupRGBA
                };
                if (await AddGroupAsync(group))
                {
                    importResult.NewGroupIds.Add(group.Id);
                }
            }
            var tags = fields[14].Split(',').Where(x => !string.IsNullOrWhiteSpace(x));
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
                UseGroupColor = useGroupColor,
                RepeatFrom = repeatFrom,
                RepeatEndDate = repeatEndDate,
                Tags = tags.ToList()
            };
            var res = await AddTransactionAsync(transaction);
            if (res.Successful)
            {
                importResult.NewTransactionIds.Add(transaction.Id);
                importResult.AddTags(res.NewTags);
                if (transaction.RepeatInterval != TransactionRepeatInterval.Never)
                {
                    foreach (var pair in Transactions)
                    {
                        if (pair.Value.RepeatFrom == transaction.Id)
                        {
                            importResult.NewTransactionIds.Add(pair.Value.Id);
                        }
                    }
                }
            }
        }
        return importResult;
    }

    /// <summary>
    /// Imports transactions from an OFX file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="defaultTransactionRGBA">The default color for a transaction</param>
    /// <returns>ImportResult</returns>
    private async Task<ImportResult> ImportFromOFXAsync(string path, string defaultTransactionRGBA)
    {
        OFXDocument? ofx = null;
        //Check For Security
        var ofxString = File.ReadAllText(path);
        if (ofxString.Contains("SECURITY:TYPE1"))
        {
            ofxString = ofxString.Replace("SECURITY:TYPE1", "SECURITY:NONE");
        }
        //Parse OFX
        try
        {
            ofx = new OFXDocumentParser().Import(ofxString);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return ImportResult.Empty;
        }
        //Transactions
        var importResult = new ImportResult();
        foreach (var transaction in ofx!.Transactions)
        {
            if (transaction.Amount != 0)
            {
                var t = new Transaction(NextAvailableTransactionId)
                {
                    Description = string.IsNullOrWhiteSpace(transaction.Name) ? (string.IsNullOrWhiteSpace(transaction.Memo) ? _("N/A") : transaction.Memo) : transaction.Name,
                    Date = DateOnly.FromDateTime(transaction.Date),
                    Type = transaction.Amount > 0 ? TransactionType.Income : TransactionType.Expense,
                    Amount = Math.Abs(transaction.Amount),
                    RGBA = defaultTransactionRGBA
                };
                if ((await AddTransactionAsync(t)).Successful)
                {
                    importResult.NewTransactionIds.Add(t.Id);
                }
            }
        }
        return importResult;
    }

    /// <summary>
    /// Imports transactions from a QIF file
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="defaultTransactionRGBA">The default color for a transaction</param>
    /// <param name="defaultGroupRGBA">The default color for a group</param>
    /// <returns>ImportResult</returns>
    private async Task<ImportResult> ImportFromQIFAsync(string path, string defaultTransactionRGBA, string defaultGroupRGBA)
    {
        QifDocument? qif = null;
        try
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            qif = QifDocument.Load(File.OpenRead(path));
            Thread.CurrentThread.CurrentCulture = oldCulture;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return ImportResult.Empty;
        }
        var importResult = new ImportResult();
        //Groups
        foreach (var group in qif.CategoryListTransactions)
        {
            if (Groups.Values.FirstOrDefault(x => x.Name == group.CategoryName) == null)
            {
                var g = new Group(NextAvailableGroupId)
                {
                    Name = group.CategoryName,
                    Description = group.Description,
                    RGBA = defaultGroupRGBA
                };
                if (await AddGroupAsync(g))
                {
                    importResult.NewGroupIds.Add(g.Id);
                }
            }
        }
        //Transactions
        foreach (var transaction in qif.BankTransactions.Concat(qif.CashTransactions).Concat(qif.CreditCardTransactions))
        {
            if (transaction.Amount != 0)
            {
                var group = Groups.Values.FirstOrDefault(x => x.Name == transaction.Category);
                var t = new Transaction(NextAvailableTransactionId)
                {
                    Description = string.IsNullOrWhiteSpace(transaction.Memo) ? _("N/A") : transaction.Memo,
                    Date = DateOnly.FromDateTime(transaction.Date),
                    Type = transaction.Amount > 0 ? TransactionType.Income : TransactionType.Expense,
                    Amount = Math.Abs(transaction.Amount),
                    GroupId = group == null ? -1 : (int)group.Id,
                    UseGroupColor = group != null,
                    RGBA = defaultTransactionRGBA
                };
                if ((await AddTransactionAsync(t)).Successful)
                {
                    importResult.NewTransactionIds.Add(t.Id);
                }
            }
        }
        return importResult;
    }

    /// <summary>
    /// Exports the account to a CSV file
    /// </summary>
    /// <param name="path">The path to the CSV file</param>
    /// <param name="exportMode">The information to export</param>
    /// <param name="filteredIds">A list of filtered ids</param>
    /// <returns>True if successful, else false</returns>
    public bool ExportToCSV(string path, ExportMode exportMode, List<uint> filteredIds)
    {
        string result = "";
        result += "ID;Date (en_US Format);Description;Type;RepeatInterval;RepeatFrom (-1=None,0=Original,Other=Id Of Source);RepeatEndDate (en_US Format);Amount (en_US Format);RGBA;UseGroupColor (0 for false, 1 for true);Group(Id Starts At 1);GroupName;GroupDescription;GroupRGBA;Tags\n";
        var transactions = Transactions;
        if (exportMode == ExportMode.CurrentView)
        {
            transactions = new Dictionary<uint, Transaction>();
            foreach (var id in filteredIds)
            {
                transactions.Add(id, Transactions[id]);
            }
        }
        foreach (var pair in transactions)
        {
            result += $"{pair.Value.Id};{pair.Value.Date.ToString("d", new CultureInfo("en-US"))};{pair.Value.Description};{(int)pair.Value.Type};{(int)pair.Value.RepeatInterval};{pair.Value.RepeatFrom};{(pair.Value.RepeatEndDate != null ? pair.Value.RepeatEndDate.Value.ToString("d", new CultureInfo("en-US")) : "")};{pair.Value.Amount};{pair.Value.RGBA};{(pair.Value.UseGroupColor ? "1" : "0")};{pair.Value.GroupId};";
            if (pair.Value.GroupId != -1)
            {
                var group = Groups[(uint)pair.Value.GroupId];
                result += $"{group.Name};{group.Description};{group.RGBA};";
            }
            else
            {
                result += ";;;";
            }
            result += $"{string.Join(',', pair.Value.Tags)}\n";
        }
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            File.WriteAllText(path, result);
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return false;
        }
    }

    /// <summary>
    /// Exports the account to a PDF file
    /// </summary>
    /// <param name="path">The path to the PDF file</param>
    /// <param name="exportMode">The information to export</param>
    /// <param name="filteredIds">A list of filtered ids</param>
    /// <param name="password">The password to protect the PDF file with (null for no security)</param>
    /// <returns>True if successful, else false</returns>
    public bool ExportToPDF(string path, ExportMode exportMode, List<uint> filteredIds, string? password)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        try
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
            var cultureAmount = CultureHelpers.GetNumberCulture(Metadata);
            var regionAmount = new RegionInfo(cultureAmount.Name);
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
                        row.RelativeItem(1).AlignRight().Width(32, Unit.Point).Height(32, Unit.Point).Image(appiconStream).FitArea();
                    });
                    //Content
                    page.Content().PaddingVertical(0.4f, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);
                        //Generated Date
                        col.Item().Text(_("Generated: {0}", DateTime.Now.ToString("g", CultureHelpers.DateCulture)));
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
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(_("Overview"));
                            //Data
                            var maxDate = DateOnly.FromDateTime(DateTime.Today);
                            var transactions = Transactions;
                            if (exportMode == ExportMode.CurrentView)
                            {
                                transactions = new Dictionary<uint, Transaction>();
                                foreach (var id in filteredIds)
                                {
                                    transactions.Add(id, Transactions[id]);
                                }
                            }
                            foreach (var pair in transactions)
                            {
                                if (pair.Value.Date > maxDate)
                                {
                                    maxDate = pair.Value.Date;
                                }
                            }
                            tbl.Cell().Text(_("Total"));
                            var total = GetTotal(filteredIds);
                            tbl.Cell().AlignRight().Text($"{(total < 0 ? "-  " : "+  ")}{total.ToAmountString(cultureAmount, Configuration.Current.UseNativeDigits)}");
                            tbl.Cell().Background(Colors.Grey.Lighten3).Text(_("Income"));
                            tbl.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text(GetIncome(filteredIds).ToAmountString(cultureAmount, Configuration.Current.UseNativeDigits));
                            tbl.Cell().Text(_("Expense"));
                            tbl.Cell().AlignRight().Text(GetExpense(filteredIds).ToAmountString(cultureAmount, Configuration.Current.UseNativeDigits));
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Image(GenerateGraph(GraphType.IncomeExpenseOverTime, false, filteredIds, -1, -1, false));
                        });
                        //Metadata
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //Type, Currency
                                x.RelativeColumn();
                                x.RelativeColumn();
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(_("Account Settings"));
                            tbl.Cell().Text(_("Account Type")).SemiBold();
                            tbl.Cell().Text(_("Currency")).SemiBold();
                            //Data
                            tbl.Cell().Background(Colors.Grey.Lighten3).Text(Metadata.AccountType switch
                            {
                                AccountType.Checking => _("Checking"),
                                AccountType.Savings => _("Savings"),
                                AccountType.Business => _("Business"),
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
                                x.RelativeColumn(1);
                                x.RelativeColumn(2);
                                x.RelativeColumn(1);
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten1).Text(_("Groups"));
                            tbl.Cell().Text(_("Name")).SemiBold();
                            tbl.Cell().Text(_("Description")).SemiBold();
                            tbl.Cell().AlignRight().Text(_("Amount")).SemiBold();
                            //Data
                            var i = 0;
                            foreach (var pair in Groups.OrderBy(x => x.Value.Name == _("Ungrouped") ? " " : x.Value.Name))
                            {
                                var balance = GetGroupTotal(pair.Value, filteredIds);
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Name);
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Description);
                                tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).AlignRight().Text($"{(balance < 0 ? "−  " : "+  ")}{balance.ToAmountString(cultureAmount, Configuration.Current.UseNativeDigits)}");
                                i++;
                            }
                            tbl.Cell().ColumnSpan(3).Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Image(GenerateGraph(GraphType.IncomeExpensePerGroup, false, filteredIds));
                        });
                        //Transactions
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //ID, Date, Description, Type, GroupName, Tags, Notes, Amount
                                x.RelativeColumn(1.5f);
                                x.RelativeColumn(2);
                                x.RelativeColumn(3);
                                x.RelativeColumn(2);
                                x.RelativeColumn(2);
                                x.RelativeColumn(2);
                                x.RelativeColumn(3);
                                x.RelativeColumn(2);
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(8).Background(Colors.Grey.Lighten1).Text(_("Transactions"));
                            tbl.Cell().Text(_("Id")).SemiBold();
                            tbl.Cell().Text(_("Date")).SemiBold();
                            tbl.Cell().Text(_("Description")).SemiBold();
                            tbl.Cell().Text(_("Type")).SemiBold();
                            tbl.Cell().Text(_("Group Name")).SemiBold();
                            tbl.Cell().Text(_("Tags")).SemiBold();
                            tbl.Cell().Text(_("Notes")).SemiBold();
                            tbl.Cell().AlignRight().Text(_("Amount")).SemiBold();
                            //Data
                            var transactions = Transactions;
                            if (exportMode == ExportMode.CurrentView)
                            {
                                transactions = new Dictionary<uint, Transaction>();
                                foreach (var id in filteredIds)
                                {
                                    transactions.Add(id, Transactions[id]);
                                }
                            }
                            foreach (var pair in transactions)
                            {
                                var hex = "#32"; //120
                                var rgba = pair.Value.UseGroupColor ? Groups[pair.Value.GroupId <= 0 ? 0u : (uint)pair.Value.GroupId].RGBA : pair.Value.RGBA;
                                if (string.IsNullOrWhiteSpace(rgba))
                                {
                                    hex = "#32FFFFFF";
                                }
                                else
                                {
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
                                }
                                tbl.Cell().Background(hex).Text(pair.Value.Id.ToString());
                                tbl.Cell().Background(hex).Text(pair.Value.Date.ToString("d", CultureHelpers.DateCulture));
                                tbl.Cell().Background(hex).Text(pair.Value.Description.Trim());
                                tbl.Cell().Background(hex).Text(pair.Value.Type switch
                                {
                                    TransactionType.Income => _("Income"),
                                    TransactionType.Expense => _("Expense"),
                                    _ => ""
                                });
                                tbl.Cell().Background(hex).Text(pair.Value.GroupId == -1 ? _("Ungrouped") : Groups[(uint)pair.Value.GroupId].Name);
                                tbl.Cell().Background(hex).Text(string.Join(", ", pair.Value.Tags));
                                tbl.Cell().Background(hex).Text(pair.Value.Notes);
                                tbl.Cell().Background(hex).AlignRight().Text($"{(pair.Value.Type == TransactionType.Income ? "+  " : "−  ")}{pair.Value.Amount.ToAmountString(cultureAmount, Configuration.Current.UseNativeDigits)}");
                            }
                        });
                        //Receipts
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(x =>
                            {
                                //ID, Receipt
                                x.RelativeColumn(.5f);
                                x.RelativeColumn(2);
                            });
                            //Headers
                            tbl.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten1).Text(_("Receipts"));
                            tbl.Cell().Text(_("Id")).SemiBold();
                            tbl.Cell().Text(_("Receipt")).SemiBold();
                            //Data
                            var transactions = Transactions;
                            if (exportMode == ExportMode.CurrentView)
                            {
                                transactions = new Dictionary<uint, Transaction>();
                                foreach (var id in filteredIds)
                                {
                                    transactions.Add(id, Transactions[id]);
                                }
                            }
                            var i = 0;
                            foreach (var pair in transactions)
                            {
                                if (pair.Value.Receipt != null)
                                {
                                    using var memoryStream = new MemoryStream();
                                    pair.Value.Receipt.Save(memoryStream, new JpegEncoder());
                                    tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).Text(pair.Value.Id.ToString());
                                    tbl.Cell().Background(i % 2 == 0 ? Colors.Grey.Lighten3 : Colors.White).MinWidth(300).MinHeight(300).MaxWidth(300).MaxHeight(300).Image(memoryStream.ToArray()).FitArea();
                                    i++;
                                }
                            }
                        });
                    });
                    //Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem(2).Text(_("Nickvision Denaro Account")).FontColor(Colors.Grey.Medium);
                        row.RelativeItem(1).Text(x =>
                        {
                            var pageString = _("Page {0}");
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
            if (password != null)
            {
                var pdf = PdfReader.Open(path);
                var pdfSecurity = pdf.SecuritySettings;
                pdfSecurity.UserPassword = password;
                pdf.Save(path);
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Generates a graph based on the type
    /// </summary>
    /// <param name="type">GraphType</param>
    /// <param name="darkMode">Whether or not to draw the graph in dark mode</param>
    /// <param name="filteredIds">A list of filtered ids</param>
    /// <param name="width">The width of the graph</param>
    /// <param name="height">The height of the graph</param>
    /// <param name="showLegend">Whether or not to show the legend</param>
    /// <returns>The byte[] of the graph</returns>
    public byte[] GenerateGraph(GraphType type, bool darkMode, List<uint> filteredIds, int width = -1, int height = -1, bool showLegend = true)
    {
        InMemorySkiaSharpChart? chart = null;
        if (type == GraphType.IncomeExpensePie)
        {
            var income = GetIncome(filteredIds);
            var expense = GetExpense(filteredIds);
            chart = new SKPieChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new PieSeries<decimal> { Name = _("Income"), Values = new decimal[] { income }, Fill = new SolidColorPaint(SKColors.Green) },
                    new PieSeries<decimal> { Name = _("Expense"), Values = new decimal[] { expense }, Fill = new SolidColorPaint(SKColors.Red) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeExpensePerGroup)
        {
            var data = new Dictionary<string, decimal[]>();
            foreach (var groupId in Groups.Keys)
            {
                var group = Groups[groupId];
                data[group.Name] = new[] { GetGroupIncome(group, filteredIds), GetGroupExpense(group, filteredIds) };
            }
            chart = new SKCartesianChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new ColumnSeries<decimal>() { Name = _("Income"), Values = data.OrderBy(x => x.Key == _("Ungrouped") ? " " : x.Key).Select(x => x.Value[0]).ToArray(), Fill = new SolidColorPaint(SKColors.Green) },
                    new ColumnSeries<decimal>() { Name = _("Expense"), Values = data.OrderBy(x => x.Key == _("Ungrouped") ? " " : x.Key).Select(x => x.Value[1]).ToArray(), Fill = new SolidColorPaint(SKColors.Red) },
                },
                XAxes = new Axis[]
                {
                    new Axis() { Labels = data.Keys.OrderBy(x => x == _("Ungrouped") ? " " : x).ToArray(), LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                YAxes = new Axis[]
                {
                    new Axis() { LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeExpenseOverTime)
        {
            //Graph
            var data = new Dictionary<DateOnly, decimal[]>();
            foreach (var id in filteredIds)
            {
                var transaction = Transactions[id];
                if (!data.ContainsKey(transaction.Date))
                {
                    data.Add(transaction.Date, new decimal[2] { 0m, 0m });
                }
                if (transaction.Type == TransactionType.Income)
                {
                    data[transaction.Date][0] += transaction.Amount;
                }
                else
                {
                    data[transaction.Date][1] += transaction.Amount;
                }
            }
            chart = new SKCartesianChart()
            {
                Background = SKColor.Empty,
                Series = new ISeries[]
                {
                    new LineSeries<decimal>() { Name = _("Income"), Values = data.OrderBy(x => x.Key).Select(x => x.Value[0]).ToArray(), GeometryFill = new SolidColorPaint(SKColors.Green), GeometryStroke = new SolidColorPaint(SKColors.Green), Fill = null, Stroke = new SolidColorPaint(SKColors.Green) },
                    new LineSeries<decimal>() { Name = _("Expense"), Values = data.OrderBy(x => x.Key).Select(x => x.Value[1]).ToArray(), GeometryFill = new SolidColorPaint(SKColors.Red), GeometryStroke = new SolidColorPaint(SKColors.Red), Fill = null, Stroke = new SolidColorPaint(SKColors.Red) }
                },
                XAxes = new Axis[]
                {
                    new Axis() { Labels = data.Keys.Order().Select(x => x.ToString("d", CultureHelpers.DateCulture)).ToArray(), LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black), LabelsRotation = 50 }
                },
                YAxes = new Axis[]
                {
                    new Axis() { LabelsPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black) }
                },
                LegendPosition = showLegend ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        else if (type == GraphType.IncomeByGroup || type == GraphType.ExpenseByGroup)
        {
            var data = new Dictionary<uint, decimal>();
            if (type == GraphType.IncomeByGroup)
            {
                foreach (var groupId in Groups.Keys)
                {
                    var group = Groups[groupId];
                    data[groupId] = GetGroupIncome(group, filteredIds);
                }
            }
            else
            {
                foreach (var groupId in Groups.Keys)
                {
                    var group = Groups[groupId];
                    data[groupId] = GetGroupExpense(group, filteredIds);
                }
            }
            var series = new List<ISeries>(data.Count);
            foreach (var pair in data.OrderBy(x => Groups[x.Key].Name == _("Ungrouped") ? " " : Groups[x.Key].Name))
            {
                var hex = "#FF"; //255
                var rgba = string.IsNullOrWhiteSpace(Groups[pair.Key].RGBA) ? Configuration.Current.GroupDefaultColor : Groups[pair.Key].RGBA;
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
                series.Add(new PieSeries<decimal>()
                {
                    Name = Groups[pair.Key].Name,
                    Values = new decimal[] { pair.Value },
                    Fill = new SolidColorPaint(SKColor.Parse(hex))
                });
            }
            chart = new SKPieChart()
            {
                Title = new LabelVisual()
                {
                    Text = type == GraphType.IncomeByGroup ? _("Income") : _("Expense"),
                    Paint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
                    Padding = new LiveChartsCore.Drawing.Padding(15),
                    TextSize = 16
                },
                Background = SKColor.Empty,
                Series = series,
                LegendPosition = showLegend ? LegendPosition.Bottom : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(darkMode ? SKColors.White : SKColors.Black),
            };
        }
        if (chart != null)
        {
            if (width > 0)
            {
                chart.Width = width;
            }
            if (height > 0)
            {
                chart.Height = height;
            }
            return chart.GetImage().Encode().ToArray();
        }
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Populates the TransactionReminders list
    /// </summary>
    private void CalculateTransactionReminders()
    {
        TransactionReminders.Clear();
        if (Metadata.TransactionRemindersThreshold == RemindersThreshold.Never)
        {
            return;
        }
        var today = DateOnly.FromDateTime(DateTime.Today);
        foreach (var pair in Transactions)
        {
            var upcomingDate = today;
            if (pair.Value.RepeatFrom == 0 && pair.Value.Date <= today) //repeat transactions
            {
                var latestRepeat = pair.Value;
                foreach (var pair2 in Transactions)
                {
                    if (pair2.Value.RepeatFrom == pair.Value.Id)
                    {
                        if (pair2.Value.Date > latestRepeat.Date)
                        {
                            latestRepeat = pair2.Value;
                        }
                    }
                }
                var nextRepeatDate = latestRepeat.Date;
                if (pair.Value.RepeatInterval == TransactionRepeatInterval.Daily)
                {
                    nextRepeatDate = nextRepeatDate.AddDays(1);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Weekly)
                {
                    nextRepeatDate = nextRepeatDate.AddDays(7);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Biweekly)
                {
                    nextRepeatDate = nextRepeatDate.AddDays(14);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Monthly)
                {
                    nextRepeatDate = nextRepeatDate.AddMonths(1);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Quarterly)
                {
                    nextRepeatDate = nextRepeatDate.AddMonths(3);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Yearly)
                {
                    nextRepeatDate = nextRepeatDate.AddYears(1);
                }
                else if (pair.Value.RepeatInterval == TransactionRepeatInterval.Biyearly)
                {
                    nextRepeatDate = nextRepeatDate.AddYears(2);
                }
                if (nextRepeatDate > today)
                {
                    upcomingDate = nextRepeatDate;
                }
            }
            else if (pair.Value.Date > today) //future transactions
            {
                upcomingDate = pair.Value.Date;
            }
            if (upcomingDate != today) //add reminder
            {
                var culture = CultureHelpers.GetNumberCulture(Metadata);
                if (Metadata.TransactionRemindersThreshold == RemindersThreshold.OneDayBefore && upcomingDate.AddDays(-1) == today)
                {
                    TransactionReminders.Add(($"{pair.Value.Description} - {pair.Value.Amount.ToAmountString(culture, Configuration.Current.UseNativeDigits)}", _("Tomorrow")));
                }
                else if (Metadata.TransactionRemindersThreshold == RemindersThreshold.OneWeekBefore && upcomingDate.AddDays(-7) <= today)
                {
                    TransactionReminders.Add(($"{pair.Value.Description} - {pair.Value.Amount.ToAmountString(culture, Configuration.Current.UseNativeDigits)}", _("One week from now")));
                }
                else if (Metadata.TransactionRemindersThreshold == RemindersThreshold.OneMonthBefore && upcomingDate.AddMonths(-1) <= today)
                {
                    TransactionReminders.Add(($"{pair.Value.Description} - {pair.Value.Amount.ToAmountString(culture, Configuration.Current.UseNativeDigits)}", _("One month from now")));
                }
                else if (Metadata.TransactionRemindersThreshold == RemindersThreshold.TwoMonthsBefore && upcomingDate.AddMonths(-2) <= today)
                {
                    TransactionReminders.Add(($"{pair.Value.Description} - {pair.Value.Amount.ToAmountString(culture, Configuration.Current.UseNativeDigits)}", _("Two months from now")));
                }
            }
        }
    }

    /// <summary>
    /// Backups the account to CSV backup folder location
    /// </summary>
    private void BackupAccountToCSV()
    {
        if (!(_isEncrypted ?? false) && Directory.Exists(Configuration.Current.CSVBackupFolder))
        {
            ExportToCSV($"{Configuration.Current.CSVBackupFolder}{System.IO.Path.DirectorySeparatorChar}{Metadata.Name}.csv", ExportMode.All, new List<uint>());
        }
    }
}

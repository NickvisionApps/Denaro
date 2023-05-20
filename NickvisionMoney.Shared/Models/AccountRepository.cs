using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NickvisionMoney.Shared.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace NickvisionMoney.Shared.Models;


class AccountRepository : IDisposable
{

    /// <summary>
    /// The path of the account
    /// </summary>
    public string Path { get; init; }
    private SqliteConnection? _database;
    public SqliteConnection Database { get => _database!; }
    private bool? _isEncrypted;
    private bool _loggedIn;
    public uint NextAvailableGroupId { get; set; }
    public uint NextAvailableTransactionId { get; set; }
    public bool NeedsAccountSetup { get; set; }
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

    public bool LoggedIn { get => _loggedIn; }

    internal AccountRepository(string path)
    {
        _loggedIn = false;
        _isEncrypted = null;
        Path = path;
        NextAvailableGroupId = 1;
        NextAvailableTransactionId = 1;
        NeedsAccountSetup = true;
    }
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

    internal bool Migrate()
    {
        if (!_loggedIn)
        {
            return false;
        }
        //Setup Metadata Table
        using var cmdTableMetadata = _database!.CreateCommand();
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
        cmdTableTransactions.CommandText = "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT, useGroupColor INTEGER, notes TEXT)";
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

        return true;
    }

    internal bool UpdateMetadata(AccountMetadata metadata)
    {
        using var cmdUpdateMetadata = _database!.CreateCommand();
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
        cmdUpdateMetadata.Parameters.AddWithValue("$customGroupSeparator", string.IsNullOrEmpty(metadata.CustomCurrencyGroupSeparator) ? "empty" : metadata.CustomCurrencyGroupSeparator);
        cmdUpdateMetadata.Parameters.AddWithValue("$customDecimalDigits", metadata.CustomCurrencyDecimalDigits ?? 2);
        if (cmdUpdateMetadata.ExecuteNonQuery() > 0)
        {
            return true;
        }
        return false;
    }

    internal bool SetPassword(string password)
    {
        //Remove Password If Empty (Decrypts)
        if (string.IsNullOrEmpty(password))
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
        cmdQuote.Parameters.AddWithValue("$password", password);
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
                Password = password
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
                Password = password
            }.ConnectionString);
            _database.Open();
            _isEncrypted = true;
        }
        return true;
    }

    internal AccountMetadata GetMetadata()
    {
        var metadata = new AccountMetadata(System.IO.Path.GetFileNameWithoutExtension(Path), AccountType.Checking);
        //Get Metadata
        using var cmdQueryMetadata = _database.CreateCommand();
        cmdQueryMetadata.CommandText = "SELECT * FROM metadata where id = 0";
        using var readQueryMetadata = cmdQueryMetadata.ExecuteReader();
        if (readQueryMetadata.HasRows)
        {
            readQueryMetadata.Read();
            metadata.Name = readQueryMetadata.GetString(1);
            metadata.AccountType = (AccountType)readQueryMetadata.GetInt32(2);
            metadata.UseCustomCurrency = readQueryMetadata.GetBoolean(3);
            metadata.CustomCurrencySymbol = string.IsNullOrEmpty(readQueryMetadata.GetString(4)) ? null : readQueryMetadata.GetString(4);
            metadata.CustomCurrencyCode = string.IsNullOrEmpty(readQueryMetadata.GetString(5)) ? null : readQueryMetadata.GetString(5);
            metadata.DefaultTransactionType = (TransactionType)readQueryMetadata.GetInt32(6);
            metadata.ShowGroupsList = readQueryMetadata.GetBoolean(7);
            metadata.SortFirstToLast = readQueryMetadata.GetBoolean(8);
            metadata.SortTransactionsBy = readQueryMetadata.IsDBNull(9) ? SortBy.Id : (SortBy)readQueryMetadata.GetInt32(9);
            metadata.CustomCurrencyDecimalSeparator = readQueryMetadata.IsDBNull(10) ? null : readQueryMetadata.GetString(10);
            metadata.CustomCurrencyGroupSeparator = readQueryMetadata.IsDBNull(11) ? null : (readQueryMetadata.GetString(11) == "empty" ? "" : readQueryMetadata.GetString(11));
            metadata.CustomCurrencyDecimalDigits = readQueryMetadata.IsDBNull(12) ? null : readQueryMetadata.GetInt32(12);
            NeedsAccountSetup = metadata.UseCustomCurrency && (string.IsNullOrEmpty(metadata.CustomCurrencySymbol) || string.IsNullOrEmpty(metadata.CustomCurrencyCode) || string.IsNullOrEmpty(metadata.CustomCurrencyDecimalSeparator) || metadata.CustomCurrencyGroupSeparator == null || metadata.CustomCurrencyDecimalDigits == null);
        }
        else
        {
            using var cmdAddMetadata = _database.CreateCommand();
            cmdAddMetadata.CommandText = "INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits) VALUES (0, $name, $type, $useCustomCurrency, $customSymbol, $customCode, $defaultTransactionType, $showGroupsList, $sortFirstToLast, $sortTransactionsBy, $customDecimalSeparator, $customGroupSeparator, $customDecimalDigits)";
            cmdAddMetadata.Parameters.AddWithValue("$name", metadata.Name);
            cmdAddMetadata.Parameters.AddWithValue("$type", (int)metadata.AccountType);
            cmdAddMetadata.Parameters.AddWithValue("$useCustomCurrency", metadata.UseCustomCurrency);
            cmdAddMetadata.Parameters.AddWithValue("$customSymbol", metadata.CustomCurrencySymbol ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$customCode", metadata.CustomCurrencyCode ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$defaultTransactionType", (int)metadata.DefaultTransactionType);
            cmdAddMetadata.Parameters.AddWithValue("$showGroupsList", metadata.ShowGroupsList);
            cmdAddMetadata.Parameters.AddWithValue("$sortFirstToLast", metadata.SortFirstToLast);
            cmdAddMetadata.Parameters.AddWithValue("$sortTransactionsBy", (int)metadata.SortTransactionsBy);
            cmdAddMetadata.Parameters.AddWithValue("$customDecimalSeparator", metadata.CustomCurrencyDecimalSeparator ?? "");
            cmdAddMetadata.Parameters.AddWithValue("$customGroupSeparator", string.IsNullOrEmpty(metadata.CustomCurrencyGroupSeparator) ? "empty" : metadata.CustomCurrencyGroupSeparator);
            cmdAddMetadata.Parameters.AddWithValue("$customDecimalDigits", metadata.CustomCurrencyDecimalDigits ?? 2);
            cmdAddMetadata.ExecuteNonQuery();
        }
        return metadata;
    }

    internal Dictionary<uint, Transaction> GetTransactions()
    {
        var transactions = new Dictionary<uint, Transaction>();
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
                RepeatEndDate = readQueryTransactions.IsDBNull(10) ? null : (string.IsNullOrEmpty(readQueryTransactions.GetString(10)) ? null : DateOnly.Parse(readQueryTransactions.GetString(10), new CultureInfo("en-US", false))),
                Notes = readQueryTransactions.IsDBNull(12) ? "" : readQueryTransactions.GetString(12),
            };
            var receiptString = readQueryTransactions.IsDBNull(8) ? "" : readQueryTransactions.GetString(8);
            if (!string.IsNullOrEmpty(receiptString))
            {
                transaction.Receipt = Image.Load(Convert.FromBase64String(receiptString), new JpegDecoder());
            }
            transactions.Add(transaction.Id, transaction);
            if (transaction.Id >= NextAvailableTransactionId)
            {
                NextAvailableTransactionId = transaction.Id + 1;
            }
        }
        return transactions;
    }

    internal Dictionary<uint, Group> GetGroups()
    {
        var groups = new Dictionary<uint, Group>();

        //Get Groups
        using var localizer = new Localizer();
        groups.Add(0, new Group(0)
        {
            Name = localizer["Ungrouped"],
            Description = localizer["UngroupedDescription"],
            Balance = 0m,
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
                Balance = 0m,
                RGBA = readQueryGroups.IsDBNull(3) ? "" : readQueryGroups.GetString(3)
            };
            groups.Add(group.Id, group);
            if (group.Id >= NextAvailableGroupId)
            {
                NextAvailableGroupId = group.Id + 1;
            }
        }
        return groups;
    }

    internal async Task<Group?> AddGroupAsync(Group group)
    {
        using var cmdAddGroup = _database!.CreateCommand();
        cmdAddGroup.CommandText = "INSERT INTO groups (id, name, description, rgba) VALUES ($id, $name, $description, $rgba)";
        cmdAddGroup.Parameters.AddWithValue("$id", group.Id);
        cmdAddGroup.Parameters.AddWithValue("$name", group.Name);
        cmdAddGroup.Parameters.AddWithValue("$description", group.Description);
        cmdAddGroup.Parameters.AddWithValue("$rgba", group.RGBA);
        if (await cmdAddGroup.ExecuteNonQueryAsync() > 0)
        {
            return group;
        }
        return null;
    }

    internal async Task<Group?> UpdateGroupAsync(Group group)
    {
        using var cmdUpdateGroup = _database!.CreateCommand();
        cmdUpdateGroup.CommandText = "UPDATE groups SET name = $name, description = $description, rgba = $rgba WHERE id = $id";
        cmdUpdateGroup.Parameters.AddWithValue("$name", group.Name);
        cmdUpdateGroup.Parameters.AddWithValue("$description", group.Description);
        cmdUpdateGroup.Parameters.AddWithValue("$rgba", group.RGBA);
        cmdUpdateGroup.Parameters.AddWithValue("$id", group.Id);
        if (await cmdUpdateGroup.ExecuteNonQueryAsync() > 0)
        {
            return group;
        }
        return null;
    }

    internal async Task<Transaction?> AddTransactionAsync(Transaction transaction)
    {
        using var cmdAddTransaction = _database!.CreateCommand();
        cmdAddTransaction.CommandText = "INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba, receipt, repeatFrom, repeatEndDate, useGroupColor, notes) VALUES ($id, $date, $description, $type, $repeat, $amount, $gid, $rgba, $receipt, $repeatFrom, $repeatEndDate, $useGroupColor, $notes)";
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
            return transaction;
        }
        return null;
    }


    public void Dispose()
    {
        if (_database != null)
        {
            _database.Close();
            _database.Dispose();
            _database = null;
        }
    }
}
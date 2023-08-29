using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// Types of an account
/// </summary>
public enum AccountType
{
    Checking = 0,
    Savings,
    Business
}

/// <summary>
/// Ways to sort transactions
/// </summary>
public enum SortBy
{
    Id = 0,
    Date,
    Amount
}

/// <summary>
/// Thresholds for when to show a reminder
/// </summary>
public enum RemindersThreshold
{
    Never,
    OneDayBefore,
    OneWeekBefore,
    OneMonthBefore,
    TwoMonthsBefore
}

/// <summary>
/// A model of metadata for an account
/// </summary>
public class AccountMetadata : ICloneable
{
    private int? _customCurrencyAmountStyle;
    
    /// <summary>
    /// The name of the account
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// The type of the account
    /// </summary>
    public AccountType AccountType { get; set; }
    /// <summary>
    /// Whether or not to use a custom currency
    /// </summary>
    public bool UseCustomCurrency { get; set; }
    /// <summary>
    /// The symbol of the custom currency
    /// </summary>
    public string? CustomCurrencySymbol { get; set; }
    /// <summary>
    /// The code of the custom currency
    /// </summary>
    public string? CustomCurrencyCode { get; set; }
    /// <summary>
    /// Decimal separator for custom currency
    /// </summary>
    public string? CustomCurrencyDecimalSeparator { get; set; }
    /// <summary>
    /// Group separator for custom currency
    /// </summary>
    public string? CustomCurrencyGroupSeparator { get; set; }
    /// <summary>
    /// Decimal digits number for custom currency
    /// </summary>
    public int? CustomCurrencyDecimalDigits { get; set; }
    /// <summary>
    /// The default transaction type of the account
    /// </summary>
    public TransactionType DefaultTransactionType { get; set; }
    /// <summary>
    /// The threshold for showing transaction reminders
    /// </summary>
    public RemindersThreshold TransactionRemindersThreshold { get; set; }
    /// <summary>
    /// Whether or not to show the groups section on the account view
    /// </summary>
    public bool ShowGroupsList { get; set; }
    /// <summary>
    /// Whether or not to show the tags section on the account view
    /// </summary>
    public bool ShowTagsList { get; set; }
    /// <summary>
    /// Whether or not to sort transactions from first to last
    /// </summary>
    public bool SortFirstToLast { get; set; }
    /// <summary>
    /// The way in which to sort transactions
    /// </summary>
    public SortBy SortTransactionsBy { get; set; }

    /// <summary>
    /// Constructs a new AccountMetadata
    /// </summary>
    /// <param name="name">The name of the account</param>
    /// <param name="accountType">The type of the account</param>
    internal AccountMetadata(string name, AccountType accountType)
    {
        Name = name;
        AccountType = accountType;
        UseCustomCurrency = false;
        CustomCurrencySymbol = null;
        CustomCurrencyCode = null;
        CustomCurrencyAmountStyle = null;
        CustomCurrencyDecimalSeparator = null;
        CustomCurrencyGroupSeparator = null;
        CustomCurrencyDecimalDigits = null;
        DefaultTransactionType = TransactionType.Income;
        TransactionRemindersThreshold = RemindersThreshold.OneDayBefore;
        ShowGroupsList = true;
        ShowTagsList = true;
        SortFirstToLast = false;
        SortTransactionsBy = SortBy.Date;
    }

    /// <summary>
    /// The style to use for displaying an amount
    /// </summary>
    /// <remarks>Must be a value between 0 and 3. See https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0#remarks for the values' meaning</remarks>
    public int? CustomCurrencyAmountStyle
    {
        get => _customCurrencyAmountStyle;

        set
        {
            if (value != null && (value < 0 || value > 3))
            {
                value = 0;
            }
            _customCurrencyAmountStyle = value;
        }
    }
    
    /// <summary>
    /// Loads metadata from an account file
    /// </summary>
    /// <param name="path">The path to the account file</param>
    /// <returns>AccountMetadata?</returns>
    public static AccountMetadata? LoadFromAccountFile(string path, string? password)
    {
        if (Path.GetExtension(path).ToLower() != ".nmoney")
        {
            return null;
        }
        var connectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        };
        if (!string.IsNullOrEmpty(password))
        {
            connectionString.Password = password;
        }
        using var database = new SqliteConnection(connectionString.ConnectionString);
        try
        {
            database.Open();
        }
        catch
        {
            database.Close();
            return null;
        }
        // Get Metadata
        var result = new AccountMetadata(Path.GetFileNameWithoutExtension(path), AccountType.Checking);
        var cmdQueryMetadata = database.CreateCommand();
        cmdQueryMetadata.CommandText = "SELECT * FROM metadata where id = 0";
        using var readQueryMetadata = cmdQueryMetadata.ExecuteReader();
        if (readQueryMetadata.HasRows)
        {
            readQueryMetadata.Read();
            result.Name = readQueryMetadata.GetString(1);
            result.AccountType = (AccountType)readQueryMetadata.GetInt32(2);
            result.UseCustomCurrency = readQueryMetadata.GetBoolean(3);
            result.CustomCurrencySymbol = string.IsNullOrEmpty(readQueryMetadata.GetString(4)) ? null : readQueryMetadata.GetString(4);
            result.CustomCurrencyCode = string.IsNullOrEmpty(readQueryMetadata.GetString(5)) ? null : readQueryMetadata.GetString(5);
            result.DefaultTransactionType = (TransactionType)readQueryMetadata.GetInt32(6);
            result.ShowGroupsList = readQueryMetadata.GetBoolean(7);
            result.SortFirstToLast = readQueryMetadata.GetBoolean(8);
            result.SortTransactionsBy = readQueryMetadata.FieldCount <= 9 || readQueryMetadata.IsDBNull(9) ? SortBy.Id : (SortBy)readQueryMetadata.GetInt32(9);
            result.CustomCurrencyDecimalSeparator = readQueryMetadata.FieldCount <= 10 || readQueryMetadata.IsDBNull(10) ? null : (string.IsNullOrEmpty(readQueryMetadata.GetString(10)) ? null : readQueryMetadata.GetString(10));
            result.CustomCurrencyGroupSeparator = readQueryMetadata.FieldCount <= 11 || readQueryMetadata.IsDBNull(11) ? null : (string.IsNullOrEmpty(readQueryMetadata.GetString(11)) ? null : readQueryMetadata.GetString(11));
            result.CustomCurrencyDecimalDigits = readQueryMetadata.FieldCount <= 12 || readQueryMetadata.IsDBNull(12) ? null : readQueryMetadata.GetInt32(12);
            result.ShowTagsList = readQueryMetadata.FieldCount <= 13 || readQueryMetadata.IsDBNull(13) ? true : readQueryMetadata.GetBoolean(13);
            result.TransactionRemindersThreshold = readQueryMetadata.FieldCount <= 14 || readQueryMetadata.IsDBNull(14) ? RemindersThreshold.OneDayBefore : (RemindersThreshold)readQueryMetadata.GetInt32(14);
            result.CustomCurrencyAmountStyle = readQueryMetadata.FieldCount <= 15 || readQueryMetadata.IsDBNull(15) ? null : readQueryMetadata.GetInt32(15);
        }
        database.Close();
        return result;
    }

    /// <summary>
    /// Clones the account metadata
    /// </summary>
    /// <returns>A new AccountMetadata</returns>
    public object Clone()
    {
        return new AccountMetadata(Name, AccountType)
        {
            UseCustomCurrency = UseCustomCurrency,
            CustomCurrencySymbol = CustomCurrencySymbol,
            CustomCurrencyCode = CustomCurrencyCode,
            CustomCurrencyAmountStyle = CustomCurrencyAmountStyle,
            CustomCurrencyDecimalSeparator = CustomCurrencyDecimalSeparator,
            CustomCurrencyGroupSeparator = CustomCurrencyGroupSeparator,
            CustomCurrencyDecimalDigits = CustomCurrencyDecimalDigits,
            DefaultTransactionType = DefaultTransactionType,
            TransactionRemindersThreshold = TransactionRemindersThreshold,
            ShowGroupsList = ShowGroupsList,
            ShowTagsList = ShowTagsList,
            SortFirstToLast = SortFirstToLast,
            SortTransactionsBy = SortTransactionsBy
        };
    }
}

using Microsoft.Data.Sqlite;
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
    Date
}

/// <summary>
/// A model of metadata for an account
/// </summary>
public class AccountMetadata
{
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
    /// The default transaction type of the account
    /// </summary>
    public TransactionType DefaultTransactionType { get; set; }
    /// <summary>
    /// Whether or not to show the groups section on the account view
    /// </summary>
    public bool ShowGroupsList { get; set; }
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
        DefaultTransactionType = TransactionType.Income;
        ShowGroupsList = true;
        SortFirstToLast = true;
        SortTransactionsBy = SortBy.Id;
    }

    public static AccountMetadata? LoadFromAccountFile(string path)
    {
        if(Path.GetExtension(path) != ".nmoney")
        {
            return null;
        }
        using var database = new SqliteConnection(new SqliteConnectionStringBuilder()
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadOnly
        }.ConnectionString);
        database.Open();
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
            result.SortTransactionsBy = readQueryMetadata.IsDBNull(9) ? SortBy.Id : (SortBy)readQueryMetadata.GetInt32(9);
        }
        database.Close();
        return result;
    }
}

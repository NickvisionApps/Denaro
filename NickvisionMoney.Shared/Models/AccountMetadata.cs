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
    }
}

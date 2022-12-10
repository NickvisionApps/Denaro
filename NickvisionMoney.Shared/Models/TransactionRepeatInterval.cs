namespace NickvisionMoney.Shared.Models;

/// <summary>
/// Repeat intervals for a transaction
/// </summary>
public enum TransactionRepeatInterval
{
    Never = 0,
    Daily,
    Weekly,
    Biweekly,
    Monthly,
    Quarterly,
    Yearly,
    Biyearly
}

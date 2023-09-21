using System.Collections.Generic;
using NickvisionMoney.Shared.Models;


namespace NickvisionMoney.Shared.Helpers;

public class AccountHelper
{
    /// <summary>
    /// Gets the total amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The total amount for the date range</returns>
    public static decimal GetTotal(IDictionary<uint, Transaction> transactions)
    {
        var income = GetIncome(transactions);
        var expense = GetExpense(transactions);
        return income - expense;
    }

    /// <summary>
    /// Gets the expense amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The expense amount for the date range</returns>
    public static decimal GetExpense(IDictionary<uint, Transaction> transactions)
    {
        var expense = 0m;
        foreach (var pair in transactions)
        {
            if (pair.Value.Type == TransactionType.Expense)
            {
                expense += pair.Value.Amount;
            }
        }
        return expense;
    }

    /// <summary>
    /// Gets the income amount for the date range
    /// </summary>
    /// <param name="transactions">The collection of transactions to consider</param>
    /// <returns>The income amount for the date range</returns>
    public static decimal GetIncome(IDictionary<uint, Transaction> transactions)
    {
        var income = 0m;
        foreach (var pair in transactions)
        {
            if (pair.Value.Type == TransactionType.Income)
            {
                income += pair.Value.Amount;
            }
        }
        return income;
    }
}